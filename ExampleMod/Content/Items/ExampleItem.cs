using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items
{
	public class ExampleItem : ModItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("This is a modded item."); //The (English) text shown below your weapon's name
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
