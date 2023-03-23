using System;
using Terraria.ID;
using Terraria.Localization;

namespace Terraria;

public sealed record Condition(LocalizedText Description, Func<bool> Predicate)
{
	public Condition(string LocalizationKey, Func<bool> Predicate) : this(Language.GetOrRegister(LocalizationKey), Predicate) { }

	public bool IsMet() => Predicate();

	//Liquids
	public static readonly Condition NearWater =			new("Conditions.NearWater",				() => Main.LocalPlayer.adjWater || Main.LocalPlayer.adjTile[TileID.Sinks]);
	public static readonly Condition NearLava =				new("Conditions.NearLava",				() => Main.LocalPlayer.adjLava);
	public static readonly Condition NearHoney =			new("Conditions.NearHoney",				() => Main.LocalPlayer.adjHoney);
	public static readonly Condition NearShimmer =			new("Conditions.NearShimmer",			() => Main.LocalPlayer.adjShimmer);
	//Time
	public static readonly Condition TimeDay =				new("Conditions.TimeDay",				() => Main.dayTime);
	public static readonly Condition TimeNight =			new("Conditions.TimeNight",				() => !Main.dayTime);
	//Biomes
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
	public static readonly Condition InOverworldHeight =	new("Conditions.InOverworldHeight",		() => Main.LocalPlayer.ZoneOverworldHeight);
	public static readonly Condition InDirtLayerHeight =	new("Conditions.InDirtLayerHeight",		() => Main.LocalPlayer.ZoneDirtLayerHeight);
	public static readonly Condition InRockLayerHeight =	new("Conditions.InRockLayerHeight",		() => Main.LocalPlayer.ZoneRockLayerHeight);
	public static readonly Condition InUnderworldHeight =	new("Conditions.InUnderworldHeight",	() => Main.LocalPlayer.ZoneUnderworldHeight);
	public static readonly Condition InBeach =				new("Conditions.InBeach",				() => Main.LocalPlayer.ZoneBeach);
	public static readonly Condition InRain =				new("Conditions.InRain",				() => Main.LocalPlayer.ZoneRain);
	public static readonly Condition InSandstorm =			new("Conditions.InSandstorm",			() => Main.LocalPlayer.ZoneSandstorm);
	public static readonly Condition InOldOneArmy =			new("Conditions.InOldOneArmy",			() => Main.LocalPlayer.ZoneOldOneArmy);
	public static readonly Condition InGranite =			new("Conditions.InGranite",				() => Main.LocalPlayer.ZoneGranite);
	public static readonly Condition InMarble =				new("Conditions.InMarble",				() => Main.LocalPlayer.ZoneMarble);
	public static readonly Condition InHive =				new("Conditions.InHive",				() => Main.LocalPlayer.ZoneHive);
	public static readonly Condition InGemCave =			new("Conditions.InGemCave",				() => Main.LocalPlayer.ZoneGemCave);
	public static readonly Condition InLihzhardTemple =		new("Conditions.InLihzardTemple",		() => Main.LocalPlayer.ZoneLihzhardTemple);
	public static readonly Condition InGraveyardBiome =		new("Conditions.InGraveyardBiome",		() => Main.LocalPlayer.ZoneGraveyard);
	//WorldType
	public static readonly Condition CrimsonWorld =			new("Conditions.WorldCrimson",			() => WorldGen.crimson);
	public static readonly Condition CorruptWorld =			new("Conditions.WorldCorrupt",			() => !WorldGen.crimson);
	//WorldSeed
	public static readonly Condition DrunkWorld =			new("Conditions.WorldDrunk",			() => Main.drunkWorld);
	public static readonly Condition RemixWorld =			new("Conditions.WorldRemix",			() => Main.remixWorld);
	public static readonly Condition ForTheWorthyWorld =	new("Conditions.WorldForTheWorthy",		() => Main.getGoodWorld);
	public static readonly Condition TenthAnniversaryWorld =new("Conditions.WorldAnniversary",		() => Main.tenthAnniversaryWorld);
	public static readonly Condition DontStarveWorld =		new("Conditions.WorldDontStarve",		() => Main.dontStarveWorld);
	public static readonly Condition ZenithWorld =			new("Conditions.WorldZenith",			() => Main.remixWorld && Main.getGoodWorld);
}
			