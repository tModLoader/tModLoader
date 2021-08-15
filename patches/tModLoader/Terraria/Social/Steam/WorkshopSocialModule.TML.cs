using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.UI;
using Terraria.ModLoader.UI.ModBrowser;
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

			if (!WorkshopHelper.QueryHelper.CheckWorkshopConnection()) {
				base.IssueReporter.ReportInstantUploadProblem("tModLoader.NoWorkshopAccess");
				return false;
			}

			// TODO: Test that this obeys the StringComparison limitations previously enforced. ExampleMod vs Examplemod need to not be allowed
			// -> Haven't tested fix. Not sure if this same restriction applies from a ModOrganizer code perspective.
			// -> If workshop folder exists, it will overwrite existing mod, allowing lowering of version number. <- I do not follow, this line doesn't make sense. Lowering the version is checked against the Mod Browser, not the local item.
			// Oh yeah, publish a private mod, modname collision with a public mod later created. <- there is no solution to this. You take a risk in keeping it private.
			var existing = Interface.modBrowser.FindModDownloadItem(buildData["name"]);
			ulong currPublishID = 0;

			if (existing != null) {
				ulong existingID = UIModBrowser.SteamWorkshop.GetSteamOwner(ulong.Parse(existing.PublishId));
				var currID = Steamworks.SteamUser.GetSteamID();

				if (existingID != currID.m_SteamID) {
					IssueReporter.ReportInstantUploadProblem("tModLoader.ModAlreadyUploaded");
					return false;
				}

				if (new Version(buildData["version"].Replace("v", "")) <= new Version(existing.Version.Replace("v", ""))) {
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
					var temp = Interface.modBrowser.FindModDownloadItem(modRef);

					if (temp != null)
						workshopDeps += temp.PublishId + ",";
				}
			}

			buildData["workshopdeps"] = workshopDeps;

			string contentFolderPath = GetTemporaryFolderPath() + modFile.Name;

			if (MakeTemporaryFolder(contentFolderPath)) {
				File.Copy(modFile.path, Path.Combine(contentFolderPath, modFile.Name + ".tmod"), true);

				// If the manifest doesn't exist, try copying it from the Mod Source folder
				string targetManifest = contentFolderPath + Path.DirectorySeparatorChar + "workshop.json";
				string sourceManifest = buildData["manifestfolder"] + Path.DirectorySeparatorChar + "workshop.json";
				if (!File.Exists(targetManifest))
					if (File.Exists(sourceManifest))
						File.Copy(sourceManifest, targetManifest);

				var modPublisherInstance = new WorkshopHelper.ModPublisherInstance();

				_publisherInstances.Add(modPublisherInstance);

				modPublisherInstance.PublishContent(_publishedItems, base.IssueReporter, Forget, name, description, contentFolderPath, settings.PreviewImagePath, settings.Publicity, usedTagsInternalNames, buildData, currPublishID);

				return true;
			}

			return false;
		}
	}
}
