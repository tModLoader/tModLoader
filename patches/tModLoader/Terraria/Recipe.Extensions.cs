using System.Linq;
using Terraria.ModLoader;
using Terraria.ModLoader.Exceptions;

namespace Terraria
{
	public partial class Recipe
	{
		#region HasX
		public bool HasResult(int itemID) => createItem.type == itemID;

		public bool HasResult(Mod mod, string itemName) {
			mod ??= Mod;

			if (!ModContent.TryFind(mod.Name, itemName, out ModItem item))
				throw new RecipeException($"The item {itemName} does not exist in the mod {mod.Name}.\r\nIf you are trying to use a vanilla item, try removing the first argument.");

			return HasResult(item.Type);
		}

		public bool HasResult(ModItem item) => HasResult(item.Type);

		public bool HasResult<T>() where T : ModItem => HasResult(ModContent.ItemType<T>());

		public bool HasIngredient(int itemID) {
			foreach (Item item in requiredItem) {
				if (item.type == -1) break;

				if (item.type == itemID) {
					return true;
				}
			}

			return false;
		}

		public bool HasIngredient(Mod mod, string itemName) {
			mod ??= Mod;

			if (!ModContent.TryFind(mod.Name, itemName, out ModItem item))
				throw new RecipeException($"The item {itemName} does not exist in the mod {mod.Name}.\r\nIf you are trying to use a vanilla item, try removing the first argument.");

			return HasIngredient(item.Type);
		}

		public bool HasIngredient(ModItem item) => HasIngredient(item.Type);

		public bool HasIngredient<T>() where T : ModItem => HasIngredient(ModContent.ItemType<T>());

		public bool HasRecipeGroup(int id) {
			foreach (int groupID in acceptedGroups) {
				if (groupID == -1) break;

				if (groupID == id) {
					return true;
				}
			}

			return false;
		}

		public bool HasRecipeGroup(string name) {
			if (!RecipeGroup.recipeGroupIDs.ContainsKey(name))
				throw new RecipeException($"A recipe group with the name {name} does not exist.");

			int id = RecipeGroup.recipeGroupIDs[name];
			return HasRecipeGroup(id);
		}

		public bool HasRecipeGroup(RecipeGroup group) => HasRecipeGroup(group.ID);

		public bool HasTile(int tileID) {
			foreach (int item in requiredTile) {
				if (item == -1) break;

				if (item == tileID)
					return true;
			}

			return false;
		}

		public bool HasTile(Mod mod, string tileName) {
			mod ??= Mod;

			if (!ModContent.TryFind(mod.Name, tileName, out ModTile item))
				throw new RecipeException($"The tile {tileName} does not exist in the mod {mod.Name}.\r\nIf you are trying to use a vanilla tile, try removing the first argument.");

			return HasTile(item.Type);
		}

		public bool HasTile(ModTile tile) => HasTile(tile.Type);

		public bool HasTile<T>() where T : ModTile => HasTile(ModContent.TileType<T>());

		public bool HasCondition(Condition condition) => Conditions.Any(c => c == condition);
		#endregion

		#region TryGetX
		public bool TryGetResult(int itemID, out Item result) {
			if (createItem.type == itemID) {
				result = createItem;
				return true;
			}

			result = null;
			return false;
		}

		public bool TryGetResult(Mod mod, string itemName, out Item result) {
			mod ??= Mod;

			if (!ModContent.TryFind(mod.Name, itemName, out ModItem item))
				throw new RecipeException($"The item {itemName} does not exist in the mod {mod.Name}.\r\nIf you are trying to use a vanilla item, try removing the first argument.");

			return TryGetResult(item.Type, out result);
		}

		public bool TryGetResult(ModItem item, out Item result) => TryGetResult(item.Type, out result);

		public bool TryGetResult<T>(out Item result) where T : ModItem => TryGetResult(ModContent.ItemType<T>(), out result);

		public bool TryGetIngredient(int itemID, out Item ingredient) {
			foreach (Item item in requiredItem) {
				if (item.type == -1) break;

				if (item.type == itemID) {
					ingredient = item;
					return true;
				}
			}

			ingredient = null;
			return false;
		}

		public bool TryGetIngredient(Mod mod, string itemName, out Item ingredient) {
			mod ??= Mod;

			if (!ModContent.TryFind(mod.Name, itemName, out ModItem item))
				throw new RecipeException($"The item {itemName} does not exist in the mod {mod.Name}.\r\nIf you are trying to use a vanilla item, try removing the first argument.");

			return TryGetIngredient(item.Type, out ingredient);
		}

		public bool TryGetIngredient(ModItem item, out Item ingredient) => TryGetIngredient(item.Type, out ingredient);

		public bool TryGetIngredient<T>(out Item ingredient) where T : ModItem => TryGetIngredient(ModContent.ItemType<T>(), out ingredient);
		#endregion

		// should we remove by index or by item/tile type?
		#region RemoveX
		
		#endregion
		
		// create item should get only replaced -> we don't want recipes without a result
		#region ReplaceX
		
		#endregion
	}
}