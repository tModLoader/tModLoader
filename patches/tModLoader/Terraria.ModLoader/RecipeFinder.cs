using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader.Exceptions;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This class will search through all existing recipes for you based on criteria that you give it. It's useful for finding a particular vanilla recipe that you wish to remove or edit. Use this by creating new instances with the empty constructor for each search you perform.
	/// </summary>
	public class RecipeFinder
	{
		private List<Item> items = new List<Item>();
		private List<int> groups = new List<int>();
		private Item result = new Item();
		private List<int> tiles = new List<int>();
		/// <summary>
		/// Adds the requirement of being nearby water to the search criteria. Defaults to false.
		/// </summary>
		public bool needWater;
		/// <summary>
		/// Adds the requirement of being nearby lava to the search criteria. Defaults to false.
		/// </summary>
		public bool needLava;
		/// <summary>
		/// Adds the requirement of being nearby honey to the search criteria. Defaults to false.
		/// </summary>
		public bool needHoney;

		public RecipeFinder() {
		}

		/// <summary>
		/// Adds an ingredient with the given item type and stack size to the search criteria.
		/// </summary>
		/// <param name="itemID">The item ID of the ingredient to add.</param>
		/// <param name="stack">The stack of the ingredient to add.</param>
		public void AddIngredient(int itemID, int stack = 1) {
			if (itemID <= 0 || itemID >= ItemLoader.ItemCount) {
				throw new RecipeException("No item has ID " + itemID);
			}
			Item item = new Item();
			item.SetDefaults(itemID, false);
			item.stack = stack;
			items.Add(item);
		}

		/// <summary>
		/// Adds a recipe group ingredient with the given RecipeGroup name and stack size to the search criteria.
		/// </summary>
		/// <param name="name">The name of the recipegroup to accept.</param>
		/// <param name="stack">The stack of the recipegroup to accept.</param>
		public void AddRecipeGroup(string name, int stack = 1) {
			if (!RecipeGroup.recipeGroupIDs.ContainsKey(name)) {
				throw new RecipeException("No recipe group is named " + name);
			}
			int id = RecipeGroup.recipeGroupIDs[name];
			RecipeGroup rec = RecipeGroup.recipeGroups[id];
			AddIngredient(rec.ValidItems[rec.IconicItemIndex], stack);
			groups.Add(id);
		}

		/// <summary>
		/// Sets the search criteria's result to the given item type and stack size.
		/// </summary>
		/// <param name="itemID">The item ID of the item to set as result.</param>
		/// <param name="stack">The stack of the item to set as result.</param>
		public void SetResult(int itemID, int stack = 1) {
			if (itemID <= 0 || itemID >= ItemLoader.ItemCount) {
				throw new RecipeException("No item has ID " + itemID);
			}
			result.SetDefaults(itemID, false);
			result.stack = stack;
		}

		/// <summary>
		/// Adds a required crafting station with the given tile type to the search criteria.
		/// </summary>
		/// <param name="tileID">The tile ID of the tile to add.</param>
		public void AddTile(int tileID) {
			if (tileID < 0 || tileID >= TileLoader.TileCount) {
				throw new RecipeException("No tile has ID " + tileID);
			}
			tiles.Add(tileID);
		}

		/// <summary>
		/// Searches for a recipe that matches the search criteria exactly, then returns it. That means the recipe will have exactly the same ingredients, tiles, liquid requirements, recipe groups, and result; even the stack sizes will match. If no recipe with an exact match is found, this will return null.
		/// </summary>
		/// <returns>The recipe found matching the finder's criteria.</returns>
		public Recipe FindExactRecipe() {
			for (int k = 0; k < Recipe.numRecipes; k++) {
				Recipe recipe = Main.recipe[k];
				bool matches = true;
				List<Item> checkItems = new List<Item>(items);
				for (int i = 0; i < Recipe.maxRequirements; i++) {
					Item item = recipe.requiredItem[i];
					if (item.type == 0) {
						break;
					}
					bool itemMatched = false;
					for (int j = 0; j < checkItems.Count; j++) {
						if (item.type == checkItems[j].type && item.stack == checkItems[j].stack) {
							itemMatched = true;
							checkItems.RemoveAt(j);
							break;
						}
					}
					if (!itemMatched) {
						matches = false;
						break;
					}
				}
				if (checkItems.Count > 0) {
					matches = false;
				}
				List<int> checkGroups = new List<int>(groups);
				List<int> acceptedGroups = GetAcceptedGroups(recipe);
				for (int i = 0; i < acceptedGroups.Count; i++) {
					int group = acceptedGroups[i];
					bool groupMatched = false;
					for (int j = 0; j < checkGroups.Count; j++) {
						if (group == checkGroups[j]) {
							groupMatched = true;
							checkGroups.RemoveAt(j);
							break;
						}
					}
					if (!groupMatched) {
						matches = false;
						break;
					}
				}
				if (checkGroups.Count > 0) {
					matches = false;
				}
				if (result.type != recipe.createItem.type || result.stack != recipe.createItem.stack) {
					matches = false;
				}
				List<int> checkTiles = new List<int>(tiles);
				for (int i = 0; i < Recipe.maxRequirements; i++) {
					int tile = recipe.requiredTile[i];
					if (tile == -1) {
						break;
					}
					bool tileMatched = false;
					for (int j = 0; j < checkTiles.Count; j++) {
						if (tile == checkTiles[j]) {
							tileMatched = true;
							checkTiles.RemoveAt(j);
							break;
						}
					}
					if (!tileMatched) {
						matches = false;
						break;
					}
				}
				if (checkTiles.Count > 0) {
					matches = false;
				}
				if (needWater != recipe.needWater) {
					matches = false;
				}
				else if (needLava != recipe.needLava) {
					matches = false;
				}
				else if (needHoney != recipe.needHoney) {
					matches = false;
				}
				if (matches) {
					return recipe;
				}
			}
			return null;
		}

		/// <summary>
		/// Searches for all recipes that include the search criteria, then returns them in a list. In terms of ingredients, it will search for recipes that include all the search criteria ingredients, with stack sizes greater than or equal to the search criteria. It will also make sure the recipes include all search criteria recipe groups and tiles. If the search criteria includes a result, the recipes will also have the same result with a stack size greater than or equal to the search criteria. Finally, if needWater, needLava, or needHoney are set to true, the found recipes will also have them set to true.
		/// </summary>
		/// <returns>A list containing found recipes matching the finder's criteria.</returns>
		public List<Recipe> SearchRecipes() {
			List<Recipe> recipes = new List<Recipe>();
			for (int k = 0; k < Recipe.numRecipes; k++) {
				Recipe recipe = Main.recipe[k];
				bool matches = true;
				List<Item> checkItems = new List<Item>(items);
				for (int i = 0; i < Recipe.maxRequirements; i++) {
					Item item = recipe.requiredItem[i];
					if (item.type == 0) {
						break;
					}
					for (int j = 0; j < checkItems.Count; j++) {
						if (item.type == checkItems[j].type && item.stack >= checkItems[j].stack) {
							checkItems.RemoveAt(j);
							break;
						}
					}
				}
				if (checkItems.Count > 0) {
					matches = false;
				}
				List<int> checkGroups = new List<int>(groups);
				List<int> acceptedGroups = GetAcceptedGroups(recipe);
				for (int i = 0; i < acceptedGroups.Count; i++) {
					int group = acceptedGroups[i];
					for (int j = 0; j < checkGroups.Count; j++) {
						if (group == checkGroups[j]) {
							checkGroups.RemoveAt(j);
							break;
						}
					}
				}
				if (checkGroups.Count > 0) {
					matches = false;
				}
				if (result.type != 0) {
					if (result.type != recipe.createItem.type) {
						matches = false;
					}
					else if (result.stack > recipe.createItem.stack) {
						matches = false;
					}
				}
				List<int> checkTiles = new List<int>(tiles);
				for (int i = 0; i < Recipe.maxRequirements; i++) {
					int tile = recipe.requiredTile[i];
					if (tile == -1) {
						break;
					}
					for (int j = 0; j < checkTiles.Count; j++) {
						if (tile == checkTiles[j]) {
							checkTiles.RemoveAt(j);
							break;
						}
					}
				}
				if (checkTiles.Count > 0) {
					matches = false;
				}
				if (needWater && !recipe.needWater) {
					matches = false;
				}
				else if (needLava && !recipe.needLava) {
					matches = false;
				}
				else if (needHoney && !recipe.needHoney) {
					matches = false;
				}
				if (matches) {
					recipes.Add(recipe);
				}
			}
			return recipes;
		}

		private static List<int> GetAcceptedGroups(Recipe recipe) {
			List<int> acceptedGroups = new List<int>(recipe.acceptedGroups);
			if (recipe.anyWood) {
				acceptedGroups.Add(RecipeGroupID.Wood);
			}
			if (recipe.anyIronBar) {
				acceptedGroups.Add(RecipeGroupID.IronBar);
			}
			if (recipe.anySand) {
				acceptedGroups.Add(RecipeGroupID.Sand);
			}
			if (recipe.anyPressurePlate) {
				acceptedGroups.Add(RecipeGroupID.PressurePlate);
			}
			if (recipe.anyFragment) {
				acceptedGroups.Add(RecipeGroupID.Fragment);
			}
			return acceptedGroups;
		}
	}
}
