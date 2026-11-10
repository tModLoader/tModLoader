using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.UI;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;
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
	private UIPanel modListPanel;
	private UIPanel configListPanel;
	private UIScrollbar modListScrollbar;
	private UIScrollbar configListScrollbar;
	private UIText configListPrompt;

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
			Top = { Pixels = -46 }, // -35 is common for most UIs, but UIWorkshopHub uses -46 to fit more content
			BackgroundColor = UICommon.DefaultUIBlue,
		}.WithPadding(15f);
		uIElement.Append(uIHeaderTextPanel);

		modListPanel = new UIPanel {
			Width = { Pixels = uIPanel.PaddingTop / -2, Percent = 0.5f },
			Height = { Percent = 1f },
		};
		uIPanel.Append(modListPanel);

		configListPanel = new UIPanel {
			Width = { Pixels = uIPanel.PaddingTop / -2, Percent = 0.5f },
			Height = { Percent = 1f },
			HAlign = 1f,
		};
		uIPanel.Append(configListPanel);

		float headerHeight = 35;
		var modListHeader = new UIText(Language.GetText("tModLoader.MenuMods"), 0.5f, true) {
			Top = { Pixels = 5 },
			HAlign = 0.5f,
		};
		modListPanel.Append(modListHeader);

		var configListHeader = new UIText(Language.GetText("tModLoader.ModConfigs"), 0.5f, true) {
			Top = { Pixels = 5 },
			HAlign = 0.5f,
		};
		configListPanel.Append(configListHeader);

		modList = new UIList {
			Top = { Pixels = headerHeight },
			Width = { Percent = 1f },
			Height = { Pixels = -headerHeight, Percent = 1f },
			ListPadding = 5f,
			HAlign = 1f,
			ManualSortMethod = (list) => { }, // Elements added in order, no need to sort.
		};
		modListPanel.Append(modList);

		configList = new UIList {
			Top = { Pixels = headerHeight },
			Width = { Percent = 1f },
			Height = { Pixels = -headerHeight, Percent = 1f },
			ListPadding = 5f,
			HAlign = 0f,
			ManualSortMethod = (list) => { }, // Elements added in order, no need to sort.
		};
		configListPanel.Append(configList);

		modListScrollbar = new UIScrollbar {
			Top = { Pixels = headerHeight },
			Height = { Pixels = -headerHeight, Percent = 1f },
		};
		modListScrollbar.SetView(100f, 1000f);
		modList.SetScrollbar(modListScrollbar);
		modListPanel.Append(modListScrollbar);

		configListScrollbar = new UIScrollbar {
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

		configListPrompt = new UIText(Language.GetText("tModLoader.ModConfigSelectModToViewConfigs")) {
			Width = { Percent = 1f },
			HAlign = 0.5f,
			VAlign = 0.5f,
			IsWrapped = true,
		};
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
		modList.Clear();
		configList.Clear();

		configListPrompt.Remove();
		modListScrollbar.Remove();
		configListScrollbar.Remove();

		modList.ViewPosition = 0f;
		configList.ViewPosition = 0f;

		// Select the mod that we clicked on, otherwise don't select anything
		selectedMod = null;
		if (ModToSelectOnOpen != null) {
			selectedMod = ModToSelectOnOpen;
			ModToSelectOnOpen = null;
		}

		// Populate UI
		PopulateMods();
		if (selectedMod != null) {
			PopulateConfigs();
		}
		else {
			configListPanel.Append(configListPrompt);
		}
	}

	private void PopulateMods()
	{
		modList?.Clear();

		// Have to sort by display name because normally mods are sorted by internal names
		var mods = ModLoader.Mods.ToList();
		mods.Sort((x, y) => x.DisplayNameClean.CompareTo(y.DisplayNameClean));

		foreach (var mod in mods) {
			if (mod.Name == "ModLoader")
				continue;

			var (modPanel, _) = CreatePanel(mod.DisplayName, mod.SmallModIcon ?? Mod.PlaceholderSmallModIcon, null);

			if (ConfigManager.Configs.ContainsKey(mod)) {
				modPanel.OnLeftClick += (_, _) => {
					if (selectedMod == mod)
						return;

					SoundEngine.PlaySound(SoundID.MenuOpen);
					selectedMod = mod;
					PopulateConfigs();
				};



				modPanel.OnMouseOver += (_, _) => {
					SoundEngine.PlaySound(SoundID.MenuTick);
					modPanel.BackgroundColor = selectedMod == mod ? UICommon.DefaultUIBlue : UICommon.MainPanelBackground * (1 / 0.8f);
					modPanel.BorderColor = UICommon.DefaultUIBorderMouseOver;
				};

				modPanel.OnMouseOut += (_, _) => {
					modPanel.BackgroundColor = selectedMod == mod ? UICommon.DefaultUIBlueMouseOver : UICommon.MainPanelBackground;
					modPanel.BorderColor = UICommon.DefaultUIBorder;
				};
			}
			else {
				modPanel.BackgroundColor = Color.Gray;

				modPanel.OnDraw += (_, _) => {
					if (!modPanel.IsMouseHovering)
						return;

					UICommon.TooltipMouseText(Language.GetTextValue("tModLoader.ModConfigModLoaderButNoConfigs"));
				};
			}

			modList.Add(modPanel);
		}

		HandleDynamicScrollbar(modListScrollbar, modList, modListPanel);
	}

	private void PopulateConfigs()
	{
		configList?.Clear();
		configListPrompt.Remove();

		if (selectedMod == null || !ConfigManager.Configs.TryGetValue(selectedMod, out var configs))
			return;

		// Have to sort by display name because normally configs are sorted by internal names
		// TODO: Support sort by attribute or some other custom ordering then replicate logic in UIModConfig.SetMod too
		var sortedConfigs = configs.OrderBy(x => Utils.CleanChatTags(x.DisplayName.Value)).ToList();

		foreach (var config in sortedConfigs) {
			var indicatorTexture = UICommon.ConfigSideIndicatorTexture;
			// -2 to account for padding in texture to avoid texture atlas issues
			var indicatorFrame = indicatorTexture.Frame(2, 1, config.Mode == ConfigScope.ServerSide ? 1 : 0, 0, -2);
			var (configPanel, sideIndicator) = CreatePanel(config.DisplayName, indicatorTexture, indicatorFrame);

			configPanel.WithFadedMouseOver();
			configPanel.OnLeftClick += (_, _) => {
				SoundEngine.PlaySound(SoundID.MenuOpen);

				Interface.modConfig.SetMod(selectedMod, config);
				if (Main.gameMenu)
					Main.menuMode = Interface.modConfigID;
				else
					Main.InGameUI.SetState(Interface.modConfig);
			};

			sideIndicator.OnDraw += (_, _) => {
				if (!sideIndicator.IsMouseHovering)
					return;

				string hoverText = Language.GetTextValue(config.Mode == ConfigScope.ServerSide ? "tModLoader.ModConfigServerSide" : "tModLoader.ModConfigClientSide");
				UICommon.TooltipMouseText(hoverText);
			};

			configList.Add(configPanel);
		}

		HandleDynamicScrollbar(configListScrollbar, configList, configListPanel);
	}

	private static void HandleDynamicScrollbar(UIScrollbar scrollbar, UIList list, UIElement parent)
	{
		scrollbar.Remove();
		list.Recalculate();
		list.Width.Pixels = 0;
		if (scrollbar.CanScroll) {
			list.Width.Pixels = -25;
			parent.Append(scrollbar);
		}
		list.Recalculate();
	}

	// TODO: switch this to UIButton in the future when it is updated to use UIMarqueeTextPanel
	private static (UIMarqueeTextPanel, UIImageFramed) CreatePanel(object text, Asset<Texture2D> iconTexture, Rectangle? iconFrame)
	{
		// TODO: fix clipping into the edge of the UI list
		var panel = new UIMarqueeTextPanel(text) {
			MinWidth = { Percent = 1f },
			MinHeight = { Pixels = 40 },
			TextHAlign = 0f,
			MarqueeText = {
				IsScrolling = false,
			},
		};

		panel.MaxWidth = panel.MinWidth;
		panel.MaxHeight = panel.MinHeight;

		panel.OnMouseOver += (_, _) => panel.MarqueeText.IsScrolling = true;
		panel.OnMouseOut += (_, _) => panel.MarqueeText.IsScrolling = false;

		// TODO: redo all of these calculations
		float iconOffset = iconFrame?.Width ?? iconTexture.Width();
		// float indicatorPadding = 6; // Supposed to be 12, but the icons have a lot of spare transparency on the sides which wastes space
		panel.PaddingLeft += iconOffset;

		var icon = new UIImageFramed(iconTexture, iconFrame ?? Utils.CornerRectangle(Vector2.Zero, iconTexture.Size())) {
			VAlign = 0.5f,
			HAlign = 0f,
			Color = Color.White,
			MarginLeft = -iconOffset,// + 4, // 4 for alignment
		};

		// The original calculations were for when the panel padding was 6px, but it was later changed to 12 px
		icon.MarginLeft -= 6;
		icon.MarginTop -= 6;

		panel.Append(icon);

		return (panel, icon);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);

		UILinkPointNavigator.Shortcuts.BackButtonCommand = 100;
		UILinkPointNavigator.Shortcuts.BackButtonGoto = Interface.modsMenuID;
	}
}