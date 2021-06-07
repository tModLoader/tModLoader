using Steamworks;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.DataStructures;
using Terraria.GameContent.UI.States;
using Terraria.IO;
using Terraria.Social.Base;

namespace Terraria.Social.Steam
{
	public partial class WorkshopSocialModule
	{
		public override List<string> GetListOfMods() => _downloader.ModPaths;

		public override bool TryGetInfoForMod(string fullPath, out FoundWorkshopEntryInfo info) {
			info = null;
			if (!Directory.Exists(fullPath))
				return false;

			if (AWorkshopEntry.TryReadingManifest(fullPath + Path.DirectorySeparatorChar + "workshop.json", out info))
				return true;

			return false;
		}

		public override void PublishMod(string modSource, WorkshopItemPublishSettings settings) {
			if (false) {
				base.IssueReporter.ReportInstantUploadProblem("localizationString: CustomRejectCondition 1");
				return;
			}

			string name = world.Name;
			string textForWorld = GetTextForWorld(world);
			string[] usedTagsInternalNames = settings.GetUsedTagsInternalNames();
			string text = GetTemporaryFolderPath() + world.GetFileName(includeExtension: false);
			if (MakeTemporaryFolder(text)) {
				WorkshopHelper.UGCBased.WorldPublisherInstance worldPublisherInstance = new WorkshopHelper.UGCBased.WorldPublisherInstance(world);
				_publisherInstances.Add(worldPublisherInstance);
				worldPublisherInstance.PublishContent(_publishedItems, base.IssueReporter, Forget, name, textForWorld, text, settings.PreviewImagePath, settings.Publicity, usedTagsInternalNames);
			}
		}
	}
}
