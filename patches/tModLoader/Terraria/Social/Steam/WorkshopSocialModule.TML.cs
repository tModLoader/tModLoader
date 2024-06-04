using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using Terraria.ModLoader;
using Terraria.ModLoader.UI.ModBrowser;
using Terraria.ModLoader.Core;
using Terraria.Social.Base;
using Terraria.Utilities;
using Terraria.Localization;
using System.Collections;

namespace Terraria.Social.Steam;

public partial class WorkshopSocialModule
{
	public override List<string> GetListOfMods() => _downloader.ModPaths;
	private ulong currPublishID = 0;

	public override bool TryGetInfoForMod(TmodFile modFile, out FoundWorkshopEntryInfo info)
	{
		info = null;
		var query = new QueryParameters() {
			searchModSlugs = new string[] { modFile.Name },
			queryType = QueryType.SearchDirect
		};

		if (!WorkshopHelper.TryGetModDownloadItemsByInternalName(query, out List<ModDownloadItem> mods)) {
			IssueReporter.ReportInstantUploadProblem("tModLoader.NoWorkshopAccess");
			return false;
		}

		currPublishID = 0;

		if (!mods.Any() || mods[0] == null) {
			// This logic is for using a local copy of Workshop.json to figure out what the publish ID is. 
			// It is currently unused and would need modifications to get the 'mod download item' for later.
			/*
			if (!AWorkshopEntry.TryReadingManifest(  <GET PATH> + Path.DirectorySeparatorChar + "workshop.json", out info))
				return false;

			currPublishID = info.workshopEntryId;
			mods[0] = Get Mod From Publish ID ()
			*/
			return false;
		}

		currPublishID = ulong.Parse(mods[0].PublishId.m_ModPubId);

		// Update the subscribed mod to be the latest version published, so keeps all versions (stable, preview) together
		WorkshopBrowserModule.Instance.DownloadItem(mods[0], uiProgress: null);

		// Grab the tags from workshop.json
		ModOrganizer.WorkshopFileFinder.Refresh(new WorkshopIssueReporter()); // Force detection in case mod wasn't installed
		string searchFolder = Path.Combine(Directory.GetParent(ModOrganizer.WorkshopFileFinder.ModPaths[0]).ToString(), $"{currPublishID}");

		return ModOrganizer.TryReadManifest(searchFolder, out info);
	}

	public override bool PublishMod(TmodFile modFile, NameValueCollection buildData, WorkshopItemPublishSettings settings)
	{
		try {
			return _PublishMod(modFile, buildData, settings);
		}
		catch (Exception e) {
			IssueReporter.ReportInstantUploadProblem(e.Message);
			return false;
		}
	}

	private bool _PublishMod(TmodFile modFile, NameValueCollection buildData, WorkshopItemPublishSettings settings)
	{
		if (!SteamedWraps.SteamClient) {
			IssueReporter.ReportInstantUploadProblem("tModLoader.SteamPublishingLimit");
			return false;
		}
		
		if (modFile.TModLoaderVersion.MajorMinor() != BuildInfo.tMLVersion.MajorMinor()) {
			IssueReporter.ReportInstantUploadProblem("tModLoader.WrongVersionCantPublishError");
			return false;
		}

		if (BuildInfo.IsDev) {
			IssueReporter.ReportInstantUploadProblem("tModLoader.BetaModCantPublishError");
			return false;
		}

		string workshopFolderPath = GetTemporaryFolderPath() + modFile.Name;
		buildData["versionsummary"] = $"{new Version(buildData["modloaderversion"])}:{buildData["version"]}";
		// Needed for backwards compat from previous version metadata
		buildData["trueversion"] = buildData["version"];

		if (currPublishID != 0) {
			// Publish by updating the files available on the current published version
			workshopFolderPath = Path.Combine(Directory.GetParent(ModOrganizer.WorkshopFileFinder.ModPaths[0]).ToString(), $"{currPublishID}");

			FixErrorsInWorkshopFolder(workshopFolderPath);

			if (!CalculateVersionsData(workshopFolderPath, ref buildData, out string failureMessage)) {
				IssueReporter.ReportInstantUploadProblem(failureMessage);
				return false;
			}
		}

		string name = buildData["displaynameclean"];
		if (name.Length >= Steamworks.Constants.k_cchPublishedDocumentTitleMax) {
			IssueReporter.ReportInstantUploadProblem("tModLoader.TitleLengthExceedLimit");
			return false;
		}

		string description = CalculateDescriptionAndChangeNotes(isCi: false, buildData, ref settings.ChangeNotes);

		List<string> tagsList = new List<string>();
		tagsList.AddRange(settings.GetUsedTagsInternalNames());
		tagsList.Add(buildData["modside"]);

		if (!TryCalculateWorkshopDeps(ref buildData)) {
			IssueReporter.ReportInstantUploadProblem("tModLoader.NoWorkshopAccess");
			return false;
		}
		
		string contentFolderPath = $"{workshopFolderPath}/{BuildInfo.tMLVersion.Major}.{BuildInfo.tMLVersion.Minor}";

		if (MakeTemporaryFolder(contentFolderPath)) {
			string modPath = Path.Combine(contentFolderPath, modFile.Name + ".tmod");

			// Solxan: File.Copy sometimes fails to delete the file that it needs to replace.
			//TODO: But why though? Needs deeper look later.
			File.Copy(modFile.path, modPath, true);

			// Cleanup Old Folders
			ModOrganizer.CleanupOldPublish(workshopFolderPath);

			// Should be called after folder created & cleaned up
			tagsList.AddRange(DetermineSupportedVersionsFromWorkshop(workshopFolderPath));

			var modPublisherInstance = new WorkshopHelper.ModPublisherInstance();

			_publisherInstances.Add(modPublisherInstance);

			modPublisherInstance.PublishContent(_publishedItems, base.IssueReporter, Forget, name, description, workshopFolderPath, settings.PreviewImagePath, settings.Publicity, tagsList.ToArray(), buildData, currPublishID, settings.ChangeNotes);

			return true;
		}

		return false;
	}

	// Output version string: "2022.05.10.20:0.2.0;2022.06.10.20:0.2.1;2022.07.10.20:0.2.2"
	// Return False if the mod version did not increase for the particular tml version
	// Return False if the mod version isn't less than releases on future tml version
	// This will have up to 1 more version than is actually relevant, but that won't break anything
	public static bool CalculateVersionsData(string workshopPath, ref NameValueCollection buildData, out string failureMessage)
	{
		var buildVersion = new Version(buildData["version"]);

		foreach (var tmod in Directory.EnumerateFiles(workshopPath, "*.tmod*", SearchOption.AllDirectories)) {
			var mod = OpenModFile(tmod);
			// Mod must have a larger version than all releases on older (or this) tModLoader versions
			if (mod.tModLoaderVersion.MajorMinor() <= BuildInfo.tMLVersion.MajorMinor()) {
				if (mod.Version >= buildVersion) {
					failureMessage = Language.GetTextValue("tModLoader.ModVersionTooSmall", buildVersion, mod.Version);
					if (mod.Version.Minor > buildVersion.Minor)
						failureMessage += $"\nThe 2nd number \"{buildVersion.Minor}\" is less than \"{mod.Version.Minor}\".";
					else if (mod.Version.Revision > buildVersion.Revision)
						failureMessage += $"\nThe 3rd number \"{buildVersion.Revision}\" is less than {mod.Version.Revision}\".";
					else if (mod.Version.MinorRevision > buildVersion.MinorRevision)
						failureMessage += $"\nThe 4th number \"{buildVersion.MinorRevision}\" is less than {mod.Version.MinorRevision}\".";
					return false;
				}
			}

			// The mod also can't have a larger version than releases on future tModLoader versions
			if (mod.tModLoaderVersion.MajorMinor() > BuildInfo.tMLVersion.MajorMinor()) {
				if (mod.Version < buildVersion) {
					failureMessage = Language.GetTextValue("tModLoader.ModVersionLargerThanFutureVersions", buildVersion, mod.Version, mod.tModLoaderVersion.MajorMinor());
					return false;
				}
			}

			if (mod.tModLoaderVersion.MajorMinor() != BuildInfo.tMLVersion.MajorMinor())
				buildData["versionsummary"] += $";{mod.tModLoaderVersion}:{mod.Version}";
		}

		failureMessage = string.Empty;
		return true;
	}

	internal static HashSet<string> DetermineSupportedVersionsFromWorkshop(string repo)
	{
		var summary = ModOrganizer.AnalyzeWorkshopTmods(repo);
		return summary.Select(info => SocialBrowserModule.GetBrowserVersionNumber(info.tModVersion)).ToHashSet();
	}

	internal static LocalMod OpenModFile(string path)
	{
		var sModFile = new TmodFile(path);
		using (sModFile.Open())
			return new LocalMod(ModLocation.Workshop, sModFile);
	}

	private static bool TryCalculateWorkshopDeps(ref NameValueCollection buildData)
	{
		string workshopDeps = "";

		if (buildData["modreferences"].Length > 0) {
			var query = new QueryParameters() { searchModSlugs = buildData["modreferences"].Split(",") };
			if (!WorkshopHelper.TryGetPublishIdByInternalName(query, out var modIds))
				return false;

			foreach (string modRef in modIds) {
				if (modRef != "0")
					workshopDeps += modRef + ",";
			}
		}

		buildData["workshopdeps"] = workshopDeps;
		return true;
	}

	public static void FixErrorsInWorkshopFolder(string workshopFolderPath)
	{
		// This eliminates uploaded mod source files that occurred prior to the fix of #2263
		if (Directory.Exists(Path.Combine(workshopFolderPath, "bin"))) {
			foreach (var sourceFile in Directory.EnumerateFiles(workshopFolderPath))
				File.Delete(sourceFile);

			foreach (var sourceFolder in Directory.EnumerateDirectories(workshopFolderPath)) {
				if (!sourceFolder.Contains("2022.0"))
					Directory.Delete(sourceFolder, true);
			}
		}

		// This eliminates version 9999 in case someone bypasses the IsDev Check for testing or whatever
		string devRemnant = Path.Combine(workshopFolderPath, "9999.0");
		if (Directory.Exists(devRemnant)) {
			Directory.Delete(devRemnant, true);
		}
	}

	private static string CalculateDescriptionAndChangeNotes(bool isCi, NameValueCollection buildData, ref string changeNotes)
	{
		string workshopDescFile = Path.Combine(buildData["sourcesfolder"], "description_workshop.txt");
		string workshopDesc;
		if (!File.Exists(workshopDescFile))
			workshopDesc = buildData["description"];
		else
			workshopDesc = File.ReadAllText(workshopDescFile);

		// Add version metadata override to allow CI publishing
		string descriptionFinal = "";
		if (isCi)
			descriptionFinal += $"[quote=GithubActions(Don't Modify)]Version Summary {buildData["versionsummary"]}[/quote]";

		descriptionFinal += $"{workshopDesc}" + $"[quote=tModLoader {buildData["name"]}]\nDeveloped By {buildData["author"]}[/quote]";

		ModCompile.UpdateSubstitutedDescriptionValues(ref descriptionFinal, buildData["trueversion"], buildData["homepage"]);

		if (descriptionFinal.Length >= Steamworks.Constants.k_cchPublishedDocumentDescriptionMax) {
			//IssueReporter.ReportInstantUploadProblem("tModLoader.DescriptionLengthExceedLimit");
			throw new Exception(Language.GetTextValue("tModLoader.DescriptionLengthExceedLimit", Steamworks.Constants.k_cchPublishedDocumentDescriptionMax));
		}

		// If the modder hasn't supplied any change notes, then we will provde some default ones for them
		if (string.IsNullOrWhiteSpace(changeNotes)) {
			changeNotes = "Version {ModVersion} has been published to {tMLBuildPurpose} tModLoader v{tMLVersion}";
			if (!string.IsNullOrWhiteSpace(buildData["homepage"]))
				changeNotes += ", learn more at the [url={ModHomepage}]homepage[/url]";
		}

		ModCompile.UpdateSubstitutedDescriptionValues(ref changeNotes, buildData["trueversion"], buildData["homepage"]);

		return descriptionFinal;
	}

	public static void SteamCMDPublishPreparer(string modFolder)
	{
		if (!Program.LaunchParameters.ContainsKey("-ciprep") || !Program.LaunchParameters.ContainsKey("-publishedmodfiles"))
			return;

		Console.WriteLine("Preparing Files for CI...");
		Program.LaunchParameters.TryGetValue("-ciprep", out string changeNotes);

		// Folder containing all the current copies of the mod on the workshop
		Program.LaunchParameters.TryGetValue("-publishedmodfiles", out string publishedModFiles);

		// folder which will be used for the upload when the artifact is downloaded in post-build action. 
		Program.LaunchParameters.TryGetValue("-uploadfolder", out string uploadFolder); 

		// The Folder where we will put all the files that should be included in the build artifact
		string publishFolder = $"{ModOrganizer.modPath}/Workshop";

		string modName = Directory.GetParent(modFolder).Name;

		// Create a namevalue collection for checking versioning
		string newModPath = Path.Combine(ModOrganizer.modPath, $"{modName}.tmod");
		LocalMod newMod = OpenModFile(newModPath);

		var buildData = new NameValueCollection() {
			["version"] = newMod.Version.ToString(),
			["versionsummary"] = $"{newMod.tModLoaderVersion}:{newMod.Version}",
			["description"] = newMod.properties.description,
			["homepage"] = newMod.properties.homepage,
			["sourcesfolder"] = modFolder
		};

		// Needed for backwards compat from previous version metadata
		//TODO: why 'trueversion'?????
		buildData["trueversion"] = buildData["version"];

		if (!CalculateVersionsData(publishedModFiles, ref buildData, out string failureMessage)) {
			Utils.LogAndConsoleErrorMessage(failureMessage);
			Console.WriteLine(failureMessage);
			return;
		}

		Console.WriteLine($"Built Mod Version is: {buildData["trueversion"]}. tMod Version is: {BuildInfo.tMLVersion}");

		// Create the directory that the new tmod file will be added to, if it doesn't exist
		string contentFolder = $"{publishFolder}/{BuildInfo.tMLVersion.MajorMinor()}";
		if (!Directory.Exists(contentFolder))
			Directory.CreateDirectory(contentFolder);

		// Ensure the publish folder has all published information needed.
		FileUtilities.CopyFolder(publishedModFiles, publishFolder); // Copy all existing workshop files to output
		File.Copy(newModPath, Path.Combine(contentFolder, $"{modName}.tmod"), true); // Copy the new file to the output

		// Cleanup Old Folders
		ModOrganizer.CleanupOldPublish(publishFolder);

		// Assign Workshop Description
		string descriptionFinal = CalculateDescriptionAndChangeNotes(isCi: true, buildData, ref changeNotes);

		// Make the publish.vdf file
		string manifest = Path.Combine(publishedModFiles, "workshop.json");
		AWorkshopEntry.TryReadingManifest(manifest, out var steamInfo);

		string vdf = $"{ModOrganizer.modPath}/publish.vdf";

		string[] lines =
		{
				"\"workshopitem\"",
				"{",
				"\"appid\" \"" + "1281930"  + "\"",
				"\"publishedfileid\" \"" + steamInfo.workshopEntryId + "\"",
				"\"contentfolder\" \"" + $"{uploadFolder}/Workshop" + "\"",
				"\"changenote\" \"" + changeNotes + "\"",
				"\"description\" \"" + descriptionFinal + "\"",
				"}"
			};

		if (File.Exists(vdf))
			File.Delete(vdf);
		File.WriteAllLines(vdf, lines);

		Console.WriteLine("CI Files Prepared");
	}
}
