using ExampleMod.Content.Items;
using ExampleMod.Content.Items.Consumables;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Content
{
	public static class ExampleRecipes
	{
		public static RecipeGroup ExampleRecipeGroup;

		public static void AddRecipeGroups() {
			// Store this recipe group in a variable so we can use it later
			ExampleRecipeGroup = new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemType<ExampleItem>())}", ItemType<ExampleItem>());
			RecipeGroup.RegisterGroup("ExampleMod:ExampleItem", ExampleRecipeGroup);
		}

		public static void Load(Mod mod) {
			AddRecipeGroups();

			// Create a recipe that creates one Example Healing Potion
			mod.CreateRecipe(ItemType<ExampleHealingPotion>())
				// Adds a custom condition, that the player must be at <1/2 health for the recipe to work.
				// The first argument is a NetworkText instance, i.e. localized text. The key used here is defined in 'Localization/*.lang' files.
				// The second argument uses a lambda expression to create a delegate, you can learn more about both in Google.
				.AddCondition(NetworkText.FromKey("RecipeConditions.LowHealth"), r => Main.LocalPlayer.statLife < Main.LocalPlayer.statLifeMax / 2)
				// When you're done, call this to register the recipe. Note that there's a semicolon at the end of the chain.
				.Register();

			// Create a recipe that creates one Battle Potion
			mod.CreateRecipe(ItemID.BattlePotion)
				// Make this recipe require a Bottle
				.AddIngredient(ItemID.Bottle)
				// Make this recipe require a 5 Fruit of any type
				.AddRecipeGroup(RecipeGroupID.Fruit, 5)
				// This example shows how to add multiple conditions. This recipe will be available when all conditions are met.
				// Recipe.Condition contains common conditions - biomes, near liquids, time
				.AddCondition(Recipe.Condition.InDesert, Recipe.Condition.NearWater, Recipe.Condition.TimeNight)
				.Register();

			// todo: add more examples (string based, from other mods)


			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			//The following recipe showcases and explains all methods (functions) present on ModRecipe, and uses an 'advanced' style called 'chaining'.//
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

			//The reason why the said chaining works is that all methods on ModRecipe, with the exception of Register(), return its own instance,
			//which lets you call subsequent methods on that return value, without having to type a local variable's name.
			//When using chaining, note that only the last line is supposed to have a semicolon (;).

			//Start a new Recipe. (Prepend with "ModRecipe " if 1st recipe in code block.)
			// CreateRecipe()
			// 	//Adds a Vanilla Ingredient. 
			// 	//Look up ItemIDs: https://github.com/tModLoader/tModLoader/wiki/Vanilla-Item-IDs
			// 	//To specify more than one ingredient type, use multiple recipe.AddIngredient() calls.
			// 	.AddIngredient(ItemID.DirtBlock)
			// 	//An optional 2nd argument will specify a stack of the item. Any calls to any AddIngredient overload without a stack value at the end will have the stack default to 1. 
			// 	.AddIngredient(ItemID.Acorn, 10)
			// 	//We can also specify the current item as an ingredient
			// 	.AddIngredient(this)
			// 	//Adds a Mod Ingredient. Do not attempt ItemID.EquipMaterial, it's not how it works.
			// 	.AddIngredient<ExampleSword>()
			// 	//An alternate string-based approach to the above. Try to only use it for other mods' items, because it's slower. 
			// 	.AddIngredient(mod, "ExampleSword")
			//
			// 	//RecipeGroups allow you create a recipe that accepts items from a group of similar ingredients. For example, all varieties of Wood are in the vanilla "Wood" Group
			// 	//Check here for other vanilla groups: https://github.com/tModLoader/tModLoader/wiki/Intermediate-Recipes#using-existing-recipegroups
			// 	.AddRecipeGroup("Wood")
			// 	//Just like with AddIngredient, there's a stack parameter with a default value of 1.
			// 	.AddRecipeGroup("IronBar", 2)
			// 	//Here is using a mod recipe group. Check out ExampleMod.AddRecipeGroups() to see how to register a recipe group.
			// 	.AddRecipeGroup("ExampleMod:ExampleItem", 2)
			//
			// 	//Adds a vanilla tile requirement.
			// 	//To specify a crafting station, specify a tile. Look up TileIDs: https://github.com/tModLoader/tModLoader/wiki/Vanilla-Tile-IDs
			// 	.AddTile(TileID.WorkBenches)
			// 	//Adds a mod tile requirement. To specify more than one crafting station, use multiple recipe.AddTile() calls.
			// 	.AddTile<ExampleWorkbench>()
			// 	//An alternate string-based approach to the above. Try to only use it for other mods' tiles, because it's slower.
			// 	.AddTile(mod, "ExampleWorkbench")
			//
			// 	//Adds pre-defined conditions. These 3 lines combine to make so that the recipe must be crafted in desert waters at night.
			// 	.AddCondition(Recipe.Condition.InDesert)
			// 	.AddCondition(Recipe.Condition.NearWater)
			// 	.AddCondition(Recipe.Condition.TimeNight)
			// 	//Adds a custom condition, that the player must be at <1/2 health for the recipe to work.
			// 	//The first argument is a NetworkText instance, i.e. localized text. The key used here is defined in 'Localization/*.lang' files.
			// 	//The second argument uses a lambda expression to create a delegate, you can learn more about both in Google.
			// 	.AddCondition(NetworkText.FromKey("RecipeConditions.LowHealth"), r => Main.LocalPlayer.statLife < Main.LocalPlayer.statLifeMax / 2)
			//
			// 	//When you're done, call this to register the recipe. Note that there's a semicolon at the end of the chain.
			// 	.Register();
		}

		public static void Unload() {
			ExampleRecipeGroup = null;
		}
	}
}