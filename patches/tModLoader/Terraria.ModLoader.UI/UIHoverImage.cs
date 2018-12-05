using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
				float x = Main.fontMouseText.MeasureString(hoverText).X;
				Vector2 vector = new Vector2((float)Main.mouseX, (float)Main.mouseY) + new Vector2(16f);
				if (vector.Y > (float)(Main.screenHeight - 30)) {
					vector.Y = (float)(Main.screenHeight - 30);
				}
				if (vector.X > (float)(Parent.GetDimensions().Width + Parent.GetDimensions().X - x - 16)) {
					vector.X = (float)(Parent.GetDimensions().Width + Parent.GetDimensions().X - x - 16);
				}
				Utils.DrawBorderStringFourWay(spriteBatch, Main.fontMouseText, hoverText, vector.X, vector.Y, new Color((int)Main.mouseTextColor, (int)Main.mouseTextColor, (int)Main.mouseTextColor, (int)Main.mouseTextColor), Color.Black, Vector2.Zero, 1f);
			}
		}
	}
}
