/*
using Terraria;
using Terraria.ModLoader;
using Terraria.Enums;
using ExampleMod.Content.Tiles.Banners;

namespace ExampleMod.Content.Items.Placeable.Banners
{
	// All banner placing items are essentially the same aside from their placeStyle.
	// This class is used by the commented out EnemyBanner.EnemyBannerLoader class to automatically load items for each banner.
	public class EnemyBannerItem : ModItem
	{
		private string itemName;
		private int placeStyle;

		public override string Name => itemName;

		protected override bool CloneNewInstances => true;

		public EnemyBannerItem(string itemName, int placeStyle) {
			this.itemName = itemName;
			this.placeStyle = placeStyle;
		}

		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<EnemyBanner>(), placeStyle);
			Item.width = 10;
			Item.height = 24;
			Item.SetShopValues(ItemRarityColor.Blue1, Item.buyPrice(silver: 10));
		}
	}
}
*/