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
			int ans = NewText(context, 1, position, stay5TimesLonger);
			if (ans == -1)
				return -1;
			Main.popupText[ans].color = color;
			Main.popupText[ans].name = text;
			Main.popupText[ans].npcNetID = 0;
			return ans;
		}
	}
}
