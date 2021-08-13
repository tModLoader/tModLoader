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
		// Here we've made a helper method we can use to shorten our code.
		// This is because many of our recipes follow the same terminology: one ingredient, one result, one possible required tile
		private static void MakeSimpleRecipe(Mod mod, string modIngredient, short resultType, int ingredientStack = 1, int resultStack = 1, string reqTile = null)
			// notice the last parameters can be made optional by specifying a default value
		{
			Recipe recipe = mod.CreateRecipe(resultType, resultStack); // make a new recipe for our mod with the specified type and with the specified stack.
			recipe.AddIngredient(null, modIngredient, ingredientStack); // add the ingredient, passing null for the mod means it will use our mod, we could also pass mod from the arguments
			if (reqTile != null) {
				// when a required tile is specified
				recipe.AddTile(null, reqTile); // we add it 
			}

			recipe.Register(); // finally, add the recipe
		}

		// Add recipes
		public static void AddExampleRecipes(Mod mod) {
			// ExampleItem crafts into the following items
			// Check the method signature of MakeSimpleRecipes for the arguments, this is a method signature:
			// private static void MakeSimpleRecipe(Mod mod, string modIngredient, short resultType, int ingredientStack = 1, int resultStack = 1, string reqTile = null) 

			MakeSimpleRecipe(mod, "ExampleItem", ItemID.Silk, 999);
			MakeSimpleRecipe(mod, "ExampleItem", ItemID.IronOre, 999);
			MakeSimpleRecipe(mod, "ExampleItem", ItemID.GravitationPotion, 20);
			MakeSimpleRecipe(mod, "ExampleItem", ItemID.GoldChest); // notice how we can omit the stack, it has a default value
			MakeSimpleRecipe(mod, "ExampleItem", ItemID.MusicBoxDungeon);

			// Instead of having to call AddBossRecipes from our main file, we can also call it here, as a result the method can remain private
			AddBossRecipes(mod);
		}

		// Add boss related recipes
		private static void AddBossRecipes(Mod mod) {
			// BossItem crafts into the following items
			// We are using the same helper method here, and we are making use of the reqTile parameter
			MakeSimpleRecipe(mod, "BossItem", ItemID.SuspiciousLookingEye, 10, 20, "ExampleWorkbench");
			MakeSimpleRecipe(mod, "BossItem", ItemID.BloodySpine, 10, 20, "ExampleWorkbench");
			MakeSimpleRecipe(mod, "BossItem", ItemID.Abeemination, 10, 20, "ExampleWorkbench");
			// notice how we can skip optional parameters by specifying the target parameter with 'reqTile:', this means the resultStack will remain 1
			MakeSimpleRecipe(mod, "BossItem", ItemID.GuideVoodooDoll, 10, reqTile: "ExampleWorkbench");
			MakeSimpleRecipe(mod, "BossItem", ItemID.MechanicalEye, 10, 20, "ExampleWorkbench");
			MakeSimpleRecipe(mod, "BossItem", ItemID.MechanicalWorm, 10, 20, "ExampleWorkbench");
			MakeSimpleRecipe(mod, "BossItem", ItemID.MechanicalSkull, 10, 20, "ExampleWorkbench");
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
				RecipeGroupID.IronBar // add a new recipe group, in this case the vanilla one for iron or lead bars.
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