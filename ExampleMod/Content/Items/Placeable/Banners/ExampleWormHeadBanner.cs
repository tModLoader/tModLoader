using ExampleMod.Content.Tiles.Banners;
using Terraria.Enums;
using Terraria.ModLoader;
using Terraria;

namespace ExampleMod.Content.Items.Placeable.Banners
{
	public class ExampleWormHeadBanner : ModItem
	{
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<EnemyBanner>(), (int)EnemyBanner.StyleID.ExampleWormHead);
			Item.width = 10;
			Item.height = 24;
			Item.SetShopValues(ItemRarityColor.Blue1, Item.buyPrice(silver: 10));
		}
	}
}

