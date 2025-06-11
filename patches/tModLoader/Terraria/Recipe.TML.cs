using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Exceptions;

#pragma warning disable IDE0060 //Remove unused parameter.

namespace Terraria;

/// <summary>
/// A Recipe is a collection of ingredients, tiles, and a resulting Item. This is how players can craft items in the game.<br/>
/// The <see href="https://github.com/tModLoader/tModLoader/wiki/Basic-Recipes">Basic Recipes Guide</see> teaches how to add new recipes to the game and how to manipulate existing recipes.<br/>
/// Use <see cref="Recipe.Create(int, int)"/> to create a Recipe instance resulting in the specified item. Use <see cref="AddIngredient(int, int)"/> to add ingredients and <see cref="AddTile(int)"/> to add crafting stations. Finally, use <see cref="Register"/> to complete the recipe and register it to the game.<br/>
/// Recipes can only be added in <see cref="ModSystem.AddRecipes"/>, <see cref="ModItem.AddRecipes"/>, and <see cref="GlobalItem.AddRecipes"/>.<br/>
/// Recipes should be edited only in <see cref="ModSystem.PostAddRecipes"/>.
/// </summary>
public partial class Recipe
{
	[Obsolete($"Replaced by {nameof(IngredientQuantityRules)} due to not accounting for shimmer decrafting")]
	public static class ConsumptionRules
	{
		[Obsolete($"Replaced by {nameof(IngredientQuantityRules)}.{nameof(IngredientQuantityRules.Alchemy)} due to not accounting for shimmer decrafting")]
		public static ConsumeItemCallback Alchemy = (Recipe recipe, int type, ref int amount) => IngredientQuantityRules.Alchemy(recipe, type, ref amount, false);
	}

	public static class IngredientQuantityRules
	{
		/// <summary> Gives 1/3 chance for every ingredient to not be consumed, if used at an alchemy table. (!) This behaviour is already automatically given to all items that can be made at a placed bottle tile. </summary>
		public static IngredientQuantityCallback Alchemy = (Recipe recipe, int type, ref int amount, bool isDecrafting) => {
			if (!Main.LocalPlayer.alchemyTable && !isDecrafting)
				return;
			for (int i = amount; i > 0; i--) {
				if (Main.rand.NextBool(3))
					amount--;
			}
		};
	}

	public readonly Mod Mod;
	public readonly List<Condition> Conditions = new();
	public readonly List<Condition> DecraftConditions = new();

	public delegate void OnCraftCallback(Recipe recipe, Item item, List<Item> consumedItems, Item destinationStack);

	[Obsolete($"Replaced by {nameof(IngredientQuantityCallback)} due to not accounting for shimmer decrafting")]
	public delegate void ConsumeItemCallback(Recipe recipe, int type, ref int amount);
	/// <summary>
	/// Called for both <see cref="Create()"/> and <see cref="Item.GetShimmered"/>, using <paramref name="isDecrafting"/> = <see langword="true"/> to denote a shimmer operation
	/// </summary>
	public delegate void IngredientQuantityCallback(Recipe recipe, int type, ref int amount, bool isDecrafting);
	internal OnCraftCallback OnCraftHooks { get; private set; }
	internal IngredientQuantityCallback ConsumeIngredientHooks { get; private set; }

	private void AddGroup(int id)
	{
		acceptedGroups.Add(id);
	}

	/// <summary>
	/// The index of the recipe in the Main.recipe array.
	/// </summary>
	public int RecipeIndex { get; internal set; }

	public (Recipe target, bool after) Ordering { get; internal set; }

	/// <summary>
	/// Any recipe with this flag won't be shown in game.
	/// </summary>
	public bool Disabled { get; private set; }

	/// <summary>
	/// Any recipe with this flag won't be decrafted in shimmer.
	/// </summary>
	public bool DecraftDisabled => notDecraftable;

	/// <summary>
	/// Adds an ingredient to this recipe with the given item type and stack size. Ex: <c>recipe.AddIngredient(ItemID.IronAxe)</c>
	/// </summary>
	/// <param name="itemID">The item identifier.</param>
	/// <param name="stack">The stack.</param>
	public Recipe AddIngredient(int itemID, int stack = 1)
	{
		requiredItem.Add(new Item(itemID) { stack = stack });

		return this;
	}

	/// <summary>
	/// Adds an ingredient to this recipe with the given item name from the given mod, and with the given stack stack. If the mod parameter is null, then it will automatically use an item from the mod creating this recipe.
	/// </summary>
	/// <param name="mod">The mod.</param>
	/// <param name="itemName">Name of the item.</param>
	/// <param name="stack">The stack.</param>
	/// <exception cref="RecipeException">The item " + itemName + " does not exist in mod " + mod.Name + ". If you are trying to use a vanilla item, try removing the first argument.</exception>
	public Recipe AddIngredient(Mod mod, string itemName, int stack = 1)
	{
		mod ??= this.Mod;

		if (!ModContent.TryFind(mod.Name, itemName, out ModItem item))
			throw new RecipeException($"The item {itemName} does not exist in the mod {mod.Name}.\r\nIf you are trying to use a vanilla item, try removing the first argument.");

		return AddIngredient(item, stack);
	}

	/// <summary>
	/// Adds an ingredient to this recipe of the given type of item and stack size.
	/// </summary>
	/// <param name="item">The item.</param>
	/// <param name="stack">The stack.</param>
	public Recipe AddIngredient(ModItem item, int stack = 1) => AddIngredient(item.Type, stack);

	/// <summary>
	/// Adds an ingredient to this recipe of the given type of item and stack size.
	/// </summary>
	/// <param name="stack">The stack.</param>
	public Recipe AddIngredient<T>(int stack = 1) where T : ModItem
		=> AddIngredient(ModContent.ItemType<T>(), stack);

	/// <summary>
	/// Adds a recipe group ingredient to this recipe with the given RecipeGroup name and stack size.
	/// <br/> Recipe groups allow a recipe to use alternate ingredients without making multiple recipes. For example the "IronBar" group accepts either <see cref="ItemID.IronBar"/> or <see cref="ItemID.LeadBar"/>. The <see href="https://github.com/tModLoader/tModLoader/wiki/Intermediate-Recipes#recipe-groups">Recipe Groups wiki guide</see> has more information.
	/// <br/> To use a vanilla recipe group, use <see cref="AddRecipeGroup(int, int)"/> using a <see cref="RecipeGroupID"/> entry instead.
	/// </summary>
	/// <param name="name">The name.</param>
	/// <param name="stack">The stack.</param>
	/// <exception cref="RecipeException">A recipe group with the name " + name + " does not exist.</exception>
	public Recipe AddRecipeGroup(string name, int stack = 1)
	{
		if (!RecipeGroup.recipeGroupIDs.ContainsKey(name))
			throw new RecipeException($"A recipe group with the name {name} does not exist.");

		int id = RecipeGroup.recipeGroupIDs[name];
		var group = RecipeGroup.recipeGroups[id];

		AddIngredient(group.IconicItemId, stack);
		AddGroup(id);

		return this;
	}

	/// <summary>
	/// Adds a recipe group ingredient to this recipe with the given RecipeGroupID and stack size.
	/// <br/> Recipe groups allow a recipe to use alternate ingredients without making multiple recipes. For example the <see cref="RecipeGroupID.IronBar"/> group accepts either <see cref="ItemID.IronBar"/> or <see cref="ItemID.LeadBar"/>. The <see href="https://github.com/tModLoader/tModLoader/wiki/Intermediate-Recipes#recipe-groups">Recipe Groups wiki guide</see> has more information.
	/// <br/> Vanilla recipe group IDs can be found in <see cref="RecipeGroupID"/> and modded recipe group IDs will be returned from <see cref="RecipeGroup.RegisterGroup(string, RecipeGroup)"/>. <see cref="AddRecipeGroup(string, int)"/> can be used instead if the ID number is not known but the name is known.
	/// </summary>
	/// <param name="recipeGroupId">The RecipeGroupID.</param>
	/// <param name="stack">The stack.</param>
	/// <exception cref="RecipeException">A recipe group with the ID " + recipeGroupID + " does not exist.</exception>
	public Recipe AddRecipeGroup(int recipeGroupId, int stack = 1)
	{
		if (!RecipeGroup.recipeGroups.ContainsKey(recipeGroupId))
			throw new RecipeException($"A recipe group with the ID {recipeGroupId} does not exist.");

		RecipeGroup rec = RecipeGroup.recipeGroups[recipeGroupId];

		AddIngredient(rec.IconicItemId, stack);
		AddGroup(recipeGroupId);

		return this;
	}

	/// <summary>
	/// Adds a recipe group ingredient to this recipe with the given RecipeGroup.
	/// </summary>
	/// <param name="recipeGroup">The RecipeGroup.</param>
	/// <param name="stack"></param>
	public Recipe AddRecipeGroup(RecipeGroup recipeGroup, int stack = 1)
	{
		AddIngredient(recipeGroup.IconicItemId, stack);
		AddGroup(recipeGroup.RegisteredId);

		return this;
	}

	/// <summary>
	/// Adds a required crafting station with the given tile type to this recipe. Ex: <c>recipe.AddTile(TileID.WorkBenches)</c>
	/// </summary>
	/// <param name="tileID">The tile identifier.</param>
	/// <exception cref="RecipeException">No tile has ID " + tileID</exception>
	public Recipe AddTile(int tileID)
	{
		if (tileID < 0 || tileID >= TileLoader.TileCount)
			throw new RecipeException($"No tile has ID '{tileID}'.");

		requiredTile.Add(tileID);

		return this;
	}

	/// <summary>
	/// Adds a required crafting station to this recipe with the given tile name from the given mod. If the mod parameter is null, then it will automatically use a tile from the mod creating this recipe.
	/// </summary>
	/// <param name="mod">The mod.</param>
	/// <param name="tileName">Name of the tile.</param>
	/// <exception cref="RecipeException">The tile " + tileName + " does not exist in mod " + mod.Name + ". If you are trying to use a vanilla tile, try using Recipe.AddTile(tileID).</exception>
	public Recipe AddTile(Mod mod, string tileName)
	{
		mod ??= this.Mod;

		if (!ModContent.TryFind(mod.Name, tileName, out ModTile tile))
			throw new RecipeException($"The tile {tileName} does not exist in the mod {mod.Name}.\r\nIf you are trying to use a vanilla tile, try using Recipe.AddTile(tileID).");

		return AddTile(tile);
	}

	/// <summary>
	/// Adds a required crafting station to this recipe of the given type of tile.
	/// </summary>
	/// <param name="tile">The tile.</param>
	public Recipe AddTile(ModTile tile) => AddTile(tile.Type);

	/// <summary>
	/// Adds a required crafting station to this recipe of the given type of tile.
	/// </summary>
	public Recipe AddTile<T>() where T : ModTile
		=> AddTile(ModContent.TileType<T>());

	/// <summary>
	/// Sets a condition delegate that will determine whether or not the recipe will be to be available for the player to use. The condition can be unrelated to items or tiles (for example, biome or time).
	/// </summary>
	/// <param name="description">A description of this condition.</param>
	/// <param name="condition">A function returning whether the condition is met.</param>
	public Recipe AddCondition(LocalizedText description, Func<bool> condition) => AddCondition(new Condition(description, condition));

	/// <summary>
	/// Adds an array of conditions that will determine whether or not the recipe will be to be available for the player to use. The conditions can be unrelated to items or tiles (for example, biome or time).
	/// </summary>
	/// <param name="conditions">An array of conditions.</param>
	public Recipe AddCondition(params Condition[] conditions) => AddCondition((IEnumerable<Condition>)conditions);

	public Recipe AddCondition(Condition condition)
	{
		Conditions.Add(condition);

		return this;
	}

	/// <summary>
	/// Adds a collection of conditions that will determine whether or not the recipe will be to be available for the player to use. The conditions can be unrelated to items or tiles (for example, biome or time).
	/// </summary>
	/// <param name="conditions">A collection of conditions.</param>
	public Recipe AddCondition(IEnumerable<Condition> conditions)
	{
		Conditions.AddRange(conditions);

		return this;
	}

	/// <summary>
	/// Sets a condition delegate that will determine whether or not the recipe can be shimmered/decrafted. The condition can be unrelated to items or tiles (for example, biome or time).
	/// </summary>
	/// <param name="description">A description of this condition.</param>
	/// <param name="condition">The predicate delegate condition.</param>
	public Recipe AddDecraftCondition(LocalizedText description, Func<bool> condition) => AddDecraftCondition(new Condition(description, condition));

	/// <summary>
	/// Adds an array of conditions that will determine whether or not the recipe can be shimmered/decrafted. The conditions can be unrelated to items or tiles (for example, biome or time).
	/// </summary>
	/// <param name="conditions">An array of conditions.</param>
	public Recipe AddDecraftCondition(params Condition[] conditions) => AddDecraftCondition((IEnumerable<Condition>)conditions);

	public Recipe AddDecraftCondition(Condition condition)
	{
		DecraftConditions.Add(condition);
		return this;
	}

	/// <summary>
	/// Adds a collection of conditions that will determine whether or not the recipe can be shimmered/decrafted. The conditions can be unrelated to items or tiles (for example, biome or time).
	/// </summary>
	/// <param name="conditions">A collection of conditions.</param>
	public Recipe AddDecraftCondition(IEnumerable<Condition> conditions)
	{
		DecraftConditions.AddRange(conditions);
		return this;
	}

	/// <summary>
	/// Adds every condition from Recipe.Conditions to Recipe.DecraftConditions, checking for duplicates.
	/// </summary>
	public Recipe ApplyConditionsAsDecraftConditions()
	{
		foreach (Condition condition in Conditions) {
			if (!DecraftConditions.Contains(condition)) {
				DecraftConditions.Add(condition);
			}
		}
		return this;
	}

	/// <summary>
	/// Sets a check that is used during load to prevent this being shimmered/decrafted.
	/// </summary>
	/// <exception cref="RecipeException">A Recipe can only be disabled inside Recipe related methods.</exception>
	public Recipe DisableDecraft()
	{
		if (!RecipeLoader.setupRecipes)
			throw new RecipeException("A Recipe can only be disabled inside Recipe related methods.");

		notDecraftable = true;
		return this;
	}

	/// <summary>
	/// Sets a callback that will allow you to make anything happen when the recipe is used to create an item.
	/// </summary>
	public Recipe AddOnCraftCallback(OnCraftCallback callback)
	{
		OnCraftHooks += callback;

		return this;
	}

	/// <summary>
	/// Sets a callback that allows you to determine how many of a certain ingredient is consumed when this recipe is used. Return the number of ingredients that will actually be consumed. By default returns numRequired.
	/// </summary>

	[Obsolete($"Replaced by {nameof(AddConsumeIngredientCallback)} due to not accounting for shimmer decrafting")]
	public Recipe AddConsumeItemCallback(ConsumeItemCallback callback)
	{
		ConsumeIngredientHooks += (Recipe recipe, int type, ref int num, bool decraft) => callback(recipe, type, ref num);

		return this;
	}

	/// <summary>
	/// Sets a callback that allows you to determine how many of a certain ingredient is consumed when this recipe is used. Return the number of ingredients that will actually be consumed. By default returns numRequired.
	/// </summary>
	public Recipe AddConsumeIngredientCallback(IngredientQuantityCallback callback)
	{
		ConsumeIngredientHooks += callback;

		return this;
	}

	#region Ordering

	/// <summary>
	/// Sets the Ordering of this recipe. This recipe can't already have one.
	/// </summary>
	private Recipe SetOrdering(Recipe recipe, bool after)
	{
		if (!RecipeLoader.setupRecipes)
			throw new RecipeException("You can only move recipes during setup");
		if (Main.recipe[recipe.RecipeIndex] != recipe)
			throw new RecipeException("The selected recipe is not registered.");
		if (Ordering.target != null)
			throw new RecipeException("This recipe already has an ordering.");
		Ordering = (recipe, after);

		var target = recipe;
		do {
			if (target == this)
				throw new Exception("Recipe ordering loop!");

			target = target.Ordering.target;
		} while (target != null);


		return this;
	}

	/// <summary>
	/// Sorts the recipe before the first one creating the item of the ID given as parameter.
	/// </summary>
	public Recipe SortBeforeFirstRecipesOf(int itemId)
	{
		Recipe target = RecipeLoader.FirstRecipeForItem[itemId];
		if (target != null) {
			return SortBefore(target);
		}

		return this;
	}

	/// <summary>
	/// Sorts the recipe before the one given as parameter. Both recipes must already be registered.
	/// </summary>
	public Recipe SortBefore(Recipe recipe) => SetOrdering(recipe, false);

	/// <summary>
	/// Sorts the recipe after the first one creating the item of the ID given as parameter.
	/// </summary>
	public Recipe SortAfterFirstRecipesOf(int itemId)
	{
		Recipe target = RecipeLoader.FirstRecipeForItem[itemId];
		if (target != null) {
			return SortAfter(target);
		}

		return this;
	}

	/// <summary>
	/// Sorts the recipe after the one given as parameter. Both recipes must already be registered.
	/// </summary>
	public Recipe SortAfter(Recipe recipe) => SetOrdering(recipe, true);

	#endregion

	/// <summary>
	/// Returns a clone of this recipe except the source mod of the Recipe will the currently loading mod.
	/// <br/> The clone will have to be registered after being tweaked.
	/// </summary>
	public Recipe Clone()
	{
		if (!RecipeLoader.setupRecipes)
			throw new RecipeException("A Recipe can only be cloned inside recipe related methods");

		ArgumentNullException.ThrowIfNull(RecipeLoader.CurrentMod);
		var clone = new Recipe(RecipeLoader.CurrentMod);

		clone.createItem = createItem.Clone();

		clone.requiredItem = new List<Item>(requiredItem.Select(x => x.Clone()).ToArray());
		clone.requiredTile = new List<int>(requiredTile.ToArray());
		clone.acceptedGroups = new List<int>(acceptedGroups.ToArray());
		clone.notDecraftable = notDecraftable;
		clone.crimson = crimson;
		clone.corruption = corruption;

		// These fields shouldn't be true, but are here just in case.
		clone.needHoney = needHoney;
		clone.needWater = needWater;
		clone.needLava = needLava;
		clone.anyWood = anyWood;
		clone.anyIronBar = anyIronBar;
		clone.anyPressurePlate = anyPressurePlate;
		clone.anySand = anySand;
		clone.anyFragment = anyFragment;
		clone.needSnowBiome = needSnowBiome;
		clone.needGraveyardBiome = needGraveyardBiome;
		clone.needEverythingSeed = needEverythingSeed;

		clone.OnCraftHooks = OnCraftHooks;
		clone.ConsumeIngredientHooks = ConsumeIngredientHooks;
		foreach (Condition condition in Conditions) {
			clone.AddCondition(condition);
		}

		foreach (Condition condition in DecraftConditions) {
			clone.AddDecraftCondition(condition);
		}

		// A subsequent call to Register() will re-add this hook if Bottles is a required tile, so we remove
		// it here to not have multiple duplicate hooks.
		if (clone.requiredTile.Contains(TileID.Bottles))
			clone.ConsumeIngredientHooks -= IngredientQuantityRules.Alchemy;

		return clone;
	}

	/// <summary>
	/// Adds this recipe to the game. Call this after you have finished setting the result, ingredients, etc.
	/// </summary>
	/// <exception cref="RecipeException">A recipe without any result has been added.</exception>
	public Recipe Register()
	{
		if (createItem == null || createItem.type == 0)
			throw new RecipeException("A recipe without any result has been added.");

		if (RecipeIndex >= 0)
			throw new RecipeException("There was an attempt to register an already registered recipe.");

		if (requiredTile.Contains(TileID.Bottles))
			AddConsumeIngredientCallback(IngredientQuantityRules.Alchemy);

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
		numRecipes++;

		if (RecipeLoader.FirstRecipeForItem[createItem.type] == null)
			RecipeLoader.FirstRecipeForItem[createItem.type] = this;

		return this;
	}

	/// <summary>
	/// Creates a recipe resulting in the given item and amount but does not yet register it into the game. Call this at the very beginning when creating a new craft.
	/// </summary>
	/// <param name="result">What item will be given when the craft has been completed</param>
	/// <param name="amount">The stack -> how many result items given when the recipe is crafted. (eg. 1 wood -> 4 wood platform)</param>
	/// <exception cref="RecipeException">A Recipe can only be created inside recipe related methods</exception>
	public static Recipe Create(int result, int amount = 1)
	{
		if (!RecipeLoader.setupRecipes)
			throw new RecipeException("A Recipe can only be created inside recipe related methods");

		ArgumentNullException.ThrowIfNull(RecipeLoader.CurrentMod);
		var recipe = new Recipe(RecipeLoader.CurrentMod);

		recipe.createItem.SetDefaults(result, false);
		recipe.createItem.stack = amount;

		return recipe;
	}

	private static void FixRecipeGroups()
	{
		// Remove recipe group assignments to recipes that don't actually have any items in the recipe groups anymore for one reason or another.
		for (int i = 0; i < numRecipes; i++) {
			Recipe recipe = Main.recipe[i];

			if (recipe.acceptedGroups.Count > 0) {
				var toRemove = new List<int>();

				foreach (int num in recipe.acceptedGroups) {
					if (!RecipeGroup.recipeGroups[num].ValidItems.Intersect(recipe.requiredItem.Select(x => x.type)).Any()) {
						toRemove.Add(num);
					}
				}

				foreach (int group in toRemove) {
					recipe.acceptedGroups.Remove(group);
				}
			}
		}
	}
}
