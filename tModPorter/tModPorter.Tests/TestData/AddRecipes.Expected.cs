using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader; 

public class ModItemAddRecipes : ModItem
{
	public override void AddRecipes() {
		Recipe recipe = CreateRecipe();
		recipe.AddIngredient(ItemID.Wood, 10);
		recipe.AddTile(TileID.WorkBenches);
		recipe.Register();

		recipe = CreateRecipe(10);
		recipe.AddIngredient(ItemID.StoneBlock);
		recipe.Register();
	}
	
	public void WithComments() {
		Recipe recipe = CreateRecipe(); // a// d
		// b
		recipe.AddIngredient(ItemID.Wood, 10);
		/* c */ recipe.AddTile(TileID.WorkBenches);
		recipe.Register(); /* e */

		recipe = CreateRecipe(); // f// g
	}

	public void WithLoop() {
		var ingreds = new[] { ItemID.BluePhasesaber, ItemID.RedPhasesaber, ItemID.GreenPhasesaber, ItemID.PurplePhasesaber, ItemID.WhitePhasesaber, ItemID.YellowPhasesaber };

		var recipe = CreateRecipe(10);
		foreach (var ingred in ingreds) {
			recipe.AddIngredient(ingred);
		}
		recipe.Register();

		// now with multiple recipes
		foreach (var ingred in ingreds) {
			recipe = CreateRecipe(1);
			recipe.AddIngredient(ingred);
			recipe.Register();
		}
	}

	public void WithConditional(bool setting) {
		var recipe = CreateRecipe();
		if (setting) {
			recipe.AddIngredient(ItemID.StoneBlock);
		}
		recipe.Register();
	}

	public void UnableToFindAssignment(bool setting) {
#if COMPILE_ERROR
		var recipe = Mod.CreateRecipe();
		if (setting) {
			recipe = Mod.CreateRecipe();
			recipe.AddIngredient(ItemID.StoneBlock);
		}
		recipe.SetResult(this);/* tModPorter Pass result to CreateRecipe. */
		recipe.Register();
#endif
	}
}

public class ModAddRecipes : Mod
{
	public override void AddRecipes() {
		var recipe = CreateRecipe(ModContent.ItemType<ModItemAddRecipes>());
		recipe.AddIngredient(ItemID.Wood, 10);
		recipe.AddTile(TileID.WorkBenches);
		recipe.Register();
	}

	public void AddRecipes(Mod mod, ModItem item) {
		var recipe = mod.CreateRecipe(item.Type, 5);
		recipe.AddIngredient(ItemID.Wood, 10);
		recipe.AddTile(TileID.WorkBenches);
		recipe.Register();
	}

#if COMPILE_ERROR
	public Recipe MakeRecipe() => CreateRecipe();

	public void AddRecipeViaAnotherCall(ModItem item) {
		var recipe = MakeRecipe(item.Type);
		recipe.AddIngredient(ItemID.Wood, 10);
		recipe.Register();

		recipe = MakeRecipe(item.Type, 2);
		recipe.Register();
	}
#endif

	public Action InLambda() => () => {
		var recipe = CreateRecipe(ModContent.ItemType<ModItemAddRecipes>());
		recipe.AddIngredient(ItemID.Wood, 10);
		recipe.AddTile(TileID.WorkBenches);
		recipe.Register();
	};

	public Action InDelegate() => delegate() {
		var recipe = CreateRecipe(ModContent.ItemType<ModItemAddRecipes>());
		recipe.AddIngredient(ItemID.Wood, 10);
		recipe.AddTile(TileID.WorkBenches);
		recipe.Register();
	};
}