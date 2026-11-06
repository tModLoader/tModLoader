using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader;
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

		private UIMarqueeText marqueeText;
		private UIMarqueeTextPanel marqueeTextPanel;

		public void Load(Mod mod) {
			instance = this;
		}

		public void Unload() {
		}

		// TODO: better side by side comparison
		// - also include a UIMarqueeText with the padding Y really big so ou can see the bottom of the text avoid getting clipped
		// TODO: demo of marquee on hover
		// TODO: demo of align to start
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

			var comparisonMarqueeText = new UIText("Testing Marquee Text (Regular)") {
				Top = new(top, 0f),
				MaxWidth = new(100f, 0f),
			};
			panel.Append(comparisonMarqueeText);
			top += 20;

			marqueeText = new UIMarqueeText("Testing Marquee Text (Marquee)") {
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

			marqueeTextPanel = new UIMarqueeTextPanel("Testing Marquee Text Panel (Marquee)") {
				Top = new(top, 0f),
				MaxWidth = new(150f, 0f),
			};
			panel.Append(marqueeTextPanel);
			top += 50;

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
			// Reset the marquee text and delay the scrolling when the UI is opened
			marqueeText.ResetScroll();
			marqueeTextPanel.ResetScroll();
			marqueeText.MarqueeText.SetScrollDelay(60);
			marqueeTextPanel.MarqueeText.SetScrollDelay(60);
		}

		private void BackButton_OnLeftClick(UIMouseEvent evt, UIElement listeningElement) {
			IngameFancyUI.Close();
		}
	}
}
