using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Terraria.ID;
using Terraria.ModLoader.Exceptions;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.Utilities;
using Terraria.DataStructures;

namespace Terraria.ModLoader;

/// <summary>
/// This is where all Recipe hooks are gathered and called.
/// </summary>
public static class RecipeLoader
{
	internal static Recipe[] FirstRecipeForItem = new Recipe[ItemID.Count];
	/// <summary>
	/// Cloned list of Items consumed when crafting.  Cleared after the OnCreate and OnCraft hooks.
	/// </summary>
	internal static List<Item> ConsumedItems = new List<Item>();

	/// <summary>
	/// Set when tML sets up modded recipes. Used to detect misuse of CreateRecipe
	/// </summary>
	internal static bool setupRecipes = false;

	/// <summary>
	/// The mod currently adding recipes.
	/// </summary>
	internal static Mod CurrentMod { get; private set; }

	internal static void Unload()
	{
		setupRecipes = false;
		FirstRecipeForItem = new Recipe[Recipe.maxRecipes];
	}

	internal static void AddRecipes()
	{
		var addRecipesMethod = typeof(Mod).GetMethod(nameof(Mod.AddRecipes), BindingFlags.Instance | BindingFlags.Public)!;
		foreach (Mod mod in ModLoader.Mods) {
			CurrentMod = mod;
			try {
				addRecipesMethod.Invoke(mod, Array.Empty<object>());
				SystemLoader.AddRecipes(mod);
				LoaderUtils.ForEachAndAggregateExceptions(mod.GetContent<ModItem>(), item => item.AddRecipes());
				LoaderUtils.ForEachAndAggregateExceptions(mod.GetContent<GlobalItem>(), global => global.AddRecipes());
			}
			catch (Exception e) {
				e.Data["mod"] = mod.Name;
				throw;
			}
			finally {
				CurrentMod = null;
			}
		}
	}

	internal static void PostAddRecipes()
	{
		var postAddRecipesMethod = typeof(Mod).GetMethod(nameof(Mod.PostAddRecipes), BindingFlags.Instance | BindingFlags.Public)!;
		foreach (Mod mod in ModLoader.Mods) {
			CurrentMod = mod;
			try {
				postAddRecipesMethod.Invoke(mod, Array.Empty<object>());
				SystemLoader.PostAddRecipes(mod);
			}
			catch (Exception e) {
				e.Data["mod"] = mod.Name;
				throw;
			}
			finally {
				CurrentMod = null;
			}
		}
	}

	internal static void PostSetupRecipes()
	{
		foreach (Mod mod in ModLoader.Mods) {
			CurrentMod = mod;
			try {
				SystemLoader.PostSetupRecipes(mod);
			}
			catch (Exception e) {
				e.Data["mod"] = mod.Name;
				throw;
			}
			finally {
				CurrentMod = null;
			}
		}
	}

	/// <summary>
	/// Orders everything in the recipe according to their Ordering.
	/// </summary>
	internal static void OrderRecipes()
	{
		var sorted = SortBeforeAfter.Sort(Main.recipe, r => r.Ordering);
		for (int i = 0; i < sorted.Count; i++) {
			sorted[i].RecipeIndex = i;
			Main.recipe[i] = sorted[i];
		}
	}

	/// <summary>
	/// Returns whether or not the conditions are met for this recipe to be available for the player to use.
	/// </summary>
	/// <param name="recipe">The recipe to check.</param>
	/// <returns>Whether or not the conditions are met for this recipe.</returns>
	public static bool RecipeAvailable(Recipe recipe)
	{
		return recipe.Conditions.All(c => c.IsMet());
	}

	/// <summary>
	/// Returns whether or not the conditions are met for this recipe to be shimmered/decrafted.
	/// </summary>
	/// <param name="recipe">The recipe to check.</param>
	/// <returns>Whether or not the conditions are met for this recipe.</returns>
	public static bool DecraftAvailable(Recipe recipe)
	{
		return !recipe.notDecraftable && recipe.DecraftConditions.All(c => c.IsMet());
	}

	/// <summary>
	/// recipe.OnCraftHooks followed by Calls ItemLoader.OnCreate with a RecipeItemCreationContext
	/// </summary>
	/// <param name="item">The item crafted.</param>
	/// <param name="recipe">The recipe used to craft the item.</param>
	/// <param name="consumedItems">Materials used to craft the item.</param>
	/// <param name="destinationStack">The stack that the crafted item will be combined with</param>
	public static void OnCraft(Item item, Recipe recipe, List<Item> consumedItems, Item destinationStack)
	{
		recipe.OnCraftHooks?.Invoke(recipe, item, consumedItems, destinationStack);
		item.OnCreated(new RecipeItemCreationContext(recipe, consumedItems/*, destinationStack*/));
	}

	/// <summary>
	/// Helper version of OnCraft, used in combination with Recipe.Create and the internal ConsumedItems list
	/// </summary>
	/// <param name="item"></param>
	/// <param name="recipe"></param>
	/// <param name="destinationStack">The stack that the crafted item will be combined with</param>
	public static void OnCraft(Item item, Recipe recipe, Item destinationStack)
	{
		OnCraft(item, recipe, ConsumedItems, destinationStack);
		ConsumedItems.Clear();
	}

	/// <summary>
	/// Allows to edit the amount of item the player uses in a recipe. Also used to decide the amount a shimmer transformation returns
	/// </summary>
	/// <param name="recipe">The recipe used for the craft.</param>
	/// <param name="type">Type of the ingredient.</param>
	/// <param name="amount">Modifiable amount of the item consumed.</param>
	[Obsolete($"Replaced by {nameof(ConsumeIngredient)} due to not accounting for shimmer decrafting")]
	public static void ConsumeItem(Recipe recipe, int type, ref int amount)
	{
		ConsumeIngredient(recipe, type, ref amount, false);
	}

	/// <summary>
	/// Allows to edit the amount of item the player uses in a recipe. Also used to decide the amount a shimmer transformation returns
	/// </summary>
	/// <param name="recipe">The recipe used for the craft.</param>
	/// <param name="type">Type of the ingredient.</param>
	/// <param name="amount">Modifiable amount of the item consumed.</param>
	/// <param name="isDecrafting">If the operation takes place during shimmer decrafting.</param>
	public static void ConsumeIngredient(Recipe recipe, int type, ref int amount, bool isDecrafting = false)
	{
		recipe.ConsumeIngredientHooks?.Invoke(recipe, type, ref amount, isDecrafting);
	}
}