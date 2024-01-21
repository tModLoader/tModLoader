using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.UI;
using Terraria.UI;
using Terraria.UI.Chat;
using Terraria.UI.Gamepad;

namespace Terraria.ModLoader.Config.UI;

internal class UIModConfigList : UIState
{
	public Mod ModToSelectOnOpen;

	private Mod selectedMod;
	private UIElement uIElement;
	private UIPanel uIPanel;
	private UITextPanel<LocalizedText> backButton;
	private UIList modList;
	private UIList configList;

	public override void OnInitialize()
	{
		uIElement = new UIElement {
			Width = { Percent = 0.8f },
			MaxWidth = { Pixels = 800, Percent = 0f },
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
			ManualSortMethod = (list) => { }, // Elements added in order, no need to sort.
		};
		modListPanel.Append(modList);

		configList = new UIList {
			Top = { Pixels = headerHeight },
			Width = { Pixels = -25f, Percent = 1f },
			Height = { Pixels = -headerHeight, Percent = 1f },
			ListPadding = 5f,
			HAlign = 0f,
			ManualSortMethod = (list) => { }, // Elements added in order, no need to sort.
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

		backButton = new UITextPanel<LocalizedText>(Language.GetText("UI.Back"), 0.7f, large: true) {
			Width = { Pixels = -10f, Percent = 0.5f },
			Height = { Pixels = 50f },
			Top = { Pixels = -45f },
			VAlign = 1f,
			HAlign = 0.5f,
		}.WithFadedMouseOver();
		backButton.OnLeftClick += (_, _) => {
			SoundEngine.PlaySound(SoundID.MenuClose);

			if (Main.gameMenu)
				Main.menuMode = Interface.modsMenuID;
			else
				IngameFancyUI.Close();
		};

		uIElement.Append(backButton);
	}

	internal void Unload()
	{
		modList?.Clear();
		configList?.Clear();
		selectedMod = null;
		ModToSelectOnOpen = null;
	}

	public override void OnActivate()
	{
		modList?.Clear();
		configList?.Clear();

		// Select the mod that we clicked on, otherwise don't select anything
		selectedMod = null;
		if (ModToSelectOnOpen != null) {
			selectedMod = ModToSelectOnOpen;
			ModToSelectOnOpen = null;
		}

		// Populate UI
		PopulateMods();
		if (selectedMod != null)
			PopulateConfigs();
	}

	private void PopulateMods()
	{
		modList?.Clear();

		// Have to sort by display name because normally mods are sorted by internal names
		var mods = ModLoader.Mods.ToList();
		mods.Sort((x, y) => CleanChatTags(x.DisplayName).CompareTo(CleanChatTags(y.DisplayName)));

		foreach (var mod in mods) {
			if (ConfigManager.Configs.TryGetValue(mod, out _)) {
				var modPanel = new UIButton<string>(mod.DisplayName) {
					MaxWidth = { Percent = 0.95f },
					HAlign = 0.5f,
					ScalePanel = true,
					AltPanelColor = UICommon.MainPanelBackground,
					AltHoverPanelColor = UICommon.MainPanelBackground * (1 / 0.8f),
					UseAltColors = () => selectedMod != mod,
					ClickSound = SoundID.MenuTick,
				};

				modPanel.OnLeftClick += delegate (UIMouseEvent evt, UIElement listeningElement) {
					selectedMod = mod;
					PopulateConfigs();
				};

				modList.Add(modPanel);
			}
		}
	}

	private void PopulateConfigs()
	{
		configList?.Clear();

		if (selectedMod == null || !ConfigManager.Configs.TryGetValue(selectedMod, out var configs))
			return;

		// Have to sort by display name because normally configs are sorted by internal names
		// TODO: Support sort by attribute or some other custom ordering then replicate logic in UIModConfig.SetMod too
		var sortedConfigs = configs.OrderBy(x => CleanChatTags(x.DisplayName.Value)).ToList();

		foreach (var config in sortedConfigs) {
			float indicatorOffset = 20;

			var configPanel = new UIButton<LocalizedText>(config.DisplayName) {
				MaxWidth = { Percent = 0.95f },
				HAlign = 0.5f,
				ScalePanel = true,
				UseInnerDimensions = true,
				ClickSound = SoundID.MenuOpen,
			};
			configPanel.PaddingRight += indicatorOffset;

			configPanel.OnLeftClick += delegate (UIMouseEvent evt, UIElement listeningElement) {
				Interface.modConfig.SetMod(selectedMod, config);
				if (Main.gameMenu)
					Main.menuMode = Interface.modConfigID;
				else
					Main.InGameUI.SetState(Interface.modConfig);
			};

			configList.Add(configPanel);

			// ConfigScope indicator
			var indicatorTexture = Main.Assets.Request<Texture2D>("Images/UI/Settings_Toggle");
			var indicatorFrame = indicatorTexture.Frame(2, 1, 1, 0);
			var serverColor = Colors.RarityRed;
			var clientColor = Colors.RarityCyan;

			var sideIndicator = new UIImageFramed(indicatorTexture, indicatorFrame) {
				VAlign = 0.5f,
				HAlign = 1f,
				Color = config.Mode == ConfigScope.ServerSide ? serverColor : clientColor,
				MarginRight = -indicatorOffset
			};

			sideIndicator.OnUpdate += delegate (UIElement affectedElement) {
				if (sideIndicator.IsMouseHovering) {
					string colorCode = config.Mode == ConfigScope.ServerSide ? serverColor.Hex3() : clientColor.Hex3();
					string hoverText = Language.GetTextValue(config.Mode == ConfigScope.ServerSide ? "tModLoader.ModConfigServerSide" : "tModLoader.ModConfigClientSide");
					Main.instance.MouseText($"[c/{colorCode}:{hoverText}]");
				}
			};

			configPanel.Append(sideIndicator);
		}
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);

		UILinkPointNavigator.Shortcuts.BackButtonCommand = 100;
		UILinkPointNavigator.Shortcuts.BackButtonGoto = Interface.modsMenuID;
	}

	// TODO: this should be in utils, also add color parameter
	private static string CleanChatTags(string text)
	{
		return string.Join("", ChatManager.ParseMessage(text, Color.White)
				.Where(x => x.GetType() == typeof(TextSnippet))
				.Select(x => x.Text));
	}
}