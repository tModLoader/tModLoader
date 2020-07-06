using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameInput;
using Terraria.UI;

namespace ExampleMod.UI
{
	// This class wraps the vanilla ItemSlot class into a UIElement. The ItemSlot class was made before the UI system was made, so it can't be used normally with UIState. 
	// By wrapping the vanilla ItemSlot class, we can easily use ItemSlot.
	// ItemSlot isn't very modder friendly and operates based on a "Context" number that dictates how the slot behaves when left, right, or shift clicked and the background used when drawn. 
	// If you want more control, you might need to write your own UIElement.
	// I've added basic functionality for validating the item attempting to be placed in the slot via the validItem Func. 
	// See ExamplePersonUI for usage and use the Awesomify chat option of Example Person to see in action.
	internal class VanillaItemSlotWrapper : UIElement
	{
		internal Item Item;
		private readonly int _context;
		private readonly float _scale;
		internal Func<Item, bool> ValidItemFunc;

		public VanillaItemSlotWrapper(int context = ItemSlot.Context.BankItem, float scale = 1f) {
			_context = context;
			_scale = scale;
			Item = new Item();
			Item.SetDefaults(0);

			Width.Set(Main.inventoryBack9Texture.Width * scale, 0f);
			Height.Set(Main.inventoryBack9Texture.Height * scale, 0f);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			float oldScale = Main.inventoryScale;
			Main.inventoryScale = _scale;
			Rectangle rectangle = GetDimensions().ToRectangle();

			if (ContainsPoint(Main.MouseScreen) && !PlayerInput.IgnoreMouseInterface) {
				Main.LocalPlayer.mouseInterface = true;
				if (ValidItemFunc == null || ValidItemFunc(Main.mouseItem)) {
					// Handle handles all the click and hover actions based on the context.
					ItemSlot.Handle(ref Item, _context);
				}
			}
			// Draw draws the slot itself and Item. Depending on context, the color will change, as will drawing other things like stack counts.
			ItemSlot.Draw(spriteBatch, ref Item, _context, rectangle.TopLeft());
			Main.inventoryScale = oldScale;
		}
	}
}