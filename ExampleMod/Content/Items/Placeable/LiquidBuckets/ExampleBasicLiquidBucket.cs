using ExampleMod.Content.Liquids;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Placeable.LiquidBuckets
{
	public class ExampleBasicLiquidBucket : ModItem
	{
		public override void SetStaticDefaults() {
			ItemID.Sets.IsLavaImmuneRegardlessOfRarity[Type] = true;
			ItemID.Sets.AlsoABuildingItem[Type] = true;
			ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<ExampleBottomlessBucket>();
			ItemID.Sets.DuplicationMenuToolsFilter[Type] = true;

			LiquidID.Sets.CreateLiquidBucketItem[ModContent.LiquidType<ExampleLiquid>()] = Type;

			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 5;
		}

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

		//It's recommended that you look at ExampleLiquidBucket to see how buckets work
		public override void HoldItem(Player player) {
			ExampleLiquidBucket.PlaceLiquid(player, Item, ModContent.LiquidType<ExampleBasicLiquid>());
		}
	}
}
