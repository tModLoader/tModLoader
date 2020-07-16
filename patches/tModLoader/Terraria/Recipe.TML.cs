using System;
using Terraria.Localization;
using Terraria.ModLoader;

#pragma warning disable IDE0060 //Remove unused parameter.

namespace Terraria
{
	public partial class Recipe
	{
		public interface ICondition
		{
			string Description { get; }

			bool RecipeAvailable(ModRecipe recipe);
		}

		public sealed class Condition : ICondition
		{
			#region Conditions

			//Liquids
			public static readonly Condition NearWater = new Condition(NetworkText.FromKey("RecipeConditions.NearWater"), _ => Main.LocalPlayer.adjWater);
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
			private readonly Predicate<ModRecipe> Predicate;

			public string Description => DescriptionText.ToString();

			public Condition(NetworkText description, Predicate<ModRecipe> predicate) {
				DescriptionText = description ?? throw new ArgumentNullException(nameof(description));
				Predicate = predicate ?? throw new ArgumentNullException(nameof(description));
			}

			public bool RecipeAvailable(ModRecipe recipe) => Predicate(recipe);
		}

		public static class ConsumptionRules
		{
			/// <summary> Gives 1/3 chance for every ingredient to not be consumed, if used at an alchemy table. (!) This behavior is already automatically given to all items that can be made at a placed bottle tile. </summary>
			public static int Alchemy(ModRecipe _, int type, int amount) {
				if (!Main.LocalPlayer.alchemyTable) {
					return amount;
				}

				int amountUsed = 0;

				for (int i = 0; i < amount; i++) {
					if (Main.rand.Next(3) != 0) {
						amountUsed++;
					}
				}

				return amountUsed;
			}
		}
	}
}
