using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.GameContent.UI.Elements;

namespace Terraria.ModLoader.UI
{
	internal class UIHoverImage : UIImage
	{
		internal string HoverText;
		internal Action<string> delayedDrawStorage;

		public UIHoverImage(Texture2D texture, string hoverText) : base(texture) {
			HoverText = hoverText;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			base.DrawSelf(spriteBatch);

			if (IsMouseHovering) {
				if (delayedDrawStorage != null) {
					delayedDrawStorage.Invoke(HoverText);
				}
				else {
					var bounds = Parent.GetDimensions().ToRectangle();
					bounds.Y = 0;
					bounds.Height = Main.screenHeight;
					UICommon.DrawHoverStringInBounds(spriteBatch, HoverText, bounds);
				}
			}
		}
	}
}
