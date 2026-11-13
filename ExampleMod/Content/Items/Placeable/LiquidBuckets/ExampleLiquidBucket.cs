using ExampleMod.Content.Liquids;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Placeable.LiquidBuckets
{
	//This is an example of a modded bucket
	//Here we do some extra logic to make our bucket dispense liquid
	//While also using an ID Set to allow buckets to create this item
	public class ExampleLiquidBucket : ModItem
	{
		//The SetStaticDefaults of a bucket
		public override void SetStaticDefaults() {
			ItemID.Sets.IsLavaImmuneRegardlessOfRarity[Type] = true;
			ItemID.Sets.AlsoABuildingItem[Type] = true; //Unused ID Set, but may be useful for other modders or when the game updates
			ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<ExampleBottomlessBucket>();
			ItemID.Sets.DuplicationMenuToolsFilter[Type] = true;

			//Here is how we make buckets create this item
			//Using the CreateLiquidBucketItem ID Set, we are able to set which liquid creates an item as well as which item it creates
			//NOTE: a liquid can only create 1 bucket item at a time
			LiquidID.Sets.CreateLiquidBucketItem[ModContent.LiquidType<ExampleLiquid>()] = Type;

			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 5;
		}

		//The normal SetDefaults for a bucket
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

		//We use HoldItem for bucket logic, as it's the hook ran just before the bucket logic is done.
		//This is so no other extra item logic is run inbetween this hook and when we would normally have bucket logic called
		public override void HoldItem(Player player) {
			LiquidLoader.PlaceLiquidViaBucket(player, Item, ModContent.LiquidType<ExampleLiquid>());
		}
	}
}
