using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace ExampleMod.Common.UI.ExampleMarqueeText
{
	/// <summary>
	/// This is an example of an in-game fullscreen UI that showcases marquee text, which is text that scrolls back and forth when it overflows.
	/// For more details, see ExampleFullscreenUI.
	/// To view this in-game, type "/marqueetextdemo" into chat (see ShowMarqueeTextUICommand.cs)
	/// </summary>
	internal class ExampleMarqueeTextUI : UIState, ILoadable
	{
		public static ExampleMarqueeTextUI instance;

		private static LocalizedText HeaderText { get; set; }
		private static LocalizedText DescriptionText { get; set; }
		private static LocalizedText NormalTextLabel { get; set; }
		private static LocalizedText MarqueeTextLabel { get; set; }
		private static LocalizedText MarqueeTextHoverLabel { get; set; }
		private static LocalizedText DynamicWidthMarqueeTextLabel { get; set; }
		private static LocalizedText ConstantWidthMarqueeTextLabel { get; set; }

		private UIMarqueeText marqueeText;
		private UIMarqueeTextPanel marqueeTextPanel;
		private UIMarqueeText[] marqueeTextOnHover;
		private UIMarqueeTextPanel[] marqueeTextPanelOnHover;
		private UIMarqueeTextPanel dynamicWidthMarqueeTextPanel;
		private UIMarqueeTextPanel constantWidthMarqueeTextPanel;

		public void Load(Mod mod) {
			instance = this;

			HeaderText = mod.GetLocalization("UI.ExampleMarqueeTextUI.Header");
			DescriptionText = mod.GetLocalization("UI.ExampleMarqueeTextUI.Description");
			NormalTextLabel = mod.GetLocalization("UI.ExampleMarqueeTextUI.NormalText");
			MarqueeTextLabel = mod.GetLocalization("UI.ExampleMarqueeTextUI.MarqueeText");
			MarqueeTextHoverLabel = mod.GetLocalization("UI.ExampleMarqueeTextUI.MarqueeTextHover");
			DynamicWidthMarqueeTextLabel = mod.GetLocalization("UI.ExampleMarqueeTextUI.DynamicWidthMarqueeText");
			ConstantWidthMarqueeTextLabel = mod.GetLocalization("UI.ExampleMarqueeTextUI.ConstantWidthMarqueeText");
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
			var header = new UIText(HeaderText) { // "Marquee Text Examples"
				IsWrapped = true,
				Width = StyleDimension.Fill,
				HAlign = 0.5f
			};
			panel.Append(header);
			top += 40;

			var description = new UIText(DescriptionText) { // "This is a demo of the marquee text elements added by tML."
				Top = new(top, 0f),
				TextOriginX = 0f,
				IsWrapped = true,
				Width = StyleDimension.Fill
			};
			panel.Append(description);
			top += 60;

			AddSpacer(panel, ref top);

			const float UIPanelPadding = 12f;
			const float TextWidth = 150f;
			const float PanelWidth = TextWidth + 2 * UIPanelPadding;

			// All of the marquee text elements automatically set Width and Height based on the size of their text
			// This is different to UIText and UITextPanel, which set MinWidth and MinHeight based on the size of their text
			// This means that if you want to restrict a marquee text elements size, you can set MaxWidth and MaxHeight
			// This allows you to have UI elements automatically scale up and down in size, but will cause text to marquee when they reach their max size

			var comparisonMarqueeText = new UIText(NormalTextLabel) { // "Normal Text"
				Top = new(top + UIPanelPadding, 0f), // +12f for the padding on top of the text panel (to align the text with the panel)
				MaxWidth = new(100f, 0f),
				HAlign = 0f,
			};
			panel.Append(comparisonMarqueeText);

			var comparisonMarqueeTextPanel = new UITextPanel<object>(NormalTextLabel) { // "Normal Text"
				Top = new(top, 0f),
				MaxWidth = new(PanelWidth, 0f), // +24f for padding (since this makes the UIMarqueeText the same size as this)
				HAlign = 1f,
			};
			panel.Append(comparisonMarqueeTextPanel);
			top += 50;

			marqueeText = new UIMarqueeText(MarqueeTextLabel) { // "Marquee Text (watch me scroll)"
				Top = new(top + UIPanelPadding, 0f),
				MaxWidth = new(TextWidth, 0f),
				HAlign = 0f,
			};
			panel.Append(marqueeText);

			marqueeTextPanel = new UIMarqueeTextPanel(MarqueeTextLabel) { // "Marquee Text (watch me scroll)"
				Top = new(top, 0f),
				MaxWidth = new(PanelWidth, 0f),
				HAlign = 1f,
			};
			panel.Append(marqueeTextPanel);
			top += 50;

			AddSpacer(panel, ref top);

			// Showcase of marquee on hover
			// Also compares different TextHAlign/TextOriginX values combined with AlignToStartWhenNotScrolling = true
			// All of these should look/behave the same

			(UIMarqueeText, UIMarqueeTextPanel) CreateMarqueeTextOnHover(float textHAlign) {
				var text = new UIMarqueeText(MarqueeTextHoverLabel.Format(textHAlign)) { // "Hover Marquee Text: Align = {0}"
					Top = new(top + UIPanelPadding, 0f),
					MaxWidth = new(TextWidth, 0f),
					HAlign = 0f,
					TextOriginX = textHAlign,
					MarqueeText = {
						IsScrolling = false,
					},
				};
				text.OnMouseOver += (evt, element) => text.MarqueeText.IsScrolling = true;
				text.OnMouseOut += (evt, element) => text.MarqueeText.IsScrolling = false;
				panel.Append(text);

				var textPanel = new UIMarqueeTextPanel(MarqueeTextHoverLabel.Format(textHAlign)) { // "Hover Marquee Text: Align = {0}"
					Top = new(top, 0f),
					MaxWidth = new(PanelWidth, 0f),
					HAlign = 1f,
					TextHAlign = textHAlign,
					MarqueeText = {
						IsScrolling = false,
					},
				};
				textPanel.OnMouseOver += (evt, element) => textPanel.MarqueeText.IsScrolling = true;
				textPanel.OnMouseOut += (evt, element) => textPanel.MarqueeText.IsScrolling = false;
				panel.Append(textPanel);
				top += 50;

				return (text, textPanel);
			}

			marqueeTextOnHover = [null, null, null];
			marqueeTextPanelOnHover	= [null, null, null];

			(marqueeTextOnHover[0], marqueeTextPanelOnHover[0]) = CreateMarqueeTextOnHover(0f);
			(marqueeTextOnHover[1], marqueeTextPanelOnHover[1]) = CreateMarqueeTextOnHover(0.5f);
			(marqueeTextOnHover[2], marqueeTextPanelOnHover[2]) = CreateMarqueeTextOnHover(1f);

			AddSpacer(panel, ref top);

			// This examples compare using dynamic width and constant width with UIMarqueeTextPanel (both with a maximum width)

			const float TargetWidth = 175f + 24f;

			dynamicWidthMarqueeTextPanel = new UIMarqueeTextPanel(DynamicWidthMarqueeTextLabel) { // "Dynamic Width: {0}"
				Top = new(top, 0f),
				MaxWidth = new(TargetWidth, 0f),
				HAlign = 0f,
			};
			dynamicWidthMarqueeTextPanel.WithFadedMouseOver();
			dynamicWidthMarqueeTextPanel.OnLeftClick += (evt, element) => UpdateConstantAndDynamicMarqueeText();
			panel.Append(dynamicWidthMarqueeTextPanel);

			constantWidthMarqueeTextPanel = new UIMarqueeTextPanel(ConstantWidthMarqueeTextLabel) { // "Constant Width: {0}"
				Top = new(top, 0f),
				// Set MinWidth and MaxWidth to the same to make it constant width
				// Width is overwritten by calling SetText, so this is how to make it actually work
				MinWidth = new(TargetWidth, 0f),
				MaxWidth = new(TargetWidth, 0f),
				HAlign = 1f,
			};
			constantWidthMarqueeTextPanel.WithFadedMouseOver();
			constantWidthMarqueeTextPanel.OnLeftClick += (evt, element) => UpdateConstantAndDynamicMarqueeText();
			panel.Append(constantWidthMarqueeTextPanel);

			top += 50;

			AddSpacer(panel, ref top);

			var backButton = new UITextPanel<LocalizedText>(Language.GetText("UI.Back"), 0.7f) {
				Top = new(top, 0f),
				Width = new(-10f, 1 / 3f),
				Height = new(30f, 0f),
				HAlign = 0.5f,
			};
			backButton.WithFadedMouseOver();
			backButton.OnLeftClick += BackButton_OnLeftClick;
			panel.Append(backButton);
			top += 40;

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
			UpdateConstantAndDynamicMarqueeText();

			// Reset the marquee text and delay the scrolling when the UI is opened
			marqueeText.ResetScroll();
			marqueeTextPanel.ResetScroll();

			foreach (var text in marqueeTextOnHover) {
				text.ResetScroll();
			}

			foreach (var textPanel in marqueeTextPanelOnHover) {
				textPanel.ResetScroll();
			}

			marqueeText.MarqueeText.SetScrollDelay(60);
			marqueeTextPanel.MarqueeText.SetScrollDelay(60);
		}

		private void UpdateConstantAndDynamicMarqueeText() {
			int numberOfDigits = Main.rand.Next(0, 7) + 1; // 1 to 7 (inclusive)
			int number = Main.rand.Next((int)Math.Pow(10, (numberOfDigits - 1)), (int)Math.Pow(10, numberOfDigits));

			dynamicWidthMarqueeTextPanel.SetText(DynamicWidthMarqueeTextLabel.Format(number));
			constantWidthMarqueeTextPanel.SetText(ConstantWidthMarqueeTextLabel.Format(number));
			dynamicWidthMarqueeTextPanel.Recalculate();
			constantWidthMarqueeTextPanel.Recalculate();

			dynamicWidthMarqueeTextPanel.ResetScroll();
			constantWidthMarqueeTextPanel.ResetScroll();
			dynamicWidthMarqueeTextPanel.MarqueeText.SetScrollDelay(60);
			constantWidthMarqueeTextPanel.MarqueeText.SetScrollDelay(60);
		}

		private void BackButton_OnLeftClick(UIMouseEvent evt, UIElement listeningElement) {
			IngameFancyUI.Close();
		}
	}
}
