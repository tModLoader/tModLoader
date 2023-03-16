using System;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.Events;
using Terraria.Localization;

namespace Terraria.ModLoader;

public sealed partial class NPCShop {
	public interface ICondition {
		string Description { get; }

		bool IsAvailable();
	}

	public sealed class Condition : ICondition {
		#region Common conditions
		// Time
		public static readonly Condition TimeDay = new(NetworkText.FromKey("RecipeConditions.TimeDay"), () => Main.dayTime);
		public static readonly Condition TimeNight = new(NetworkText.FromKey("RecipeConditions.TimeNight"), () => !Main.dayTime);

		// Events
		public static readonly Condition Christmas = new(NetworkText.FromKey("ShopConditions.Christmas"), () => Main.xMas);
		public static readonly Condition Halloween = new(NetworkText.FromKey("ShopConditions.Halloween"), () => Main.halloween);
		public static readonly Condition BloodMoon = new(NetworkText.FromKey("ShopConditions.BloodMoon"), () => Main.bloodMoon);
		public static readonly Condition NotBloodMoon = new(NetworkText.FromKey("ShopConditions.NotBloodMoon"), () => !Main.bloodMoon);
		public static readonly Condition Eclipse = new(NetworkText.FromKey("ShopConditions.SolarEclipse"), () => Main.eclipse);
		public static readonly Condition NotEclipse = new(NetworkText.FromKey("ShopConditions.NotSolarEclipse"), () => !Main.eclipse);
		public static readonly Condition EclipseOrBloodMoon = new Condition(NetworkText.FromKey("ShopConditions.BloodOrSun"), () => Main.bloodMoon || Main.eclipse);
		public static readonly Condition NotEclipseAndNotBloodMoon = new Condition(NetworkText.FromKey("ShopConditions.NotBloodOrSun"), () => !Main.bloodMoon && !Main.eclipse);
		public static readonly Condition Thunderstorm = new(NetworkText.FromKey("ShopConditions.Thunderstorm"), () => Main.IsItStorming);
		public static readonly Condition BirthdayPartyIsUp = new(NetworkText.FromLiteral("During birthday party"), () => BirthdayParty.PartyIsUp);
		public static readonly Condition NightLanternsUp = new(NetworkText.FromKey("ShopConditions.NightLanterns"), () => LanternNight.LanternsUp);
		public static readonly Condition HappyWindyDay = new(NetworkText.FromKey("ShopConditions.HappyWindyDay"), () => Main.IsItAHappyWindyDay);

		// Biomes
		public static readonly Condition InShoppingForestBiome = new(NetworkText.FromKey("ShopConditions.InShoppingForest"), () => Main.LocalPlayer.ShoppingZone_Forest);
		public static readonly Condition InSnowBiome = new(NetworkText.FromKey("RecipeConditions.InSnow"), () => Main.LocalPlayer.ZoneSnow);
		public static readonly Condition InJungleBiome = new(NetworkText.FromKey("RecipeConditions.InJungle"), () => Main.LocalPlayer.ZoneJungle);
		public static readonly Condition InCorruptBiome = new(NetworkText.FromKey("RecipeConditions.InCorrupt"), () => Main.LocalPlayer.ZoneCorrupt);
		public static readonly Condition InCrimsonBiome = new(NetworkText.FromKey("RecipeConditions.InCrimson"), () => Main.LocalPlayer.ZoneCrimson);
		public static readonly Condition InHallowBiome = new(NetworkText.FromKey("RecipeConditions.InHallow"), () => Main.LocalPlayer.ZoneHallow);
		public static readonly Condition InDesertBiome = new(NetworkText.FromKey("RecipeConditions.InDesert"), () => Main.LocalPlayer.ZoneDesert);
		public static readonly Condition InGraveyard = new(NetworkText.FromKey("ShopConditions.InGraveyard"), () => Main.LocalPlayer.ZoneGraveyard);
		public static readonly Condition InGlowshroomBiome = new(NetworkText.FromKey("RecipeConditions.InGlowshroom"), () => Main.LocalPlayer.ZoneGlowshroom);
		public static readonly Condition InBeachBiome = new(NetworkText.FromKey("RecipeConditions.InBeach"), () => Main.LocalPlayer.ZoneBeach);
		public static readonly Condition InUnderworld = new(NetworkText.FromKey("RecipeConditions.InUnderworldHeight"), () => Main.LocalPlayer.ZoneUnderworldHeight);
		public static readonly Condition InDungeonBiome = new(NetworkText.FromKey("RecipeConditions.InDungeon"), () => Main.LocalPlayer.ZoneDungeon);
		public static readonly Condition InSpace = new(NetworkText.FromKey("RecipeConditions.InSkyHeight"), () => Main.LocalPlayer.ZoneSkyHeight);
		public static readonly Condition InEvilBiome = new(NetworkText.FromKey("ShopConditions.InEvilBiome"), () => Main.LocalPlayer.ZoneCrimson || Main.LocalPlayer.ZoneCorrupt);

		public static readonly Condition NotInEvilBiome = new(NetworkText.FromKey("ShopConditions.NotInEvilBiome"), () => !Main.LocalPlayer.ZoneCrimson && !Main.LocalPlayer.ZoneCorrupt);
		public static readonly Condition NotInHallowBiome = new(NetworkText.FromKey("ShopConditions.NotInHallowBiome"), () => !Main.LocalPlayer.ZoneHallow);
		public static readonly Condition NotInGraveyard = new(NetworkText.FromKey("ShopConditions.NotInGraveyard"), () => !Main.LocalPlayer.ZoneGraveyard);
		public static readonly Condition NotInUnderworld = new(NetworkText.FromKey("ShopConditions.NotInUnderworld"), () => !Main.LocalPlayer.ZoneUnderworldHeight);

		// World States
		public static readonly Condition Hardmode = new(NetworkText.FromKey("ShopConditions.InHardmode"), () => Main.hardMode);
		public static readonly Condition PreHardmode = new(NetworkText.FromKey("ShopConditions.NotInHardmode"), () => !Main.hardMode);
		public static readonly Condition CorruptionWorld = new(NetworkText.FromKey("ShopConditions.CorruptWorld"), () => !WorldGen.crimson);
		public static readonly Condition CrimsonWorld = new(NetworkText.FromKey("ShopConditions.CrimsonWorld"), () => WorldGen.crimson);
		public static readonly Condition ForTheWorthy = new(NetworkText.FromKey("ShopConditions.WorldForTheWorhy"), () => Main.getGoodWorld);
		public static readonly Condition RemixWorld = new(NetworkText.FromKey("ShopConditions.WorldRemix"), () => Main.remixWorld);
		public static readonly Condition TenthAnniversary = new(NetworkText.FromKey("ShopConditions.WorldAnniversary"), () => Main.tenthAnniversaryWorld);
		public static readonly Condition NotForTheWorthy = new(NetworkText.FromKey("ShopConditions.WorldNotForTheWorthy"), () => !Main.getGoodWorld);
		public static readonly Condition NotRemixWorld = new(NetworkText.FromKey("ShopConditions.WorldNotRemix"), () => !Main.remixWorld);
		public static readonly Condition NotTenthAnniversary = new(NetworkText.FromKey("ShopConditions.WorldNotAnniversary"), () => !Main.tenthAnniversaryWorld);

		// Bosses
		public static readonly Condition DownedKingSlime = new(NetworkText.FromKey("ShopConditions.DownedKingSlime"), () => NPC.downedSlimeKing);
		public static readonly Condition DownedEyeOfCthulhu = new(NetworkText.FromKey("ShopConditions.DownedEyeOfCthulhu"), () => NPC.downedBoss1);
		public static readonly Condition DownedEowOrBoc = new(NetworkText.FromKey("ShopConditions.DownedBoss2"), () => NPC.downedBoss2);
		public static readonly Condition DownedEaterOfWorlds = new(NetworkText.FromKey("ShopConditions.DownedEaterOfWorlds"), () => NPC.downedBoss2 && !WorldGen.crimson);
		public static readonly Condition DownedBrainOfCthulhu = new(NetworkText.FromKey("ShopConditions.DownedBrainOfCthulhu"), () => NPC.downedBoss2 && WorldGen.crimson);
		public static readonly Condition DownedQueenBee = new(NetworkText.FromKey("ShopConditions.DownedQueenBee"), () => NPC.downedQueenBee);
		public static readonly Condition DownedSkeletron = new(NetworkText.FromKey("ShopConditions.DownedSkeletron"), () => NPC.downedBoss3);
		public static readonly Condition DownedQueenSlime = new(NetworkText.FromKey("ShopConditions.DownedQueenSlime"), () => NPC.downedQueenSlime);
		public static readonly Condition DownedMechBossAny = new(NetworkText.FromKey("ShopConditions.DownedMechBossAny"), () => NPC.downedMechBossAny);
		public static readonly Condition DownedTwins = new(NetworkText.FromKey("ShopConditions.DownedTwins"), () => NPC.downedMechBoss2);
		public static readonly Condition DownedDestroyer = new(NetworkText.FromKey("ShopConditions.DownedDestroyer"), () => NPC.downedMechBoss1);
		public static readonly Condition DownedSkeletronPrime = new(NetworkText.FromKey("ShopConditions.DownedSkeletronPrime"), () => NPC.downedMechBoss3);
		public static readonly Condition DownedPlantera = new(NetworkText.FromKey("ShopConditions.DownedPlantera"), () => NPC.downedPlantBoss);
		public static readonly Condition DownedEmpressOfLight = new(NetworkText.FromKey("ShopConditions.DownedEmpressOfLight"), () => NPC.downedEmpressOfLight);
		public static readonly Condition DownedGolem = new(NetworkText.FromKey("ShopConditions.DownedGolem"), () => NPC.downedGolemBoss);
		public static readonly Condition DownedCultist = new(NetworkText.FromKey("ShopConditions.DownedLunaticCultist"), () => NPC.downedAncientCultist);
		public static readonly Condition DownedMoonLord = new(NetworkText.FromKey("ShopConditions.DownedMoonLord"), () => NPC.downedMoonlord);
		public static readonly Condition DownedClown = new(NetworkText.FromKey("ShopConditions.DownedClown"), () => NPC.downedClown);
		public static readonly Condition DownedPirates = new(NetworkText.FromKey("ShopConditions.DownedPirates"), () => NPC.downedPirates);
		public static readonly Condition DownedMartians = new(NetworkText.FromKey("ShopConditions.DownedMartians"), () => NPC.downedMartians);
		public static readonly Condition DownedFrostLegion = new(NetworkText.FromKey("ShopConditions.DownedFrostLegion"), () => NPC.downedFrost);
		public static readonly Condition DownedSolarPillar = new(NetworkText.FromKey("ShopConditions.DownedSolarPillar"), () => NPC.downedTowerSolar);
		public static readonly Condition DownedVortexPillar = new(NetworkText.FromKey("ShopConditions.DownedVortexPillar"), () => NPC.downedTowerVortex);
		public static readonly Condition DownedNebulaPillar = new(NetworkText.FromKey("ShopConditions.DownedNebulaPillar"), () => NPC.downedTowerNebula);
		public static readonly Condition DownedStardustPillar = new(NetworkText.FromKey("ShopConditions.DownedStardustPillar"), () => NPC.downedTowerStardust);

		public static readonly Condition NotDownedKingSlime = new(NetworkText.FromKey("ShopConditions.NotNotDownedKingSlime"), () => !NPC.downedSlimeKing);
		public static readonly Condition NotDownedEyeOfCthulhu = new(NetworkText.FromKey("ShopConditions.NotDownedEyeOfCthulhu"), () => !NPC.downedBoss1);
		public static readonly Condition NotDownedEowOrBoc = new(NetworkText.FromKey("ShopConditions.NotDownedBoss2"), () => !NPC.downedBoss2);
		public static readonly Condition NotDownedEaterOfWorlds = new(NetworkText.FromKey("ShopConditions.NotDownedEaterOfWorlds"), () => !NPC.downedBoss2 && !WorldGen.crimson);
		public static readonly Condition NotDownedBrainOfCthulhu = new(NetworkText.FromKey("ShopConditions.NotDownedBrainOfCthulhu"), () => !NPC.downedBoss2 && WorldGen.crimson);
		public static readonly Condition NotDownedQueenBee = new(NetworkText.FromKey("ShopConditions.NotDownedQueenBee"), () => !NPC.downedQueenBee);
		public static readonly Condition NotDownedSkeletron = new(NetworkText.FromKey("ShopConditions.NotDownedSkeletron"), () => !NPC.downedBoss3);
		public static readonly Condition NotDownedQueenSlime = new(NetworkText.FromKey("ShopConditions.NotDownedQueenSlime"), () => !NPC.downedQueenSlime);
		public static readonly Condition NotDownedMechBossAny = new(NetworkText.FromKey("ShopConditions.NotDownedMechBossAny"), () => !NPC.downedMechBossAny);
		public static readonly Condition NotDownedTwins = new(NetworkText.FromKey("ShopConditions.NotDownedTwins"), () => !NPC.downedMechBoss2);
		public static readonly Condition NotDownedDestroyer = new(NetworkText.FromKey("ShopConditions.NotDownedDestroyer"), () => !NPC.downedMechBoss1);
		public static readonly Condition NotDownedSkeletronPrime = new(NetworkText.FromKey("ShopConditions.NotDownedSkeletronPrime"), () => !NPC.downedMechBoss3);
		public static readonly Condition NotDownedPlantera = new(NetworkText.FromKey("ShopConditions.NotDownedPlantera"), () => !NPC.downedPlantBoss);
		public static readonly Condition NotDownedEmpressOfLight = new(NetworkText.FromKey("ShopConditions.NotDownedEmpressOfLight"), () => !NPC.downedEmpressOfLight);
		public static readonly Condition NotDownedGolem = new(NetworkText.FromKey("ShopConditions.NotDownedGolem"), () => !NPC.downedGolemBoss);
		public static readonly Condition NotDownedCultist = new(NetworkText.FromKey("ShopConditions.NotDownedLunaticCultist"), () => !NPC.downedAncientCultist);
		public static readonly Condition NotDownedMoonLord = new(NetworkText.FromKey("ShopConditions.NotDownedMoonLord"), () => !NPC.downedMoonlord);
		public static readonly Condition NotDownedClown = new(NetworkText.FromKey("ShopConditions.NotDownedClown"), () => !NPC.downedClown);
		public static readonly Condition NotDownedPirates = new(NetworkText.FromKey("ShopConditions.NotDownedPirates"), () => !NPC.downedPirates);
		public static readonly Condition NotDownedMartians = new(NetworkText.FromKey("ShopConditions.NotDownedMartians"), () => !NPC.downedMartians);
		public static readonly Condition NotDownedFrostLegin = new(NetworkText.FromKey("ShopConditions.NotDownedFrostLegion"), () => !NPC.downedFrost);

		// Misc
		public static readonly Condition BloodMoonOrHardmode = new(NetworkText.FromKey("ShopConditions.BloodMoonOrHardmode"), () => Main.bloodMoon || Main.hardMode);
		public static readonly Condition NightOrEclipse = new Condition(NetworkText.FromKey("ShopConditions.NightOrEclipse"), () => !Main.dayTime || Main.eclipse);

		public static readonly Condition InExpertMode = new(NetworkText.FromKey("ShopConditions.InExpertMode"), () => Main.expertMode);
		public static readonly Condition InMasterMode = new(NetworkText.FromKey("ShopConditions.InMasterMode"), () => Main.masterMode);

		public static readonly Condition HappyEnough = new(NetworkText.FromKey("ShopConditions.HappyEnough"), () => Main.LocalPlayer.currentShoppingSettings.PriceAdjustment <= 0.9);
		public static readonly Condition HappyEnoughToSellPylons = new(NetworkText.FromKey("ShopConditions.HappyEnoughForPylons"), () => Main.remixWorld || HappyEnough.IsAvailable());
		public static readonly Condition AnotherTownNPCNearby = new(NetworkText.FromKey("ShopConditions.AnotherTownNPCNearby"), () => TeleportPylonsSystem.DoesPositionHaveEnoughNPCs(2, Main.LocalPlayer.Center.ToTileCoordinates16()));
		public static readonly Condition IsNpcShimmered = new(NetworkText.FromKey("ShopConditions.IsNpcShimmered"), () => Main.LocalPlayer.TalkNPC?.IsShimmerVariant ?? false);

		// Moon phases :( thanks to Chicken Bones for help with those
		public static readonly Condition MoonPhaseFull = new(NetworkText.FromKey("ShopConditions.FullMoon"), () => Main.GetMoonPhase() == MoonPhase.Full);
		public static readonly Condition MoonPhaseWaningGibbous = new(NetworkText.FromKey("ShopConditions.WaningGibbousMoon"), () => Main.GetMoonPhase() == MoonPhase.ThreeQuartersAtLeft);
		public static readonly Condition MoonPhaseThirdQuarter = new(NetworkText.FromKey("ShopConditions.ThirdQuarterMoon"), () => Main.GetMoonPhase() == MoonPhase.HalfAtLeft);
		public static readonly Condition MoonPhaseWaningCrescent = new(NetworkText.FromKey("ShopConditions.WaningCrescentMoon"), () => Main.GetMoonPhase() == MoonPhase.QuarterAtLeft);
		public static readonly Condition MoonPhaseNew = new(NetworkText.FromKey("ShopConditions.NewMoon"), () => Main.GetMoonPhase() == MoonPhase.Empty);
		public static readonly Condition MoonPhaseWaxingCrescent = new(NetworkText.FromKey("ShopConditions.WaxingCrescentMoon"), () => Main.GetMoonPhase() == MoonPhase.QuarterAtRight);
		public static readonly Condition MoonPhaseFirstQuarter = new(NetworkText.FromKey("ShopConditions.FirstQuarterMoon"), () => Main.GetMoonPhase() == MoonPhase.HalfAtRight);
		public static readonly Condition MoonPhaseWaxingGibbous = new(NetworkText.FromKey("ShopConditions.WaxingGibbousMoon"), () => Main.GetMoonPhase() == MoonPhase.ThreeQuartersAtRight);
		public static readonly Condition MoonPhasesQuarter0 = new(NetworkText.FromKey("ShopConditions.MoonPhasesQuarter0"), () => Main.moonPhase / 2 == 0);
		public static readonly Condition MoonPhasesQuarter1 = new(NetworkText.FromKey("ShopConditions.MoonPhasesQuarter1"), () => Main.moonPhase / 2 == 1);
		public static readonly Condition MoonPhasesQuarter2 = new(NetworkText.FromKey("ShopConditions.MoonPhasesQuarter2"), () => Main.moonPhase / 2 == 2);
		public static readonly Condition MoonPhasesQuarter3 = new(NetworkText.FromKey("ShopConditions.MoonPhasesQuarter3"), () => Main.moonPhase / 2 == 3);
		public static readonly Condition MoonPhasesHalf0 = new(NetworkText.FromKey("ShopConditions.MoonPhasesHalf0"), () => Main.moonPhase / 4 == 0);
		public static readonly Condition MoonPhasesHalf1 = new(NetworkText.FromKey("ShopConditions.MoonPhasesHalf1"), () => Main.moonPhase / 4 == 1);
		public static readonly Condition MoonPhasesEven = new(NetworkText.FromKey("ShopConditions.MoonPhasesEven"), () => Main.moonPhase % 2 == 0);
		public static readonly Condition MoonPhasesOdd = new(NetworkText.FromKey("ShopConditions.MoonPhasesOdd"), () => Main.moonPhase % 2 == 1);
		public static readonly Condition MoonPhasesNearNew = new(NetworkText.FromKey("ShopConditions.MoonPhasesNearNew"), () => Main.moonPhase >= 3 && Main.moonPhase <= 5);
		public static readonly Condition MoonPhasesEvenQuarters = new(NetworkText.FromKey("ShopConditions.MoonPhasesEvenQuarters"), () => Main.moonPhase / 2 % 2 == 0);
		public static readonly Condition MoonPhasesOddQuarters = new(NetworkText.FromKey("ShopConditions.MoonPhasesOddQuarters"), () => Main.moonPhase / 2 % 2 == 1);
		public static readonly Condition MoonPhases04 = new(NetworkText.FromKey("ShopConditions.MoonPhases04"), () => Main.moonPhase % 4 == 0);
		public static readonly Condition MoonPhases15 = new(NetworkText.FromKey("ShopConditions.MoonPhases15"), () => Main.moonPhase % 4 == 1);
		public static readonly Condition MoonPhases26 = new(NetworkText.FromKey("ShopConditions.MoonPhases26"), () => Main.moonPhase % 4 == 2);
		public static readonly Condition MoonPhases37 = new(NetworkText.FromKey("ShopConditions.MoonPhases37"), () => Main.moonPhase % 4 == 3);

		public static Condition PlayerCarriesItem(int itemId) => new(NetworkText.FromKey("ShopConditions.PlayerCarriesItem", Lang.GetItemName(itemId)), () => Main.LocalPlayer.HasItem(itemId));
		public static Condition GolfScoreOver(int score) => new(NetworkText.FromKey("ShopConditions.GolfScoreOver", score), () => Main.LocalPlayer.golferScoreAccumulated > score);
		public static Condition NpcIsPresent(int npcId) => new(NetworkText.FromKey("ShopConditions.NpcIsPresent", Lang.GetNPCName(npcId)), () => NPC.AnyNPCs(npcId));
		#endregion

		private readonly NetworkText DescriptionText;
		private readonly Func<bool> Predicate;

		public string Description => DescriptionText.ToString();

		public Condition(NetworkText description, Func<bool> predicate) {
			DescriptionText = description ?? throw new ArgumentNullException(nameof(description));
			Predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
		}

		public bool IsAvailable() => Predicate();
	}
}
