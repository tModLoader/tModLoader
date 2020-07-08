using System;
using Terraria.ID;
using Terraria.ModLoader.Exceptions;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This class extends Terraria.Recipe, meaning you can use it in a similar manner to vanilla recipes. However, it provides methods that simplify recipe creation. Recipes are added by creating new instances of ModRecipe, then calling the <para cref="AddRecipe"/> method.
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
		public ModRecipe SetResult(int itemID, int stack = 1) {
			createItem.SetDefaults(itemID, false);
			createItem.stack = stack;

			return this;
		}

		/// <summary>
		/// Sets the result of this recipe with the given item name from the given mod, and with the given stack stack. If the mod parameter is null, then it will automatically use an item from the mod creating this recipe.
		/// </summary>
		/// <param name="mod">The mod the item originates from.</param>
		/// <param name="itemName">Name of the item.</param>
		/// <param name="stack">The stack.</param>
		/// <exception cref="RecipeException">The item " + itemName + " does not exist in mod " + mod.Name + ". If you are trying to use a vanilla item, try removing the first argument.</exception>
		public ModRecipe SetResult(Mod mod, string itemName, int stack = 1) {
			if (mod == null)
				mod = this.mod;

			int type = mod.ItemType(itemName);
			
			if (type == 0)
				throw new RecipeException($"The item {itemName} does not exist in the mod {mod.Name}.\r\nIf you are trying to use a vanilla item, try removing the first argument.");

			return SetResult(type, stack);
		}

		/// <summary>
		/// Sets the result of this recipe to the given type of item and stack size. Useful in ModItem.AddRecipes.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <param name="stack">The stack.</param>
		public ModRecipe SetResult(ModItem item, int stack = 1) => SetResult(item.item.type, stack);

		/// <summary>
		/// Adds an ingredient to this recipe with the given item type and stack size. Ex: <c>recipe.AddIngredient(ItemID.IronAxe)</c>
		/// </summary>
		/// <param name="itemID">The item identifier.</param>
		/// <param name="stack">The stack.</param>
		public ModRecipe AddIngredient(int itemID, int stack = 1) {
			if (numIngredients >= maxRequirements)
				throw new RecipeException($"Recipe already has the maximum number of ingredients, which is {maxRequirements}.");
			
			requiredItem[numIngredients].SetDefaults(itemID, false);
			requiredItem[numIngredients].stack = stack;
			
			numIngredients++;

			return this;
		}

		/// <summary>
		/// Adds an ingredient to this recipe with the given item name from the given mod, and with the given stack stack. If the mod parameter is null, then it will automatically use an item from the mod creating this recipe.
		/// </summary>
		/// <param name="mod">The mod.</param>
		/// <param name="itemName">Name of the item.</param>
		/// <param name="stack">The stack.</param>
		/// <exception cref="RecipeException">The item " + itemName + " does not exist in mod " + mod.Name + ". If you are trying to use a vanilla item, try removing the first argument.</exception>
		public ModRecipe AddIngredient(Mod mod, string itemName, int stack = 1) {
			if (mod == null)
				mod = this.mod;
			
			int type = mod.ItemType(itemName);

			if (type == 0)
				throw new RecipeException($"The item {itemName} does not exist in the mod {mod.Name}.\r\nIf you are trying to use a vanilla item, try removing the first argument.");

			return AddIngredient(type, stack);
		}

		/// <summary>
		/// Adds an ingredient to this recipe of the given type of item and stack size.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <param name="stack">The stack.</param>
		public ModRecipe AddIngredient(ModItem item, int stack = 1) => AddIngredient(item.item.type, stack);

		/// <summary>
		/// Adds a recipe group ingredient to this recipe with the given RecipeGroup name and stack size. Vanilla recipe groups consist of "Wood", "IronBar", "PresurePlate", "Sand", and "Fragment".
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="stack">The stack.</param>
		/// <exception cref="RecipeException">A recipe group with the name " + name + " does not exist.</exception>
		public ModRecipe AddRecipeGroup(string name, int stack = 1) {
			if (!RecipeGroup.recipeGroupIDs.ContainsKey(name))
				throw new RecipeException($"A recipe group with the name {name} does not exist.");

			int id = RecipeGroup.recipeGroupIDs[name];
			var group = RecipeGroup.recipeGroups[id];
			
			AddIngredient(group.IconicItemId, stack);
			RequireGroup(name);

			return this;
		}

		/// <summary>
		/// Adds a recipe group ingredient to this recipe with the given RecipeGroupID and stack size. Vanilla recipe group IDs can be found in Terraria.ID.RecipeGroupID and modded recipe group IDs will be returned from RecipeGroup.RegisterGroup.
		/// </summary>
		/// <param name="recipeGroupId">The RecipeGroupID.</param>
		/// <param name="stack">The stack.</param>
		/// <exception cref="RecipeException">A recipe group with the ID " + recipeGroupID + " does not exist.</exception>
		public ModRecipe AddRecipeGroup(int recipeGroupId, int stack = 1) {
			if (!RecipeGroup.recipeGroups.ContainsKey(recipeGroupId))
				throw new RecipeException($"A recipe group with the ID {recipeGroupId} does not exist.");
			
			RecipeGroup rec = RecipeGroup.recipeGroups[recipeGroupId];
			
			AddIngredient(rec.IconicItemId, stack);
			RequireGroup(recipeGroupId);

			return this;
		}

		/// <summary>
		/// Adds a required crafting station with the given tile type to this recipe. Ex: <c>recipe.AddTile(TileID.WorkBenches)</c>
		/// </summary>
		/// <param name="tileID">The tile identifier.</param>
		/// <exception cref="RecipeException">No tile has ID " + tileID</exception>
		public ModRecipe AddTile(int tileID) {
			if (numTiles >= maxRequirements)
				throw new RecipeException($"Recipe already has the maximum number of required tiles, which is {maxRequirements}.");

			if (tileID < 0 || tileID >= TileLoader.TileCount)
				throw new RecipeException($"No tile has ID '{tileID}'.");
			
			requiredTile[numTiles++] = tileID;

			return this;
		}

		/// <summary>
		/// Adds a required crafting station to this recipe with the given tile name from the given mod. If the mod parameter is null, then it will automatically use a tile from the mod creating this recipe.
		/// </summary>
		/// <param name="mod">The mod.</param>
		/// <param name="tileName">Name of the tile.</param>
		/// <exception cref="RecipeException">The tile " + tileName + " does not exist in mod " + mod.Name + ". If you are trying to use a vanilla tile, try using ModRecipe.AddTile(tileID).</exception>
		public ModRecipe AddTile(Mod mod, string tileName) {
			if (mod == null)
				mod = this.mod;

			int type = mod.TileType(tileName);
			
			if (type == 0)
				throw new RecipeException($"The tile {tileName} does not exist in the mod {mod.Name}.\r\nIf you are trying to use a vanilla tile, try using ModRecipe.AddTile(tileID).");

			return AddTile(type);
		}

		/// <summary>
		/// Adds a required crafting station to this recipe of the given type of tile.
		/// </summary>
		/// <param name="tile">The tile.</param>
		public ModRecipe AddTile(ModTile tile) => AddTile(tile.Type);

		/// <summary>
		/// Whether or not the conditions are met for this recipe to be available for the player to use. This hook can be used for conditions unrelated to items or tiles (for example, biome or time).
		/// </summary>
		/// <returns>Whether or not the conditions are met for this recipe to be available for the player to use.</returns>
		public virtual bool RecipeAvailable() => true;

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
		public virtual int ConsumeItem(int type, int numRequired) => numRequired;

		/// <summary>
		/// Adds this recipe to the game. Call this after you have finished setting the result, ingredients, etc.
		/// </summary>
		/// <exception cref="RecipeException">A recipe without any result has been added.</exception>
		public void AddRecipe() {
			if (createItem == null || createItem.type == 0)
				throw new RecipeException("A recipe without any result has been added.");
			
			if (numIngredients >= maxRequirements || numTiles >= maxRequirements)
				throw new RecipeException($"A recipe with either too many tiles or too many ingredients has been added. {maxRequirements} is the max.");

			for (int k = 0; k < maxRequirements; k++) {
				if (requiredTile[k] == TileID.Bottles) {
					alchemy = true;
					break;
				}
			}

			if (numRecipes >= maxRecipes) {
				maxRecipes += 500;
				
				Array.Resize(ref Main.recipe, maxRecipes);
				Array.Resize(ref Main.availableRecipe, maxRecipes);
				Array.Resize(ref Main.availableRecipeY, maxRecipes);

				for (int k = numRecipes; k < maxRecipes; k++) {
					Main.recipe[k] = new Recipe();
					Main.availableRecipeY[k] = 65f * k;
				}
			}
			
			Main.recipe[numRecipes] = this;
			
			RecipeIndex = numRecipes;
			
			mod.recipes.Add(this);

			numRecipes++;
		}
	}
}