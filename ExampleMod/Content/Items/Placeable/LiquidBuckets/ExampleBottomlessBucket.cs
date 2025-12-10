using ExampleMod.Content.Liquids;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Placeable.LiquidBuckets
{
	//This is an example of a bottomless liquid bucket
	//I would advise that modders look at ExampleLiquidBucket for more explaination as well as it's PlaceLiquid method
	public class ExampleBottomlessBucket : ModItem
	{
		public override void SetStaticDefaults() {
			ItemID.Sets.AlsoABuildingItem[Type] = true;
			ItemID.Sets.DuplicationMenuToolsFilter[Type] = true;
			ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<ExampleLiquidSponge>();

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

		public override void HoldItem(Player player) {
			ExampleLiquidBucket.PlaceLiquid(player, Item, ModContent.LiquidType<ExampleLiquid>(), true);
		}
	}
}
