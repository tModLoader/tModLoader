using Steamworks;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using Terraria.DataStructures;
using Terraria.GameContent.UI.States;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Terraria.Social.Base;

namespace Terraria.Social.Steam
{
	public partial class WorkshopSocialModule
	{
		public override List<string> GetListOfMods() => _downloader.ModPaths;

		public override bool TryGetInfoForMod(string pathToSteamWorkshopFolder, out FoundWorkshopEntryInfo info) {
			info = null;
			if (!Directory.Exists(pathToSteamWorkshopFolder))
				return false;

			if (AWorkshopEntry.TryReadingManifest(pathToSteamWorkshopFolder + Path.DirectorySeparatorChar + "workshop.json", out info))
				return true;

			return false;
		}

		public override void PublishMod(TmodFile modFile, NameValueCollection buildData, WorkshopItemPublishSettings settings) {
			if (false) {
				base.IssueReporter.ReportInstantUploadProblem("localizationString: CustomRejectCondition 1");
				return;
			}

			string name = buildData["displaynameclean"];
			string description = buildData["description"];
			string[] usedTagsInternalNames = { "" }; //settings.GetUsedTagsInternalNames();
			string contentFolderPath = GetTemporaryFolderPath() + modFile.Name;
			if (MakeTemporaryFolder(contentFolderPath)) {
				File.Copy(modFile.path, Path.Combine(contentFolderPath, modFile.Name + ".tmod"));
				WorkshopHelper.ModPublisherInstance modPublisherInstance = new WorkshopHelper.ModPublisherInstance();
				_publisherInstances.Add(modPublisherInstance);
				modPublisherInstance.PublishContent(_publishedItems, base.IssueReporter, Forget, name, description, contentFolderPath, settings.PreviewImagePath, settings.Publicity, usedTagsInternalNames);
			}
		}
	}
}
