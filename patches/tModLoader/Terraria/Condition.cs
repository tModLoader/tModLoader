using System;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;

namespace Terraria;

public sealed record Condition(LocalizedText Description, Func<bool> Predicate)
{
	public Condition(string LocalizationKey, Func<bool> Predicate) : this(Language.GetOrRegister(LocalizationKey), Predicate) { }

	public bool IsMet() => Predicate();

	// Near Liquids
	public static readonly Condition NearWater =			new("Conditions.NearWater",				() => Main.LocalPlayer.adjWater || Main.LocalPlayer.adjTile[TileID.Sinks]);
	public static readonly Condition NearLava =				new("Conditions.NearLava",				() => Main.LocalPlayer.adjLava);
	public static readonly Condition NearHoney =			new("Conditions.NearHoney",				() => Main.LocalPlayer.adjHoney);
	public static readonly Condition NearShimmer =			new("Conditions.NearShimmer",			() => Main.LocalPlayer.adjShimmer);
	// Time
	public static readonly Condition TimeDay =				new("Conditions.TimeDay",				() => Main.dayTime);
	public static readonly Condition TimeNight =			new("Conditions.TimeNight",				() => !Main.dayTime);
	// Biomes
	public static readonly Condition InDungeon =			new("Conditions.InDungeon",				() => Main.LocalPlayer.ZoneDungeon);
	public static readonly Condition InCorrupt =			new("Conditions.InCorrupt",				() => Main.LocalPlayer.ZoneCorrupt);
	public static readonly Condition InHallow =				new("Conditions.InHallow",				() => Main.LocalPlayer.ZoneHallow);
	public static readonly Condition InMeteor =				new("Conditions.InMeteor",				() => Main.LocalPlayer.ZoneMeteor);
	public static readonly Condition InJungle =				new("Conditions.InJungle",				() => Main.LocalPlayer.ZoneJungle);
	public static readonly Condition InSnow =				new("Conditions.InSnow",				() => Main.LocalPlayer.ZoneSnow);
	public static readonly Condition InCrimson =			new("Conditions.InCrimson",				() => Main.LocalPlayer.ZoneCrimson);
	public static readonly Condition InWaterCandle =		new("Conditions.InWaterCandle",			() => Main.LocalPlayer.ZoneWaterCandle);
	public static readonly Condition InPeaceCandle =		new("Conditions.InPeaceCandle",			() => Main.LocalPlayer.ZonePeaceCandle);
	public static readonly Condition InTowerSolar =			new("Conditions.InTowerSolar",			() => Main.LocalPlayer.ZoneTowerSolar);
	public static readonly Condition InTowerVortex =		new("Conditions.InTowerVortex",			() => Main.LocalPlayer.ZoneTowerVortex);
	public static readonly Condition InTowerNebula =		new("Conditions.InTowerNebula",			() => Main.LocalPlayer.ZoneTowerNebula);
	public static readonly Condition InTowerStardust =		new("Conditions.InTowerStardust",		() => Main.LocalPlayer.ZoneTowerStardust);
	public static readonly Condition InDesert =				new("Conditions.InDesert",				() => Main.LocalPlayer.ZoneDesert);
	public static readonly Condition InGlowshroom =			new("Conditions.InGlowshroom",			() => Main.LocalPlayer.ZoneGlowshroom);
	public static readonly Condition InUndergroundDesert =	new("Conditions.InUndergroundDesert",	() => Main.LocalPlayer.ZoneUndergroundDesert);
	public static readonly Condition InSkyHeight =			new("Conditions.InSkyHeight",			() => Main.LocalPlayer.ZoneSkyHeight);
	public static readonly Condition InSpace =				InSkyHeight;
	public static readonly Condition InOverworldHeight =	new("Conditions.InOverworldHeight",		() => Main.LocalPlayer.ZoneOverworldHeight);
	public static readonly Condition InDirtLayerHeight =	new("Conditions.InDirtLayerHeight",		() => Main.LocalPlayer.ZoneDirtLayerHeight);
	public static readonly Condition InRockLayerHeight =	new("Conditions.InRockLayerHeight",		() => Main.LocalPlayer.ZoneRockLayerHeight);
	public static readonly Condition InUnderworldHeight =	new("Conditions.InUnderworldHeight",	() => Main.LocalPlayer.ZoneUnderworldHeight);
	public static readonly Condition InUnderworld =			InUnderworldHeight;
	public static readonly Condition InBeach =				new("Conditions.InBeach",				() => Main.LocalPlayer.ZoneBeach);
	public static readonly Condition InRain =				new("Conditions.InRain",				() => Main.LocalPlayer.ZoneRain);
	public static readonly Condition InSandstorm =			new("Conditions.InSandstorm",			() => Main.LocalPlayer.ZoneSandstorm);
	public static readonly Condition InOldOneArmy =			new("Conditions.InOldOneArmy",			() => Main.LocalPlayer.ZoneOldOneArmy);
	public static readonly Condition InGranite =			new("Conditions.InGranite",				() => Main.LocalPlayer.ZoneGranite);
	public static readonly Condition InMarble =				new("Conditions.InMarble",				() => Main.LocalPlayer.ZoneMarble);
	public static readonly Condition InHive =				new("Conditions.InHive",				() => Main.LocalPlayer.ZoneHive);
	public static readonly Condition InGemCave =			new("Conditions.InGemCave",				() => Main.LocalPlayer.ZoneGemCave);
	public static readonly Condition InLihzhardTemple =		new("Conditions.InLihzardTemple",		() => Main.LocalPlayer.ZoneLihzhardTemple);
	public static readonly Condition InGraveyard =			new("Conditions.InGraveyard",			() => Main.LocalPlayer.ZoneGraveyard);
	public static readonly Condition InAether =				new("Conditions.InAether",				() => Main.LocalPlayer.ZoneShimmer);

	public static readonly Condition InShoppingZoneForest = new("Conditions.InShoppingForest",		() => Main.LocalPlayer.ShoppingZone_Forest);
	public static readonly Condition InBelowSurface =		new("Conditions.InBelowSurface",		() => Main.LocalPlayer.ShoppingZone_BelowSurface);
	public static readonly Condition InEvilBiome =			new("Conditions.InEvilBiome",			() => Main.LocalPlayer.ZoneCrimson || Main.LocalPlayer.ZoneCorrupt);

	public static readonly Condition NotInEvilBiome =		new("Conditions.NotInEvilBiome",		() => !Main.LocalPlayer.ZoneCrimson && !Main.LocalPlayer.ZoneCorrupt);
	public static readonly Condition NotInHallow =			new("Conditions.NotInHallow",			() => !Main.LocalPlayer.ZoneHallow);
	public static readonly Condition NotInGraveyard =		new("Conditions.NotInGraveyard",		() => !Main.LocalPlayer.ZoneGraveyard);
	public static readonly Condition NotInUnderworld =		new("Conditions.NotInUnderworld",		() => !Main.LocalPlayer.ZoneUnderworldHeight);
	// Difficulty
	public static readonly Condition InClassicMode =		new("Conditions.InClassicMode",			() => !Main.expertMode);
	public static readonly Condition InExpertMode =			new("Conditions.InExpertMode",			() => Main.expertMode);
	public static readonly Condition InMasterMode =			new("Conditions.InMasterMode",			() => Main.masterMode);
	public static readonly Condition InJourneyMode =		new("Conditions.InJourneyMode",			() => Main.GameModeInfo.IsJourneyMode);
	// World Flags
	public static readonly Condition Hardmode =				new("Conditions.InHardmode",			() => Main.hardMode);
	public static readonly Condition PreHardmode =			new("Conditions.PreHardmode",			() => !Main.hardMode);
	public static readonly Condition SmashedShadowOrb =		new("Conditions.SmashedShadowOrb",		() => WorldGen.shadowOrbSmashed);
	public static readonly Condition CrimsonWorld =			new("Conditions.WorldCrimson",			() => WorldGen.crimson);
	public static readonly Condition CorruptWorld =			new("Conditions.WorldCorrupt",			() => !WorldGen.crimson);
	// World Types
	public static readonly Condition DrunkWorld =				new("Conditions.WorldDrunk",				() => Main.drunkWorld);
	public static readonly Condition RemixWorld =				new("Conditions.WorldRemix",				() => Main.remixWorld);
	public static readonly Condition NotTheBeesWorld =			new("Conditions.WorldNotTheBees",			() => Main.notTheBeesWorld);
	public static readonly Condition ForTheWorthyWorld =		new("Conditions.WorldForTheWorthy",			() => Main.getGoodWorld);
	public static readonly Condition TenthAnniversaryWorld =	new("Conditions.WorldAnniversary",			() => Main.tenthAnniversaryWorld);
	public static readonly Condition DontStarveWorld =			new("Conditions.WorldDontStarve",			() => Main.dontStarveWorld);
	public static readonly Condition NoTrapsWorld =				new("Conditions.WorldNoTraps",				() => Main.noTrapsWorld);
	public static readonly Condition ZenithWorld =				new("Conditions.WorldZenith",				() => Main.remixWorld && Main.getGoodWorld);
	
	public static readonly Condition NotDrunkWorld =			new("Conditions.WorldNotDrunk",				() => !Main.drunkWorld);
	public static readonly Condition NotRemixWorld =			new("Conditions.WorldNotRemix",				() => !Main.remixWorld);
	public static readonly Condition NotNotTheBeesWorld =		new("Conditions.WorldNotNotTheBees",		() => !Main.notTheBeesWorld);
	public static readonly Condition NotForTheWorthy =			new("Conditions.WorldNotForTheWorthy",		() => !Main.getGoodWorld);
	public static readonly Condition NotTenthAnniversaryWorld =	new("Conditions.WorldNotAnniversary",		() => !Main.tenthAnniversaryWorld);
	public static readonly Condition NotDontStarveWorld =		new("Conditions.WorldNotDontStarve",		() => !Main.dontStarveWorld);
	public static readonly Condition NotNoTrapsWorld =			new("Conditions.WorldNotNoTraps",			() => !Main.noTrapsWorld);
	public static readonly Condition NotZenithWorld =			new("Conditions.WorldNotZenith",			() => !ZenithWorld.IsMet());
	// Events
	public static readonly Condition Christmas =				new("Conditions.Christmas",					() => Main.xMas);
	public static readonly Condition Halloween =				new("Conditions.Halloween",					() => Main.halloween);
	public static readonly Condition BloodMoon =				new("Conditions.BloodMoon",					() => Main.bloodMoon);
	public static readonly Condition NotBloodMoon =				new("Conditions.NotBloodMoon",				() => !Main.bloodMoon);
	public static readonly Condition Eclipse =					new("Conditions.SolarEclipse",				() => Main.eclipse);
	public static readonly Condition NotEclipse =				new("Conditions.NotSolarEclipse",			() => !Main.eclipse);
	public static readonly Condition EclipseOrBloodMoon =		new("Conditions.BloodOrSun",				() => Main.bloodMoon || Main.eclipse);
	public static readonly Condition NotEclipseAndNotBloodMoon =new("Conditions.NotBloodOrSun",				() => !Main.bloodMoon && !Main.eclipse);

	public static readonly Condition Thunderstorm =				new("Conditions.Thunderstorm",				() => Main.IsItStorming);
	public static readonly Condition BirthdayParty =			new("Conditions.BirthdayParty",				() => GameContent.Events.BirthdayParty.PartyIsUp);
	public static readonly Condition LanternNight =				new("Conditions.NightLanterns",				() => GameContent.Events.LanternNight.LanternsUp);
	public static readonly Condition HappyWindyDay =			new("Conditions.HappyWindyDay",				() => Main.IsItAHappyWindyDay);
	// Bosses
	public static readonly Condition DownedKingSlime =			new("Conditions.DownedKingSlime",			() => NPC.downedSlimeKing);
	public static readonly Condition DownedEyeOfCthulhu =		new("Conditions.DownedEyeOfCthulhu",		() => NPC.downedBoss1);
	public static readonly Condition DownedEowOrBoc =			new("Conditions.DownedBoss2",				() => NPC.downedBoss2);
	public static readonly Condition DownedEaterOfWorlds =		new("Conditions.DownedEaterOfWorlds",		() => NPC.downedBoss2 && !WorldGen.crimson);
	public static readonly Condition DownedBrainOfCthulhu =		new("Conditions.DownedBrainOfCthulhu",		() => NPC.downedBoss2 && WorldGen.crimson);
	public static readonly Condition DownedQueenBee =			new("Conditions.DownedQueenBee",			() => NPC.downedQueenBee);
	public static readonly Condition DownedSkeletron =			new("Conditions.DownedSkeletron",			() => NPC.downedBoss3);
	public static readonly Condition DownedDeerclops =			new("Conditions.DownedDeerclops",			() => NPC.downedDeerclops);
	public static readonly Condition DownedQueenSlime =			new("Conditions.DownedQueenSlime",			() => NPC.downedQueenSlime);
	public static readonly Condition DownedEarlygameBoss =		new("Conditions.DownedEarlygameBoss",		() => NPC.downedBoss1 || NPC.downedBoss2 || NPC.downedBoss3 || NPC.downedQueenBee || Main.hardMode);
	public static readonly Condition DownedMechBossAny =		new("Conditions.DownedMechBossAny",			() => NPC.downedMechBossAny);
	public static readonly Condition DownedTwins =				new("Conditions.DownedTwins",				() => NPC.downedMechBoss2);
	public static readonly Condition DownedDestroyer =			new("Conditions.DownedDestroyer",			() => NPC.downedMechBoss1);
	public static readonly Condition DownedSkeletronPrime =		new("Conditions.DownedSkeletronPrime",		() => NPC.downedMechBoss3);
	public static readonly Condition DownedMechBossAll =		new("Conditions.DownedMechBossAll",			() => NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3);
	public static readonly Condition DownedPlantera =			new("Conditions.DownedPlantera",			() => NPC.downedPlantBoss);
	public static readonly Condition DownedEmpressOfLight =		new("Conditions.DownedEmpressOfLight",		() => NPC.downedEmpressOfLight);
	public static readonly Condition DownedDukeFishron =		new("Conditions.DownedDukeFishron",			() => NPC.downedFishron);
	public static readonly Condition DownedGolem =				new("Conditions.DownedGolem",				() => NPC.downedGolemBoss);
	public static readonly Condition DownedMourningWood =		new("Conditions.DownedMourningWood",		() => NPC.downedHalloweenTree);
	public static readonly Condition DownedPumpking =			new("Conditions.DownedPumpking",			() => NPC.downedHalloweenKing);
	public static readonly Condition DownedEverscream =			new("Conditions.DownedEverscream",			() => NPC.downedChristmasTree);
	public static readonly Condition DownedSantaNK1 =			new("Conditions.DownedSantaNK1",			() => NPC.downedChristmasSantank);
	public static readonly Condition DownedIceQueen =			new("Conditions.DownedIceQueen",			() => NPC.downedChristmasIceQueen);
	public static readonly Condition DownedCultist =			new("Conditions.DownedLunaticCultist",		() => NPC.downedAncientCultist);
	public static readonly Condition DownedMoonLord =			new("Conditions.DownedMoonLord",			() => NPC.downedMoonlord);
	public static readonly Condition DownedClown =				new("Conditions.DownedClown",				() => NPC.downedClown);
	public static readonly Condition DownedGoblinArmy =			new("Conditions.DownedGoblinArmy",			() => NPC.downedGoblins);
	public static readonly Condition DownedPirates =			new("Conditions.DownedPirates",			 	() => NPC.downedPirates);
	public static readonly Condition DownedMartians =			new("Conditions.DownedMartians",		 	() => NPC.downedMartians);
	public static readonly Condition DownedFrostLegion =	  	new("Conditions.DownedFrostLegion",			() => NPC.downedFrost);
	public static readonly Condition DownedSolarPillar =	  	new("Conditions.DownedSolarPillar",			() => NPC.downedTowerSolar);
	public static readonly Condition DownedVortexPillar =	  	new("Conditions.DownedVortexPillar",		() => NPC.downedTowerVortex);
	public static readonly Condition DownedNebulaPillar =	  	new("Conditions.DownedNebulaPillar",		() => NPC.downedTowerNebula);
	public static readonly Condition DownedStardustPillar =		new("Conditions.DownedStardustPillar",		() => NPC.downedTowerStardust);
	public static readonly Condition DownedOldOnesArmyAny =		new("Conditions.DownedOldOnesArmyAny",		() => GameContent.Events.DD2Event.DownedInvasionAnyDifficulty);
	public static readonly Condition DownedOldOnesArmyT1 =		new("Conditions.DownedOldOnesArmyT1",		() => GameContent.Events.DD2Event.DownedInvasionT1);
	public static readonly Condition DownedOldOnesArmyT2 =		new("Conditions.DownedOldOnesArmyT2",		() => GameContent.Events.DD2Event.DownedInvasionT2);
	public static readonly Condition DownedOldOnesArmyT3 =		new("Conditions.DownedOldOnesArmyT3",		() => GameContent.Events.DD2Event.DownedInvasionT3);

	public static readonly Condition NotDownedKingSlime =	  	new("Conditions.NotDownedKingSlime",		() => !NPC.downedSlimeKing);
	public static readonly Condition NotDownedEyeOfCthulhu =	new("Conditions.NotDownedEyeOfCthulhu",		() => !NPC.downedBoss1);
	public static readonly Condition NotDownedEowOrBoc =	  	new("Conditions.NotDownedBoss2",		 	() => !NPC.downedBoss2);
	public static readonly Condition NotDownedEaterOfWorlds =	new("Conditions.NotDownedEaterOfWorlds", 	() => !NPC.downedBoss2 && !WorldGen.crimson);
	public static readonly Condition NotDownedBrainOfCthulhu =	new("Conditions.NotDownedBrainOfCthulhu",	() => !NPC.downedBoss2 && WorldGen.crimson);
	public static readonly Condition NotDownedQueenBee =	  	new("Conditions.NotDownedQueenBee",		 	() => !NPC.downedQueenBee);
	public static readonly Condition NotDownedSkeletron =	  	new("Conditions.NotDownedSkeletron",	 	() => !NPC.downedBoss3);
	public static readonly Condition NotDownedDeerclops =		new("Conditions.NotDownedDeerclops",		() => !NPC.downedDeerclops);
	public static readonly Condition NotDownedQueenSlime =	  	new("Conditions.NotDownedQueenSlime",	 	() => !NPC.downedQueenSlime);
	public static readonly Condition NotDownedMechBossAny =		new("Conditions.NotDownedMechBossAny",	 	() => !NPC.downedMechBossAny);
	public static readonly Condition NotDownedTwins =			new("Conditions.NotDownedTwins",		 	() => !NPC.downedMechBoss2);
	public static readonly Condition NotDownedDestroyer =		new("Conditions.NotDownedDestroyer",	 	() => !NPC.downedMechBoss1);
	public static readonly Condition NotDownedSkeletronPrime =	new("Conditions.NotDownedSkeletronPrime",	() => !NPC.downedMechBoss3);
	public static readonly Condition NotDownedPlantera =		new("Conditions.NotDownedPlantera",		 	() => !NPC.downedPlantBoss);
	public static readonly Condition NotDownedEmpressOfLight =	new("Conditions.NotDownedEmpressOfLight",	() => !NPC.downedEmpressOfLight);
	public static readonly Condition NotDownedDukeFishron =		new("Conditions.NotDownedDukeFishron",		() => !NPC.downedFishron);
	public static readonly Condition NotDownedGolem =			new("Conditions.NotDownedGolem",		 	() => !NPC.downedGolemBoss);
	public static readonly Condition NotDownedMourningWood =	new("Conditions.NotDownedMourningWood",		() => !NPC.downedHalloweenTree);
	public static readonly Condition NotDownedPumpking =		new("Conditions.NotDownedPumpking",			() => !NPC.downedHalloweenKing);
	public static readonly Condition NotDownedEverscream =		new("Conditions.NotDownedEverscream",		() => !NPC.downedChristmasTree);
	public static readonly Condition NotDownedSantaNK1 =		new("Conditions.NotDownedSantaNK1",			() => !NPC.downedChristmasSantank);
	public static readonly Condition NotDownedIceQueen =		new("Conditions.NotDownedIceQueen",			() => !NPC.downedChristmasIceQueen);
	public static readonly Condition NotDownedCultist =			new("Conditions.NotDownedLunaticCultist",	() => !NPC.downedAncientCultist);
	public static readonly Condition NotDownedMoonLord =		new("Conditions.NotDownedMoonLord",		 	() => !NPC.downedMoonlord);
	public static readonly Condition NotDownedClown =			new("Conditions.NotDownedClown",		 	() => !NPC.downedClown);
	public static readonly Condition NotDownedGoblinArmy =		new("Conditions.NotDownedGoblinArmy",		() => !NPC.downedGoblins);
	public static readonly Condition NotDownedPirates =			new("Conditions.NotDownedPirates",		 	() => !NPC.downedPirates);
	public static readonly Condition NotDownedMartians =		new("Conditions.NotDownedMartians",			() => !NPC.downedMartians);
	public static readonly Condition NotDownedFrostLegin =		new("Conditions.NotDownedFrostLegion",		() => !NPC.downedFrost);
	public static readonly Condition NotDownedSolarPillar =		new("Conditions.NotDownedSolarPillar",		() => !NPC.downedTowerSolar);
	public static readonly Condition NotDownedVortexPillar =	new("Conditions.NotDownedVortexPillar",		() => !NPC.downedTowerVortex);
	public static readonly Condition NotDownedNebulaPillar =	new("Conditions.NotDownedNebulaPillar",		() => !NPC.downedTowerNebula);
	public static readonly Condition NotDownedStardustPillar =	new("Conditions.NotDownedStardustPillar",	() => !NPC.downedTowerStardust);
	public static readonly Condition NotDownedOldOnesArmyAny =	new("Conditions.NotDownedOldOnesArmyAny",	() => !GameContent.Events.DD2Event.DownedInvasionAnyDifficulty);
	public static readonly Condition NotDownedOldOnesArmyT1 =	new("Conditions.NotDownedOldOnesArmyT1",	() => !GameContent.Events.DD2Event.DownedInvasionT1);
	public static readonly Condition NotDownedOldOnesArmyT2 =	new("Conditions.NotDownedOldOnesArmyT2",	() => !GameContent.Events.DD2Event.DownedInvasionT2);
	public static readonly Condition NotDownedOldOnesArmyT3 =	new("Conditions.NotDownedOldOnesArmyT3",	() => !GameContent.Events.DD2Event.DownedInvasionT3);

	// Misc
	public static readonly Condition BloodMoonOrHardmode =		new("Conditions.BloodMoonOrHardmode",		() => Main.bloodMoon || Main.hardMode);
	public static readonly Condition NightOrEclipse =			new("Conditions.NightOrEclipse",			() => !Main.dayTime || Main.eclipse);

	public static readonly Condition Multiplayer =				new("Conditions.InMultiplayer",				() => Main.netMode != NetmodeID.SinglePlayer);
	public static readonly Condition HappyEnough =				new("Conditions.HappyEnough",				() => Main.LocalPlayer.currentShoppingSettings.PriceAdjustment <= 0.9);
	public static readonly Condition HappyEnoughToSellPylons =	new("Conditions.HappyEnoughForPylons",		() => Main.remixWorld || HappyEnough.IsMet());
	public static readonly Condition AnotherTownNPCNearby =		new("Conditions.AnotherTownNPCNearby",		() => TeleportPylonsSystem.DoesPositionHaveEnoughNPCs(2, Main.LocalPlayer.Center.ToTileCoordinates16()));
	public static readonly Condition IsNpcShimmered =			new("Conditions.IsNpcShimmered",			() => Main.LocalPlayer.TalkNPC?.IsShimmerVariant ?? false);

	// Moon phases :( thanks to Chicken Bones for help with those
	public static readonly Condition MoonPhaseFull =			new("Conditions.FullMoon",					() => Main.GetMoonPhase() == MoonPhase.Full);
	public static readonly Condition MoonPhaseWaningGibbous =	new("Conditions.WaningGibbousMoon",			() => Main.GetMoonPhase() == MoonPhase.ThreeQuartersAtLeft);
	public static readonly Condition MoonPhaseThirdQuarter =	new("Conditions.ThirdQuarterMoon",			() => Main.GetMoonPhase() == MoonPhase.HalfAtLeft);
	public static readonly Condition MoonPhaseWaningCrescent =	new("Conditions.WaningCrescentMoon",		() => Main.GetMoonPhase() == MoonPhase.QuarterAtLeft);
	public static readonly Condition MoonPhaseNew =				new("Conditions.NewMoon",					() => Main.GetMoonPhase() == MoonPhase.Empty);
	public static readonly Condition MoonPhaseWaxingCrescent =	new("Conditions.WaxingCrescentMoon",		() => Main.GetMoonPhase() == MoonPhase.QuarterAtRight);
	public static readonly Condition MoonPhaseFirstQuarter =	new("Conditions.FirstQuarterMoon",			() => Main.GetMoonPhase() == MoonPhase.HalfAtRight);
	public static readonly Condition MoonPhaseWaxingGibbous =	new("Conditions.WaxingGibbousMoon",			() => Main.GetMoonPhase() == MoonPhase.ThreeQuartersAtRight);
	public static readonly Condition MoonPhasesQuarter0 =		new("Conditions.MoonPhasesQuarter0",		() => Main.moonPhase / 2 == 0);
	public static readonly Condition MoonPhasesQuarter1 =		new("Conditions.MoonPhasesQuarter1",		() => Main.moonPhase / 2 == 1);
	public static readonly Condition MoonPhasesQuarter2 =		new("Conditions.MoonPhasesQuarter2",		() => Main.moonPhase / 2 == 2);
	public static readonly Condition MoonPhasesQuarter3 =		new("Conditions.MoonPhasesQuarter3",		() => Main.moonPhase / 2 == 3);
	public static readonly Condition MoonPhasesHalf0 =			new("Conditions.MoonPhasesHalf0",			() => Main.moonPhase / 4 == 0);
	public static readonly Condition MoonPhasesHalf1 =			new("Conditions.MoonPhasesHalf1",			() => Main.moonPhase / 4 == 1);
	public static readonly Condition MoonPhasesEven =			new("Conditions.MoonPhasesEven",			() => Main.moonPhase % 2 == 0);
	public static readonly Condition MoonPhasesOdd =			new("Conditions.MoonPhasesOdd",				() => Main.moonPhase % 2 == 1);
	public static readonly Condition MoonPhasesNearNew =		new("Conditions.MoonPhasesNearNew",			() => Main.moonPhase >= 3 && Main.moonPhase <= 5);
	public static readonly Condition MoonPhasesEvenQuarters =	new("Conditions.MoonPhasesEvenQuarters",	() => Main.moonPhase / 2 % 2 == 0);
	public static readonly Condition MoonPhasesOddQuarters =	new("Conditions.MoonPhasesOddQuarters",		() => Main.moonPhase / 2 % 2 == 1);
	public static readonly Condition MoonPhases04 =				new("Conditions.MoonPhases04",				() => Main.moonPhase % 4 == 0);
	public static readonly Condition MoonPhases15 =				new("Conditions.MoonPhases15",				() => Main.moonPhase % 4 == 1);
	public static readonly Condition MoonPhases26 =				new("Conditions.MoonPhases26",				() => Main.moonPhase % 4 == 2);
	public static readonly Condition MoonPhases37 =				new("Conditions.MoonPhases37",				() => Main.moonPhase % 4 == 3);

	// Parameters
	public static Condition PlayerCarriesItem(int itemId) => new(Language.GetText("Conditions.PlayerCarriesItem").WithFormatArgs(Lang.GetItemName(itemId)), () => Main.LocalPlayer.HasItem(itemId));
	public static Condition GolfScoreOver(int score) => new(Language.GetText("Conditions.GolfScoreOver").WithFormatArgs(score), () => Main.LocalPlayer.golferScoreAccumulated >= score);
	public static Condition NpcIsPresent(int npcId) => new(Language.GetText("Conditions.NpcIsPresent").WithFormatArgs(Lang.GetNPCName(npcId)), () => NPC.AnyNPCs(npcId));
	public static Condition AnglerQuestsFinishedOver(int quests) => new(Language.GetText("Conditions.AnglerQuestsFinishedOver").WithFormatArgs(quests), () => Main.LocalPlayer.anglerQuestsFinished >= quests);
	public static Condition BestiaryFilledPercent(int percent) {
		if (percent >= 100)
			return new Condition("Conditions.BestiaryFull", () => Main.GetBestiaryProgressReport().CompletionPercent >= 1f);

		return new(Language.GetText("Conditions.BestiaryPercentage").WithFormatArgs(percent), () => Main.GetBestiaryProgressReport().CompletionPercent >= percent / 100f);
	}
}
			