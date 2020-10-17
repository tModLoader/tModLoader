using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Terraria.Localization;
using Terraria.ModLoader.Exceptions;

namespace Terraria.ModLoader
{
	public class NewRecipeFinder : IEnumerable<Recipe>
	{
		private List<Item> items = new List<Item>();
		private List<int> tiles = new List<int>();
		private List<RecipeGroup> groups = new List<RecipeGroup>();
		private List<Recipe.Condition> conditions = new List<Recipe.Condition>();
		private Item result;

		private NewRecipeFinder() {
		}

		public static NewRecipeFinder Create(int itemID, int stack = 0) {
			if (itemID <= 0 || itemID >= ItemLoader.ItemCount)
				throw new Exception("No item has ID " + itemID);

			if (stack < 0)
				throw new Exception("Stack has to be >= 0 (0 in case you don't care about count)");

			return new NewRecipeFinder { result = new Item(itemID) { stack = stack } };
		}

		public static NewRecipeFinder Create() => new NewRecipeFinder();

		public IEnumerator<Recipe> GetEnumerator() {
			for (int k = 0; k < Recipe.numRecipes; k++) {
				Recipe recipe = Main.recipe[k];

				// result must match
				if (result != null) {
					if (recipe.createItem.IsTheSameAs(result) && result.stack != 0 && recipe.createItem.stack != result.stack) continue;
				}

				// has to contain specified conditions
				List<Recipe.Condition> checkConditions = new List<Recipe.Condition>(conditions);
				foreach (Recipe.Condition condition in recipe.Conditions) {
					for (int i = 0; i < checkConditions.Count; i++) {
						if (condition == checkConditions[i]) {
							checkConditions.RemoveAt(i);
							break;
						}
					}
				}

				if (checkConditions.Count > 0) continue;

				// has to contain specified tiles
				List<int> checkTiles = new List<int>(tiles);
				foreach (int tile in recipe.requiredTile) {
					if (tile == -1) break;

					for (int i = 0; i < checkTiles.Count; i++) {
						if (tile == checkTiles[i]) {
							checkTiles.RemoveAt(i);
							break;
						}
					}
				}

				if (checkTiles.Count > 0) continue;

				// has to contain specified ingredients
				List<Item> checkIngredients = new List<Item>(items);
				foreach (Item ingredient in recipe.requiredItem) {
					if (ingredient.type <= 0) break;

					for (int i = 0; i < checkIngredients.Count; i++) {
						if (ingredient.IsTheSameAs(checkIngredients[i])) {
							if (checkIngredients[i].stack == 0) {
								checkIngredients.RemoveAt(i);
								break;
							}

							if (ingredient.stack == checkIngredients[i].stack) {
								checkIngredients.RemoveAt(i);
								break;
							}
						}
					}
				}

				if (checkIngredients.Count > 0) continue;

				// has to contain specified groups
				List<int> checkGroups = new List<int>(groups.Select(x => x.ID));
				foreach (int group in recipe.acceptedGroups) {
					if (group == -1) break;
					
					for (int i = 0; i < checkGroups.Count; i++) {
						if (group == checkGroups[i]) {
							checkGroups.RemoveAt(i);
							break;
						}
					}
				}
				
				if (checkGroups.Count > 0) continue;

				yield return recipe;
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		#region AddX
		/// <summary>
		///     Adds an ingredient to this recipe with the given item type and stack size. Ex:
		///     <c>recipe.AddIngredient(ItemID.IronAxe)</c>
		/// </summary>
		/// <param name="itemID">The item identifier.</param>
		/// <param name="stack">The stack.</param>
		public NewRecipeFinder AddIngredient(int itemID, int stack = 0) {
			if (itemID <= 0 || itemID >= ItemLoader.ItemCount)
				throw new Exception("No item has ID " + itemID);

			if (stack < 0)
				throw new Exception("Stack has to be >= 0 (0 in case you don't care about count)");

			Item item = new Item(itemID) { stack = stack };
			items.Add(item);
			return this;
		}

		/// <summary>
		///     Adds an ingredient to this recipe with the given item name from the given mod, and with the given stack stack. If
		///     the mod parameter is null, then it will automatically use an item from the mod creating this recipe.
		/// </summary>
		/// <param name="mod">The mod.</param>
		/// <param name="itemName">Name of the item.</param>
		/// <param name="stack">The stack.</param>
		/// <exception cref="RecipeException">
		///     The item " + itemName + " does not exist in mod " + mod.Name + ". If you are trying
		///     to use a vanilla item, try removing the first argument.
		/// </exception>
		public NewRecipeFinder AddIngredient(Mod mod, string itemName, int stack = 0) {
			if (!ModContent.TryFind(mod.Name, itemName, out ModItem item))
				throw new RecipeException($"The item {itemName} does not exist in the mod {mod.Name}.\r\nIf you are trying to use a vanilla item, try removing the first argument.");

			return AddIngredient(item, stack);
		}

		/// <summary>
		///     Adds an ingredient to this recipe of the given type of item and stack size.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <param name="stack">The stack.</param>
		public NewRecipeFinder AddIngredient(ModItem item, int stack = 0) => AddIngredient(item.Type, stack);

		/// <summary>
		///     Adds an ingredient to this recipe of the given type of item and stack size.
		/// </summary>
		/// <typeparam name="T">The item type.</typeparam>
		/// <param name="stack">The stack.</param>
		public NewRecipeFinder AddIngredient<T>(int stack = 0) where T : ModItem
			=> AddIngredient(ModContent.ItemType<T>(), stack);

		/// <summary>
		///     Adds a recipe group ingredient to this recipe with the given RecipeGroup name and stack size. Vanilla recipe groups
		///     consist of "Wood", "IronBar", "PresurePlate", "Sand", and "Fragment".
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="stack">The stack.</param>
		/// <exception cref="RecipeException">A recipe group with the name " + name + " does not exist.</exception>
		public NewRecipeFinder AddRecipeGroup(string name, int stack = 0) {
			if (!RecipeGroup.recipeGroupIDs.ContainsKey(name))
				throw new RecipeException($"A recipe group with the name {name} does not exist.");

			int id = RecipeGroup.recipeGroupIDs[name];
			RecipeGroup group = RecipeGroup.recipeGroups[id];

			AddIngredient(group.IconicItemId, stack);
			groups.Add(group);

			return this;
		}

		/// <summary>
		///     Adds a recipe group ingredient to this recipe with the given RecipeGroupID and stack size. Vanilla recipe group IDs
		///     can be found in Terraria.ID.RecipeGroupID and modded recipe group IDs will be returned from
		///     RecipeGroup.RegisterGroup.
		/// </summary>
		/// <param name="recipeGroupId">The RecipeGroupID.</param>
		/// <param name="stack">The stack.</param>
		/// <exception cref="RecipeException">A recipe group with the ID " + recipeGroupID + " does not exist.</exception>
		public NewRecipeFinder AddRecipeGroup(int recipeGroupId, int stack = 0) {
			if (!RecipeGroup.recipeGroups.ContainsKey(recipeGroupId))
				throw new RecipeException($"A recipe group with the ID {recipeGroupId} does not exist.");

			RecipeGroup rec = RecipeGroup.recipeGroups[recipeGroupId];

			AddIngredient(rec.IconicItemId, stack);
			groups.Add(rec);

			return this;
		}

		/// <summary>
		///     Adds a recipe group ingredient to this recipe with the given RecipeGroup.
		/// </summary>
		/// <param name="recipeGroup">The RecipeGroup.</param>
		/// <param name="stack">The stack.</param>
		public NewRecipeFinder AddRecipeGroup(RecipeGroup recipeGroup, int stack = 0) {
			AddIngredient(recipeGroup.IconicItemId, stack);
			groups.Add(recipeGroup);

			return this;
		}

		/// <summary>
		///     Adds a required crafting station with the given tile type to this recipe. Ex:
		///     <c>recipe.AddTile(TileID.WorkBenches)</c>
		/// </summary>
		/// <param name="tileID">The tile identifier.</param>
		/// <exception cref="RecipeException">No tile has ID " + tileID</exception>
		public NewRecipeFinder AddTile(int tileID) {
			if (tileID < 0 || tileID >= TileLoader.TileCount)
				throw new RecipeException($"No tile has ID '{tileID}'.");

			tiles.Add(tileID);

			return this;
		}

		/// <summary>
		///     Adds a required crafting station to this recipe with the given tile name from the given mod. If the mod parameter
		///     is null, then it will automatically use a tile from the mod creating this recipe.
		/// </summary>
		/// <param name="mod">The mod.</param>
		/// <param name="tileName">Name of the tile.</param>
		/// <exception cref="RecipeException">
		///     The tile " + tileName + " does not exist in mod " + mod.Name + ". If you are trying
		///     to use a vanilla tile, try using Recipe.AddTile(tileID).
		/// </exception>
		public NewRecipeFinder AddTile(Mod mod, string tileName) {
			if (!ModContent.TryFind(mod.Name, tileName, out ModTile tile))
				throw new RecipeException($"The tile {tileName} does not exist in the mod {mod.Name}.\r\nIf you are trying to use a vanilla tile, try using Recipe.AddTile(tileID).");

			return AddTile(tile);
		}

		/// <summary>
		///     Adds a required crafting station to this recipe of the given type of tile.
		/// </summary>
		/// <param name="tile">The tile.</param>
		public NewRecipeFinder AddTile(ModTile tile) => AddTile(tile.Type);

		/// <summary>
		///     Adds a required crafting station to this recipe of the given type of tile.
		/// </summary>
		public NewRecipeFinder AddTile<T>() where T : ModTile
			=> AddTile(ModContent.TileType<T>());

		/// <summary>
		///     Sets a condition delegate that will determine whether or not the recipe will be to be available for the player to
		///     use. The condition can be unrelated to items or tiles (for example, biome or time).
		/// </summary>
		/// <param name="condition">The predicate delegate condition.</param>
		/// <param name="description">
		///     A description of this condition. Use NetworkText.FromKey, or NetworkText.FromLiteral for
		///     this.
		/// </param>
		public NewRecipeFinder AddCondition(NetworkText description, Predicate<Recipe> condition) => AddCondition(new Recipe.Condition(description, condition));

		/// <summary>
		///     Adds an array of conditions that will determine whether or not the recipe will be to be available for the player to
		///     use. The conditions can be unrelated to items or tiles (for example, biome or time).
		/// </summary>
		/// <param name="conditions">An array of conditions.</param>
		public NewRecipeFinder AddCondition(params Recipe.Condition[] conditions) => AddCondition((IEnumerable<Recipe.Condition>)conditions);

		/// <summary>
		///     Adds a collectiom of conditions that will determine whether or not the recipe will be to be available for the
		///     player to use. The conditions can be unrelated to items or tiles (for example, biome or time).
		/// </summary>
		/// <param name="conditions">A collection of conditions.</param>
		public NewRecipeFinder AddCondition(IEnumerable<Recipe.Condition> conditions) {
			this.conditions.AddRange(conditions);

			return this;
		}
		#endregion
	}
}