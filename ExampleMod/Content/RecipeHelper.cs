using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content
{
	// In this class we separate recipe related code from our main class
	public static class RecipeHelper
	{
		public static void Load(Mod mod) {
			AddExampleRecipes(mod);
			ExampleRecipeEditing(mod);
		}

		// Here we've made a helper method we can use to shorten our code.
		// This is because many of our recipes follow the same terminology: one ingredient, one result, one possible required tile
		// notice the last parameters can be made optional by specifying a default value
		private static void MakeSimpleRecipe<TModItem>(Mod mod, short resultType, int ingredientStack = 1, int resultStack = 1)
			where TModItem : ModItem {
			mod.CreateRecipe(resultType, resultStack) // make a new recipe for our mod with the specified type and with the specified stack.
				.AddIngredient<TModItem>(ingredientStack) // add the ingredient
				.Register(); // finally, add the recipe
		}

		private static void MakeSimpleRecipe<TModItem, TModTile>(Mod mod, short resultType, int ingredientStack = 1, int resultStack = 1)
			where TModItem : ModItem
			where TModTile : ModTile {
			mod.CreateRecipe(resultType, resultStack) // make a new recipe for our mod with the specified type and with the specified stack.
				.AddIngredient<TModItem>(ingredientStack) // add the ingredient
				.AddTile<TModTile>() // add the tile
				.Register(); // finally, add the recipe
		}

		// Add recipes
		public static void AddExampleRecipes(Mod mod) {
			// ExampleItem crafts into the following items
			// Check the method signature of MakeSimpleRecipes for the arguments, this is a method signature:
			// private static void MakeSimpleRecipe<TModItem>(Mod mod, short resultType, int ingredientStack = 1, int resultStack = 1)
			//	 where TModItem : ModItem
			// and
			// private static void MakeSimpleRecipe<TModItem, TModTile>(Mod mod, short resultType, int ingredientStack = 1, int resultStack = 1)
			//	 where TModItem : ModItem
			//	 where TModTile : ModTile

			MakeSimpleRecipe<Items.ExampleItem>(mod, ItemID.Silk, 999);
			MakeSimpleRecipe<Items.ExampleItem>(mod, ItemID.IronOre, 999);
			MakeSimpleRecipe<Items.ExampleItem>(mod, ItemID.GravitationPotion, 20);
			MakeSimpleRecipe<Items.ExampleItem>(mod, ItemID.GoldChest); // notice how we can omit the stack, it has a default value
			MakeSimpleRecipe<Items.ExampleItem>(mod, ItemID.MusicBoxDungeon);

			// Instead of having to call AddBossRecipes from our main file, we can also call it here, as a result the method can remain private
			AddBossRecipes(mod);
		}

		// Add boss related recipes
		private static void AddBossRecipes(Mod mod) {
			// BossItem crafts into the following items
			// We are using the same helper method here, and we are making use of the reqTile parameter
			MakeSimpleRecipe<Items.BossItem, Tiles.Furniture.ExampleWorkbench>(mod, ItemID.SuspiciousLookingEye, 10, 20);
			MakeSimpleRecipe<Items.BossItem, Tiles.Furniture.ExampleWorkbench>(mod, ItemID.BloodySpine, 10, 20);
			MakeSimpleRecipe<Items.BossItem, Tiles.Furniture.ExampleWorkbench>(mod, ItemID.Abeemination, 10, 20);
			// notice how we can omit optional parameters, this means the resultStack will remain 1
			MakeSimpleRecipe<Items.BossItem, Tiles.Furniture.ExampleWorkbench>(mod, ItemID.GuideVoodooDoll, 10);
			MakeSimpleRecipe<Items.BossItem, Tiles.Furniture.ExampleWorkbench>(mod, ItemID.MechanicalEye, 10, 20);
			MakeSimpleRecipe<Items.BossItem, Tiles.Furniture.ExampleWorkbench>(mod, ItemID.MechanicalWorm, 10, 20);
			MakeSimpleRecipe<Items.BossItem, Tiles.Furniture.ExampleWorkbench>(mod, ItemID.MechanicalSkull, 10, 20);
			// Here we see another way to retrieve type ids from classnames, using generic calls
			// This way you don't have to specify the mod, because you simply pass the ID of the item as you would for vanilla items.
			// Useful for those who program in an IDE who wish to avoid spelling mistakes.
			// What's also neat is that the references to classes can be automatically included in refactors, string literals cannot. (unless you have ReSharper)
			mod.CreateRecipe(ItemID.LihzahrdPowerCell, 20)
				.AddIngredient(ModContent.ItemType<Content.Items.BossItem>(), 10) // Items is our namespace (ExampleMod.Content.Items), BossItem our class
				.AddTile(ModContent.TileType<Content.Tiles.Furniture.ExampleWorkbench>()) // Tiles is our namespace (ExampleMod.Content.Furniture.Tiles), ExampleWorkbench our class
				.Register();
		}

		// Showcase RecipeFinder and RecipeEditor
		// With these classes, you can find and edit recipes
		public static void ExampleRecipeEditing(Mod mod) {
			// In the following example, we find recipes that uses a chain as ingredient and then we remove that ingredient from the recipe.
			List<(int type, int stack)> items = new() {
				(ItemID.Chain, 1) // Chain (with a stack of 1)
			};
			IEnumerable<Recipe> recipes = Main.recipe.Take(Recipe.numRecipes)
				.Where(recipe => items.All(item => recipe.requiredItem.Any(reqItem => reqItem.type == item.type && reqItem.stack >= item.stack)));

			// loop every recipe found by the finder
			foreach (Recipe r in recipes) {
				r.requiredItem.RemoveAll(req => req.type == ItemID.Chain); // for the currently looped recipe, delete the Chain ingredient.
			}

			// The following is a more precise example, finding an exact recipe and deleting it if possible.
			List<int> groups = new() {
				RecipeGroupID.IronBar, // add a new recipe group, in this case the vanilla one for iron or lead bars.
				//RecipeGroup.recipeGroupIDs["ExampleMod:ExampleItem"] // add a modded recipe group
			};
			List<int> tiles = new() {
				TileID.Anvils // add a required tile, any anvil
			};
			(int type, int stack) result = (ItemID.Chain, 10); // set the result to be 10 chains

			// try to find the exact recipe matching our criteria
			Recipe exactRecipe = Main.recipe.Take(Recipe.numRecipes)
				.Where(recipe => groups.All(recipe.HasRecipeGroup))
				.Where(recipe => tiles.All(recipe.HasTile))
				.Where(recipe => recipe.createItem.type == result.type && recipe.createItem.stack == result.stack)
				.FirstOrDefault();

			bool isRecipeFound = exactRecipe != null; // if our recipe is not null, it means we found the exact recipe
			if (isRecipeFound) // since our recipe is found, we can continue
			{
				exactRecipe.RemoveRecipe();
			}
		}
	}
}