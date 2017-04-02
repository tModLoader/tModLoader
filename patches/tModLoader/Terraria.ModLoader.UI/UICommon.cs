using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	static class UICommon
	{
		internal static Color defaultUIBlue = new Color(73, 94, 171);
		internal static Color defaultUIBlueMouseOver = new Color(63, 82, 151) * 0.7f;

		internal static void FadedMouseOver(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(SoundID.MenuTick);
			((UIPanel)evt.Target).BackgroundColor = defaultUIBlue;
		}

		internal static void FadedMouseOut(UIMouseEvent evt, UIElement listeningElement)
		{
			((UIPanel)evt.Target).BackgroundColor = defaultUIBlueMouseOver;
		}

		internal static void CustomFadedMouseOver(Color customColor, UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(SoundID.MenuTick);
			((UIPanel)evt.Target).BackgroundColor = customColor;
		}

		internal static void CustomFadedMouseOut(Color customColor, UIMouseEvent evt, UIElement listeningElement)
		{
			((UIPanel)evt.Target).BackgroundColor = customColor;
		}
	}
}
