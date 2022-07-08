using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader.UI;
using Terraria.Social.Steam;
using Terraria.UI;

namespace Terraria.GameContent.UI.States
{
	partial class UIWorkshopHub
	{
		private UIElement _buttonMods;
		private UIElement _buttonModSources;
		private UIElement _buttonModBrowser;
		private UIElement _buttonModPack;

		private UIElement MakeButton_OpenModsMenu() {
			UIElement uIElement = MakeFancyButtonMod($"Terraria.GameContent.UI.States.HubManageMods", "tModLoader.MenuManageMods");
			uIElement.OnClick += Click_OpenModsMenu;
			_buttonMods = uIElement;
			return uIElement;
		}

		private void Click_OpenModsMenu(UIMouseEvent evt, UIElement listeningElement) {
			SoundEngine.PlaySound(10);
			Interface.modsMenu.PreviousUIState = this;
			Main.MenuUI.SetState(Interface.modsMenu);
		}

		private UIElement MakeButton_OpenModSourcesMenu() {
			UIElement uIElement = MakeFancyButtonMod($"Terraria.GameContent.UI.States.HubDevelopMods", "tModLoader.MenuDevelopMods");
			uIElement.OnClick += Click_OpenModSourcesMenu;
			_buttonModSources = uIElement;
			return uIElement;
		}

		private void Click_OpenModSourcesMenu(UIMouseEvent evt, UIElement listeningElement) {
			SoundEngine.PlaySound(10);
			Interface.modSources.PreviousUIState = this;
			Main.MenuUI.SetState(Interface.modSources);
		}

		private UIElement MakeButton_OpenModBrowserMenu() {
			UIElement uIElement = MakeFancyButtonMod($"Terraria.GameContent.UI.States.HubDownloadMods", "tModLoader.MenuDownloadMods");
			uIElement.OnClick += Click_OpenModBrowserMenu;
			_buttonModBrowser = uIElement;
			return uIElement;
		}

		private void Click_OpenModBrowserMenu(UIMouseEvent evt, UIElement listeningElement) {
			SoundEngine.PlaySound(10);
			Interface.modBrowser.PreviousUIState = this;
			Main.MenuUI.SetState(Interface.modBrowser);
		}

		private UIElement MakeButton_ModPackMenu() {
			UIElement uIElement = MakeFancyButtonMod($"Terraria.GameContent.UI.States.HubModPacks", "tModLoader.ModsModPacks");
			uIElement.OnClick += Click_OpenModPackMenu;
			_buttonModPack = uIElement;
			return uIElement;
		}

		private void Click_OpenModPackMenu(UIMouseEvent evt, UIElement listeningElement) {
			SoundEngine.PlaySound(10);
			Interface.modPacksMenu.PreviousUIState = this;
			Main.MenuUI.SetState(Interface.modPacksMenu);
		}

		private UIElement MakeFancyButtonMod(string iconImagePath, string textKey) {
			UIPanel uIPanel = new UIPanel();
			int num = -3;
			int num2 = -3;
			uIPanel.Width = StyleDimension.FromPixelsAndPercent(num, 0.5f);
			uIPanel.Height = StyleDimension.FromPixelsAndPercent(num2, 0.33f);
			uIPanel.OnMouseOver += SetColorsToHovered;
			uIPanel.OnMouseOut += SetColorsToNotHovered;
			uIPanel.BackgroundColor = new Color(63, 82, 151) * 0.7f;
			uIPanel.BorderColor = new Color(89, 116, 213) * 0.7f;
			uIPanel.SetPadding(6f);
			UIImage uIImage = new UIImage(ModLoader.ModLoader.ManifestAssets.Request<Texture2D>(iconImagePath)) {
				IgnoresMouseInteraction = true,
				VAlign = 0.5f
			};

			uIImage.Left.Set(2f, 0f);
			uIPanel.Append(uIImage);
			uIPanel.OnMouseOver += ShowOptionDescription;
			uIPanel.OnMouseOut += ClearOptionDescription;
			UIText uIText = new UIText(Language.GetText(textKey), 0.45f, large: true) {
				HAlign = 0f,
				VAlign = 0.5f,
				Width = StyleDimension.FromPixelsAndPercent(-80f, 1f),
				Height = StyleDimension.FromPixelsAndPercent(0f, 1f),
				Top = StyleDimension.FromPixelsAndPercent(5f, 0f),
				Left = StyleDimension.FromPixels(80f),
				IgnoresMouseInteraction = true,
				TextOriginX = 0f,
				TextOriginY = 0f
			};

			uIText.PaddingLeft = 0f;
			uIText.PaddingRight = 20f;
			uIText.PaddingTop = 10f;
			uIText.IsWrapped = true;
			uIPanel.Append(uIText);
			uIPanel.SetSnapPoint("Button", 0);
			return uIPanel;
		}
	}
}
