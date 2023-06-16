using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace Terraria.ModLoader.Config.UI;

internal class UIModConfigList : UIState
{
	private UIElement uIElement;
	private UIPanel uIPanel;
	private UITextPanel<LocalizedText> buttonBack;
	private UIList modList;
	private UIList configList;

	public Mod selectedMod;

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
			Height = { Pixels = -110, Percent = 1f },
			BackgroundColor = UICommon.MainPanelBackground,
		};
		uIElement.Append(uIPanel);

		var uIHeaderTextPanel = new UITextPanel<LocalizedText>(Language.GetText("tModLoader.ModConfiguration"), 0.8f, true) {
			HAlign = 0.5f,
			Top = { Pixels = -35f },
			BackgroundColor = UICommon.DefaultUIBlue,
		}.WithPadding(15f);
		uIElement.Append(uIHeaderTextPanel);

		var modListPanel = new UIPanel {
			Width = { Pixels = uIPanel.PaddingTop / -2, Percent = 0.5f },
			Height = { Percent = 1f },
		};
		uIPanel.Append(modListPanel);

		var configListPanel = new UIPanel {
			Width = { Pixels = uIPanel.PaddingTop / -2, Percent = 0.5f },
			Height = { Percent = 1f },
			HAlign = 1f,
		};
		uIPanel.Append(configListPanel);

		float headerHeight = 35;
		var modListHeader = new UIText(Language.GetText("tModLoader.MenuMods"), 0.5f, true) {
			Top = { Pixels = 5 },
			Left = { Pixels = 12.5f },
			HAlign = 0.5f,
		};
		modListPanel.Append(modListHeader);

		var configListHeader = new UIText(Language.GetText("tModLoader.ModConfigs"), 0.5f, true) {
			Top = { Pixels = 5 },
			Left = { Pixels = -12.5f },
			HAlign = 0.5f,
		};
		configListPanel.Append(configListHeader);

		modList = new UIList {
			Top = { Pixels = headerHeight },
			Width = { Pixels = -25, Percent = 1f },
			Height = { Pixels = -headerHeight, Percent = 1f },
			ListPadding = 5f,
			HAlign = 1f,
		};
		modListPanel.Append(modList);

		configList = new UIList {
			Top = { Pixels = headerHeight },
			Width = { Pixels = -25f, Percent = 1f },
			Height = { Pixels = -headerHeight, Percent = 1f },
			ListPadding = 5f,
			HAlign = 0f,
		};
		configListPanel.Append(configList);

		var modListScrollbar = new UIScrollbar {
			Top = { Pixels = headerHeight },
			Height = { Pixels = -headerHeight, Percent = 1f },
		};
		modListScrollbar.SetView(100f, 1000f);
		modList.SetScrollbar(modListScrollbar);
		modListPanel.Append(modListScrollbar);

		var configListScrollbar = new UIScrollbar {
			Top = { Pixels = headerHeight },
			Height = { Pixels = -headerHeight, Percent = 1f },
			HAlign = 1f,
		};
		configListScrollbar.SetView(100f, 1000f);
		configList.SetScrollbar(configListScrollbar);
		configListPanel.Append(configListScrollbar);

		buttonBack = new UITextPanel<LocalizedText>(Language.GetText("UI.Back"), 0.7f, large: true) {
			Width = { Pixels = -10f, Percent = 0.5f },
			Height = { Pixels = 50f },
			Top = { Pixels = -45f },
			VAlign = 1f,
			HAlign = 0.5f,
		}.WithFadedMouseOver();
		buttonBack.OnLeftClick += BackClick;
		uIElement.Append(buttonBack);
	}

	private static void BackClick(UIMouseEvent evt, UIElement listeningElement)
	{
		SoundEngine.PlaySound(11, -1, -1, 1);

		if (Main.gameMenu) {
			Main.menuMode = Interface.modsMenuID;
		}
		else {
			IngameFancyUI.Close();
		}
	}

	internal void Unload()
	{
		modList?.Clear();
		configList?.Clear();
		selectedMod = null;
	}

	public override void OnActivate()
	{
		Main.clrInput();
		modList.Clear();
		configList?.Clear();
		selectedMod = null;
		PopulateMods();
	}

	internal void PopulateMods()
	{
		modList?.Clear();

		foreach (var mod in ModLoader.Mods) {
			if (ConfigManager.Configs.TryGetValue(mod, out _)) {
				var modPanel = new UITextPanel<string>(mod.DisplayName) {
					HAlign = 0.5f
				};
				modPanel.OnMouseOver += delegate (UIMouseEvent evt, UIElement listeningElement) {
					SoundEngine.PlaySound(SoundID.MenuTick);
				};
				modPanel.OnUpdate += delegate (UIElement affectedElement) {
					modPanel.BackgroundColor = selectedMod == mod ? UICommon.DefaultUIBlue : UICommon.DefaultUIBlueMouseOver;
					modPanel.BorderColor = modPanel.IsMouseHovering ? UICommon.DefaultUIBorderMouseOver : UICommon.DefaultUIBorder;
				};
				modPanel.OnLeftClick += delegate (UIMouseEvent evt, UIElement listeningElement) {
					SoundEngine.PlaySound(SoundID.MenuOpen);
					selectedMod = mod;
					PopulateConfigs();
				};

				modList.Add(modPanel);
			}
		}
	}

	internal void PopulateConfigs()
	{
		configList?.Clear();

		if (!ConfigManager.Configs.TryGetValue(selectedMod, out var configs)) {
			return;
		}

		foreach (var config in configs) {
			var configPanel = new UITextPanel<LocalizedText>(config.DisplayName) {
				HAlign = 0.5f,
			};
			configPanel.WithFadedMouseOver();
			configPanel.OnLeftClick += delegate (UIMouseEvent evt, UIElement listeningElement) {
				SoundEngine.PlaySound(SoundID.MenuOpen);

				Interface.modConfig.SetMod(selectedMod, config, true);
				if (Main.gameMenu) {
					Main.menuMode = Interface.modConfigID;
				}
				else {
					Main.InGameUI.SetState(Interface.modConfig);
				}
			};
			configPanel.OnUpdate += delegate (UIElement affectedElement) {
				if (configPanel.IsMouseHovering)
					Main.instance.MouseText(Language.GetTextValue(config.Mode == ConfigScope.ServerSide ? "tModLoader.ModConfigServerSide" : "tModLoader.ModConfigClientSide"));
			};
			configList.Add(configPanel);
		}
	}
}