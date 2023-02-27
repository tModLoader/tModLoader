using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Exceptions;

#pragma warning disable IDE0060 //Remove unused parameter.

namespace Terraria;

public partial class Recipe
{
	public interface ICondition
	{
		string Description { get; }

		bool RecipeAvailable(Recipe recipe);
	}

	public sealed class Condition : ICondition
	{
		#region Conditions

		//Liquids
		public static readonly Condition NearWater = new Condition(NetworkText.FromKey("RecipeConditions.NearWater"), _ => Main.LocalPlayer.adjWater || Main.LocalPlayer.adjTile[TileID.Sinks]);
		public static readonly Condition NearLava = new Condition(NetworkText.FromKey("RecipeConditions.NearLava"), _ => Main.LocalPlayer.adjLava);
		public static readonly Condition NearHoney = new Condition(NetworkText.FromKey("RecipeConditions.NearHoney"), _ => Main.LocalPlayer.adjHoney);
		//Time
		public static readonly Condition TimeDay = new Condition(NetworkText.FromKey("RecipeConditions.TimeDay"), _ => Main.dayTime);
		public static readonly Condition TimeNight = new Condition(NetworkText.FromKey("RecipeConditions.TimeNight"), _ => !Main.dayTime);
		//Biomes
		public static readonly Condition InDungeon = new Condition(NetworkText.FromKey("RecipeConditions.InDungeon"), _ => Main.LocalPlayer.ZoneDungeon);
		public static readonly Condition InCorrupt = new Condition(NetworkText.FromKey("RecipeConditions.InCorrupt"), _ => Main.LocalPlayer.ZoneCorrupt);
		public static readonly Condition InHallow = new Condition(NetworkText.FromKey("RecipeConditions.InHallow"), _ => Main.LocalPlayer.ZoneHallow);
		public static readonly Condition InMeteor = new Condition(NetworkText.FromKey("RecipeConditions.InMeteor"), _ => Main.LocalPlayer.ZoneMeteor);
		public static readonly Condition InJungle = new Condition(NetworkText.FromKey("RecipeConditions.InJungle"), _ => Main.LocalPlayer.ZoneJungle);
		public static readonly Condition InSnow = new Condition(NetworkText.FromKey("RecipeConditions.InSnow"), _ => Main.LocalPlayer.ZoneSnow);
		public static readonly Condition InCrimson = new Condition(NetworkText.FromKey("RecipeConditions.InCrimson"), _ => Main.LocalPlayer.ZoneCrimson);
		public static readonly Condition InWaterCandle = new Condition(NetworkText.FromKey("RecipeConditions.InWaterCandle"), _ => Main.LocalPlayer.ZoneWaterCandle);
		public static readonly Condition InPeaceCandle = new Condition(NetworkText.FromKey("RecipeConditions.InPeaceCandle"), _ => Main.LocalPlayer.ZonePeaceCandle);
		public static readonly Condition InTowerSolar = new Condition(NetworkText.FromKey("RecipeConditions.InTowerSolar"), _ => Main.LocalPlayer.ZoneTowerSolar);
		public static readonly Condition InTowerVortex = new Condition(NetworkText.FromKey("RecipeConditions.InTowerVortex"), _ => Main.LocalPlayer.ZoneTowerVortex);
		public static readonly Condition InTowerNebula = new Condition(NetworkText.FromKey("RecipeConditions.InTowerNebula"), _ => Main.LocalPlayer.ZoneTowerNebula);
		public static readonly Condition InTowerStardust = new Condition(NetworkText.FromKey("RecipeConditions.InTowerStardust"), _ => Main.LocalPlayer.ZoneTowerStardust);
		public static readonly Condition InDesert = new Condition(NetworkText.FromKey("RecipeConditions.InDesert"), _ => Main.LocalPlayer.ZoneDesert);
		public static readonly Condition InGlowshroom = new Condition(NetworkText.FromKey("RecipeConditions.InGlowshroom"), _ => Main.LocalPlayer.ZoneGlowshroom);
		public static readonly Condition InUndergroundDesert = new Condition(NetworkText.FromKey("RecipeConditions.InUndergroundDesert"), _ => Main.LocalPlayer.ZoneUndergroundDesert);
		public static readonly Condition InSkyHeight = new Condition(NetworkText.FromKey("RecipeConditions.InSkyHeight"), _ => Main.LocalPlayer.ZoneSkyHeight);
		public static readonly Condition InOverworldHeight = new Condition(NetworkText.FromKey("RecipeConditions.InOverworldHeight"), _ => Main.LocalPlayer.ZoneOverworldHeight);
		public static readonly Condition InDirtLayerHeight = new Condition(NetworkText.FromKey("RecipeConditions.InDirtLayerHeight"), _ => Main.LocalPlayer.ZoneDirtLayerHeight);
		public static readonly Condition InRockLayerHeight = new Condition(NetworkText.FromKey("RecipeConditions.InRockLayerHeight"), _ => Main.LocalPlayer.ZoneRockLayerHeight);
		public static readonly Condition InUnderworldHeight = new Condition(NetworkText.FromKey("RecipeConditions.InUnderworldHeight"), _ => Main.LocalPlayer.ZoneUnderworldHeight);
		public static readonly Condition InBeach = new Condition(NetworkText.FromKey("RecipeConditions.InBeach"), _ => Main.LocalPlayer.ZoneBeach);
		public static readonly Condition InRain = new Condition(NetworkText.FromKey("RecipeConditions.InRain"), _ => Main.LocalPlayer.ZoneRain);
		public static readonly Condition InSandstorm = new Condition(NetworkText.FromKey("RecipeConditions.InSandstorm"), _ => Main.LocalPlayer.ZoneSandstorm);
		public static readonly Condition InOldOneArmy = new Condition(NetworkText.FromKey("RecipeConditions.InOldOneArmy"), _ => Main.LocalPlayer.ZoneOldOneArmy);
		public static readonly Condition InGranite = new Condition(NetworkText.FromKey("RecipeConditions.InGranite"), _ => Main.LocalPlayer.ZoneGranite);
		public static readonly Condition InMarble = new Condition(NetworkText.FromKey("RecipeConditions.InMarble"), _ => Main.LocalPlayer.ZoneMarble);
		public static readonly Condition InHive = new Condition(NetworkText.FromKey("RecipeConditions.InHive"), _ => Main.LocalPlayer.ZoneHive);
		public static readonly Condition InGemCave = new Condition(NetworkText.FromKey("RecipeConditions.InGemCave"), _ => Main.LocalPlayer.ZoneGemCave);
		public static readonly Condition InLihzhardTemple = new Condition(NetworkText.FromKey("RecipeConditions.InLihzardTemple"), _ => Main.LocalPlayer.ZoneLihzhardTemple);
		public static readonly Condition InGraveyardBiome = new Condition(NetworkText.FromKey("RecipeConditions.InGraveyardBiome"), _ => Main.LocalPlayer.ZoneGraveyard);
		//WorldType
		public static readonly Condition EverythingSeed = new Condition(NetworkText.FromKey("RecipeConditions.EverythingSeed"), _ => Main.remixWorld && Main.getGoodWorld);
		public static readonly Condition CrimsonWorld = new Condition(NetworkText.FromKey("RecipeConditions.CrimsonWorld"), _ => WorldGen.crimson);
		public static readonly Condition CorruptWorld = new Condition(NetworkText.FromKey("RecipeConditions.CorruptWorld"), _ => !WorldGen.crimson);

		#endregion

		private readonly NetworkText DescriptionText;
		private readonly Predicate<Recipe> Predicate;

		public string Description => DescriptionText.ToString();

		public Condition(NetworkText description, Predicate<Recipe> predicate)
		{
			DescriptionText = description ?? throw new ArgumentNullException(nameof(description));
			Predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
		}

		public bool RecipeAvailable(Recipe recipe) => Predicate(recipe);
	}

	public static class ConsumptionRules
	{
		/// <summary> Gives 1/3 chance for every ingredient to not be consumed, if used at an alchemy table. (!) This behavior is already automatically given to all items that can be made at a placed bottle tile. </summary>
		public static ConsumeItemCallback Alchemy = (Recipe recipe, int type, ref int amount) => {
			if (!Main.LocalPlayer.alchemyTable) return;

			int amountUsed = 0;

			for (int i = 0; i < amount; i++) {
				if (!Main.rand.NextBool(3))
					amountUsed++;
			}

			amount = amountUsed;
		};
	}

	public readonly Mod Mod;
	public readonly List<Condition> Conditions = new List<Condition>();
	public readonly List<Condition> DecraftConditions = new List<Condition>();

	public delegate void OnCraftCallback(Recipe recipe, Item item, List<Item> consumedItems, Item destinationStack);
	public delegate void ConsumeItemCallback(Recipe recipe, int type, ref int amount);

	internal OnCraftCallback OnCraftHooks { get; private set; }
	internal ConsumeItemCallback ConsumeItemHooks { get; private set; }

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
	/// <param name="condition">The predicate delegate condition.</param>
	/// <param name="description">A description of this condition. Use NetworkText.FromKey, or NetworkText.FromLiteral for this.</param>
	public Recipe AddCondition(NetworkText description, Predicate<Recipe> condition) => AddCondition(new Condition(description, condition));

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
	/// <param name="condition">The predicate delegate condition.</param>
	/// <param name="description">A description of this condition. Use NetworkText.FromKey, or NetworkText.FromLiteral for this.</param>
	public Recipe AddDecraftCondition(NetworkText description, Predicate<Recipe> condition) => AddDecraftCondition(new Condition(description, condition));

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
	/// Adds every condition from Recipie.Condtions to Recipie.DecraftContions, checking for duplicates.
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
	public Recipe AddConsumeItemCallback(ConsumeItemCallback callback)
	{
		ConsumeItemHooks += callback;

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
		clone.alchemy = alchemy;
		clone.needSnowBiome = needSnowBiome;
		clone.needGraveyardBiome = needGraveyardBiome;
		clone.needEverythingSeed = needEverythingSeed;

		clone.OnCraftHooks = OnCraftHooks;
		clone.ConsumeItemHooks = ConsumeItemHooks;
		foreach (Condition condition in Conditions) {
			clone.AddCondition(condition);
		}

		foreach (Condition condition in DecraftConditions) {
			clone.AddDecraftCondition(condition);
		}

		// A subsequent call to Register() will re-add this hook if Bottles is a required tile, so we remove
		// it here to not have multiple dupliocate hooks.
		if (clone.requiredTile.Contains(TileID.Bottles))
			clone.ConsumeItemHooks -= ConsumptionRules.Alchemy;

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
			AddConsumeItemCallback(ConsumptionRules.Alchemy);

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
