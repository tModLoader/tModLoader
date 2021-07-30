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

			RefreshSubscriptionsAndPublishings();
			if (!WorkshopHelper.QueryHelper.CheckWorkshopConnection()) {
				base.IssueReporter.ReportInstantUploadProblem("tModLoader.NoWorkshopAccess");
				return false;
			}

			// TODO: Test that this obeys the StringComparison limitations previously enforced. ExampleMod vs Examplemod need to not be allowed
			var existing = Interface.modBrowser.FindModDownloadItem(buildData["name"]);

			if (existing != null) {
				ulong existingID = UIModBrowser.SteamWorkshop.GetSteamOwner(existing.QueryIndex);
				var currID = Steamworks.SteamUser.GetSteamID();

				if (existingID != currID.m_SteamID) {
					IssueReporter.ReportInstantUploadProblem("tModLoader.ModAlreadyUploaded");
					return false;
				}

				if (new Version(buildData["version"].Replace("v", "")) <= new Version(existing.Version.Replace("v", ""))) {
					IssueReporter.ReportInstantUploadProblem("tModLoader.ModVersionInfoUnchanged");
					return false;
				} 
			}

			string name = buildData["displaynameclean"];
			string description = buildData["description"];
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

				var modPublisherInstance = new WorkshopHelper.ModPublisherInstance();

				_publisherInstances.Add(modPublisherInstance);

				modPublisherInstance.PublishContent(_publishedItems, base.IssueReporter, Forget, name, description, contentFolderPath, settings.PreviewImagePath, settings.Publicity, usedTagsInternalNames, buildData);

				return true;
			}

			return false;
		}
	}
}
