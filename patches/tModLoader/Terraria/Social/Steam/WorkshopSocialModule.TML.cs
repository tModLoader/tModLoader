using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
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

			if (AWorkshopEntry.TryReadingManifest(contentFolderPath + Path.DirectorySeparatorChar + "workshop.json", out info))
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
