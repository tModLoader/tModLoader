using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	public static class UICommon
	{
		public static Color defaultUIBlue = new Color(73, 94, 171);
		public static Color defaultUIBlueMouseOver = new Color(63, 82, 151) * 0.7f;
		public static Color mainPanelBackground = new Color(33, 43, 79) * 0.8f;

		public static StyleDimension MaxPanelWidth = new StyleDimension(600, 0);

		public static T WithFadedMouseOver<T>(this T elem, Color overColor = default(Color), Color outColor = default(Color)) where T : UIPanel {
			if (overColor == default(Color))
				overColor = defaultUIBlue;

			if (outColor == default(Color))
				outColor = defaultUIBlueMouseOver;

			elem.OnMouseOver += (evt, _) => {
				Main.PlaySound(SoundID.MenuTick);
				elem.BackgroundColor = overColor;
			};
			elem.OnMouseOut += (evt, _) => {
				elem.BackgroundColor = outColor;
			};
			return elem;
		}

		public static T WithPadding<T>(this T elem, float pixels) where T : UIElement {
			elem.SetPadding(pixels);
			return elem;
		}

		public static T WithPadding<T>(this T elem, string name, int id, Vector2? anchor = null, Vector2? offset = null) where T : UIElement {
			elem.SetSnapPoint(name, id, anchor, offset);
			return elem;
		}

		public static T WithView<T>(this T elem, float viewSize, float maxViewSize) where T : UIScrollbar {
			elem.SetView(viewSize, maxViewSize);
			return elem;
		}

		public static void AddOrRemoveChild(this UIElement elem, UIElement child, bool add) {
			if (!add) 
				elem.RemoveChild(child);
			else if (!elem.HasChild(child)) 
				elem.Append(child);
		}
	}
}
