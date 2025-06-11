using ExampleMod.Content.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace ExampleMod.Common
{
	public class ExamplePlayerDrawLayer : PlayerDrawLayer
	{
		// Returning true in this property makes this layer appear on the minimap player head icon.
		public override bool IsHeadLayer => true;

		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
			// The layer will be visible only if the player is holding an ExampleItem in their hands. Or if another modder forces this layer to be visible.
			return drawInfo.drawPlayer.HeldItem?.type == ModContent.ItemType<ExampleItem>();

			// If you'd like to reference another PlayerDrawLayer's visibility,
			// you can do so by getting its instance via ModContent.GetInstance<OtherDrawLayer>(), and calling GetDefaultVisibility on it
		}

		// This layer will be a 'child' of the head layer, and draw before (beneath) it.
		// If the Head layer is hidden, this layer will also be hidden.
		// If the Head layer is moved, this layer will move with it.
		public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.Head);
		// If you want to make a layer which isn't a child of another layer, use `new Between(Layer1, Layer2)` to specify the position.
		// If you want to make a 'mobile' layer which can render in different locations depending on the drawInfo, use a `Multiple` position.

		protected override void Draw(ref PlayerDrawSet drawInfo) {
			// The following code draws ExampleItem's texture behind the player's head.
			var exampleItemTexture = TextureAssets.Item[ModContent.ItemType<ExampleItem>()];

			var position = drawInfo.Center + new Vector2(0f, -20f) - Main.screenPosition;
			position = new Vector2((int)position.X, (int)position.Y); // You'll sometimes want to do this, to avoid quivering.

			// Queues a drawing of a sprite. Do not use SpriteBatch in drawlayers!
			drawInfo.DrawDataCache.Add(new DrawData(
				exampleItemTexture.Value, // The texture to render.
				position, // Position to render at.
				null, // Source rectangle.
				Color.White, // Color.
				0f, // Rotation.
				exampleItemTexture.Size() * 0.5f, // Origin. Uses the texture's center.
				1f, // Scale.
				SpriteEffects.None, // SpriteEffects.
				0 // 'Layer'. This is always 0 in Terraria.
			));
		}
	}
}
