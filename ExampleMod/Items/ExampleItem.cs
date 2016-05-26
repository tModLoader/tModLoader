using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Items
{
	public class ExampleItem : ModItem
	{
		public override void SetDefaults()
		{
			item.name = "Example Item";
			item.width = 20;
			item.height = 20;
			item.maxStack = 999;
			AddTooltip("This is a modded item.");
			item.value = 100;
			item.rare = 1;
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.DirtBlock);
			recipe.SetResult(this, 999);
			recipe.AddRecipe();
			recipe = new ModRecipe(mod);
			recipe.AddRecipeGroup("ExampleMod:ExampleItem");
			recipe.SetResult(this, 999);
			recipe.AddRecipe();
		}
	}
}