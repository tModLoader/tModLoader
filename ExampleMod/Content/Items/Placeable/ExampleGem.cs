using ExampleMod.Content.Tiles;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Placeable
{
	public class ExampleGem : ModItem
	{
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<ExampleExposedGem>(), 0);
			Item.alpha = 50;
			Item.value = 7500;
		}
	}
}
