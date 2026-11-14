using ExampleMod.Content.Liquids;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Placeable.LiquidBuckets
{
	// This is an example of a modded bucket
	// Here we do some extra logic to make our bucket dispense liquid
	// While also using an ID Set to allow buckets to create this item
	public class ExampleLiquidBucket : ModItem
	{
		// The SetStaticDefaults of a bucket
		public override void SetStaticDefaults() {
			ItemID.Sets.IsLavaImmuneRegardlessOfRarity[Type] = true;
			ItemID.Sets.AlsoABuildingItem[Type] = true; // Unused ID Set, but may be useful for other modders or when the game updates
			ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<ExampleBottomlessBucket>();
			ItemID.Sets.DuplicationMenuToolsFilter[Type] = true;

			// This sets what type of liquid the bucket will create when placed.
			ItemID.Sets.LiquidBucketPlaceAsID[Type] = ModContent.LiquidType<ExampleLiquid>();

			// Here is how we make buckets create this item.
			// Unlike Example Bottomless bucket, we have A->B and B->A transforms so we need to set both sides
			// Using the CreateLiquidBucketItem ID Set, we are able to set what a liquid replaces the empty bucket with on interaction
			// NOTE: a liquid can only create 1 bucket item at a time.
			LiquidID.Sets.CreateLiquidBucketItem[ModContent.LiquidType<ExampleLiquid>()] = Type;

			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 5;
		}

		// The normal SetDefaults for a bucket
		public override void SetDefaults() {
			Item.width = 20;
			Item.height = 24;
			Item.maxStack = Item.CommonMaxStack;
			Item.useTurn = true;
			Item.autoReuse = true;
			Item.useAnimation = 15;
			Item.useTime = 10;
			Item.useStyle = ItemUseStyleID.Swing;
		}
	}
}
