using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader.UI;
using Terraria.UI;
using Terraria.UI.Gamepad;
using Terraria.Localization;
using Terraria.GameContent;

namespace Terraria.ModLoader.Config.UI;
// TODO: Revert individual button.
internal class UIModConfig : UIState
{
	// Used for animations in elements like NPCDefinition
	public int UpdateCount { get; private set; }

	private UIElement uIElement;
	private UIPanel uIPanel;
	private UIList configList;
	private UIScrollbar uIScrollbar;
	private UIFocusInputTextField searchBar;
	private UITextPanel<string> configNamePanel;
	private UITextPanel<LocalizedText> subConfigBackButton;
	private UIButton<LocalizedText> saveConfigButton;
	private UIButton<LocalizedText> backButton;
	private UIButton<LocalizedText> revertConfigButton;
	private UIButton<LocalizedText> restoreDefaultsConfigButton;
	private UIPanel notificationModal;
	private UIMessageBox notificationModalText;
	private UIText notificationModalHeader;
	private UIImage modalInputBlocker;

	// TODO: use only one config and just load from disk instead
	// do we need 2 copies? We can discard changes by reloading.
	// We can save pending changes by saving file then loading/reloading mods.
	// when we get new server configs from server...replace, don't save?
	// reload manually, reload fresh server config?
	// need some CopyTo method to preserve references....hmmm
	private Mod mod;
	private ModConfig config;// Config from ConfigManager.Configs
	private ModConfig pendingConfig;// The clone of the config that is modified

	private UIConfigElement CurrentPage => subPages.Peek();
	private Stack<UIConfigElement> subPages = new();
	private List<UIConfigElement> configElements = new();
	private bool hasUnsavedChanges = false;
	private bool openedFromModder = false;

	public override void OnInitialize()
	{
		uIElement = new UIElement {
			Width = { Percent = 0.8f },
			MaxWidth = UICommon.MaxPanelWidth,
			Top = { Pixels = 220 },
			Height = { Pixels = -220, Percent = 1f },
			HAlign = 0.5f,
		};
		Append(uIElement);

		uIPanel = new UIPanel {
			Width = { Percent = 1f },
			Height = { Pixels = -65, Percent = 1f },
			BackgroundColor = UICommon.MainPanelBackground,
		};
		uIElement.Append(uIPanel);

		var headerTextPanel = new UITextPanel<LocalizedText>(Language.GetText("tModLoader.ModConfigModConfig"), 0.8f, true) {
			HAlign = 0.5f,
			Top = { Pixels = -35 },
			BackgroundColor = UICommon.DefaultUIBlue,
		}.WithPadding(15f);
		uIElement.Append(headerTextPanel);

		// TODO: clean this up
		subConfigBackButton = new UITextPanel<LocalizedText>(Language.GetText("UI.Back")) {
			Width = { Pixels = 75 },
			Height = { Pixels = 40 },
			Top = { Pixels = 5 },
		};
		uIPanel.Append(subConfigBackButton);// TODO: temporary
		// Don't append

		// TODO: fix name overflowing
		configNamePanel = new UITextPanel<string>("") {
			Width = { Pixels = -185 - 85, Percent = 1f },
			Height = { Pixels = 40 },
			Top = { Pixels = 5 },
			Left = { Pixels = 80 },
		};
		uIPanel.Append(configNamePanel);

		var textBoxBackground = new UIPanel {
			Width = { Pixels = 175 },
			Height = { Pixels = 30 },
			Top = { Pixels = 10 },
			HAlign = 1f,
		}.WithPadding(0f);
		uIPanel.Append(textBoxBackground);

		// TODO: localize properly
		searchBar = new UIFocusInputTextField(Language.GetTextValue("tModLoader.ModConfigFilterOptions")) {
			Top = { Pixels = 5 },
			Left = { Pixels = 10 },
			Width = { Pixels = -20, Percent = 1f },
			Height = { Pixels = 20 },
		};
		searchBar.OnTextChange += (_, _) => UpdateConfigList();
		searchBar.OnRightClick += (_, _) => searchBar.SetText("");
		searchBar.SetText("");
		textBoxBackground.Append(searchBar);

		float listTop = 50;
		configList = new UIList {
			Width = { Pixels = -25, Percent = 1f },
			Height = { Pixels = -listTop, Percent = 1f },
			Top = { Pixels = listTop },
			ListPadding = 2f,
		};
		uIPanel.Append(configList);

		uIScrollbar = new UIScrollbar {
			Top = { Pixels = listTop },
			Height = { Pixels = -listTop, Percent = 1f },
			HAlign = 1f,
		};
		uIScrollbar.SetView(100f, 1000f);
		configList.SetScrollbar(uIScrollbar);
		uIPanel.Append(uIScrollbar);

		backButton = new UIButton<LocalizedText>(Language.GetText("tModLoader.ModConfigBack")) {
			Width = { Pixels = -10, Percent = 0.25f },
			Height = { Pixels = 40 },
			Top = { Pixels = -20 },
			HAlign = 0 / 3f,
			VAlign = 1f,
			AltPanelColor = Color.Red * 0.7f,
			AltHoverPanelColor = Color.Red,
			UseAltColours = () => hasUnsavedChanges,
			ClickSound = SoundID.MenuClose,
			HoverSound = SoundID.MenuTick,
		};
		backButton.OnLeftClick += delegate (UIMouseEvent evt, UIElement listeningElement)
		{

			if (Main.gameMenu)
			{
				Main.menuMode = openedFromModder ? MenuID.Title : Interface.modConfigListID;
			}
			else
			{
				if (openedFromModder)
					IngameFancyUI.Close();
				else
					Main.InGameUI.SetState(Interface.modConfigList);
			}
		};
		uIElement.Append(backButton);

		saveConfigButton = new UIButton<LocalizedText>(Language.GetText("tModLoader.ModConfigSaveConfig")) {
			ClickSound = SoundID.MenuOpen,
			HoverSound = SoundID.MenuTick,
		};
		saveConfigButton.CopyStyle(backButton);
		saveConfigButton.HAlign = 1 / 3f;
		saveConfigButton.OnLeftClick += delegate (UIMouseEvent evt, UIElement listeningElement) {
			pendingConfig.Save();
			RefreshUI();
		};
		// Don't append

		revertConfigButton = new UIButton<LocalizedText>(Language.GetText("tModLoader.ModConfigRevertChanges")) {
			ClickSound = SoundID.MenuClose,
			HoverSound = SoundID.MenuTick,
		};
		revertConfigButton.CopyStyle(backButton);
		revertConfigButton.HAlign = 2 / 3f;
		revertConfigButton.OnLeftClick += delegate (UIMouseEvent evt, UIElement listeningElement) {
			ConfigManager.RevertConfig(pendingConfig, config);
			RefreshUI();
		};
		// Don't append

		restoreDefaultsConfigButton = new UIButton<LocalizedText>(Language.GetText("tModLoader.ModConfigRestoreDefaults")) {
			ClickSound = SoundID.MenuOpen,
			HoverSound = SoundID.MenuTick,
		};
		restoreDefaultsConfigButton.CopyStyle(backButton);
		restoreDefaultsConfigButton.HAlign = 3 / 3f;
		restoreDefaultsConfigButton.OnLeftClick += delegate (UIMouseEvent evt, UIElement listeningElement) {
			SoundEngine.PlaySound(SoundID.MenuOpen);
			ConfigManager.Reset(pendingConfig);// Reset to defaults
			RefreshUI();
		};
		uIElement.Append(restoreDefaultsConfigButton);

		notificationModal = new UIPanel {// TODO: fix vertical alignment
			Width = { Pixels = 500 },
			Height = { Pixels = 350 },
			HAlign = 0.5f,
			VAlign = 0.5f,
			BackgroundColor = UICommon.MainPanelBackground * (1 / 0.8f),
		};
		// Don't append

		notificationModalText = new UIMessageBox("") {
			Width = { Percent = 1f },
			Height = { Pixels = -50, Percent = 1f },
			HAlign = 0.5f,
			VAlign = 1f,
			TextOriginX = 0.5f,
			TextOriginY = 0.5f,
		};
		notificationModal.Append(notificationModalText);

		notificationModalHeader = new UIText("", 0.75f, large: true) {
			Top = { Pixels = 10 },
			HAlign = 0.5f,
		};
		notificationModal.Append(notificationModalHeader);

		var modalCloseButton = new UIImageButton(Main.Assets.Request<Texture2D>("Images/UI/SearchCancel")) {
			HAlign = 1f,
		};
		modalCloseButton.OnLeftClick += (_, _) => ClearMessage();
		notificationModal.Append(modalCloseButton);

		modalInputBlocker = new UIImage(TextureAssets.Extra[190]) {
			Width = { Percent = 1 },
			Height = { Percent = 1 },
			Color = new Color(0, 0, 0, 0),
			ScaleToFit = true,
		};
		modalInputBlocker.OnLeftClick += (_, _) => ClearMessage();
		// Don't append
	}

	public override void OnActivate()
	{
		pendingConfig = ConfigManager.GeneratePopulatedClone(config);
		searchBar.SetText("");
		uIScrollbar.ViewPosition = 0f;
		subPages.Clear();
		ClearMessage(sound: false);

		// Populating config elements now so they can save their state if the list is refreshed
		configElements.Clear();
		foreach (PropertyFieldWrapper memberInfo in ConfigManager.GetDisplayedVariables(pendingConfig)) {
			ConfigElementRegistry.HandleElement(configElements, pendingConfig, memberInfo);
		}

		RefreshUI();
	}

	internal void Unload()
	{
		mod = null;
		config = null;
		pendingConfig = null;

		UpdateCount = 0;
		configList?.Clear();
		subPages?.Clear();
		configElements?.Clear();
	}

	internal void SetMod(Mod mod, ModConfig config, bool openedFromModder = false)
	{
		this.openedFromModder = openedFromModder;
		this.mod = mod;
		this.config = config;
	}

	public void RefreshUI()
	{
		UpdateConfigList();
		CheckSaveButton();
		UpdateSeparatePage();
	}

	// Updates the main config list
	public void UpdateConfigList()
	{
		configList.Clear();

		foreach (var element in configElements) {
			configList.Add(element);
			element.RefreshUI();
		}

		Recalculate();
	}

	// Checks if the config has been changed and updates the save and revert buttons
	public void CheckSaveButton()
	{
		// Compare JSON because otherwise reference types act weird
		string pendingJson = JsonConvert.SerializeObject(pendingConfig, ConfigManager.serializerSettings);
		string existingJson = JsonConvert.SerializeObject(config, ConfigManager.serializerSettings);
				hasUnsavedChanges = pendingJson != existingJson;

		saveConfigButton.Remove();
		revertConfigButton.Remove();
		if (hasUnsavedChanges) {
			uIElement.Append(saveConfigButton);
			uIElement.Append(revertConfigButton);
		}
	}

	// Updates the header panel, separate page back button, and separate page contents
	// TODO
	public void UpdateSeparatePage()
	{
		string configName = mod.DisplayName + " - " + config.DisplayName.Value;
		string subPagesText = string.Join(" > ", subPages.Reverse());
		configNamePanel.SetText(configName + subPagesText);
	}

	public void OpenSeparatePage(UIConfigElement element)
	{
		subPages.Push(element);
		UpdateSeparatePage();
	}

	public void ClearMessage(bool sound = true)
	{
		SetMessage("", "");
		if (sound)
			SoundEngine.PlaySound(SoundID.MenuClose);
	}

	public void SetMessage(string text, string header, Color color = default, bool sendChatMessage = true)
	{
		if (color == default)
			color = Color.White;

		RemoveChild(notificationModal);
		RemoveChild(modalInputBlocker);
		notificationModalText.SetText("");
		notificationModalHeader.SetText("");
		notificationModalHeader.TextColor = Color.White;

		if (string.IsNullOrEmpty(text) && string.IsNullOrEmpty(header)) {
			return;
		}

		SoundEngine.PlaySound(SoundID.MenuOpen);
		Append(modalInputBlocker);
		Append(notificationModal);
		notificationModalText.Activate();
		notificationModalText.SetText(text);
		notificationModalHeader.SetText(header);
		notificationModalHeader.TextColor = color;

		if (sendChatMessage && !Main.gameMenu && Main.InGameUI.CurrentState != Interface.modConfig)
			Main.NewText($"[c/{color.Hex3()}:{header}] - {text}");
	}

	// Make scrolling outside of the main list still scroll the main list (if there is scrollbar hell then this helps a lot)
	public override void ScrollWheel(UIScrollWheelEvent evt)
	{
		base.ScrollWheel(evt);

		if (!configList.ContainsPoint(Main.MouseScreen))
			uIScrollbar.ViewPosition -= evt.ScrollWheelValue;
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);
		UpdateCount++;
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);

		if (revertConfigButton.IsMouseHovering)
			Main.instance.MouseText(Language.GetTextValue("tModLoader.ModConfigRevertChangesHover"));

		if (restoreDefaultsConfigButton.IsMouseHovering)
			Main.instance.MouseText(Language.GetTextValue("tModLoader.ModConfigRestoreDefaultsHover"));

		UILinkPointNavigator.Shortcuts.BackButtonCommand = 100;
		UILinkPointNavigator.Shortcuts.BackButtonGoto = Interface.modsMenuID;
	}
}
