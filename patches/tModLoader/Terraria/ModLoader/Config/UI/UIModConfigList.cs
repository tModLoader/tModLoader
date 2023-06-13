using Terraria.Audio;
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

	// TODO panel sizing
	public override void OnInitialize()
	{
		uIElement = new UIElement {
			Width = { Pixels = 0f, Percent = 0.8f },
			MaxWidth = { Pixels = 800f, Percent = 0f },
			Top = { Pixels = 110f, Percent = 0f },
			Height = { Pixels = -110f, Percent = 1f },
			HAlign = 0.5f
		};
		Append(uIElement);

		uIPanel = new UIPanel {
			Width = { Pixels = 0f, Percent = 1f },
			Height = { Pixels = -110f, Percent = 1f },
			BackgroundColor = UICommon.MainPanelBackground
		};
		uIElement.Append(uIPanel);

		var modListPanel = new UIPanel {
			Width = { Pixels = uIPanel.PaddingTop / -2, Percent = 0.5f },
			Height = { Pixels = 0f, Percent = 1f },
			HAlign = 0f
		};
		uIPanel.Append(modListPanel);

		var configListPanel = new UIPanel {
			Width = { Pixels = uIPanel.PaddingTop / -2, Percent = 0.5f },
			Height = { Pixels = 0f, Percent = 1f },
			HAlign = 1f
		};
		uIPanel.Append(configListPanel);

		modList = new UIList {
			Width = { Pixels = -25f, Percent = 1f },
			Height = { Pixels = 0f, Percent = 1f },
			ListPadding = 5f,
			HAlign = 1f
		};
		modListPanel.Append(modList);

		configList = new UIList {
			Width = { Pixels = -25f, Percent = 1f },
			Height = { Pixels = 0f, Percent = 1f },
			ListPadding = 5f,
			HAlign = 0f
		};
		configListPanel.Append(configList);

		UIScrollbar modListScrollbar = new UIScrollbar {
			Height = { Pixels = 0f, Percent = 1f },
			HAlign = 0f
		};
		modListScrollbar.SetView(100f, 1000f);
		modList.SetScrollbar(modListScrollbar);
		modListPanel.Append(modListScrollbar);

		UIScrollbar configListScrollbar = new UIScrollbar {
			Height = { Pixels = 0f, Percent = 1f },
			HAlign = 1f
		};
		configListScrollbar.SetView(100f, 1000f);
		configList.SetScrollbar(configListScrollbar);
		configListPanel.Append(configListScrollbar);

		UITextPanel<LocalizedText> uIHeaderTextPanel = new UITextPanel<LocalizedText>(Language.GetText("tModLoader.ModConfiguration"), 0.8f, true) {
			Top = { Pixels = -35f, Percent = 0f },
			BackgroundColor = UICommon.DefaultUIBlue,
			HAlign = 0.5f
		};
		uIHeaderTextPanel.SetPadding(15f);
		uIElement.Append(uIHeaderTextPanel);

		buttonBack = new UITextPanel<LocalizedText>(Language.GetText("UI.Back"), 0.7f, large: true) {
			Width = { Pixels = -10f, Percent = 0.5f },
			Height = { Pixels = 50f, Percent = 0f },
			Top = { Pixels = -45f, Percent = 0f },
			VAlign = 1f,
			HAlign = 0.5f
		};
		buttonBack.WithFadedMouseOver();
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

	// TODO improve
	internal void PopulateMods()
	{
		modList?.Clear();

		foreach (var mod in ModLoader.Mods) {
			if (ConfigManager.Configs.TryGetValue(mod, out _)) {
				string modName = mod.DisplayName;

				var modPanel = new UITextPanel<string>(modName) {
					HAlign = 0.5f
				};
				modPanel.WithFadedMouseOver();
				modPanel.OnLeftClick += delegate (UIMouseEvent evt, UIElement listeningElement) {
					SoundEngine.PlaySound(SoundID.MenuOpen);
					// TODO: selected indicator
					selectedMod = mod;
					PopulateConfigs();
				};

				modList.Add(modPanel);
			}
		}
	}

	// TODO improve
	internal void PopulateConfigs()
	{
		configList?.Clear();

		if (!ConfigManager.Configs.TryGetValue(selectedMod, out var configs)) {
			return;
		}

		foreach (var config in configs) {
			var configPanel = new UITextPanel<string>(config.DisplayName.Value) {
				HAlign = 0.5f
			};
			configPanel.WithFadedMouseOver();
			configPanel.OnLeftClick += delegate (UIMouseEvent evt, UIElement listeningElement) {
				SoundEngine.PlaySound(SoundID.MenuOpen);

				// TODO: add server side indicator
				Interface.modConfig.SetMod(selectedMod, config, true);
				if (Main.gameMenu) {
					Main.menuMode = Interface.modConfigID;
				}
				else {
					Main.InGameUI.SetState(Interface.modConfig);
				}
			};

			configList.Add(configPanel);
		}
	}
}