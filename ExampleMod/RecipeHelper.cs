using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod
{
	public static class RecipeHelper
	{
		public static void AddBossRecipes(Mod mod)
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "BossItem", 10);
			recipe.AddTile(null, "ExampleWorkbench");
			recipe.SetResult(ItemID.SuspiciousLookingEye, 20);
			recipe.AddRecipe();
			recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "BossItem", 10);
			recipe.AddTile(null, "ExampleWorkbench");
			recipe.SetResult(ItemID.WormFood, 20);
			recipe.AddRecipe();
			recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "BossItem", 10);
			recipe.AddTile(null, "ExampleWorkbench");
			recipe.SetResult(ItemID.BloodySpine, 20);
			recipe.AddRecipe();
			recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "BossItem", 10);
			recipe.AddTile(null, "ExampleWorkbench");
			recipe.SetResult(ItemID.Abeemination, 20);
			recipe.AddRecipe();
			recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "BossItem", 10);
			recipe.AddTile(null, "ExampleWorkbench");
			recipe.SetResult(ItemID.GuideVoodooDoll);
			recipe.AddRecipe();
			recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "BossItem", 10);
			recipe.AddTile(null, "ExampleWorkbench");
			recipe.SetResult(ItemID.MechanicalEye, 20);
			recipe.AddRecipe();
			recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "BossItem", 10);
			recipe.AddTile(null, "ExampleWorkbench");
			recipe.SetResult(ItemID.MechanicalWorm, 20);
			recipe.AddRecipe();
			recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "BossItem", 10);
			recipe.AddTile(null, "ExampleWorkbench");
			recipe.SetResult(ItemID.MechanicalSkull, 20);
			recipe.AddRecipe();
			// Here we see another way to retrieve type ids from classnames. Useful for those who program in an IDE who wish to avoid spelling mistakes.
			recipe = new ModRecipe(mod);
			recipe.AddIngredient(mod.ItemType<Items.BossItem>(), 10);
			recipe.AddTile(mod.TileType<Tiles.ExampleWorkbench>());
			recipe.SetResult(ItemID.LihzahrdPowerCell, 20);
			recipe.AddRecipe();
		}

		public static void TestRecipeEditor(Mod mod)
		{
			RecipeFinder finder = new RecipeFinder();
			finder.AddIngredient(ItemID.Chain);
			foreach (Recipe recipe in finder.SearchRecipes())
			{
				RecipeEditor editor = new RecipeEditor(recipe);
				editor.DeleteIngredient(ItemID.Chain);
			}

			finder = new RecipeFinder();
			finder.AddRecipeGroup("IronBar");
			finder.AddTile(TileID.Anvils);
			finder.SetResult(ItemID.Chain, 10);
			Recipe recipe2 = finder.FindExactRecipe();
			if (recipe2 != null)
			{
				RecipeEditor editor = new RecipeEditor(recipe2);
				editor.DeleteRecipe();
			}
		}
	}
}