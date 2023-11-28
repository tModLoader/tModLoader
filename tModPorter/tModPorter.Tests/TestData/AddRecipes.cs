using System;
using Terraria.ID;
using Terraria.ModLoader; 

public class ModItemAddRecipes : ModItem
{
	public override void AddRecipes() {
		ModRecipe recipe = new ModRecipe(mod);
		recipe.AddIngredient(ItemID.Wood, 10);
		recipe.AddTile(TileID.WorkBenches);
		recipe.SetResult(this);
		recipe.AddRecipe();

		recipe = new ModRecipe(mod);
		recipe.AddIngredient(ItemID.StoneBlock);
		recipe.SetResult(this, 10);
		recipe.AddRecipe();
	}
	
	public void WithComments() {
		ModRecipe recipe = new ModRecipe(mod); // a
		// b
		recipe.AddIngredient(ItemID.Wood, 10);
		/* c */ recipe.AddTile(TileID.WorkBenches);
		recipe.SetResult(this); // d
		recipe.AddRecipe(); /* e */

		recipe = new ModRecipe(mod); // f
		recipe.SetResult(this); // g
	}

	public void WithLoop() {
		var ingreds = new[] { ItemID.BluePhasesaber, ItemID.RedPhasesaber, ItemID.GreenPhasesaber, ItemID.PurplePhasesaber, ItemID.WhitePhasesaber, ItemID.YellowPhasesaber };

		var recipe = new ModRecipe(mod);
		foreach (var ingred in ingreds) {
			recipe.AddIngredient(ingred);
		}
		recipe.SetResult(this, 10);
		recipe.AddRecipe();

		// now with multiple recipes
		foreach (var ingred in ingreds) {
			recipe = new ModRecipe(mod);
			recipe.AddIngredient(ingred);
			recipe.SetResult(this, 1);
			recipe.AddRecipe();
		}
	}

	public void WithConditional(bool setting) {
		var recipe = new ModRecipe(mod);
		if (setting) {
			recipe.AddIngredient(ItemID.StoneBlock);
		}
		recipe.SetResult(this);
		recipe.AddRecipe();
	}

	public void UnableToFindAssignment(bool setting) {
		var recipe = new ModRecipe(mod);
		if (setting) {
			recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.StoneBlock);
		}
		recipe.SetResult(this);
		recipe.AddRecipe();
	}
}

public class ModAddRecipes : Mod
{
	public override void AddRecipes() {
		var recipe = new ModRecipe(this);
		recipe.AddIngredient(ItemID.Wood, 10);
		recipe.AddTile(TileID.WorkBenches);
		recipe.SetResult(ModContent.ItemType<ModItemAddRecipes>());
		recipe.AddRecipe();
	}

	public void AddRecipes(Mod mod, ModItem item) {
		var recipe = new ModRecipe(mod);
		recipe.AddIngredient(ItemID.Wood, 10);
		recipe.AddTile(TileID.WorkBenches);
		recipe.SetResult(item, 5);
		recipe.AddRecipe();
	}

	public ModRecipe MakeRecipe() => new ModRecipe(this);

	public void AddRecipeViaAnotherCall(ModItem item) {
		var recipe = MakeRecipe();
		recipe.AddIngredient(ItemID.Wood, 10);
		recipe.SetResult(item);
		recipe.AddRecipe();

		recipe = MakeRecipe();
		recipe.SetResult(item, 2);
		recipe.AddRecipe();
	}

	public Action InLambda() => () => {
		var recipe = new ModRecipe(this);
		recipe.AddIngredient(ItemID.Wood, 10);
		recipe.AddTile(TileID.WorkBenches);
		recipe.SetResult(ModContent.ItemType<ModItemAddRecipes>());
		recipe.AddRecipe();
	};

	public Action InDelegate() => delegate() {
		var recipe = new ModRecipe(this);
		recipe.AddIngredient(ItemID.Wood, 10);
		recipe.AddTile(TileID.WorkBenches);
		recipe.SetResult(ModContent.ItemType<ModItemAddRecipes>());
		recipe.AddRecipe();
	};

	public void PortModCreateRecipe(ModItem modItem) {
		var recipe = CreateRecipe(ModContent.ItemType<ModItemAddRecipes>());
		recipe.Register();

		recipe = modItem.Mod.CreateRecipe(modItem.Type);
		recipe.Register();
	}

	public Mod GetMod() => this;

	public void GetModMayHaveSideEffects() {
		var recipe = new ModRecipe(GetMod());
		recipe.AddIngredient(ItemID.Wood, 10);
		recipe.AddTile(TileID.WorkBenches);
		recipe.SetResult(ModContent.ItemType<ModItemAddRecipes>());
		recipe.AddRecipe();

		recipe = GetMod().CreateRecipe(ModContent.ItemType<ModItemAddRecipes>());
		recipe.AddIngredient(ItemID.Wood, 10);
		recipe.AddTile(TileID.WorkBenches);
		recipe.AddRecipe();
	}

	public void NewRecipeMethodConditionRefactors(Recipe recipe) {
		recipe.AddCondition(Recipe.Condition.TimeDay);
		recipe.AddCondition(Recipe.Condition.InGraveyardBiome);
	}
}