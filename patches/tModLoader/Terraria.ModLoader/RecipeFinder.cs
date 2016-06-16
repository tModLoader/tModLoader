using System;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader.Exceptions;

namespace Terraria.ModLoader
{
	public class RecipeFinder
	{
		private List<Item> items = new List<Item>();
		private List<int> groups = new List<int>();
		private Item result = new Item();
		private List<int> tiles = new List<int>();
		public bool needWater;
		public bool needLava;
		public bool needHoney;

		public RecipeFinder()
		{
		}

		public void AddIngredient(int itemID, int stack = 1)
		{
			if (itemID <= 0 || itemID >= ItemLoader.ItemCount)
			{
				throw new RecipeException("No item has ID " + itemID);
			}
			Item item = new Item();
			item.SetDefaults(itemID, false);
			item.stack = stack;
			items.Add(item);
		}

		public void AddIngredient(string itemName, int stack = 1)
		{
			Item item = new Item();
			item.SetDefaults(itemName);
			if (item.type == 0)
			{
				throw new RecipeException("No item is named " + itemName);
			}
			item.stack = stack;
			items.Add(item);
		}

		public void AddRecipeGroup(string name, int stack = 1)
		{
			if (!RecipeGroup.recipeGroupIDs.ContainsKey(name))
			{
				throw new RecipeException("No recipe group is named " + name);
			}
			int id = RecipeGroup.recipeGroupIDs[name];
			RecipeGroup rec = RecipeGroup.recipeGroups[id];
			AddIngredient(rec.ValidItems[rec.IconicItemIndex], stack);
			groups.Add(id);
		}

		public void SetResult(int itemID, int stack = 1)
		{
			if (itemID <= 0 || itemID >= ItemLoader.ItemCount)
			{
				throw new RecipeException("No item has ID " + itemID);
			}
			result.SetDefaults(itemID, false);
			result.stack = stack;
		}

		public void SetResult(string itemName, int stack = 1)
		{
			result.SetDefaults(itemName);
			if (result.type == 0)
			{
				throw new RecipeException("No item is named " + itemName);
			}
			result.stack = 1;
		}

		public void AddTile(int tileID)
		{
			if (tileID < 0 || tileID >= TileLoader.TileCount)
			{
				throw new RecipeException("No tile has ID " + tileID);
			}
			tiles.Add(tileID);
		}

		public Recipe FindExactRecipe()
		{
			for (int k = 0; k < Recipe.numRecipes; k++)
			{
				Recipe recipe = Main.recipe[k];
				bool matches = true;
				List<Item> checkItems = new List<Item>(items);
				for (int i = 0; i < Recipe.maxRequirements; i++)
				{
					Item item = recipe.requiredItem[i];
					if (item.type == 0)
					{
						break;
					}
					bool itemMatched = false;
					for (int j = 0; j < checkItems.Count; j++)
					{
						if (item.type == checkItems[j].type && item.stack == checkItems[j].stack)
						{
							itemMatched = true;
							checkItems.RemoveAt(j);
							break;
						}
					}
					if (!itemMatched)
					{
						matches = false;
						break;
					}
				}
				if (checkItems.Count > 0)
				{
					matches = false;
				}
				List<int> checkGroups = new List<int>(groups);
				List<int> acceptedGroups = GetAcceptedGroups(recipe);
				for (int i = 0; i < acceptedGroups.Count; i++)
				{
					int group = acceptedGroups[i];
					bool groupMatched = false;
					for (int j = 0; j < checkGroups.Count; j++)
					{
						if (group == checkGroups[j])
						{
							groupMatched = true;
							checkGroups.RemoveAt(j);
							break;
						}
					}
					if (!groupMatched)
					{
						matches = false;
						break;
					}
				}
				if (checkGroups.Count > 0)
				{
					matches = false;
				}
				if (result.type != recipe.createItem.type || result.stack != recipe.createItem.stack)
				{
					matches = false;
				}
				List<int> checkTiles = new List<int>(tiles);
				for (int i = 0; i < Recipe.maxRequirements; i++)
				{
					int tile = recipe.requiredTile[i];
					if (tile == -1)
					{
						break;
					}
					bool tileMatched = false;
					for (int j = 0; j < checkTiles.Count; j++)
					{
						if (tile == checkTiles[j])
						{
							tileMatched = true;
							checkTiles.RemoveAt(j);
							break;
						}
					}
					if (!tileMatched)
					{
						matches = false;
						break;
					}
				}
				if (checkTiles.Count > 0)
				{
					matches = false;
				}
				if (needWater != recipe.needWater)
				{
					matches = false;
				}
				else if (needLava != recipe.needLava)
				{
					matches = false;
				}
				else if (needHoney != recipe.needHoney)
				{
					matches = false;
				}
				if (matches)
				{
					return recipe;
				}
			}
			return null;
		}

		public List<Recipe> SearchRecipes()
		{
			List<Recipe> recipes = new List<Recipe>();
			for (int k = 0; k < Recipe.numRecipes; k++)
			{
				Recipe recipe = Main.recipe[k];
				bool matches = true;
				List<Item> checkItems = new List<Item>(items);
				for (int i = 0; i < Recipe.maxRequirements; i++)
				{
					Item item = recipe.requiredItem[i];
					if (item.type == 0)
					{
						break;
					}
					for (int j = 0; j < checkItems.Count; j++)
					{
						if (item.type == checkItems[j].type && item.stack >= checkItems[j].stack)
						{
							checkItems.RemoveAt(j);
							break;
						}
					}
				}
				if (checkItems.Count > 0)
				{
					matches = false;
				}
				List<int> checkGroups = new List<int>(groups);
				List<int> acceptedGroups = GetAcceptedGroups(recipe);
				for (int i = 0; i < acceptedGroups.Count; i++)
				{
					int group = acceptedGroups[i];
					for (int j = 0; j < checkGroups.Count; j++)
					{
						if (group == checkGroups[j])
						{
							checkGroups.RemoveAt(j);
							break;
						}
					}
				}
				if (checkGroups.Count > 0)
				{
					matches = false;
				}
				if (result.type != 0)
				{
					if (result.type != recipe.createItem.type)
					{
						matches = false;
					}
					else if (result.stack > recipe.createItem.stack)
					{
						matches = false;
					}
				}
				List<int> checkTiles = new List<int>(tiles);
				for (int i = 0; i < Recipe.maxRequirements; i++)
				{
					int tile = recipe.requiredTile[i];
					if (tile == -1)
					{
						break;
					}
					for (int j = 0; j < checkTiles.Count; j++)
					{
						if (tile == checkTiles[j])
						{
							checkTiles.RemoveAt(j);
							break;
						}
					}
				}
				if (checkTiles.Count > 0)
				{
					matches = false;
				}
				if (needWater && !recipe.needWater)
				{
					matches = false;
				}
				else if (needLava && !recipe.needLava)
				{
					matches = false;
				}
				else if (needHoney && !recipe.needHoney)
				{
					matches = false;
				}
				if (matches)
				{
					recipes.Add(recipe);
				}
			}
			return recipes;
		}

		private static List<int> GetAcceptedGroups(Recipe recipe)
		{
			List<int> acceptedGroups = new List<int>(recipe.acceptedGroups);
			if (recipe.anyWood)
			{
				acceptedGroups.Add(RecipeGroupID.Wood);
			}
			if (recipe.anyIronBar)
			{
				acceptedGroups.Add(RecipeGroupID.IronBar);
			}
			if (recipe.anySand)
			{
				acceptedGroups.Add(RecipeGroupID.Sand);
			}
			if (recipe.anyPressurePlate)
			{
				acceptedGroups.Add(RecipeGroupID.PressurePlate);
			}
			if (recipe.anyFragment)
			{
				acceptedGroups.Add(RecipeGroupID.Fragment);
			}
			return acceptedGroups;
		}
	}
}
