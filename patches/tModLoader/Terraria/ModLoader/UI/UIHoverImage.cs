using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent.UI.Elements;

namespace Terraria.ModLoader.UI;

internal class UIHoverImage : UIImage
{
	internal string HoverText;
	internal bool UseTooltipMouseText; // Not sure if all would benefit from this, opt in.

	public UIHoverImage(Asset<Texture2D> texture, string hoverText) : base(texture)
	{
		HoverText = hoverText;
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		base.DrawSelf(spriteBatch);

		if (IsMouseHovering) {
			var bounds = Parent.GetDimensions().ToRectangle();
			bounds.Y = 0;
			bounds.Height = Main.screenHeight;
			if(UseTooltipMouseText)
				UICommon.TooltipMouseText(HoverText);
			else
				UICommon.DrawHoverStringInBounds(spriteBatch, HoverText, bounds);
		}
	}
}
