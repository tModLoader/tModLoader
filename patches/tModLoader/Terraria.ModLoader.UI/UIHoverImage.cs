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
				var bounds = Parent.GetDimensions().ToRectangle();
				bounds.Y = 0;
				bounds.Height = Main.screenHeight;
				UICommon.DrawHoverStringInBounds(spriteBatch, hoverText, bounds);
			}
		}
	}
}
