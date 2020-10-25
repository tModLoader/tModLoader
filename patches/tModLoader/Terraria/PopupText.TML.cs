using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent;

namespace Terraria
{
	public partial class PopupText
	{
		public static int NewText(PopupTextContext context, string text, Color color, Vector2 position, bool stay5TimesLonger) {
		if (!Main.showItemText)
			return -1;

		if (Main.netMode == 2)
			return -1;

		int num = FindNextItemTextSlot();
		if (num >= 0) {
			Vector2 value = FontAssets.MouseText.Value.MeasureString(text);
			PopupText popupText = Main.popupText[num];
			Main.popupText[num].alpha = 1f;
			popupText.alphaDir = -1;
			popupText.active = true;
			popupText.scale = 0f;
			popupText.NoStack = true;
			popupText.rotation = 0f;
			popupText.position = position - value / 2f;
			popupText.expert = false;
			popupText.master = false;
			popupText.name = text;
			popupText.stack = 1;
			popupText.velocity.Y = -7f;
			popupText.lifeTime = 60;
			popupText.context = context;
			if (stay5TimesLonger)
				popupText.lifeTime *= 5;
			popupText.coinValue = 0;
			popupText.coinText = false;
			popupText.color = color;
		}

		return num;
	}
}
}
