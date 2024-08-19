using ExampleMod.Tiles;
using Terraria;
using Terraria.ModLoader;
using Terraria.Enums;

namespace ExampleMod.Items.Banners
{
	public class SarcophagusBanner : ModItem
	{
		// The tooltip for this item is automatically assigned from .lang files
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<ExampleBanner>(), 0);
			Item.width = 10;
			Item.height = 24;
			Item.SetShopValues(ItemRarityColor.Blue1, Item.buyPrice(silver: 10));


			// TODO: KillsToBanner example, mention.
		}
	}
}