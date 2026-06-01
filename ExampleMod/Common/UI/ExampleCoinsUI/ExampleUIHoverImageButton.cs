using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.UI;

namespace ExampleMod.Common.UI.ExampleCoinsUI
{
	// This ExampleUIHoverImageButton class inherits from UIImageButton.
	// Inheriting is a great tool for UI design.
	// By inheriting, we get the Image drawing, MouseOver sound, and fading for free from UIImageButton
	// We've added some code to allow the Button to show a text tooltip while hovered
	internal class ExampleUIHoverImageButton : UIImageButton
	{
		// Tooltip text that will be shown on hover
		internal string hoverText;

		public ExampleUIHoverImageButton(Asset<Texture2D> texture, string hoverText) : base(texture) {
			this.hoverText = hoverText;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			// When you override UIElement methods, don't forget call the base method
			// This helps to keep the basic behavior of the UIElement
			base.DrawSelf(spriteBatch);

			// IsMouseHovering becomes true when the mouse hovers over the current UIElement
			if (IsMouseHovering) {
				// Show the tooltip when hovered.
				UICommon.TooltipMouseText(hoverText);
				// Another option is "Main.hoverItemName = hoverText;". Read the docs for information on the slight behavioral differences.
			}
		}
	}
}
