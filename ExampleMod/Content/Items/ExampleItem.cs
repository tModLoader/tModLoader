using ExampleMod.Content.Items.Weapons;
using ExampleMod.Content.Tiles.Furniture;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
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
			item.value = Item.buyPrice(silver: 1); //The value of the item in copper coins.
			item.rare = ItemRarityID.Blue; //The rarity of the weapon.
		}

		public override void AddRecipes() {
			
			
			
			
			
			CreateRecipe(100)
				.AddIngredient(ItemID.DirtBlock, 10)
				.AddCondition(Recipe.Condition.TimeDay)
				.AddCondition(Recipe.Condition.InCrimson)
				.Register();
			
			
			
			
			
			
			
			
			
			
			
			//////////////////////////////////////////////////////////////////////////
			//The following basic recipe makes 100 ExampleItems out of 1 dirt block.//
			//////////////////////////////////////////////////////////////////////////

			//This creates a new ModRecipe, associated with the mod that this content piece comes from.
			var recipe = CreateRecipe(999);
			//This adds a requirement of 1 dirt block to the recipe.
			recipe.AddIngredient(ItemID.DirtBlock);
			//When you're done, call this to register the recipe.
			recipe.Register();

			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			//The following recipe showcases and explains all methods (functions) present on ModRecipe, and uses an 'advanced' style called 'chaining'.//
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

			//The reason why the said chaining works is that all methods on ModRecipe, with the exception of Register(), return its own instance,
			//which lets you call subsequent methods on that return value, without having to type a local variable's name.
			//When using chaining, note that only the last line is supposed to have a semicolon (;).

			//Start a new Recipe. (Prepend with "ModRecipe " if 1st recipe in code block.)
			CreateRecipe()
				//Adds a Vanilla Ingredient. 
				//Look up ItemIDs: https://github.com/tModLoader/tModLoader/wiki/Vanilla-Item-IDs
				//To specify more than one ingredient, use multiple recipe.AddIngredient() calls.
				.AddIngredient(ItemID.DirtBlock)
				//An optional 2nd argument will specify a stack of the item. 
				.AddIngredient(ItemID.Acorn, 10)
				//We can also specify the current item as an ingredient
				.AddIngredient(this, 2)
				//Adds a Mod Ingredient. Do not attempt ItemID.EquipMaterial, it's not how it works.
				.AddIngredient<ExampleSword>(3)
				//An alternate string-based approach to the above. Try to only use it for other mods' items, because it's slower. 
				.AddIngredient(mod, "ExampleSword", 3)

				//RecipeGroups allow you create a recipe that accepts items from a group of similar ingredients. For example, all varieties of Wood are in the vanilla "Wood" Group
				//Check here for other vanilla groups: https://github.com/tModLoader/tModLoader/wiki/Intermediate-Recipes#using-existing-recipegroups
				.AddRecipeGroup("Wood")
				//Here is using a mod recipe group. Check out ExampleMod.AddRecipeGroups() to see how to register a recipe group.
				.AddRecipeGroup("ExampleMod:ExampleItem", 2)

				//Adds a vanilla tile requirement.
				//To specify a crafting station, specify a tile. Look up TileIDs: https://github.com/tModLoader/tModLoader/wiki/Vanilla-Tile-IDs
				.AddTile(TileID.WorkBenches)
				//Adds a mod tile requirement. To specify more than one crafting station, use multiple recipe.AddTile() calls.
				.AddTile<ExampleWorkbench>()
				//An alternate string-based approach to the above. Try to only use it for other mods' tiles, because it's slower.
				.AddTile(mod, "ExampleWorkbench")

				//Adds a custom condition, that it must be night for the recipe to work. This uses a lambda expression to create a delegate, you can learn more about both through Google.
				.AddCondition(NetworkText.FromLiteral("Night Time"), r => !Main.dayTime)

				// Marks the recipe as an alchemy recipe. This makes it require an alchemy table, and gives a 1/3 chance for each ingredient to not be consumed. See https://terraria.gamepedia.com/Alchemy_Table.
				.IsAlchemy()

				//When you're done, call this to register the recipe. Note that there's a semicolon at the end of the chain.
				.Register();
		}
	}
}