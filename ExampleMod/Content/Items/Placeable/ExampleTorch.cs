using ExampleMod.Content.Dusts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Content.Items.Placeable
{
	public class ExampleTorch : ModItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("This is a modded torch.");
		}

		public override void SetDefaults() {
			item.flame = true;
			item.noWet = true;
			item.useStyle = ItemUseStyleID.Swing;
			item.useTurn = true;
			item.useAnimation = 15;
			item.useTime = 10;
			item.holdStyle = ItemHoldStyleID.HoldFront;
			item.autoReuse = true;
			item.maxStack = 999;
			item.consumable = true;
			item.createTile = TileType<Tiles.ExampleTorch>();
			item.width = 10;
			item.height = 12;
			item.value = 50;
		}

		public override void HoldItem(Player player) {
			// Randomly spawn sparkles when the torch is held. Twice bigger chance to spawn them when swinging the torch.
			if (Main.rand.Next(player.itemAnimation > 0 ? 40 : 80) == 0) {
				Dust.NewDust(new Vector2(player.itemLocation.X + 16f * player.direction, player.itemLocation.Y - 14f * player.gravDir), 4, 4, DustType<Sparkle>());
			}

			// Create a white (1.0, 1.0, 1.0) light at the torch's approximate position, when the item is held.
			Vector2 position = player.RotatedRelativePoint(new Vector2(player.itemLocation.X + 12f * player.direction + player.velocity.X, player.itemLocation.Y - 14f + player.velocity.Y), true);

			Lighting.AddLight(position, 1f, 1f, 1f);
		}

		public override void PostUpdate() {
			// Create a white (1.0, 1.0, 1.0) light when the item is in world, and isn't underwater.
			if (!item.wet) {
				Lighting.AddLight(item.Center, 1f, 1f, 1f);
			}
		}

		public override void AutoLightSelect(ref bool dryTorch, ref bool wetTorch, ref bool glowstick) {
			dryTorch = true; // This makes our item eligible for being selected with smart select at a short distance when not underwater.
		}
		
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient(ItemID.Torch, 3)
				.AddIngredient<ExampleBlock>()
				.AddTile(TileID.Furnaces)
				.Register();
		}
	}
}
