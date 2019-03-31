using Terraria.ModLoader.Exceptions;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This class allows you to make any changes you want to a recipe, whether it be adding/removing ingredients, changing the result, or removing the recipe entirely.
	/// </summary>
	public class RecipeEditor
	{
		private Recipe recipe;

		/// <summary>
		/// Creates a recipe editor that acts on the given recipe.
		/// </summary>
		/// <param name="recipe">The recipe this RecipeEditor should focus on.</param>
		public RecipeEditor(Recipe recipe) {
			this.recipe = recipe;
		}

		/// <summary>
		/// Adds an ingredient with the given item ID and stack size to the recipe. If the recipe already contains the ingredient, it will increase the stack requirement instead. Can also throw a RecipeException.
		/// </summary>
		/// <param name="itemID">The required item (ingredient) ID</param>
		/// <param name="stack">The required item (ingredient) stack</param>
		public void AddIngredient(int itemID, int stack = 1) {
			if (itemID <= 0 || itemID >= ItemLoader.ItemCount) {
				throw new RecipeException("No item has ID " + itemID);
			}
			for (int k = 0; k < Recipe.maxRequirements; k++) {
				if (recipe.requiredItem[k].type == 0) {
					recipe.requiredItem[k].SetDefaults(itemID, false);
					recipe.requiredItem[k].stack = stack;
					return;
				}
				if (recipe.requiredItem[k].type == itemID) {
					recipe.requiredItem[k].stack += stack;
					return;
				}
			}
			throw new RecipeException("Recipe already has maximum number of ingredients");
		}

		/// <summary>
		/// Sets the stack requirement of the ingredient with the given item ID in the recipe. Returns true if the operation was successful. Returns false if the recipe does not contain the ingredient. Can also throw a RecipeException.
		/// </summary>
		/// <param name="itemID">The item ID of the ingredient to set the stack on.</param>
		/// <param name="stack">The new stack amount.</param>
		/// <returns>Whether the operation was successful.</returns>
		public bool SetIngredientStack(int itemID, int stack) {
			if (itemID <= 0 || itemID >= ItemLoader.ItemCount) {
				throw new RecipeException("No item has ID " + itemID);
			}
			for (int k = 0; k < Recipe.maxRequirements; k++) {
				if (recipe.requiredItem[k].type == itemID) {
					recipe.requiredItem[k].stack = stack;
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Deletes the ingredient requirement with the given ID from the recipe. Returns true if the operation was successful. Returns false if the recipe did not contain the ingredient in the first place. Can also throw a RecipeException.
		/// </summary>
		/// <param name="itemID">The item ID of the ingredient to delete.</param>
		/// <returns>Whether the operation was successful.</returns>
		public bool DeleteIngredient(int itemID) {
			if (itemID <= 0 || itemID >= ItemLoader.ItemCount) {
				throw new RecipeException("No item has ID " + itemID);
			}
			for (int k = 0; k < Recipe.maxRequirements; k++) {
				if (recipe.requiredItem[k].type == itemID) {
					for (int j = k; j < Recipe.maxRequirements - 1; j++) {
						recipe.requiredItem[j] = recipe.requiredItem[j + 1];
					}
					recipe.requiredItem[Recipe.maxRequirements - 1] = new Item();
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Adds the recipe group with the given name to the recipe. Note that, unlike ModRecipe and RecipeFinder, this won't actually add an ingredient; it will only allow existing ingredients to be interchangeable with other items. Returns true if the operation was successful. Returns false if the recipe already accepts the given recipe group. Can also throw a RecipeException.
		/// </summary>
		/// <param name="groupName">The recipegroup name to accept.</param>
		/// <returns>Whether adding the recipegroup was successful.</returns>
		public bool AcceptRecipeGroup(string groupName) {
			int groupID;
			if (!RecipeGroup.recipeGroupIDs.TryGetValue(groupName, out groupID)) {
				throw new RecipeException("No recipe group is named " + groupName);
			}
			if (recipe.acceptedGroups.Contains(groupID)) {
				return false;
			}
			recipe.acceptedGroups.Add(groupID);
			return true;
		}

		/// <summary>
		/// Removes the recipe group with the given name from the recipe. This is the opposite of AcceptRecipeGroup; while it won't remove ingredients, it will make existing ingredients no longer be interchangeable with other items. Returns true if the operation was successful. Returns false if the recipe did not contain the recipe group in the first place. Can also throw a RecipeException.
		/// </summary>
		/// <param name="groupName">The recipegroup name to reject.</param>
		/// <returns>Whether removing the recipegroup was successful.</returns>
		public bool RejectRecipeGroup(string groupName) {
			int groupID;
			if (!RecipeGroup.recipeGroupIDs.TryGetValue(groupName, out groupID)) {
				throw new RecipeException("No recipe group is named " + groupName);
			}
			return recipe.acceptedGroups.Remove(groupID);
		}

		/// <summary>
		/// A convenience method for setting the result of the recipe. Similar to calling recipe.createItem.SetDefaults(itemID), followed by recipe.createItem.stack = stack. Can also throw a RecipeException.
		/// </summary>
		/// <param name="itemID">The ID of the item to set as result.</param>
		/// <param name="stack">The stack of the item to set as result.</param>
		public void SetResult(int itemID, int stack = 1) {
			if (itemID <= 0 || itemID >= ItemLoader.ItemCount) {
				throw new RecipeException("No item has ID " + itemID);
			}
			recipe.createItem.SetDefaults(itemID);
			recipe.createItem.stack = stack;
		}

		/// <summary>
		/// Adds the crafting station with the given tile ID to the recipe. Returns true if the operation was successful. Returns false if the recipe already requires the given tile. Can also throw a RecipeException.
		/// </summary>
		/// <param name="tileID">The tile ID to add.</param>
		/// <returns>Whether the operation was successful</returns>
		public bool AddTile(int tileID) {
			if (tileID < 0 || tileID >= TileLoader.TileCount) {
				throw new RecipeException("No tile has ID " + tileID);
			}
			for (int k = 0; k < Recipe.maxRequirements; k++) {
				if (recipe.requiredTile[k] == -1) {
					recipe.requiredTile[k] = tileID;
					return true;
				}
				if (recipe.requiredTile[k] == tileID) {
					return false;
				}
			}
			throw new RecipeException("Recipe already has maximum number of tiles");
		}

		/// <summary>
		/// Removes the crafting station with the given tile ID as a requirement from the recipe. Returns true if the operation was successful. Returns false if the recipe did not require the tile in the first place. Can also throw a RecipeException.
		/// </summary>
		/// <param name="tileID">The tile ID to remove.</param>
		/// <returns>Whether the operation was successful or not.</returns>
		public bool DeleteTile(int tileID) {
			if (tileID < 0 || tileID >= TileLoader.TileCount) {
				throw new RecipeException("No tile has ID " + tileID);
			}
			for (int k = 0; k < Recipe.maxRequirements; k++) {
				if (recipe.requiredTile[k] == tileID) {
					for (int j = k; j < Recipe.maxRequirements - 1; j++) {
						recipe.requiredTile[j] = recipe.requiredTile[j + 1];
					}
					recipe.requiredTile[Recipe.maxRequirements - 1] = -1;
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// A convenience method for setting recipe.needWater.
		/// </summary>
		/// <param name="needWater">Whether the recipe needs water.</param>
		public void SetNeedWater(bool needWater) {
			recipe.needWater = needWater;
		}

		/// <summary>
		/// A convenience method for setting recipe.needLava.
		/// </summary>
		/// <param name="needLava">Whether the recipe needs lava.</param>
		public void SetNeedLava(bool needLava) {
			recipe.needLava = needLava;
		}

		/// <summary>
		/// A convenience method for setting recipe.needHoney.
		/// </summary>
		/// <param name="needHoney">Whether the recipe needs honey.</param>
		public void SetNeedHoney(bool needHoney) {
			recipe.needHoney = needHoney;
		}

		/// <summary>
		/// Completely removes the recipe from the game, making it unusable. Returns true if the operation was successful. Returns false if the recipe was already not in the game.
		/// </summary>
		/// <returns></returns>
		public bool DeleteRecipe() {
			for (int k = 0; k < Recipe.numRecipes; k++) {
				if (Main.recipe[k] == recipe) {
					for (int j = k; j < Recipe.numRecipes - 1; j++) {
						Main.recipe[j] = Main.recipe[j + 1];
					}
					Main.recipe[Recipe.numRecipes - 1] = new Recipe();
					Recipe.numRecipes--;
					return true;
				}
			}
			return false;
		}
	}
}
