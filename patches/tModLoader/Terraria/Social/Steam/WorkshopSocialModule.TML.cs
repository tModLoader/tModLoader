using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Diagnostics;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Terraria.Social.Base;

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

			if (false && !BuildInfo.IsStable && !BuildInfo.IsPreview) {
				//TODO: Need to find the existing translation for this.
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

			if (existing != null) {
				ulong existingID = WorkshopHelper.QueryHelper.GetSteamOwner(ulong.Parse(existing.PublishId));
				var currID = Steamworks.SteamUser.GetSteamID();

				if (existingID != currID.m_SteamID) {
					IssueReporter.ReportInstantUploadProblem("tModLoader.ModAlreadyUploaded");
					return false;
				}

				if (false && new Version(buildData["version"].Replace("v", "")) <= new Version(existing.Version.Replace("v", ""))) {
					IssueReporter.ReportInstantUploadProblem("tModLoader.ModVersionInfoUnchanged");
					return false;
				}

				currPublishID = uint.Parse(existing.PublishId);
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

			string modSourceFolder = buildData["sourcesfolder"];

			string contentFolderPath = $"{modSourceFolder}/Workshop/{BuildInfo.tMLVersion.Major}.{BuildInfo.tMLVersion.Minor}";
			string workshopFolderPath = $"{modSourceFolder}/Workshop";

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

			var modFile = new TmodFile(ModOrganizer.GetActiveTmodInRepo(publishedModFiles));

			LocalMod mod;
			using (modFile.Open())
				mod = new LocalMod(modFile);

			if (newMod.properties.version <= mod.properties.version)
				throw new Exception("Mod version not incremented. Publishing item blocked until mod version is incremented");

			// Prep for the publishing folder
			string contentFolder = $"{publishFolder}/{BuildInfo.tMLVersion.Major}.{BuildInfo.tMLVersion.Minor}";
			if (!Directory.Exists(contentFolder))
				Directory.CreateDirectory(contentFolder);

			// Ensure the publish folder has all published information needed.
			Utilities.FileUtilities.CopyFolder(publishedModFiles, publishFolder);
			File.Copy(newModPath, Path.Combine(contentFolder, $"{modName}.tmod"), true);

			// Cleanup Old Folders
			ModOrganizer.CleanupOldPublish(publishFolder);

			string descriptionFinal = $"[quote=CI Autobuild (Don't Modify)]Version {newMod.properties.version} built for {newMod.properties.buildVersion} [/quote] {newMod.properties.description}";
			Console.WriteLine($"Mod Version is: {newMod.properties.version}. tMod Version is: {BuildInfo.tMLVersion}");

			// Make the publish.vdf file
			string[] lines =
			{
					"\"workshopitem\"",
					"{",
					"\"appid\" \"" + "1281930"  + "\"",
					"\"publishedfileid\" \"" + steamInfo.workshopEntryId + "\"",
					"\"contentfolder\" \"" + $"artifacts/{modName}/Workshop" + "\"",
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
