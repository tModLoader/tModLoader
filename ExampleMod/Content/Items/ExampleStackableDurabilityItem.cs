using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items
{
	public class ExampleStackableDurabilityItem : ModItem
	{
		// 0 to 1
		public float durability;

		public override void SetDefaults() {
			Item.maxStack = 99; // This item is stackable, otherwise the example wouldn't work
			Item.width = 8;
			Item.height = 8;
		}

		public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			if (durability == 1)
				return;

			Vector2 spriteSize = frame.Size() * scale;

			spriteBatch.Draw(TextureAssets.MagicPixel.Value,
				position: new Vector2(position.X, position.Y + spriteSize.Y * 0.9f),
				sourceRectangle: new Rectangle(0, 0, 1, 1),
				Color.LightGreen,
				rotation: 0f,
				origin: Vector2.Zero,
				scale: new Vector2(spriteSize.X * durability, 2f),
				SpriteEffects.None,
				layerDepth: 0f);
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			tooltips.Add(new TooltipLine(Mod, "ExampleStackableDurabilityItem", $"Durability: {(int)(durability*100)}%") { OverrideColor = Color.LightGreen });
		}

		public override void OnStack(Item decrease, int numberToBeTransfered) {
			var incomingDurability = ((ExampleStackableDurabilityItem)decrease.ModItem).durability;

			// average the durability of the incoming and existing items
			durability = (durability * Item.stack + incomingDurability * numberToBeTransfered) / (Item.stack + numberToBeTransfered);
		}

		public override void OnCreate(ItemCreationContext context) {
			if (context is RecipeCreationContext) {
				durability = Main.rand.NextFloat();
			}
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}
	}
}
