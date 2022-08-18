using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.UI.ModBrowser;
using Terraria.Social.Base;
using Terraria.Utilities;

namespace Terraria.Social.Steam
{
	public partial class WorkshopSocialModule
	{
		public override List<string> GetListOfMods() => _downloader.ModPaths;

		public override bool TryGetInfoForMod(TmodFile modFile, out FoundWorkshopEntryInfo info) {
			info = null;
			if(!WorkshopHelper.QueryHelper.CheckWorkshopConnection()) {
				base.IssueReporter.ReportInstantUploadProblem("tModLoader.NoWorkshopAccess");
				return false;
			}

			var existing = CheckIfUploaded(modFile);
			if (existing == null)
				return false;

			string searchFolder = Path.Combine(Directory.GetParent(ModOrganizer.WorkshopFileFinder.ModPaths[0]).ToString(), $"{existing.PublishId}");

			return ModOrganizer.TryReadManifest(searchFolder, out info);
		}

		private ModDownloadItem CheckIfUploaded(TmodFile modFile) {
			// TODO: Test that this obeys the StringComparison limitations previously enforced. ExampleMod vs Examplemod need to not be allowed
			// -> Haven't tested fix. Not sure if this same restriction applies from a ModOrganizer code perspective.
			// -> If workshop folder exists, it will overwrite existing mod, allowing lowering of version number. <- I do not follow, this line doesn't make sense. Lowering the version is checked against the Mod Browser, not the local item.
			// Oh yeah, publish a private mod, modname collision with a public mod later created. <- there is no solution to this. You take a risk in keeping it private.
			return WorkshopHelper.QueryHelper.FindModDownloadItem(modFile.Name);
		}

		public override bool PublishMod(TmodFile modFile, NameValueCollection buildData, WorkshopItemPublishSettings settings) {
			if (!SteamedWraps.SteamClient) {
				base.IssueReporter.ReportInstantUploadProblem("tModLoader.SteamPublishingLimit");
				return false;
			}

			if (BuildInfo.IsDev) {
				IssueReporter.ReportInstantUploadProblem("tModLoader.BetaModCantPublishError");
				return false;
			}

			var existing = CheckIfUploaded(modFile);
			ulong currPublishID = 0;
			string workshopFolderPath = GetTemporaryFolderPath() + modFile.Name;
			buildData["trueversion"] = buildData["version"];
			buildData["version"] = $"{buildData["modloaderversion"]}:{buildData["version"]}";

			if (existing != null) {
				currPublishID = ulong.Parse(existing.PublishId);

				ulong existingID = WorkshopHelper.QueryHelper.GetSteamOwner(currPublishID);
				var currID = Steamworks.SteamUser.GetSteamID();

				if (existingID != currID.m_SteamID) {
					IssueReporter.ReportInstantUploadProblem("tModLoader.ModAlreadyUploaded");
					return false;
				}

				// Update the subscribed mod to be the latest version published
				SteamedWraps.Download(null, true, new Steamworks.PublishedFileId_t(currPublishID));

				// Publish by updating the files available on the current published version
				workshopFolderPath = Path.Combine(Directory.GetParent(ModOrganizer.WorkshopFileFinder.ModPaths[0]).ToString(), $"{existing.PublishId}");

				FixErrorsInWorkshopFolder(workshopFolderPath);

				if (CalculateVersionsData(workshopFolderPath, ref buildData)) {
					IssueReporter.ReportInstantUploadProblem("tModLoader.ModVersionInfoUnchanged");
					return false;
				}
			}

			string name = buildData["displaynameclean"];
			if (name.Length >= Steamworks.Constants.k_cchPublishedDocumentTitleMax) {
				IssueReporter.ReportInstantUploadProblem("tModLoader.TitleLengthExceedLimit");
				return false;
			}

			string description = buildData["description"];
			if (description.Length >= Steamworks.Constants.k_cchPublishedDocumentDescriptionMax) {
				IssueReporter.ReportInstantUploadProblem("tModLoader.DescriptionLengthExceedLimit");
				return false;
			}

			string[] usedTagsInternalNames = settings.GetUsedTagsInternalNames();
			string[] modMetadata = { buildData["modside"] };

			string[] tagsList = usedTagsInternalNames.Concat(modMetadata).ToArray();

			CalculateWorkshopDeps(ref buildData);
			
			string contentFolderPath = $"{workshopFolderPath}/{BuildInfo.tMLVersion.Major}.{BuildInfo.tMLVersion.Minor}";

			if (MakeTemporaryFolder(contentFolderPath)) {
				string modPath = Path.Combine(contentFolderPath, modFile.Name + ".tmod");
				File.Copy(modFile.path, modPath, true);

				// Cleanup Old Folders
				ModOrganizer.CleanupOldPublish(workshopFolderPath);

				var modPublisherInstance = new WorkshopHelper.ModPublisherInstance();

				_publisherInstances.Add(modPublisherInstance);

				modPublisherInstance.PublishContent(_publishedItems, base.IssueReporter, Forget, name, description, workshopFolderPath, settings.PreviewImagePath, settings.Publicity, tagsList, buildData, currPublishID, settings.ChangeNotes);

				return true;
			}

			return false;
		}

		// Output version string: "2022.05:0.2.0;2022.06;0.2.1;2022.07:0.2.2"
		// Return False if the mod version did not increase for the particular tml version
		// This will have up to 1 more version than is actually relevant, but that won't break anything
		public static bool CalculateVersionsData(string workshopPath, ref NameValueCollection buildData) {
			foreach (var tmod in Directory.EnumerateFiles(workshopPath, "*.tmod*", SearchOption.AllDirectories)) {
				var mod = OpenModFile(tmod);
				if (mod.tModLoaderVersion.MajorMinor() <= BuildInfo.tMLVersion.MajorMinor())
					if (mod.properties.version >= new Version(buildData["trueversion"]))
						return false;

				buildData["version"] += $";{mod.tModLoaderVersion.MajorMinor()}:{mod.properties.version}";
			}

			return true;
		}

		internal static LocalMod OpenModFile(string path) {
			var sModFile = new TmodFile(path);
			using (sModFile.Open())
				return new LocalMod(sModFile);
		}

		private static void CalculateWorkshopDeps(ref NameValueCollection buildData) {
			string workshopDeps = "";

			if (buildData["modreferences"].Length > 0) {
				foreach (string modRef in buildData["modreferences"].Split(",")) {
					var temp = WorkshopHelper.QueryHelper.FindModDownloadItem(modRef);

					if (temp != null)
						workshopDeps += temp.PublishId + ",";
				}
			}

			buildData["workshopdeps"] = workshopDeps;
		}

		public static void FixErrorsInWorkshopFolder(string workshopFolderPath) {
			// This eliminates uploaded mod source files that occured prior to the fix of #2263
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

		public static void CiPublish(string modFolder) {
			if (!Program.LaunchParameters.ContainsKey("-ciprep") || !Program.LaunchParameters.ContainsKey("-publishedmodfiles"))
				return;

			Console.WriteLine("Preparing Files for CI...");
			Program.LaunchParameters.TryGetValue("-ciprep", out string changeNotes);
			Program.LaunchParameters.TryGetValue("-publishedmodfiles", out string publishedModFiles);
			Program.LaunchParameters.TryGetValue("-uploadfolder", out string uploadFolder);

			// Prep some common file paths & info
			string publishFolder = $"{ModOrganizer.modPath}/Workshop";
			string vdf = $"{ModOrganizer.modPath}/publish.vdf";

			string manifest = Path.Combine(publishedModFiles, "workshop.json");
			AWorkshopEntry.TryReadingManifest(manifest, out var steamInfo);


			/////


			string modName = Directory.GetParent(modFolder).Name;

			string newModPath = Path.Combine(ModOrganizer.modPath, $"{modName}.tmod");
			var newModFile = new TmodFile(newModPath);

			LocalMod newMod;
			using (newModFile.Open())
				newMod = new LocalMod(newModFile);

			string stable = ModOrganizer.FindOldest(publishedModFiles);
			if (!stable.Contains(".tmod"))
				stable = Directory.GetFiles(stable, "*.tmod")[0];

			LocalMod sMod;
			var sModFile = new TmodFile(stable);
			using (sModFile.Open())
				sMod = new LocalMod(sModFile);

			if (newMod.properties.version <= sMod.properties.version)
				throw new Exception("Mod version not incremented. Publishing item blocked until mod version is incremented");

			// Prep for the publishing folder
			string contentFolder = $"{publishFolder}/{BuildInfo.tMLVersion.Major}.{BuildInfo.tMLVersion.Minor}";
			if (!Directory.Exists(contentFolder))
				Directory.CreateDirectory(contentFolder);

			// Ensure the publish folder has all published information needed.
			FileUtilities.CopyFolder(publishedModFiles, publishFolder);
			File.Copy(newModPath, Path.Combine(contentFolder, $"{modName}.tmod"), true);

			// Cleanup Old Folders
			ModOrganizer.CleanupOldPublish(publishFolder);

			stable = ModOrganizer.FindOldest(publishFolder);
			if (!stable.Contains(".tmod"))
				stable = Directory.GetFiles(stable, "*.tmod")[0];

			sModFile = new TmodFile(stable);
			using (sModFile.Open())
				sMod = new LocalMod(sModFile);


			//////


			string workshopDescFile = Path.Combine(modFolder, "description_workshop.txt");
			string workshopDesc;
			if (!File.Exists(workshopDescFile))
				workshopDesc = newMod.properties.description;
			else
				workshopDesc = File.ReadAllText(workshopDescFile);

			string descriptionFinal = $"[quote=GithubActions(Don't Modify)]Version Summary {versionSummary}[/quote]" +
				$"{workshopDesc}";


			Console.WriteLine($"Built Mod Version is: {newMod.properties.version}. tMod Version is: {BuildInfo.tMLVersion}");



			// Make the publish.vdf file
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
}
