using ExampleMod.Content.Dusts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Placeable
{
	// ExampleWaterTorch is very similar to ExampleTorch, except it can be used and placed underwater, similar to Coral Torch.
	// The comments in this file will focus on the differences.
	// Both place the same tile, but a different tile style. The ExampleWaterTorch tile style has custom code seen in the ExampleTorch ModTile.
	public class ExampleWaterTorch : ModItem
	{
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;

			ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.ShimmerTorch;
			ItemID.Sets.SingleUseInGamepad[Type] = true;
			ItemID.Sets.Torches[Type] = true;
			ItemID.Sets.WaterTorches[Type] = true; // The TileObjectData.newSubTile code in the ExampleTorch ModTile is required as well to make a water torch.
		}

		public override void SetDefaults() {
			// Instead of placing style 0, style 1 is placed. The allowWaterPlacement parameter is true, which will set Item.noWet to false, allowing the item to be held underwater.
			Item.DefaultToTorch(ModContent.TileType<Tiles.ExampleTorch>(), 1, true);
			Item.value = 50;
		}

		public override void HoldItem(Player player) {
			if (Main.rand.NextBool(player.itemAnimation > 0 ? 7 : 30)) {
				Dust dust = Dust.NewDustDirect(new Vector2(player.itemLocation.X + (player.direction == -1 ? -16f : 6f), player.itemLocation.Y - 14f * player.gravDir), 4, 4, ModContent.DustType<Sparkle>(), 0f, 0f, 100);
				if (!Main.rand.NextBool(3)) {
					dust.noGravity = true;
				}

				dust.velocity *= 0.3f;
				dust.velocity.Y -= 1.5f;
				dust.position = player.RotatedRelativePoint(dust.position);
			}

			// Create a greenish (0.5, 1.5, 0.5) light at the torch's approximate position, when the item is held.
			Vector2 position = player.RotatedRelativePoint(new Vector2(player.itemLocation.X + 12f * player.direction + player.velocity.X, player.itemLocation.Y - 14f + player.velocity.Y), true);

			Lighting.AddLight(position, 0.5f, 1.5f, 0.5f);
		}

		public override void PostUpdate() {
			// Create a greenish (0.5, 1.5, 0.5) light when the item is in world, even if underwater.
			Lighting.AddLight(Item.Center, 0.5f, 1.5f, 0.5f);
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleTorch>()
				.AddIngredient(ItemID.Gel)
				.Register();
		}
	}
}
