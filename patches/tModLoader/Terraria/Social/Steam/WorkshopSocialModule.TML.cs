using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
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

			if (AWorkshopEntry.TryReadingManifest(contentFolderPath + Path.DirectorySeparatorChar + "workshop.json", out info))
				return true;

			return false;
		}

		public override void PublishMod(TmodFile modFile, NameValueCollection buildData, WorkshopItemPublishSettings settings) {
			if (!WorkshopHelper.ModManager.SteamUser) {
				base.IssueReporter.ReportInstantUploadProblem("tModLoader.SteamPublishingLimt");
				return;
			}

			if (!WorkshopHelper.QueryHelper.CheckWorkshopConnection()) {
				base.IssueReporter.ReportInstantUploadProblem("tModLoader.NoWorkshopAccess");
				return;
			}

			var existing = Interface.modBrowser.FindModDownloadItem(buildData["name"]);
			if (existing != null) {
				var existingID = UIModBrowser.SteamWorkshop.GetSteamOwner(existing.QueryIndex);
				var currID = Steamworks.SteamUser.GetSteamID();

				if (existingID != currID.m_SteamID) {
					base.IssueReporter.ReportInstantUploadProblem("tModLoader.ModAlreadyUploaded");
					return;
				}
			}

			string name = buildData["displaynameclean"];
			string description = buildData["description"];
			string[] usedTagsInternalNames = settings.GetUsedTagsInternalNames();

			string workshopDeps = "";
			if (buildData["modreferences"].Length > 0) {
				foreach (var modRef in buildData["modreferences"].Split(",")) {
					var temp = Interface.modBrowser.FindModDownloadItem(modRef);
					if (temp != null)
						workshopDeps += temp.PublishId + ",";
				}
			}
			buildData["workshopdeps"] = workshopDeps;

			string contentFolderPath = GetTemporaryFolderPath() + modFile.Name;
			if (MakeTemporaryFolder(contentFolderPath)) {
				File.Copy(modFile.path, Path.Combine(contentFolderPath, modFile.Name + ".tmod"), true);
				WorkshopHelper.ModPublisherInstance modPublisherInstance = new WorkshopHelper.ModPublisherInstance();
				_publisherInstances.Add(modPublisherInstance);
				modPublisherInstance.PublishContent(_publishedItems, base.IssueReporter, Forget, name, description, contentFolderPath, settings.PreviewImagePath, settings.Publicity, usedTagsInternalNames, buildData);
			}
		}
	}
}
