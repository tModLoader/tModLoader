using System;
using System.Collections.Generic;
using Terraria.ModLoader.Exceptions;

namespace Terraria.ModLoader
{
	public class RecipeEditor
	{
		private Recipe recipe;

		public RecipeEditor(Recipe recipe)
		{
			this.recipe = recipe;
		}

		public void AddIngredient(int itemID, int stack = 1)
		{
			if (itemID <= 0 || itemID >= ItemLoader.ItemCount)
			{
				throw new RecipeException("No item has ID " + itemID);
			}
			for (int k = 0; k < Recipe.maxRequirements; k++)
			{
				if (recipe.requiredItem[k].type == 0)
				{
					recipe.requiredItem[k].SetDefaults(itemID, false);
					recipe.requiredItem[k].stack = stack;
					return;
				}
				if (recipe.requiredItem[k].type == itemID)
				{
					recipe.requiredItem[k].stack += stack;
					return;
				}
			}
			throw new RecipeException("Recipe already has maximum number of ingredients");
		}

		public bool SetIngredientStack(int itemID, int stack)
		{
			if (itemID <= 0 || itemID >= ItemLoader.ItemCount)
			{
				throw new RecipeException("No item has ID " + itemID);
			}
			for (int k = 0; k < Recipe.maxRequirements; k++)
			{
				if (recipe.requiredItem[k].type == itemID)
				{
					recipe.requiredItem[k].stack = stack;
					return true;
				}
			}
			return false;
		}

		public bool DeleteIngredient(int itemID)
		{
			if (itemID <= 0 || itemID >= ItemLoader.ItemCount)
			{
				throw new RecipeException("No item has ID " + itemID);
			}
			for (int k = 0; k < Recipe.maxRequirements; k++)
			{
				if (recipe.requiredItem[k].type == itemID)
				{
					for (int j = k; j < Recipe.maxRequirements - 1; j++)
					{
						recipe.requiredItem[j] = recipe.requiredItem[j + 1];
					}
					recipe.requiredItem[Recipe.maxRequirements - 1] = new Item();
					return true;
				}
			}
			return false;
		}

		public bool AcceptRecipeGroup(string groupName)
		{
			int groupID;
			if (!RecipeGroup.recipeGroupIDs.TryGetValue(groupName, out groupID))
			{
				throw new RecipeException("No recipe group is named " + groupName);
			}
			if (recipe.acceptedGroups.Contains(groupID))
			{
				return false;
			}
			recipe.acceptedGroups.Add(groupID);
			return true;
		}

		public bool RejectRecipeGroup(string groupName)
		{
			int groupID;
			if (!RecipeGroup.recipeGroupIDs.TryGetValue(groupName, out groupID))
			{
				throw new RecipeException("No recipe group is named " + groupName);
			}
			return recipe.acceptedGroups.Remove(groupID);
		}

		public void SetResult(int itemID, int stack = 1)
		{
			if (itemID <= 0 || itemID >= ItemLoader.ItemCount)
			{
				throw new RecipeException("No item has ID " + itemID);
			}
			recipe.createItem.SetDefaults(itemID);
			recipe.createItem.stack = stack;
		}

		public void SetResult(string itemName, int stack = 1)
		{
			recipe.createItem.SetDefaults(itemName);
			if (recipe.createItem.type == 0)
			{
				throw new RecipeException("No item is named " + itemName);
			}
			recipe.createItem.stack = stack;
		}

		public bool AddTile(int tileID)
		{
			if (tileID < 0 || tileID >= TileLoader.TileCount)
			{
				throw new RecipeException("No tile has ID " + tileID);
			}
			for (int k = 0; k < Recipe.maxRequirements; k++)
			{
				if (recipe.requiredTile[k] == -1)
				{
					recipe.requiredTile[k] = tileID;
					return true;
				}
				if (recipe.requiredTile[k] == tileID)
				{
					return false;
				}
			}
			throw new RecipeException("Recipe already has maximum number of tiles");
		}

		public bool DeleteTile(int tileID)
		{
			if (tileID < 0 || tileID >= TileLoader.TileCount)
			{
				throw new RecipeException("No tile has ID " + tileID);
			}
			for (int k = 0; k < Recipe.maxRequirements; k++)
			{
				if (recipe.requiredTile[k] == tileID)
				{
					for (int j = k; j < Recipe.maxRequirements - 1; j++)
					{
						recipe.requiredTile[j] = recipe.requiredTile[j + 1];
					}
					recipe.requiredTile[Recipe.maxRequirements - 1] = -1;
					return true;
				}
			}
			return false;
		}

		public void SetNeedWater(bool needWater)
		{
			recipe.needWater = needWater;
		}

		public void SetNeedLava(bool needLava)
		{
			recipe.needLava = needLava;
		}

		public void SetNeedHoney(bool needHoney)
		{
			recipe.needHoney = needHoney;
		}

		public bool DeleteRecipe()
		{
			for (int k = 0; k < Recipe.numRecipes; k++)
			{
				if (Main.recipe[k] == recipe)
				{
					for (int j = k; j < Recipe.numRecipes - 1; j++)
					{
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
