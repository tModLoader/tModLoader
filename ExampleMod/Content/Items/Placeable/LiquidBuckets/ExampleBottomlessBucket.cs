using ExampleMod.Content.Liquids;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Placeable.LiquidBuckets
{
	// This is an example of a bottomless liquid bucket
	// Look at ExampleLiquidBucket for more explaination as well as it's PlaceLiquid method
	public class ExampleBottomlessBucket : ModItem
	{
		public override void SetStaticDefaults() {
			ItemID.Sets.AlsoABuildingItem[Type] = true;
			ItemID.Sets.DuplicationMenuToolsFilter[Type] = true;
			ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<ExampleLiquidSponge>();

			// Unlike ExampleLiquid Bucket, we can't create this item by using an empty bucket on ExampleLiquid. This is A->B only.
			ItemID.Sets.LiquidBucketPlaceAsID[Type] = ModContent.LiquidType<ExampleLiquid>();
			ItemID.Sets.IsBottomlessBucket[Type] = true;

			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults() {
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTurn = true;
			Item.useAnimation = 12;
			Item.useTime = 5;
			Item.width = 20;
			Item.height = 20;
			Item.autoReuse = true;
			Item.rare = ItemRarityID.Lime;
			Item.value = Item.sellPrice(0, 10);
			Item.tileBoost += 2;
		}
	}
}
