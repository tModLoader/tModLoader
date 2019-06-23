using System;
using System.Collections.Generic;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This is where all ModRecipe and GlobalRecipe hooks are gathered and called.
	/// </summary>
	public static class RecipeHooks
	{
		internal static readonly IList<GlobalRecipe> globalRecipes = new List<GlobalRecipe>();

		internal static void Add(GlobalRecipe globalRecipe) {
			globalRecipes.Add(globalRecipe);
		}

		internal static void Unload() {
			globalRecipes.Clear();
		}

		internal static void AddRecipes() {
			foreach (Mod mod in ModLoader.Mods) {
				try {
					mod.AddRecipes();
					foreach (ModItem item in mod.items.Values)
						item.AddRecipes();
					foreach (GlobalItem globalItem in mod.globalItems.Values)
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
		/// Returns whether or not the conditions are met for this recipe to be available for the player to use.
		/// </summary>
		/// <param name="recipe">The recipe to check.</param>
		/// <returns>Whether or not the conditions are met for this recipe.</returns>
		public static bool RecipeAvailable(Recipe recipe) {
			ModRecipe modRecipe = recipe as ModRecipe;
			if (modRecipe != null && !modRecipe.RecipeAvailable()) {
				return false;
			}
			foreach (GlobalRecipe globalRecipe in globalRecipes) {
				if (!globalRecipe.RecipeAvailable(recipe)) {
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Allows you to make anything happen when a player uses this recipe.
		/// </summary>
		/// <param name="item">The item crafted.</param>
		/// <param name="recipe">The recipe used to craft the item.</param>
		public static void OnCraft(Item item, Recipe recipe) {
			ModRecipe modRecipe = recipe as ModRecipe;
			if (modRecipe != null) {
				modRecipe.OnCraft(item);
			}
			foreach (GlobalRecipe globalRecipe in globalRecipes) {
				globalRecipe.OnCraft(item, recipe);
			}
		}
	}
}