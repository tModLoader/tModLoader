using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Collections.Specialized;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader.Core;
using Terraria.Social;
using Terraria.Social.Base;
using Terraria.UI;

namespace Terraria.GameContent.UI.States
{
	public class WorkshopPublishInfoStateForMods : AWorkshopPublishInfoState<TmodFile>
	{
		public const string TmlRules = "https://forums.terraria.org/index.php?threads/player-created-game-enhancements-rules-guidelines.286/";

		private readonly NameValueCollection _buildData;

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

		internal UIElement CreateTmlDisclaimer(string tagGroup) {
			float num = 60f;
			float num2 = 0f + num;

			GroupOptionButton<bool> groupOptionButton = new GroupOptionButton<bool>(option: true, null, null, Color.White, null, 1f, 0.5f, 16f) {
				HAlign = 0.5f,
				VAlign = 0f,
				Width = StyleDimension.FromPixelsAndPercent(0f, 1f),
				Left = StyleDimension.FromPixels(0f),
				Height = StyleDimension.FromPixelsAndPercent(num2 + 4f, 0f),
				Top = StyleDimension.FromPixels(0f),
				ShowHighlightWhenSelected = false
			};

			groupOptionButton.SetCurrentOption(option: false);
			groupOptionButton.Width.Set(0f, 1f);

			UIElement uIElement = new UIElement {
				HAlign = 0.5f,
				VAlign = 1f,
				Width = new StyleDimension(0f, 1f),
				Height = new StyleDimension(num, 0f)
			};

			groupOptionButton.Append(uIElement);

			UIText uIText = new UIText(Language.GetText("tModLoader.WorkshopDisclaimer")) {
				HAlign = 0f,
				VAlign = 0f,
				Width = StyleDimension.FromPixelsAndPercent(-40f, 1f),
				Height = StyleDimension.FromPixelsAndPercent(0f, 1f),
				TextColor = Color.Cyan,
				IgnoresMouseInteraction = true
			};

			uIText.PaddingLeft = 20f;
			uIText.PaddingRight = 20f;
			uIText.PaddingTop = 4f;
			uIText.IsWrapped = true;

			_disclaimerText = uIText;

			groupOptionButton.OnClick += TmlDisclaimerText_OnClick;
			groupOptionButton.OnMouseOver += TmlDisclaimerText_OnMouseOver;
			groupOptionButton.OnMouseOut += TmlDisclaimerText_OnMouseOut;

			uIElement.Append(uIText);
			uIText.SetSnapPoint(tagGroup, 0);

			_tMLDisclaimerButton = uIText;

			return groupOptionButton;
		}

		private void TmlDisclaimerText_OnMouseOut(UIMouseEvent evt, UIElement listeningElement) {
			_disclaimerText.TextColor = Color.Cyan;
			ClearOptionDescription(evt, listeningElement);
		}

		private void TmlDisclaimerText_OnMouseOver(UIMouseEvent evt, UIElement listeningElement) {
			SoundEngine.PlaySound(12);
			_disclaimerText.TextColor = Color.LightCyan;
			ShowOptionDescription(evt, listeningElement);
		}

		private void TmlDisclaimerText_OnClick(UIMouseEvent evt, UIElement listeningElement) =>	Utils.OpenToURL(TmlRules);
	}
}
