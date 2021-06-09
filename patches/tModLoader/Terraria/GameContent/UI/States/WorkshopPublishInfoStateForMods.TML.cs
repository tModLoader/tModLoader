using System.Collections.Generic;
using System.Collections.Specialized;
using Terraria.ModLoader.Core;
using Terraria.Social;
using Terraria.Social.Base;
using Terraria.UI;

namespace Terraria.GameContent.UI.States
{
	public class WorkshopPublishInfoStateForMods : AWorkshopPublishInfoState<TmodFile>
	{
		private NameValueCollection _buildData;

		public WorkshopPublishInfoStateForMods(UIState stateToGoBackTo, TmodFile modFile, NameValueCollection buildData)
			: base(stateToGoBackTo, modFile) {
			_instructionsTextKey = "Workshop.ModPublishDescription";
			_publishedObjectNameDescriptorTexKey = "Workshop.ModName";
			_buildData = buildData;
			_previewImagePath = buildData["iconpath"];
		}

		protected override string GetPublishedObjectDisplayName() {
			if (_dataObject == null)
				return "null";

			return _dataObject.Name;
		}

		protected override void GoToPublishConfirmation() {
			if ( /*SocialAPI.Workshop != null && */ _dataObject != null)
				SocialAPI.Workshop.PublishMod(_dataObject, _buildData, GetPublishSettings());

			Main.menuMode = 888;
			Main.MenuUI.SetState(_previousUIState);
		}

		protected override List<WorkshopTagOption> GetTagsToShow() => SocialAPI.Workshop.SupportedTags.ModTags;
		protected override bool TryFindingTags(out FoundWorkshopEntryInfo info) => SocialAPI.Workshop.TryGetInfoForMod(_dataObject, out info);
	}
}
