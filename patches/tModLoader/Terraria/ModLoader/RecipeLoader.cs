using System;
using System.Linq;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader.Exceptions;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This is where all Recipe and GlobalRecipe hooks are gathered and called.
	/// </summary>
	public static class RecipeLoader
	{
		internal static readonly IList<GlobalRecipe> globalRecipes = new List<GlobalRecipe>();
		internal static Recipe[] FirstRecipeForItem = new Recipe[ItemID.Count];

		/// <summary>
		/// Set when tML sets up modded recipes. Used to detect misuse of CreateRecipe
		/// </summary>
		internal static bool setupRecipes = false;

		internal static void Add(GlobalRecipe globalRecipe) {
			globalRecipes.Add(globalRecipe);
		}

		internal static void Unload() {
			globalRecipes.Clear();
			setupRecipes = false;
			FirstRecipeForItem = new Recipe[Recipe.maxRecipes];
		}

		internal static void AddRecipes() {
			foreach (Mod mod in ModLoader.Mods) {
				try {
					mod.AddRecipes();
					SystemLoader.AddRecipes(mod);

					foreach (ModItem item in mod.GetContent<ModItem>())
						item.AddRecipes();

					foreach (GlobalItem globalItem in mod.GetContent<GlobalItem>())
						globalItem.AddRecipes();
				}
				catch (Exception e) {
					e.Data["mod"] = mod.Name;
					throw;
				}
			}
		}

		internal static void PostAddRecipes() {
			foreach (Mod mod in ModLoader.Mods) {
				try {
					mod.PostAddRecipes();
					SystemLoader.PostAddRecipes(mod);
				}
				catch (Exception e) {
					e.Data["mod"] = mod.Name;
					throw;
				}
			}
		}

		internal static void PostSetupRecipes() {
			foreach (Mod mod in ModLoader.Mods) {
				try {
					SystemLoader.PostSetupRecipes(mod);
				}
				catch (Exception e) {
					e.Data["mod"] = mod.Name;
					throw;
				}
			}
		}

		/// <summary>
		/// Orders everything in the recipe according to their Ordering.
		/// </summary>
		internal static void OrderRecipes() {
			List<Recipe> recipesToOrder = new List<Recipe>();
			for (int recipeIndex = 0; recipeIndex < Main.recipe.Length; recipeIndex++) {
				Recipe recipe = Main.recipe[recipeIndex];
				Recipe parentRecipe = recipe;
				while (parentRecipe.Ordering.target != null) {
					if (recipesToOrder.Contains(parentRecipe))
						throw new RecipeException("Recipe ordering loop detected.");
					recipesToOrder.Add(parentRecipe);
					parentRecipe = parentRecipe.Ordering.target;
				}

				if (recipesToOrder.Count != 0) {
					for (int orderIndex = recipesToOrder.Count - 1; orderIndex >= 0; orderIndex--) {
						Recipe recipeToOrder = recipesToOrder[orderIndex];
						int targetIndex = (recipeToOrder.Ordering.target.RecipeIndex > recipeToOrder.RecipeIndex) switch {
							false when !recipeToOrder.Ordering.after => recipeToOrder.Ordering.target.RecipeIndex - 1,
							true when recipeToOrder.Ordering.after => recipeToOrder.Ordering.target.RecipeIndex + 1,
							_ => recipeToOrder.Ordering.target.RecipeIndex
						};

						recipeToOrder.MoveAt(targetIndex);
						recipeToOrder.Ordering = default;
					}

					recipeIndex--; //Redo this location in case a not handled recipe took its place.
					recipesToOrder.Clear();
				}
			}
		}

		/// <summary>
		/// Returns whether or not the conditions are met for this recipe to be available for the player to use.
		/// </summary>
		/// <param name="recipe">The recipe to check.</param>
		/// <returns>Whether or not the conditions are met for this recipe.</returns>
		public static bool RecipeAvailable(Recipe recipe) {
			return recipe.Conditions.All(c => c.RecipeAvailable(recipe)) && globalRecipes.All(globalRecipe => globalRecipe.RecipeAvailable(recipe));
		}

		/// <summary>
		/// Allows you to make anything happen when a player uses this recipe.
		/// </summary>
		/// <param name="item">The item crafted.</param>
		/// <param name="recipe">The recipe used to craft the item.</param>
		public static void OnCraft(Item item, Recipe recipe) {
			recipe.OnCraftHooks?.Invoke(recipe, item);

			foreach (GlobalRecipe globalRecipe in globalRecipes) {
				globalRecipe.OnCraft(item, recipe);
			}
		}

		/// <summary>
		/// Allows to edit the amount of item the player uses in a recipe.
		/// </summary>
		/// <param name="recipe">The recipe used for the craft.</param>
		/// <param name="type">Type of the ingredient.</param>
		/// <param name="amount">Modifiable amount of the item consumed.</param>
		public static void ConsumeItem(Recipe recipe, int type, ref int amount) {
			recipe.ConsumeItemHooks?.Invoke(recipe, type, ref amount);

			foreach (GlobalRecipe globalRecipe in globalRecipes) {
				globalRecipe.ConsumeItem(recipe, type, ref amount);
			}
		}
	}
}