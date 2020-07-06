using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;

namespace ExampleMod.Items.Placeable
{
	public class PuritySpiritTrophy : ModItem
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Spirit of Purity Trophy");
		}

		public override void SetDefaults() {
			item.width = 30;
			item.height = 30;
			item.maxStack = 99;
			item.useTurn = true;
			item.autoReuse = true;
			item.useAnimation = 15;
			item.useTime = 10;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.consumable = true;
			item.value = 50000;
			item.rare = ItemRarityID.Blue;
			item.createTile = TileType<Tiles.BossTrophy>();
			item.placeStyle = 1;
		}
	}
}