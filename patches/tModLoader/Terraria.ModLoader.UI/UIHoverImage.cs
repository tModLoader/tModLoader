using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.GameContent.UI.Elements;

namespace Terraria.ModLoader.UI
{
	internal class UIHoverImage : UIImage
	{
		internal string hoverText;

		public UIHoverImage(Texture2D texture, string hoverText) : base(texture) {
			this.hoverText = hoverText;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			base.DrawSelf(spriteBatch);
			if (IsMouseHovering) {
				var hoverPos = new Vector2(Main.mouseX, Main.mouseY) + new Vector2(16f);
				hoverPos.X = Math.Min(hoverPos.X, Parent.GetDimensions().Width + Parent.GetDimensions().X -  Main.fontMouseText.MeasureString(hoverText).X - 16);
				hoverPos.Y = Math.Min(hoverPos.Y, Main.screenHeight - 30);
				Utils.DrawBorderStringFourWay(spriteBatch, Main.fontMouseText, hoverText, hoverPos.X, hoverPos.Y, Main.mouseTextColorReal, Color.Black, Vector2.Zero);
			}
		}
	}
}
