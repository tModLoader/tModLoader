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
		// All items in the stack have the same durability
		// Durability is combined and averaged when stacking
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

		private static float WeightedAverage(float durability1, int stack1, float durability2, int stack2) {
			return (durability1 * stack1 + durability2 * stack2) / (stack1 + stack2);
		}

		public override void OnStack(Item decrease, int numberToBeTransfered) {
			var incomingDurability = ((ExampleStackableDurabilityItem)decrease.ModItem).durability;
			durability = WeightedAverage(durability, Item.stack, incomingDurability, numberToBeTransfered);
		}

		public override void OnCreate(ItemCreationContext context) {
			if (context is RecipeCreationContext recipe) {
				// OnCraft is called on the entire stack, after OnStack, so we need to add only 1 item's worth of durability
				int numCrafted = recipe.recipe.createItem.stack;
				int numPreCraft = Item.stack - numCrafted;
				float newItemsDurability = Main.rand.NextFloat();

				// need to compensate for the fact that a 0 durability item was stacked with it
				float oldDurability = numPreCraft == 0 ? 0 : durability * Item.stack / numPreCraft;
				durability = WeightedAverage(oldDurability, numPreCraft, newItemsDurability, numCrafted);
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
