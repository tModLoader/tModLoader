using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;

namespace Terraria.ModLoader.UI
{
	internal class UIHoverImage : UIImage
	{
		internal string HoverText;

		public UIHoverImage(Texture2D texture, string hoverText) : base(texture) {
			HoverText = hoverText;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			base.DrawSelf(spriteBatch);

			if (IsMouseHovering) {
				var bounds = Parent.GetDimensions().ToRectangle();
				bounds.Y = 0;
				bounds.Height = Main.screenHeight;
				UICommon.DrawHoverStringInBounds(spriteBatch, HoverText, bounds);
			}
		}
	}
}
