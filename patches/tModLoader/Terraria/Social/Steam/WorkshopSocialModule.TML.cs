using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Terraria.Social.Base;
using Terraria.Utilities;

namespace Terraria.Social.Steam
{
	public partial class WorkshopSocialModule
	{
		public override List<string> GetListOfMods() => _downloader.ModPaths;

		public override bool TryGetInfoForMod(TmodFile modFile, out FoundWorkshopEntryInfo info) {
			info = null;

			string contentFolderPath = GetTemporaryFolderPath() + modFile.Name;

			if (!Directory.Exists(contentFolderPath))
				return false;

			if (AWorkshopEntry.TryReadingManifest(Path.Combine(contentFolderPath, "workshop.json"), out info))
				return true;

			return false;
		}

		public override bool PublishMod(TmodFile modFile, NameValueCollection buildData, WorkshopItemPublishSettings settings) {
			if (!WorkshopHelper.ModManager.SteamUser) {
				base.IssueReporter.ReportInstantUploadProblem("tModLoader.SteamPublishingLimit");
				return false;
			}

			if (BuildInfo.IsDev) {
				IssueReporter.ReportInstantUploadProblem("tModLoader.BetaModCantPublishError");
				return false;
			}

			if (!WorkshopHelper.QueryHelper.CheckWorkshopConnection()) {
				base.IssueReporter.ReportInstantUploadProblem("tModLoader.NoWorkshopAccess");
				return false;
			}

			// TODO: Test that this obeys the StringComparison limitations previously enforced. ExampleMod vs Examplemod need to not be allowed
			// -> Haven't tested fix. Not sure if this same restriction applies from a ModOrganizer code perspective.
			// -> If workshop folder exists, it will overwrite existing mod, allowing lowering of version number. <- I do not follow, this line doesn't make sense. Lowering the version is checked against the Mod Browser, not the local item.
			// Oh yeah, publish a private mod, modname collision with a public mod later created. <- there is no solution to this. You take a risk in keeping it private.
			var existing = WorkshopHelper.QueryHelper.FindModDownloadItem(buildData["name"]);
			ulong currPublishID = 0;

			string workshopFolderPath = GetTemporaryFolderPath() + modFile.Name;

			if (existing != null) {
				currPublishID = ulong.Parse(existing.PublishId);

				ulong existingID = WorkshopHelper.QueryHelper.GetSteamOwner(currPublishID);
				var currID = Steamworks.SteamUser.GetSteamID();

				if (existingID != currID.m_SteamID) {
					IssueReporter.ReportInstantUploadProblem("tModLoader.ModAlreadyUploaded");
					return false;
				}

				// Update the subscribed mod to be the latest version published
				new WorkshopHelper.ModManager(new Steamworks.PublishedFileId_t(currPublishID)).InnerDownload(null, true);

				// Publish by updating the files available on the current published version
				workshopFolderPath = Path.Combine(Directory.GetParent(ModOrganizer.WorkshopFileFinder.ModPaths[0]).ToString(), $"{existing.PublishId}");

				if (new Version(buildData["version"].Replace("v", "")) <= new Version(existing.Version.Replace("v", ""))) {
					IssueReporter.ReportInstantUploadProblem("tModLoader.ModVersionInfoUnchanged");
					return false;
				}

				// Use the stable version of the mod for publishing metadata, not the preview version!
				if (!BuildInfo.IsStable) {
					string stable = ModOrganizer.FindOldest(workshopFolderPath);
					if (!stable.Contains(".tmod"))
						stable = Directory.GetFiles(stable, "*.tmod")[0];

					LocalMod sMod;
					var sModFile = new TmodFile(stable);
					using (sModFile.Open())
						sMod = new LocalMod(sModFile);

					buildData["modloaderversion"] = $"tModLoader v{sMod.properties.buildVersion}";
					buildData["version"] = sMod.properties.version.ToString();
					buildData["modreferences"] = string.Join(", ", sMod.properties.modReferences.Select(x => x.mod));
					buildData["modside"] = sMod.properties.side.ToFriendlyString();
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
			string workshopDeps = "";

			if (buildData["modreferences"].Length > 0) {
				foreach (string modRef in buildData["modreferences"].Split(",")) {
					var temp = WorkshopHelper.QueryHelper.FindModDownloadItem(modRef);

					if (temp != null)
						workshopDeps += temp.PublishId + ",";
				}
			}

			buildData["workshopdeps"] = workshopDeps;
			string contentFolderPath = $"{workshopFolderPath}/{BuildInfo.tMLVersion.Major}.{BuildInfo.tMLVersion.Minor}";

			if (MakeTemporaryFolder(contentFolderPath)) {
				string modPath = Path.Combine(contentFolderPath, modFile.Name + ".tmod");
				File.Copy(modFile.path, modPath, true);

				// Cleanup Old Folders
				ModOrganizer.CleanupOldPublish(workshopFolderPath);

				var modPublisherInstance = new WorkshopHelper.ModPublisherInstance();

				_publisherInstances.Add(modPublisherInstance);

				modPublisherInstance.PublishContent(_publishedItems, base.IssueReporter, Forget, name, description, workshopFolderPath, settings.PreviewImagePath, settings.Publicity, usedTagsInternalNames, buildData, currPublishID);

				return true;
			}

			return false;
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

			string descriptionFinal = $"[quote=GithubActions(Don't Modify)]Version {sMod.properties.version} built for tModLoader v{sMod.properties.buildVersion}[/quote]" +
				$"{sMod.properties.description}";
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
