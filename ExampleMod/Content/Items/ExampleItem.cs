using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;

namespace ExampleMod.Content.Items
{
	public class ExampleItem : ModItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("This is a modded item."); //The (English) text shown below your item's name
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[item.type] = 100; // How many items are needed in order to research duplication of this item in Journey mode. See https://terraria.gamepedia.com/Journey_Mode/Research_list for a list of commonly used research amounts depending on item.
		}

		public override void SetDefaults() {
			item.width = 20; //The item texture's width
			item.height = 20; //The item texture's height

			item.maxStack = 999; //The item's max stack value
			item.value = Item.buyPrice(silver: 1); //The value of the item in copper coins. Item.buyPrice & Item.sellPrice are helper methods that returns costs in copper coins based on platinum/gold/silver/copper arguments provided to it.
			item.rare = ItemRarityID.Blue; // The rarity of the weapon.
		}

		public override void AddRecipes() {
			//This creates a new ModRecipe, associated with the mod that this content piece comes from.
			CreateRecipe(999)
				//This adds a requirement of 10 dirt blocks to the recipe.
				.AddIngredient(ItemID.DirtBlock, 10)
				//When you're done, call this to register the recipe.
				.Register();
		}
	}
}
