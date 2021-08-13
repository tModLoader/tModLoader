using System;
using System.Linq;
using System.Collections.Generic;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This is where all Recipe and GlobalRecipe hooks are gathered and called.
	/// </summary>
	public static class RecipeLoader
	{
		internal static readonly IList<GlobalRecipe> globalRecipes = new List<GlobalRecipe>();

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
		}

		internal static void AddRecipes() {
			foreach (Mod mod in ModLoader.Mods) {
				try {
					mod.AddRecipes();
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
				}
				catch (Exception e) {
					e.Data["mod"] = mod.Name;
					throw;
				}
			}
		}

		/// <summary>
		/// Deletes the recipes flagged for deletion
		/// </summary>
		internal static void DeleteRecipes() {
			int shift = 0;
			for (int index = 0; index < Recipe.numRecipes; index++) {
				Recipe recipe = Main.recipe[index];
				if (recipe.FlaggedForDeletion) {
					shift++;
				}
				else {
					Main.recipe[index - shift] = recipe;
				}
			}

			Recipe.numRecipes -= shift;

			for (int index = Recipe.numRecipes; index < Recipe.numRecipes + shift; index++) {
				Main.recipe[index] = new Recipe();
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