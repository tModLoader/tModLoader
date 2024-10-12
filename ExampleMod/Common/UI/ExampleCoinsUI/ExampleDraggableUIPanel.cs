using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace ExampleMod.Common.UI.ExampleCoinsUI
{
	// This DraggableUIPanel class inherits from UIPanel
	// Inheriting is a great tool for UI design. By inheriting, we get the background drawing for free from UIPanel
	// We've added some code to allow the panel to be dragged around
	// We've also added some code to ensure that the panel will bounce back into bounds if it is dragged outside or the screen resizes
	// UIPanel does not prevent the player from using items when the mouse is clicked, so we've added that as well
	public class ExampleDraggableUIPanel : UIPanel
	{
		// Stores the offset from the top left of the UIPanel while dragging
		private Vector2 offset;
		// A flag that checks if the panel is currently being dragged
		private bool dragging;

		public override void LeftMouseDown(UIMouseEvent evt) {
			// When you override UIElement methods, don't forget call the base method
			// This helps to keep the basic behavior of the UIElement
			base.LeftMouseDown(evt);
			// When the mouse button is down on this element, then we start dragging
			if (evt.Target == this) {
				DragStart(evt);
			}
		}

		public override void LeftMouseUp(UIMouseEvent evt) {
			base.LeftMouseUp(evt);
			// When the mouse button is up, then we stop dragging
			if (evt.Target == this) {
				DragEnd(evt);
			}
		}

		private void DragStart(UIMouseEvent evt) {
			// The offset variable helps to remember the position of the panel relative to the mouse position
			// So no matter where you start dragging the panel, it will move smoothly
			offset = new Vector2(evt.MousePosition.X - Left.Pixels, evt.MousePosition.Y - Top.Pixels);
			dragging = true;
		}

		private void DragEnd(UIMouseEvent evt) {
			Vector2 endMousePosition = evt.MousePosition;
			dragging = false;

			Left.Set(endMousePosition.X - offset.X, 0f);
			Top.Set(endMousePosition.Y - offset.Y, 0f);

			Recalculate();
		}

		public override void Update(GameTime gameTime) {
			base.Update(gameTime);

			// Checking ContainsPoint and then setting mouseInterface to true is very common
			// This causes clicks on this UIElement to not cause the player to use current items
			if (ContainsPoint(Main.MouseScreen)) {
				Main.LocalPlayer.mouseInterface = true;
			}

			if (dragging) {
				Left.Set(Main.mouseX - offset.X, 0f); // Main.MouseScreen.X and Main.mouseX are the same
				Top.Set(Main.mouseY - offset.Y, 0f);
				Recalculate();
			}

			// Here we check if the DraggableUIPanel is outside the Parent UIElement rectangle
			// (In our example, the parent would be ExampleCoinsUI, a UIState. This means that we are checking that the DraggableUIPanel is outside the whole screen)
			// By doing this and some simple math, we can snap the panel back on screen if the user resizes his window or otherwise changes resolution
			var parentSpace = Parent.GetDimensions().ToRectangle();
			if (!GetDimensions().ToRectangle().Intersects(parentSpace)) {
				Left.Pixels = Utils.Clamp(Left.Pixels, 0, parentSpace.Right - Width.Pixels);
				Top.Pixels = Utils.Clamp(Top.Pixels, 0, parentSpace.Bottom - Height.Pixels);
				// Recalculate forces the UI system to do the positioning math again.
				Recalculate();
			}
		}
	}
}
