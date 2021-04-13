using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Exceptions;

#pragma warning disable IDE0060 //Remove unused parameter.

namespace Terraria
{
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

			#endregion

			private readonly NetworkText DescriptionText;
			private readonly Predicate<Recipe> Predicate;

			public string Description => DescriptionText.ToString();

			public Condition(NetworkText description, Predicate<Recipe> predicate) {
				DescriptionText = description ?? throw new ArgumentNullException(nameof(description));
				Predicate = predicate ?? throw new ArgumentNullException(nameof(description));
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

		public delegate void OnCraftCallback(Recipe recipe, Item item);
		public delegate void ConsumeItemCallback(Recipe recipe, int type, ref int amount);
		
		internal OnCraftCallback OnCraftHooks { get; private set; }
		internal ConsumeItemCallback ConsumeItemHooks { get; private set; }

		private void AddGroup(int id) {
			acceptedGroups.Add(id);
		}
		
		/// <summary>
		/// The index of the recipe in the Main.recipe array.
		/// </summary>
		public int RecipeIndex { get; private set; }

		/// <summary>
		/// Adds an ingredient to this recipe with the given item type and stack size. Ex: <c>recipe.AddIngredient(ItemID.IronAxe)</c>
		/// </summary>
		/// <param name="itemID">The item identifier.</param>
		/// <param name="stack">The stack.</param>
		public Recipe AddIngredient(int itemID, int stack = 1) {
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
		public Recipe AddIngredient(Mod mod, string itemName, int stack = 1) {
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
		/// <param name="item">The item.</param>
		/// <param name="stack">The stack.</param>
		public Recipe AddIngredient<T>(int stack = 1) where T : ModItem
			=> AddIngredient(ModContent.ItemType<T>(), stack);

		/// <summary>
		/// Adds a recipe group ingredient to this recipe with the given RecipeGroup name and stack size. Vanilla recipe groups consist of "Wood", "IronBar", "PresurePlate", "Sand", and "Fragment".
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="stack">The stack.</param>
		/// <exception cref="RecipeException">A recipe group with the name " + name + " does not exist.</exception>
		public Recipe AddRecipeGroup(string name, int stack = 1) {
			if (!RecipeGroup.recipeGroupIDs.ContainsKey(name))
				throw new RecipeException($"A recipe group with the name {name} does not exist.");

			int id = RecipeGroup.recipeGroupIDs[name];
			var group = RecipeGroup.recipeGroups[id];
			
			AddIngredient(group.IconicItemId, stack);
			AddGroup(id);

			return this;
		}

		/// <summary>
		/// Adds a recipe group ingredient to this recipe with the given RecipeGroupID and stack size. Vanilla recipe group IDs can be found in Terraria.ID.RecipeGroupID and modded recipe group IDs will be returned from RecipeGroup.RegisterGroup.
		/// </summary>
		/// <param name="recipeGroupId">The RecipeGroupID.</param>
		/// <param name="stack">The stack.</param>
		/// <exception cref="RecipeException">A recipe group with the ID " + recipeGroupID + " does not exist.</exception>
		public Recipe AddRecipeGroup(int recipeGroupId, int stack = 1) {
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
		public Recipe AddRecipeGroup(RecipeGroup recipeGroup, int stack = 1) {
			AddIngredient(recipeGroup.IconicItemId, stack);
			AddGroup(recipeGroup.ID);

			return this;
		}
		
		/// <summary>
		/// Adds a required crafting station with the given tile type to this recipe. Ex: <c>recipe.AddTile(TileID.WorkBenches)</c>
		/// </summary>
		/// <param name="tileID">The tile identifier.</param>
		/// <exception cref="RecipeException">No tile has ID " + tileID</exception>
		public Recipe AddTile(int tileID) {
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
		public Recipe AddTile(Mod mod, string tileName) {
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

		/// <summary>
		/// Adds a collectiom of conditions that will determine whether or not the recipe will be to be available for the player to use. The conditions can be unrelated to items or tiles (for example, biome or time).
		/// </summary>
		/// <param name="conditions">A collection of conditions.</param>
		public Recipe AddCondition(IEnumerable<Condition> conditions) {
			Conditions.AddRange(conditions);

			return this;
		}

		/// <summary>
		/// Sets a callback that will allow you to make anything happen when the recipe is used to create an item.
		/// </summary>
		public Recipe AddOnCraftCallback(OnCraftCallback callback) {
			OnCraftHooks += callback;

			return this;
		}

		/// <summary>
		/// Sets a callback that allows you to determine how many of a certain ingredient is consumed when this recipe is used. Return the number of ingredients that will actually be consumed. By default returns numRequired.
		/// </summary>
		public Recipe AddConsumeItemCallback(ConsumeItemCallback callback) {
			ConsumeItemHooks += callback;

			return this;
		}

		/// <summary>
		/// Adds this recipe to the game. Call this after you have finished setting the result, ingredients, etc.
		/// </summary>
		/// <exception cref="RecipeException">A recipe without any result has been added.</exception>
		public void Register() {
			if (createItem == null || createItem.type == 0)
				throw new RecipeException("A recipe without any result has been added.");

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
		}

		internal static Recipe Create(Mod mod, int result, int amount) {
			var recipe = new Recipe(mod);

			if (!RecipeLoader.setupRecipes)
				throw new RecipeException("A Recipe can only be created inside recipe related methods");

			recipe.createItem.SetDefaults(result, false);
			recipe.createItem.stack = amount;

			return recipe;
		}
	}
}
