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
			if (!Program.LaunchParameters.ContainsKey("-ciprep") || !Program.LaunchParameters.ContainsKey("-steamCmdFolder"))
				return;

			Console.WriteLine("Preparing Files for CI...");
			Program.LaunchParameters.TryGetValue("-ciprep", out string changeNotes);
			Program.LaunchParameters.TryGetValue("-steamCmdFolder", out string steamCmdFolder);
			var properties = BuildProperties.ReadBuildFile(modFolder);

			// Prep some common file paths & info
			string publishFolder = $"{modFolder}/Workshop";
			string vdf = Path.Combine(steamCmdFolder, "publish.vdf");

			string manifest = Path.Combine(modFolder, "workshop.json");
			AWorkshopEntry.TryReadingManifest(manifest, out var steamInfo);

			string modName = Path.GetFileNameWithoutExtension(modFolder);

			// Check for if the mod version is increasing
			//TODO: Finish Implementing, after getting example mod working with using version data
			/* var downloadWorkshopItem = new ProcessStartInfo() {
				FileName = Path.Combine(modFolder, steamCmdFolder, "steamCMD.exe"),
				UseShellExecute = false,
				Arguments = "+login anonymous +force_install_dir tMod \"+workshop_download_item 1281930 " + steamInfo.workshopEntryId + "\" +quit",
			};
			var p = Process.Start(downloadWorkshopItem);
			p.WaitForExit();

			LocalMod mod;
			var modFile = new TmodFile(Path.Combine(steamCmdFolder, "tMod/steamapps/workshop/content/1281930", steamInfo.workshopEntryId.ToString(), modName + ".tmod"));
			using (modFile.Open())
				mod = new LocalMod(modFile);

			if (properties.version <= mod.properties.version)
				throw new Exception("Mod version not incremented. Publishing item blocked until mod version is incremented");
			*/

			// Prep for the publishing folder
			string contentFolder = $"{publishFolder}/{BuildInfo.tMLVersion.Major}.{BuildInfo.tMLVersion.Minor}";

			File.Copy(Path.Combine(ModOrganizer.modPath, $"{modName}.tmod"), Path.Combine(contentFolder, $"{modName}.tmod"), true);
			File.Copy(manifest, Path.Combine(publishFolder, "workshop.json"), true);

			string descriptionFinal = "[quote=CI Autobuild (Don't Modify)]Version " + properties.version + " built for " + properties.buildVersion + "[/quote]" + properties.description;

			// Make the publish.vdf file
			string[] lines =
			{
					"\"workshopitem\"",
					"{",
					"\"appid\" \"" + "1281930"  + "\"",
					"\"publishedfileid\" \"" + steamInfo.workshopEntryId + "\"",
					"\"contentfolder\" \"" + publishFolder + "\"",
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
