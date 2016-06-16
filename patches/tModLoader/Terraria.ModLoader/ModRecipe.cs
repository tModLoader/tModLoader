using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader.Exceptions;

namespace Terraria.ModLoader
{
	public class ModRecipe : Recipe
	{
		public readonly Mod mod;
		private int numIngredients = 0;
		private int numTiles = 0;

		public int RecipeIndex
		{
			get;
			private set;
		}

		public ModRecipe(Mod mod)
		{
			this.mod = mod;
		}

		public void SetResult(int itemID, int stack = 1)
		{
			this.createItem.SetDefaults(itemID, false);
			this.createItem.stack = stack;
		}

		public void SetResult(string itemName, int stack = 1)
		{
			this.createItem.SetDefaults(itemName);
			if (this.createItem.type == 0)
			{
				throw new RecipeException("A vanilla item with the name " + itemName + " does not exist.");
			}
			this.createItem.stack = stack;
		}

		public void SetResult(Mod mod, string itemName, int stack = 1)
		{
			if (mod == null)
			{
				mod = this.mod;
			}
			int type = mod.ItemType(itemName);
			if (type == 0)
			{
				string message = "The item " + itemName + " does not exist in the mod " + mod.Name + "." + Environment.NewLine;
				message += "If you are trying to use a vanilla item, try removing the first argument.";
				throw new RecipeException(message);
			}
			this.SetResult(type, stack);
		}

		public void SetResult(ModItem item, int stack = 1)
		{
			this.SetResult(item.item.type, stack);
		}

		public void AddIngredient(int itemID, int stack = 1)
		{
			this.requiredItem[numIngredients].SetDefaults(itemID, false);
			this.requiredItem[numIngredients].stack = stack;
			numIngredients++;
		}

		public void AddIngredient(string itemName, int stack = 1)
		{
			this.requiredItem[numIngredients].SetDefaults(itemName);
			if (this.requiredItem[numIngredients].type == 0)
			{
				throw new RecipeException("A vanilla item with the name " + itemName + " does not exist.");
			}
			this.requiredItem[numIngredients].stack = stack;
			numIngredients++;
		}

		public void AddIngredient(Mod mod, string itemName, int stack = 1)
		{
			if (mod == null)
			{
				mod = this.mod;
			}
			int type = mod.ItemType(itemName);
			if (type == 0)
			{
				string message = "The item " + itemName + " does not exist in the mod " + mod.Name + "." + Environment.NewLine;
				message += "If you are trying to use a vanilla item, try removing the first argument.";
				throw new RecipeException(message);
			}
			this.AddIngredient(type, stack);
		}

		public void AddIngredient(ModItem item, int stack = 1)
		{
			this.AddIngredient(item.item.type, stack);
		}

		public void AddRecipeGroup(string name, int stack = 1)
		{
			if (!RecipeGroup.recipeGroupIDs.ContainsKey(name))
			{
				throw new RecipeException("A recipe group with the name " + name + " does not exist.");
			}
			int id = RecipeGroup.recipeGroupIDs[name];
			RecipeGroup rec = RecipeGroup.recipeGroups[id];
			AddIngredient(rec.ValidItems[rec.IconicItemIndex], stack);
			acceptedGroups.Add(id);
		}

		public void AddTile(int tileID)
		{
			this.requiredTile[numTiles] = tileID;
			numTiles++;
		}

		public void AddTile(Mod mod, string tileName)
		{
			if (mod == null)
			{
				mod = this.mod;
			}
			int type = mod.TileType(tileName);
			if (type == 0)
			{
				string message = "The tile " + tileName + " does not exist in the mod " + mod.Name + "." + Environment.NewLine;
				message += "If you are trying to use a vanilla tile, try using ModRecipe.AddTile(tileID).";
				throw new RecipeException(message);
			}
			this.AddTile(type);
		}

		public void AddTile(ModTile tile)
		{
			this.AddTile(tile.Type);
		}

		public virtual bool RecipeAvailable()
		{
			return true;
		}

		public virtual void OnCraft(Item item)
		{
		}

		//in Terraria.Recipe.Create before alchemy table check add
		//  ModRecipe modRecipe = this as ModRecipe;
		//  if(modRecipe != null) { num = modRecipe.ConsumeItem(item.type, item.stack); }
		public virtual int ConsumeItem(int type, int numRequired)
		{
			return numRequired;
		}

		public void AddRecipe()
		{
			if (this.createItem == null || this.createItem.type == 0)
			{
				throw new RecipeException("A recipe without any result has been added.");
			}
			for (int k = 0; k < Recipe.maxRequirements; k++)
			{
				if (this.requiredTile[k] == TileID.Bottles)
				{
					this.alchemy = true;
					break;
				}
			}
			if (Recipe.numRecipes >= Recipe.maxRecipes)
			{
				Recipe.maxRecipes += 500;
				Array.Resize(ref Main.recipe, Recipe.maxRecipes);
				Array.Resize(ref Main.availableRecipe, Recipe.maxRecipes);
				Array.Resize(ref Main.availableRecipeY, Recipe.maxRecipes);
				for (int k = Recipe.numRecipes; k < Recipe.maxRecipes; k++)
				{
					Main.recipe[k] = new Recipe();
					Main.availableRecipeY[k] = 65f * k;
				}
			}
			Main.recipe[Recipe.numRecipes] = this;
			this.RecipeIndex = Recipe.numRecipes;
			mod.recipes.Add(this);
			Recipe.numRecipes++;
		}
	}
}