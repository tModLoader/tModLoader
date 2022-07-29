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

		private UIElement MakeFancyButtonMod(string path, string textKey) {
			return MakeFancyButtonInner(ModLoader.ModLoader.ManifestAssets.Request<Texture2D>(path), textKey);
		}
	}
}
