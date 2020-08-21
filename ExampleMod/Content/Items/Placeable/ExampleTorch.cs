using ExampleMod.Content.Tiles;
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
			if (Main.rand.Next(player.itemAnimation > 0 ? 40 : 80) == 0) {
				Dust.NewDust(new Vector2(player.itemLocation.X + 16f * player.direction, player.itemLocation.Y - 14f * player.gravDir), 4, 4, DustType<Sparkle>());
			}
			Vector2 position = player.RotatedRelativePoint(new Vector2(player.itemLocation.X + 12f * player.direction + player.velocity.X, player.itemLocation.Y - 14f + player.velocity.Y), true);
			Lighting.AddLight(position, 1f, 1f, 1f);
		}

		public override void PostUpdate() {
			if (!item.wet) {
				Lighting.AddLight((int)((item.position.X + item.width / 2) / 16f), (int)((item.position.Y + item.height / 2) / 16f), 1f, 1f, 1f);
			}
		}

		public override void AutoLightSelect(ref bool dryTorch, ref bool wetTorch, ref bool glowstick) => dryTorch = true;

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient(ItemID.Torch, 3)
				.AddIngredient<ExampleBlock>()
				.AddTile(TileID.Furnaces)
				.Register();
		}
	}
}