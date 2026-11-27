using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.ModLoader.Config.UI;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace ExampleMod.Common.Configs.CustomUI
{
	/// <summary>
	/// This is a custom UIModConfig that we can modify.
	/// See ModConfigShowcaseMisc.cs to see how to set custom UI.
	/// </summary>
	public class ExampleExtendedConfigUI : UIModConfig
	{
		private Color solidColor = Color.DeepPink;
		private Color fadedColor = Color.DeepPink * 0.7f;
		private Color alternateColor = Color.HotPink;

		public override void OnActivate() {
			base.OnActivate();

			SetMessage("Hello from Example Mod!", Color.White);

			// Theme the (almost) the entire UI (not just the main panel, which is still done with [BackgroundColor(r, g, b, a)])
			headerTextPanel.BackgroundColor = solidColor;

			modNamePanel.DefaultState.PanelColor = fadedColor * 0.5f;
			modNamePanel.DefaultState.BorderColor = fadedColor;

			configSideIndicatorPanel.BackgroundColor = fadedColor * 0.5f;
			configSideIndicatorPanel.BorderColor = fadedColor;

			filterTextFieldBackground.BackgroundColor = fadedColor;

			backButton.DefaultState.PanelColor = fadedColor;
			backButton.DefaultState.HoverPanelColor = solidColor;
			saveConfigButton.AlternateState.PanelColor = fadedColor;
			saveConfigButton.AlternateState.HoverPanelColor = solidColor;
			revertConfigButton.AlternateState.PanelColor = fadedColor;
			revertConfigButton.AlternateState.HoverPanelColor = solidColor;
			restoreDefaultsConfigButton.AlternateState.PanelColor = fadedColor;
			restoreDefaultsConfigButton.AlternateState.HoverPanelColor = solidColor;

			subPageBackButton.DefaultState.PanelColor = fadedColor;
			subPageBackButton.DefaultState.HoverPanelColor = solidColor;
			subPageBreadcrumbPanel.BackgroundColor = alternateColor;
			subPageBreadcrumbPanel.BorderColor = fadedColor * 0.5f;

			var newScrollbarTexture = ModContent.Request<Texture2D>("ExampleMod/Common/Configs/CustomUI/PinkScrollbar");
			var scrollbarTextureField = typeof(UIScrollbar).GetField("_texture", BindingFlags.Instance | BindingFlags.NonPublic);
			scrollbarTextureField?.SetValue(scrollbar, newScrollbarTexture);
		}

		public override void HandleBackButtonUsage() {
			base.HandleBackButtonUsage();

			var dialogPanel = (UIPanel)confirmDialog.Children.ElementAt(1);
			dialogPanel.BackgroundColor = solidColor;

			var buttons = dialogPanel.Children.Where(e => e is UIPanel);
			foreach (var button in buttons) {
				var buttonPanel = (UIPanel)button;
				buttonPanel.BackgroundColor = fadedColor;
				// This is kind of messy because it doesn't remove the OnMouseOver/Out events from the previous WithFadedMouseOver in UIModConfig
				buttonPanel.WithFadedMouseOver(solidColor, fadedColor);
			}
		}

		public override void SetMessage(string text, Color color) {
			if (color == Color.Green || color == Color.White)
				color = Color.Lerp(solidColor, Color.White, 0.75f);

			var notification = new ConfigNotification(UICommon.ButtonModConfigNotificationTexture, text, color, alternateColor * 0.5f, alternateColor * 0.75f);
			MenuNotificationsTracker.AddNotification(notification);
		}

		public override Tuple<UIElement, UIElement> WrapIt(UIElement parent, ref int top, PropertyFieldWrapper memberInfo, object item, int order, object list = null, Type arrayType = null, int index = -1) {
			var element =  base.WrapIt(parent, ref top, memberInfo, item, order, list, arrayType, index);
			if (element.Item2 is not ConfigElement configElement) {
				return element;
			}

			var field = typeof(ConfigElement).GetField("backgroundColor", BindingFlags.Instance | BindingFlags.NonPublic);
			if (field is null) {
				throw new Exception("Unable to get ConfigElement.backgroundColor");
			}

			var existingColor = field.GetValue(configElement);
			if (existingColor is Color color && color == UICommon.DefaultUIBlue) {
				field.SetValue(configElement, solidColor);;
			}

			return element;
		}
	}
}
