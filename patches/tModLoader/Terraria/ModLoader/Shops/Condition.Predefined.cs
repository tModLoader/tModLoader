using System;
using Terraria.Enums;
using Terraria.GameContent.Events;
using Terraria.Localization;

namespace Terraria.ModLoader.Shops
{
	public class ComparisonCondition<T> : Condition where T : IComparable
	{
		private Func<T> field;
		private T other;
		private Operation operation;

		public ComparisonCondition(Func<T> field, T other, Operation operation, NetworkText description) : base(description) {
			this.field = field;
			this.other = other;
			this.operation = operation;
		}

		public override bool Evaluate() {
			T value = field();
			return operation switch
			{
				Operation.Greater => value.CompareTo(other) > 0,
				Operation.GreaterEqual => value.CompareTo(other) >= 0,
				Operation.Equal => value.CompareTo(other) == 0,
				Operation.LessEqual => value.CompareTo(other) <= 0,
				Operation.Less => value.CompareTo(other) < 0
			};
		}
	}

	public enum Operation
	{
		Greater,
		GreaterEqual,
		Equal,
		LessEqual,
		Less
	}

	public partial class Condition
	{
		public static ComparisonCondition<long> HasMoney(long value, Operation operation) => new ComparisonCondition<long>(() => {
			long totalMoney = 0L;
			for (int i = 0; i < 54; i++)
			{
				Item item = Main.LocalPlayer.inventory[i];
				if (item.IsAir) continue;
				
				totalMoney += item.type switch
				{
					71 => item.stack,
					72 => item.stack * 100,
					73 => item.stack * 10000,
					74 => item.stack * 1000000,
					_ => 0
				};
			}

			return totalMoney;
		}, value, operation, NetworkText.FromKey("Money"));

		private static Condition Construct(string key, Func<bool> predicate) {
			return new SimpleCondition(NetworkText.FromKey($"ShopConditions.{key}"), predicate);
		}

		// Time
		public static readonly Condition TimeDay = Construct("TimeDay", () => Main.dayTime);
		public static readonly Condition TimeNight = Construct("TimeDay", () => !Main.dayTime);

		// World
		public static readonly Condition Crimson = Construct("Crimson", () => WorldGen.crimson);
		public static readonly Condition Corruption = Construct("Corruption", () => !WorldGen.crimson);
		public static readonly Condition Hardmode = Construct("Hardmode", () => Main.hardMode);
		public static readonly Condition HappyNPCs = Construct("HappyNPCs", () => Main.LocalPlayer.currentShoppingSettings.PriceAdjustment <= 0.8500000238418579);
		public static readonly Condition OrbSmashed = Construct("OrbSmashed", () => WorldGen.shadowOrbSmashed);
		public static Condition NPCExists(int type) => Construct("NPCExists", () => NPC.AnyNPCs(type));

		// Event
		public static readonly Condition BloodMoon = Construct("BloodMoon", () => Main.bloodMoon);
		public static readonly Condition SolarEclipse = Construct("SolarEclipse", () => Main.eclipse);
		public static readonly Condition AstrologicalEvent = Construct("AstrologicalEvent", () => Main.eclipse || Main.bloodMoon);
		public static readonly Condition Halloween = Construct("Halloween", () => Main.halloween);
		public static readonly Condition PartyTime = Construct("PartyTime", () => BirthdayParty.PartyIsUp);
		public static readonly Condition Lanterns = Construct("Lanterns", () => LanternNight.LanternsUp);
		public static readonly Condition Christmas = Construct("Christmas", () => Main.xMas);

		public static readonly Condition DownedPirates = Construct("DownedPirates", () => NPC.downedPirates);
		public static readonly Condition DownedMartians = Construct("DownedMartians", () => NPC.downedMartians);
		public static readonly Condition DownedGoblinArmy = Construct("DownedGoblinArmy", () => NPC.downedGoblins);
		public static readonly Condition DownedClown = Construct("DownedClown", () => NPC.downedClown);
		public static readonly Condition DownedFrostLegion = Construct("DownedFrostLegion", () => NPC.downedFrost);

		// Moon Phase
		public static readonly Condition PhaseFull = Construct("PhaseFullMoon", () => Main.GetMoonPhase() == MoonPhase.Full);
		public static readonly Condition PhaseThreeQuartersAtLeft = Construct("PhaseWaningGibbous", () => Main.GetMoonPhase() == MoonPhase.ThreeQuartersAtLeft);
		public static readonly Condition PhaseHalfAtLeft = Construct("PhaseThirdQuarter", () => Main.GetMoonPhase() == MoonPhase.HalfAtLeft);
		public static readonly Condition PhaseQuarterAtLeft = Construct("PhaseWaningCrescent", () => Main.GetMoonPhase() == MoonPhase.QuarterAtLeft);
		public static readonly Condition PhaseEmpty = Construct("PhaseNewMoon", () => Main.GetMoonPhase() == MoonPhase.Empty);
		public static readonly Condition PhaseQuarterAtRight = Construct("PhaseWaxingCrescent", () => Main.GetMoonPhase() == MoonPhase.QuarterAtRight);
		public static readonly Condition PhaseHalfAtRight = Construct("PhaseFirstQuarter", () => Main.GetMoonPhase() == MoonPhase.HalfAtRight);
		public static readonly Condition PhaseThreeQuartersAtRight = Construct("PhaseWaxingGibbous", () => Main.GetMoonPhase() == MoonPhase.ThreeQuartersAtRight);

		// Boss				
		public static readonly Condition DownedKingSlime = Construct("DownedKingSlime", () => NPC.downedSlimeKing);
		public static readonly Condition DownedEoC = Construct("DownedEoC", () => NPC.downedBoss1);
		public static readonly Condition DownedEoW = Construct("DownedEoW", () => NPC.downedBoss2 && !WorldGen.crimson);
		public static readonly Condition DownedBoC = Construct("DownedBoC", () => NPC.downedBoss2 && WorldGen.crimson);
		public static readonly Condition DownedQueenBee = Construct("DownedQueenBee", () => NPC.downedQueenBee);
		public static readonly Condition DownedSkeletron = Construct("DownedSkeletron", () => NPC.downedBoss3);
		public static readonly Condition DownedWoF = Construct("DownedWoF", () => Main.hardMode);
		public static readonly Condition DownedQueenSlime = Construct("DownedQueenSlime", () => NPC.downedQueenSlime);
		public static readonly Condition DownedDestroyer = Construct("DownedDestroyer", () => NPC.downedMechBoss1);
		public static readonly Condition DownedTwins = Construct("DownedTwins", () => NPC.downedMechBoss2);
		public static readonly Condition DownedSkeletronPrime = Construct("DownedSkeletronPrime", () => NPC.downedMechBoss3);
		public static readonly Condition DownedPlantera = Construct("DownedPlantera", () => NPC.downedPlantBoss);
		public static readonly Condition DownedGolem = Construct("DownedGolem", () => NPC.downedGolemBoss);
		public static readonly Condition DownedEmpress = Construct("DownedEmpress", () => NPC.downedEmpressOfLight);
		public static readonly Condition DownedDukeFishron = Construct("DownedDukeFishron", () => NPC.downedFishron);
		public static readonly Condition DownedLunaticCultist = Construct("DownedLunaticCultist", () => NPC.downedAncientCultist);
		public static readonly Condition DownedMoonLord = Construct("DownedMoonLord", () => NPC.downedMoonlord);
		public static readonly Condition DownedAnyMechBoss = Construct("DownedAnyMechBoss", () => NPC.downedMechBossAny);
		public static readonly Condition DownedEvilBoss = Construct("DownedEvilBoss", () => NPC.downedBoss2);

		public static readonly Condition DownedSolarTower = Construct("DownedSolarTower", () => NPC.downedTowerSolar);
		public static readonly Condition DownedNebulaTower = Construct("DownedNebulaTower", () => NPC.downedTowerNebula);
		public static readonly Condition DownedStardustTower = Construct("DownedStardustTower", () => NPC.downedTowerStardust);
		public static readonly Condition DownedVortexTower = Construct("DownedVortexTower", () => NPC.downedTowerVortex);

		// Biome
		public static readonly Condition InDungeon = Construct("InDungeon", () => Main.LocalPlayer.ZoneDungeon);
		public static readonly Condition InCorrupt = Construct("InCorrupt", () => Main.LocalPlayer.ZoneCorrupt);
		public static readonly Condition InHallow = Construct("InHallow", () => Main.LocalPlayer.ZoneHallow);
		public static readonly Condition InMeteor = Construct("InMeteor", () => Main.LocalPlayer.ZoneMeteor);
		public static readonly Condition InJungle = Construct("InJungle", () => Main.LocalPlayer.ZoneJungle);
		public static readonly Condition InSnow = Construct("InSnow", () => Main.LocalPlayer.ZoneSnow);
		public static readonly Condition InCrimson = Construct("InCrimson", () => Main.LocalPlayer.ZoneCrimson);
		public static readonly Condition InWaterCandle = Construct("InWaterCandle", () => Main.LocalPlayer.ZonePeaceCandle);
		public static readonly Condition InTowerSolar = Construct("InTowerSolar", () => Main.LocalPlayer.ZoneTowerSolar);
		public static readonly Condition InTowerVortex = Construct("InTowerVortex", () => Main.LocalPlayer.ZoneTowerVortex);
		public static readonly Condition InTowerNebula = Construct("InTowerNebula", () => Main.LocalPlayer.ZoneTowerNebula);
		public static readonly Condition InTowerStardust = Construct("InTowerStardust", () => Main.LocalPlayer.ZoneTowerStardust);
		public static readonly Condition InDesert = Construct("InDesert", () => Main.LocalPlayer.ZoneDesert);
		public static readonly Condition InGlowshroom = Construct("InGlowshroom", () => Main.LocalPlayer.ZoneGlowshroom);
		public static readonly Condition InUndergroundDesert = Construct("InUndergroundDesert", () => Main.LocalPlayer.ZoneUndergroundDesert);
		public static readonly Condition InSkyHeight = Construct("InSkyHeight", () => Main.LocalPlayer.ZoneSkyHeight);
		public static readonly Condition InOverworldHeight = Construct("InOverworldHeight", () => Main.LocalPlayer.ZoneOverworldHeight);
		public static readonly Condition InDirtLayerHeight = Construct("InDirtLayerHeight", () => Main.LocalPlayer.ZoneDirtLayerHeight);
		public static readonly Condition InRockLayerHeight = Construct("InRockLayerHeight", () => Main.LocalPlayer.ZoneRockLayerHeight);
		public static readonly Condition InUnderworldHeight = Construct("InUnderworldHeight", () => Main.LocalPlayer.ZoneUnderworldHeight);
		public static readonly Condition InBeach = Construct("InBeach", () => Main.LocalPlayer.ZoneBeach);
		public static readonly Condition InRain = Construct("InRain", () => Main.LocalPlayer.ZoneRain);
		public static readonly Condition InSandstorm = Construct("InSandstorm", () => Main.LocalPlayer.ZoneSandstorm);
		public static readonly Condition InOldOneArmy = Construct("InOldOneArmy", () => Main.LocalPlayer.ZoneOldOneArmy);
		public static readonly Condition InGranite = Construct("InGranite", () => Main.LocalPlayer.ZoneGranite);
		public static readonly Condition InMarble = Construct("InMarble", () => Main.LocalPlayer.ZoneMarble);
		public static readonly Condition InHive = Construct("InHive", () => Main.LocalPlayer.ZoneHive);
		public static readonly Condition InGemCave = Construct("InGemCave", () => Main.LocalPlayer.ZoneGemCave);
		public static readonly Condition InLihzhardTemple = Construct("InLihzhardTemple", () => Main.LocalPlayer.ZoneLihzhardTemple);
		public static readonly Condition InGraveyardBiome = Construct("InGraveyardBiome", () => Main.LocalPlayer.ZoneGraveyard);
		public static readonly Condition InSpace = Construct("InSpace", () => Main.LocalPlayer.position.Y / 16f < Main.worldSurface * 0.3499999940395355);

		public static readonly Condition InOcean = Construct("InOcean", () => {
			int x = (int)((Main.screenPosition.X + Main.screenWidth * 0.5f) / 16f);
			return Main.screenPosition.Y / 16f < Main.worldSurface + 10.0 && (x < 380 || x > Main.maxTilesX - 380);
		});

		// Other
		public static Condition HasItem(int type) => Construct("HasItem", () => Main.LocalPlayer.HasItem(type));


		public static Condition GolfScore(int value, Operation operation) {
			return Construct("GolfScore", () => operation switch
			{
				Operation.Greater => Main.LocalPlayer.golferScoreAccumulated > value,
				Operation.GreaterEqual => Main.LocalPlayer.golferScoreAccumulated >= value,
				Operation.Equal => Main.LocalPlayer.golferScoreAccumulated == value,
				Operation.LessEqual => Main.LocalPlayer.golferScoreAccumulated <= value,
				Operation.Less => Main.LocalPlayer.golferScoreAccumulated < value
			});
		}

		public static Condition BestiaryCompletion(float value) => Construct("BestiaryCompletion", () => Main.GetBestiaryProgressReport().CompletionPercent >= value);
	}
}