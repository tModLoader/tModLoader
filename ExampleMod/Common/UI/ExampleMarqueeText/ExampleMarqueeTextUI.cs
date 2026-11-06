using ExampleMod.Common.Configs.ModConfigShowcases;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.UI;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace ExampleMod.Common.UI.ExampleMarqueeText
{
	/// <summary>
	///  TOOD
	/// </summary>
	// TODO: localize
	internal class ExampleMarqueeTextUI : UIState, ILoadable
	{
		public static ExampleMarqueeTextUI instance;

		public void Load(Mod mod) {
			instance = this;
		}

		public void Unload() {
		}

		public override void OnInitialize() {
			// This boilerplate is copied from ExampleFullscreenUI
			var panel = new UIPanel() {
				HAlign = 0.5f,
				VAlign = 0.5f,
				Width = new(450, 0f),
				Height = new(0, 0f),
				BackgroundColor = new Color(63, 82, 151)
			};
			Append(panel);

			int top = 0;
			var header = new UIText("Example Marquee Text") { // "Example Fullscreen UI"
				IsWrapped = true,
				Width = StyleDimension.Fill,
				HAlign = 0.5f
			};
			panel.Append(header);
			top += 40;

			var description = new UIText("This is a demo of the marquee text elements added by tML") { // "This is an example fullscreen UI, notice how other UI is hidden."
				Top = new(top, 0f),
				TextOriginX = 0f,
				IsWrapped = true,
				Width = StyleDimension.Fill
			};
			panel.Append(description);
			top += 60;

			AddSpacer(panel, ref top);

			itemDefinitionMessage = new UIText(GetItemDefinitionMessageText()) {
				TextColor = Color.Orange,
				Top = new(top, 0f),
				TextOriginX = 0f,
				IsWrapped = true,
				Width = StyleDimension.Fill
			};
			panel.Append(itemDefinitionMessage);
			top += 60;

			var randomizeButton = new UITextPanel<LocalizedText>(RandomizeItemText, 0.7f) {
				Top = new(top, 0f),
				Width = new(-10f, 1 / 3f),
				Height = new(30f, 0f)
			};
			randomizeButton.HAlign = 0f;
			randomizeButton.WithFadedMouseOver();
			randomizeButton.OnLeftClick += RandomizeButton_OnLeftClick;
			panel.Append(randomizeButton);

			var openConfigAButton = new UITextPanel<LocalizedText>(Language.GetText("tModLoader.ModsOpenConfig"), 0.7f);
			openConfigAButton.CopyStyle(randomizeButton);
			openConfigAButton.HAlign = 0.5f;
			openConfigAButton.WithFadedMouseOver();
			openConfigAButton.OnLeftClick += OpenConfigAButton_OnLeftClick;
			panel.Append(openConfigAButton);
			top += 40;

			AddSpacer(panel, ref top);

			onlyChangeableDuringNightToggle = new UICycleImage(SomeBoolToggleTexture, 2, 32, 32, 0, 0) {
				Left = new(0, 0f),
				Top = new(top, 0f),
			};
			onlyChangeableDuringNightToggle.OnLeftClick += OnlyChangeableDuringNightToggle_OnLeftClick;
			panel.Append(onlyChangeableDuringNightToggle);

			onlyChangeableDuringNightMessage = new UIText(GetOnlyChangeableDuringNightMessageText()) {
				Top = new(top + 6, 0f),
				Left = new(40f, 0f),
				TextOriginX = 0f,
				IsWrapped = true,
				Width = new(-36, 1f)
			};
			panel.Append(onlyChangeableDuringNightMessage);
			top += 38;

			someNumberMessage = new UIText(GetSomeNumberMessageText()) {
				Top = new(top + 6, 0f),
				TextOriginX = 0f,
				IsWrapped = true,
				Width = StyleDimension.Fill
			};
			panel.Append(someNumberMessage);
			top += 32;

			var openConfigBButton = new UITextPanel<LocalizedText>(Language.GetText("tModLoader.ModsOpenConfig"), 0.7f) {
				Top = new(top, 0f),
				Width = new(-10f, 1 / 3f),
				Height = new(30f, 0f)
			};
			openConfigBButton.HAlign = 0.5f;
			openConfigBButton.WithFadedMouseOver();
			openConfigBButton.OnLeftClick += OpenConfigBButton_OnLeftClick;
			panel.Append(openConfigBButton);

			var saveConfigBButton = new UITextPanel<LocalizedText>(Language.GetText("tModLoader.ModConfigSaveConfig"), 0.7f);
			saveConfigBButton.CopyStyle(openConfigBButton);
			saveConfigBButton.HAlign = 1f;
			saveConfigBButton.BackgroundColor = Color.Purple * 0.7f;
			saveConfigBButton.WithFadedMouseOver(Color.Purple, Color.Purple * 0.7f);
			saveConfigBButton.OnLeftClick += SaveConfigBButton_OnLeftClick;
			panel.Append(saveConfigBButton);
			top += 40;

			AddSpacer(panel, ref top);

			ConfigSaveStatusMessage = new UIText(Language.GetTextValue("tModLoader.ModConfigNotification")) {
				Top = new(top, 0f),
				Left = new(0f, 0f),
				TextOriginX = 0f,
				Width = StyleDimension.Fill
			};
			panel.Append(ConfigSaveStatusMessage);
			top += 30;

			var backButton = new UITextPanel<LocalizedText>(Language.GetText("UI.Back"), 0.7f) {
				TextColor = Color.Red,
				Top = new(top, 0f),
				Width = new(-10f, 1 / 3f),
				Height = new(30f, 0f)
			};
			backButton.WithFadedMouseOver();
			backButton.OnLeftClick += BackButton_OnLeftClick;
			panel.Append(backButton);
			top += 40;

			var comparisonMarqueeText = new UIText("Testing Marquee Text (Regular)") {
				Top = new(top, 0f),
				MaxWidth = new(100f, 0f),
			};
			panel.Append(comparisonMarqueeText);
			top += 20;

			var marqueeText = new UIMarqueeText("Testing Marquee Text (Marquee)") {
				Top = new(top, 0f),
				MaxWidth = new(100f, 0f),
			};
			panel.Append(marqueeText);
			top += 20;

			var comparisonMarqueeTextPanel = new UITextPanel<object>("Testing Marquee Text Panel (Regular)") {
				Top = new(top, 0f),
				MaxWidth = new(150f, 0f),
			};
			panel.Append(comparisonMarqueeTextPanel);
			top += 50;

			var marqueeTextPanel = new UIMarqueeTextPanel("Testing Marquee Text Panel (Marquee)") {
				Top = new(top, 0f),
				MaxWidth = new(150f, 0f),
			};
			panel.Append(marqueeTextPanel);
			top += 50;

			// TODO: test the set scroll delay, scroll on hover, and align to start when not scrolling

			panel.Height.Pixels = top + panel.PaddingTop + panel.PaddingBottom;
		}

		private void AddSpacer(UIPanel panel, ref int top) {
			var spacer = new UIHorizontalSeparator() {
				Width = StyleDimension.Fill,
				Color = new Color(89, 116, 213, 255),
				Top = new StyleDimension(top, 0f)
			};
			panel.Append(spacer);
			top += 16;
		}

		public override void OnActivate() {
			// TODO: reset all marquee text
			// TODO: perhaps make it automatic?
		}
	}
}
