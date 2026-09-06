using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Terraria.ID;

namespace Terraria.ModLoader;

[TestClass]
public class RecipeConsumeIngredientTests
{
	[ClassInitialize]
	public static void ClassInitialize(TestContext context)
	{
		Program.SavePath = ".";
	}

	private static Recipe MakeRecipe(int ingredientType, int stack)
	{
		var recipe = new Recipe();
		recipe.requiredItem.Add(new Item { type = ingredientType, stack = stack });
		recipe.requiredItemQuickLookup = new[] { new Recipe.RequiredItemEntry(ingredientType, stack) };
		return recipe;
	}

	[TestMethod]
	public void ConsumeIngredientCallbackAdjustsAmountConsumed()
	{
		Recipe recipe = MakeRecipe(ItemID.Chain, 1);
		recipe.AddConsumeIngredientCallback((Recipe r, int type, ref int amount, bool isDecrafting) => {
			if (type == ItemID.Chain)
				amount = 0;
		});

		var ingredients = new List<Recipe.RequiredItemEntry>();
		recipe.GetIngredientsForOneCraft(new Player(), ingredients);

		Assert.AreEqual(0, ingredients.Count);
	}

	[TestMethod]
	public void IngredientsWithoutCallbacksAreUnchanged()
	{
		Recipe recipe = MakeRecipe(ItemID.IronBar, 3);

		var ingredients = new List<Recipe.RequiredItemEntry>();
		recipe.GetIngredientsForOneCraft(new Player(), ingredients);

		Assert.AreEqual(1, ingredients.Count);
		Assert.AreEqual(ItemID.IronBar, ingredients[0].itemIdOrRecipeGroup);
		Assert.AreEqual(3, ingredients[0].stack);
	}
}
