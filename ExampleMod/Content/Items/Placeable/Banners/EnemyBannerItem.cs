using Terraria;
using Terraria.ModLoader;
using Terraria.Enums;
using Terraria.ID;
using ExampleMod.Content.Tiles.Banners;

namespace ExampleMod.Content.Items.Placeable.Banners
{
	// All banner placing items are essentially the same aside from their placeStyle and potentially the number of kills to receive the banner.
	// Because they are so similar, we are dynamically loading multiple copies of this class in EnemyBannerSystem.cs rather than make a class for each.
	// This approach, or something similar, can help streamline implementing banners for all enemies in your mod.
	public class EnemyBannerItem : ModItem
	{
		private string itemName;
		private int placeStyle;
		private int? killsToBanner;

		public override string Name => itemName;

		protected override bool CloneNewInstances => true;

		public EnemyBannerItem(string itemName, int placeStyle, int? killsToBanner = null) {
			this.itemName = itemName;
			this.placeStyle = placeStyle;
			this.killsToBanner = killsToBanner;
		}

		public override void SetStaticDefaults() {
			if (killsToBanner.HasValue) {
				ItemID.Sets.KillsToBanner[Type] = killsToBanner.Value;
			}
		}

		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<EnemyBanner>(), placeStyle);
			Item.width = 10;
			Item.height = 24;
			Item.SetShopValues(ItemRarityColor.Blue1, Item.buyPrice(silver: 10));
		}
	}

	/* If you would rather make each banner item individually, this is what they would look like:
	public class ExampleCustomAISlimeNPCBanner : ModItem
	{
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<EnemyBanner>(), EnemyBanner.StyleIDs.ExampleCustomAISlimeNPC);
			Item.width = 10;
			Item.height = 24;
			Item.SetShopValues(ItemRarityColor.Blue1, Item.buyPrice(silver: 10));
		}
	}
	*/
}