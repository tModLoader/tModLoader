using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria.Localization;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace Terraria.GameContent.UI.States;

partial class UIWorkshopHub : IHaveBackButtonCommand
{
	private UIElement _buttonMods;
	private UIElement _buttonModSources;
	private UIElement _buttonModBrowser;
	private UIElement _buttonModPack;

	public UIState PreviousUIState { get; set; }

	private void AppendTmlElements(UIElement uiElement)
	{
		var modsMenu = MakeButton_OpenModsMenu();
		modsMenu.HAlign = 0f;
		modsMenu.VAlign = 0f;
		uiElement.Append(modsMenu);

		var modSources = MakeButton_OpenModSourcesMenu();
		modSources.HAlign = 1f;
		modSources.VAlign = 0f;
		uiElement.Append(modSources);

		var modBrowser = MakeButton_OpenModBrowserMenu();
		modBrowser.HAlign = 0f;
		modBrowser.VAlign = 0.5f;
		uiElement.Append(modBrowser);

		var tbd = MakeButton_ModPackMenu();
		tbd.HAlign = 1f;
		tbd.VAlign = 0.5f;
		uiElement.Append(tbd);
	}

	private void OnChooseOptionDescription(UIElement listeningElement, ref LocalizedText localizedText)
	{
		if (listeningElement == _buttonMods)
			localizedText = Language.GetText("tModLoader.MenuManageModsDescription");

		if (listeningElement == _buttonModSources)
			localizedText = Language.GetText("tModLoader.MenuDevelopModsDescription");

		if (listeningElement == _buttonModBrowser)
			localizedText = Language.GetText("tModLoader.MenuDownloadModsDescription");

		if (listeningElement == _buttonModPack)
			localizedText = Language.GetText("tModLoader.MenuModPackDescription");
	}

	private UIElement MakeButton_OpenModsMenu()
	{
		UIElement uIElement = MakeFancyButtonMod($"Terraria.GameContent.UI.States.HubManageMods", "tModLoader.MenuManageMods");
		uIElement.OnLeftClick += Click_OpenModsMenu;
		_buttonMods = uIElement;
		return uIElement;
	}

	private void Click_OpenModsMenu(UIMouseEvent evt, UIElement listeningElement)
	{
		SoundEngine.PlaySound(10);
		Interface.modsMenu.PreviousUIState = this;
		Main.MenuUI.SetState(Interface.modsMenu);
	}

	private UIElement MakeButton_OpenModSourcesMenu()
	{
		UIElement uIElement = MakeFancyButtonMod($"Terraria.GameContent.UI.States.HubDevelopMods", "tModLoader.MenuDevelopMods");
		uIElement.OnLeftClick += Click_OpenModSourcesMenu;
		_buttonModSources = uIElement;
		return uIElement;
	}

	private void Click_OpenModSourcesMenu(UIMouseEvent evt, UIElement listeningElement)
	{
		SoundEngine.PlaySound(10);
		Interface.modSources.PreviousUIState = this;
		Main.MenuUI.SetState(Interface.modSources);
	}

	private UIElement MakeButton_OpenModBrowserMenu()
	{
		UIElement uIElement = MakeFancyButtonMod($"Terraria.GameContent.UI.States.HubDownloadMods", "tModLoader.MenuDownloadMods");
		uIElement.OnLeftClick += Click_OpenModBrowserMenu;
		_buttonModBrowser = uIElement;
		return uIElement;
	}

	private void Click_OpenModBrowserMenu(UIMouseEvent evt, UIElement listeningElement)
	{
		SoundEngine.PlaySound(10);
		Interface.modBrowser.PreviousUIState = this;
		Main.MenuUI.SetState(Interface.modBrowser);
	}

	private UIElement MakeButton_ModPackMenu()
	{
		UIElement uIElement = MakeFancyButtonMod($"Terraria.GameContent.UI.States.HubModPacks", "tModLoader.ModsModPacks");
		uIElement.OnLeftClick += Click_OpenModPackMenu;
		_buttonModPack = uIElement;
		return uIElement;
	}

	private void Click_OpenModPackMenu(UIMouseEvent evt, UIElement listeningElement)
	{
		SoundEngine.PlaySound(10);
		Interface.modPacksMenu.PreviousUIState = this;
		Main.MenuUI.SetState(Interface.modPacksMenu);
	}

	private UIElement MakeFancyButtonMod(string path, string textKey)
	{
		return MakeFancyButton_Inner(ModLoader.ModLoader.ManifestAssets.Request<Texture2D>(path), textKey);
	}
}
