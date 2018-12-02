using System;
using Terraria.ID;
using Terraria.ModLoader.Exceptions;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This class extends Terraria.Recipe, meaning you can use it in a similar manner to vanilla recipes. However, it provides methods that simplify recipe creation. Recipes are added by creating new instances of ModRecipe, then calling the AddRecipe method.
	/// </summary>
	public class ModRecipe : Recipe
	{
		public readonly Mod mod;
		private int numIngredients = 0;
		private int numTiles = 0;

		/// <summary>
		/// The index of the recipe in the Main.recipe array.
		/// </summary>
		public int RecipeIndex {
			get;
			private set;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="mod">The mod the recipe originates from.</param>
		public ModRecipe(Mod mod) {
			this.mod = mod;
		}

		/// <summary>
		/// Sets the result of this recipe with the given item type and stack size.
		/// </summary>
		/// <param name="itemID">The item identifier.</param>
		/// <param name="stack">The stack.</param>
		public void SetResult(int itemID, int stack = 1) {
			this.createItem.SetDefaults(itemID, false);
			this.createItem.stack = stack;
		}

		/// <summary>
		/// Sets the result of this recipe with the given item name from the given mod, and with the given stack stack. If the mod parameter is null, then it will automatically use an item from the mod creating this recipe.
		/// </summary>
		/// <param name="mod">The mod the item originates from.</param>
		/// <param name="itemName">Name of the item.</param>
		/// <param name="stack">The stack.</param>
		/// <exception cref="RecipeException">The item " + itemName + " does not exist in mod " + mod.Name + ". If you are trying to use a vanilla item, try removing the first argument.</exception>
		public void SetResult(Mod mod, string itemName, int stack = 1) {
			if (mod == null) {
				mod = this.mod;
			}
			int type = mod.ItemType(itemName);
			if (type == 0) {
				string message = "The item " + itemName + " does not exist in the mod " + mod.Name + "." + Environment.NewLine;
				message += "If you are trying to use a vanilla item, try removing the first argument.";
				throw new RecipeException(message);
			}
			this.SetResult(type, stack);
		}

		/// <summary>
		/// Sets the result of this recipe to the given type of item and stack size. Useful in ModItem.AddRecipes.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <param name="stack">The stack.</param>
		public void SetResult(ModItem item, int stack = 1) {
			this.SetResult(item.item.type, stack);
		}

		/// <summary>
		/// Adds an ingredient to this recipe with the given item type and stack size. Ex: <c>recipe.AddIngredient(ItemID.IronAxe)</c>
		/// </summary>
		/// <param name="itemID">The item identifier.</param>
		/// <param name="stack">The stack.</param>
		public void AddIngredient(int itemID, int stack = 1) {
			this.requiredItem[numIngredients].SetDefaults(itemID, false);
			this.requiredItem[numIngredients].stack = stack;
			numIngredients++;
		}

		/// <summary>
		/// Adds an ingredient to this recipe with the given item name from the given mod, and with the given stack stack. If the mod parameter is null, then it will automatically use an item from the mod creating this recipe.
		/// </summary>
		/// <param name="mod">The mod.</param>
		/// <param name="itemName">Name of the item.</param>
		/// <param name="stack">The stack.</param>
		/// <exception cref="RecipeException">The item " + itemName + " does not exist in mod " + mod.Name + ". If you are trying to use a vanilla item, try removing the first argument.</exception>
		public void AddIngredient(Mod mod, string itemName, int stack = 1) {
			if (mod == null) {
				mod = this.mod;
			}
			int type = mod.ItemType(itemName);
			if (type == 0) {
				string message = "The item " + itemName + " does not exist in the mod " + mod.Name + "." + Environment.NewLine;
				message += "If you are trying to use a vanilla item, try removing the first argument.";
				throw new RecipeException(message);
			}
			this.AddIngredient(type, stack);
		}

		/// <summary>
		/// Adds an ingredient to this recipe of the given type of item and stack size.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <param name="stack">The stack.</param>
		public void AddIngredient(ModItem item, int stack = 1) {
			this.AddIngredient(item.item.type, stack);
		}

		/// <summary>
		/// Adds a recipe group ingredient to this recipe with the given RecipeGroup name and stack size. Vanilla recipe groups consist of "Wood", "IronBar", "PresurePlate", "Sand", and "Fragment".
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="stack">The stack.</param>
		/// <exception cref="RecipeException">A recipe group with the name " + name + " does not exist.</exception>
		public void AddRecipeGroup(string name, int stack = 1) {
			if (!RecipeGroup.recipeGroupIDs.ContainsKey(name)) {
				throw new RecipeException("A recipe group with the name " + name + " does not exist.");
			}
			int id = RecipeGroup.recipeGroupIDs[name];
			RecipeGroup rec = RecipeGroup.recipeGroups[id];
			AddIngredient(rec.ValidItems[rec.IconicItemIndex], stack);
			acceptedGroups.Add(id);
		}

		/// <summary>
		/// Adds a required crafting station with the given tile type to this recipe. Ex: <c>recipe.AddTile(TileID.WorkBenches)</c>
		/// </summary>
		/// <param name="tileID">The tile identifier.</param>
		/// <exception cref="RecipeException">No tile has ID " + tileID</exception>
		public void AddTile(int tileID) {
			if (tileID < 0 || tileID >= TileLoader.TileCount) {
				throw new RecipeException("No tile has ID " + tileID);
			}
			this.requiredTile[numTiles] = tileID;
			numTiles++;
		}

		/// <summary>
		/// Adds a required crafting station to this recipe with the given tile name from the given mod. If the mod parameter is null, then it will automatically use a tile from the mod creating this recipe.
		/// </summary>
		/// <param name="mod">The mod.</param>
		/// <param name="tileName">Name of the tile.</param>
		/// <exception cref="RecipeException">The tile " + tileName + " does not exist in mod " + mod.Name + ". If you are trying to use a vanilla tile, try using ModRecipe.AddTile(tileID).</exception>
		public void AddTile(Mod mod, string tileName) {
			if (mod == null) {
				mod = this.mod;
			}
			int type = mod.TileType(tileName);
			if (type == 0) {
				string message = "The tile " + tileName + " does not exist in the mod " + mod.Name + "." + Environment.NewLine;
				message += "If you are trying to use a vanilla tile, try using ModRecipe.AddTile(tileID).";
				throw new RecipeException(message);
			}
			this.AddTile(type);
		}

		/// <summary>
		/// Adds a required crafting station to this recipe of the given type of tile.
		/// </summary>
		/// <param name="tile">The tile.</param>
		public void AddTile(ModTile tile) {
			this.AddTile(tile.Type);
		}

		/// <summary>
		/// Whether or not the conditions are met for this recipe to be available for the player to use. This hook can be used for conditions unrelated to items or tiles (for example, biome or time).
		/// </summary>
		/// <returns>Whether or not the conditions are met for this recipe to be available for the player to use.</returns>
		public virtual bool RecipeAvailable() {
			return true;
		}

		/// <summary>
		/// Allows you to make anything happen when the player uses this recipe. The <paramref name="item"/> parameter is the item the player has just crafted.
		/// </summary>
		/// <param name="item">The item.</param>
		public virtual void OnCraft(Item item) {
		}

		//in Terraria.Recipe.Create before alchemy table check add
		//  ModRecipe modRecipe = this as ModRecipe;
		//  if(modRecipe != null) { num = modRecipe.ConsumeItem(item.type, item.stack); }		
		/// <summary>
		/// Allows you to determine how many of a certain ingredient is consumed when this recipe is used. Return the number of ingredients that will actually be consumed. By default returns numRequired.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="numRequired">The number required.</param>
		/// <returns></returns>
		public virtual int ConsumeItem(int type, int numRequired) {
			return numRequired;
		}

		/// <summary>
		/// Adds this recipe to the game. Call this after you have finished setting the result, ingredients, etc.
		/// </summary>
		/// <exception cref="RecipeException">A recipe without any result has been added.</exception>
		public void AddRecipe() {
			if (this.createItem == null || this.createItem.type == 0) {
				throw new RecipeException("A recipe without any result has been added.");
			}
			if (this.numIngredients > 14 || this.numTiles > 14) {
				throw new RecipeException("A recipe with either too many tiles or too many ingredients has been added. 14 is the max.");
			}
			for (int k = 0; k < Recipe.maxRequirements; k++) {
				if (this.requiredTile[k] == TileID.Bottles) {
					this.alchemy = true;
					break;
				}
			}
			if (Recipe.numRecipes >= Recipe.maxRecipes) {
				Recipe.maxRecipes += 500;
				Array.Resize(ref Main.recipe, Recipe.maxRecipes);
				Array.Resize(ref Main.availableRecipe, Recipe.maxRecipes);
				Array.Resize(ref Main.availableRecipeY, Recipe.maxRecipes);
				for (int k = Recipe.numRecipes; k < Recipe.maxRecipes; k++) {
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