using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.IO;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.UI;
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
				base.IssueReporter.ReportInstantUploadProblem("localizationString: Error: Only Steam tModLoader owners can publish to Workshop in-game.");
				return;
			}

			if (Interface.modBrowser.Items.Count == 0) {
				Interface.modBrowser.PopulateModBrowser();

				if (Interface.modBrowser.Items.Count == 0) {
					base.IssueReporter.ReportInstantUploadProblem("localizationString: Error: Unable to access Steam Workshop");
					return;
				}
			}

			if (Interface.modBrowser.Items.FirstOrDefault(x => x.ModName.Equals(buildData["name"])) != null) {
				base.IssueReporter.ReportInstantUploadProblem("localizationString: Error: Mod already exists on Workshop!");
				return;
			}

			string name = buildData["displaynameclean"];
			string description = buildData["description"];
			string[] usedTagsInternalNames = settings.GetUsedTagsInternalNames();
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
