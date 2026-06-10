using ExampleMod.Content.Items.Placeable.LiquidBuckets;
using ExampleMod.Content.Liquids;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Accessories
{
	//An example of a custom water walking shoe accessory for the example liquids
	//The accessory lets the player only stand on just Example Liquids and no other liquids
	[AutoloadEquip([EquipType.Shoes])]
	public class ExampleWaders : ModItem
	{
		public override void SetStaticDefaults() {
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults() {
			Item.width = 16;
			Item.height = 24;
			Item.accessory = true;
			Item.rare = ItemRarityID.LightRed;
			Item.value = Item.buyPrice(0, 2);
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.GetModPlayer<ExampleWadersPlayer>().exampleWalk = true;
		}

		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<ExampleLiquidBucket>(), 5);
			recipe.AddIngredient(ItemID.WaterWalkingBoots);
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.Register();
		}
	}

	public class ExampleWadersPlayer : ModPlayer
	{
		public bool exampleWalk;

		public override void ResetEffects() {
			exampleWalk = false;
		}

		//PostUpdateRunSpeeds is called directly before water walking behaviour is executed
		//We use this hook to set what liquids can be walked on, as the defaults are set right before
		public override void PostUpdateRunSpeeds() {
			//Like waterWalk and waterWalk2, reversing gravity prevents the player from being able to walk on liquids
			if (Player.gravDir == -1f) {
				exampleWalk = false;
			}

			//canLiquidBeWalkedOn is an array of which liquids can be walked on, this is to track whether a liquid the player is above will support them or let them fall

			//It's important to know the defaults of this array, as it will help us to know how to set it to a value

			//All elements are True when Player.waterWalk is also True
			//All elements except LiquidID.Lava are True when Player.waterWalk2 is also True

			//Due to the defaults automatically setting our liquid to be walkable when having any waterwalking boot/buff enabled,
			//we want to make sure we set the array element to false when our accessory isn't active.

			//Here, we use "exampleWalk" to set the ability to walk on ExampleLiquid to True and False whenever our accessory is active or not.
			Player.canLiquidBeWalkedOn[ModContent.LiquidType<ExampleLiquid>()] = exampleWalk;
			Player.canLiquidBeWalkedOn[ModContent.LiquidType<ExampleBasicLiquid>()] = exampleWalk;
			Player.canLiquidBeWalkedOn[ModContent.LiquidType<ExampleComplexLiquid>()] = exampleWalk;

			//If we want to just add make our accessory allow for walking on example liquid but also allow for Water walking boots to also walk on our liquid, we use:

			//if (exampleWalk)
			//{
			//	Player.GetModPlayer<ModLiquidPlayer>().canLiquidBeWalkedOn[LiquidLoader.LiquidType<ExampleLiquid>()] = true;
			//}

			//...we do this because, whenever our accessory is active, we allow for the walking on example liquid, otherwise the normal setting of the array element is kept.
		}
	}

}
