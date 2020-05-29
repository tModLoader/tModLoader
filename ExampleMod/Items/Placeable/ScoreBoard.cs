using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;

namespace ExampleMod.Items.Placeable
{
	internal class ScoreBoard : ModItem
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Scoreboard");
			Tooltip.SetDefault("Compete for kills with friends.");
		}

		public override void SetDefaults() {
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.useTurn = true;
			item.useAnimation = 15;
			item.useTime = 10;
			item.autoReuse = true;
			item.maxStack = 999;
			item.consumable = true;
			item.createTile = TileType<Tiles.ScoreBoard>();
			item.width = 28;
			item.height = 28;
		}
	}
}
