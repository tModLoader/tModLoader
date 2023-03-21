using System;
using Terraria.ID;
using Terraria.Localization;

namespace Terraria;

public sealed record Condition(LocalizedText Description, Func<bool> Predicate)
{
	public Condition(string LocalizationKey, Func<bool> Predicate) : this(Language.GetText(LocalizationKey), Predicate) { }

	public bool IsMet() => Predicate();

	//Liquids
	public static readonly Condition NearWater =			new("Condition.NearWater",				() => Main.LocalPlayer.adjWater || Main.LocalPlayer.adjTile[TileID.Sinks]);
	public static readonly Condition NearLava =				new("Condition.NearLava",				() => Main.LocalPlayer.adjLava);
	public static readonly Condition NearHoney =			new("Condition.NearHoney",				() => Main.LocalPlayer.adjHoney);
	public static readonly Condition NearShimmer =			new("Condition.NearShimmer",			() => Main.LocalPlayer.adjShimmer);
	//Time
	public static readonly Condition TimeDay =				new("Condition.TimeDay",				() => Main.dayTime);
	public static readonly Condition TimeNight =			new("Condition.TimeNight",				() => !Main.dayTime);
	//Biomes
	public static readonly Condition InDungeon =			new("Condition.InDungeon",				() => Main.LocalPlayer.ZoneDungeon);
	public static readonly Condition InCorrupt =			new("Condition.InCorrupt",				() => Main.LocalPlayer.ZoneCorrupt);
	public static readonly Condition InHallow =				new("Condition.InHallow",				() => Main.LocalPlayer.ZoneHallow);
	public static readonly Condition InMeteor =				new("Condition.InMeteor",				() => Main.LocalPlayer.ZoneMeteor);
	public static readonly Condition InJungle =				new("Condition.InJungle",				() => Main.LocalPlayer.ZoneJungle);
	public static readonly Condition InSnow =				new("Condition.InSnow",					() => Main.LocalPlayer.ZoneSnow);
	public static readonly Condition InCrimson =			new("Condition.InCrimson",				() => Main.LocalPlayer.ZoneCrimson);
	public static readonly Condition InWaterCandle =		new("Condition.InWaterCandle",			() => Main.LocalPlayer.ZoneWaterCandle);
	public static readonly Condition InPeaceCandle =		new("Condition.InPeaceCandle",			() => Main.LocalPlayer.ZonePeaceCandle);
	public static readonly Condition InTowerSolar =			new("Condition.InTowerSolar",			() => Main.LocalPlayer.ZoneTowerSolar);
	public static readonly Condition InTowerVortex =		new("Condition.InTowerVortex",			() => Main.LocalPlayer.ZoneTowerVortex);
	public static readonly Condition InTowerNebula =		new("Condition.InTowerNebula",			() => Main.LocalPlayer.ZoneTowerNebula);
	public static readonly Condition InTowerStardust =		new("Condition.InTowerStardust",		() => Main.LocalPlayer.ZoneTowerStardust);
	public static readonly Condition InDesert =				new("Condition.InDesert",				() => Main.LocalPlayer.ZoneDesert);
	public static readonly Condition InGlowshroom =			new("Condition.InGlowshroom",			() => Main.LocalPlayer.ZoneGlowshroom);
	public static readonly Condition InUndergroundDesert =	new("Condition.InUndergroundDesert",	() => Main.LocalPlayer.ZoneUndergroundDesert);
	public static readonly Condition InSkyHeight =			new("Condition.InSkyHeight",			() => Main.LocalPlayer.ZoneSkyHeight);
	public static readonly Condition InOverworldHeight =	new("Condition.InOverworldHeight",		() => Main.LocalPlayer.ZoneOverworldHeight);
	public static readonly Condition InDirtLayerHeight =	new("Condition.InDirtLayerHeight",		() => Main.LocalPlayer.ZoneDirtLayerHeight);
	public static readonly Condition InRockLayerHeight =	new("Condition.InRockLayerHeight",		() => Main.LocalPlayer.ZoneRockLayerHeight);
	public static readonly Condition InUnderworldHeight =	new("Condition.InUnderworldHeight",		() => Main.LocalPlayer.ZoneUnderworldHeight);
	public static readonly Condition InBeach =				new("Condition.InBeach",				() => Main.LocalPlayer.ZoneBeach);
	public static readonly Condition InRain =				new("Condition.InRain",					() => Main.LocalPlayer.ZoneRain);
	public static readonly Condition InSandstorm =			new("Condition.InSandstorm",			() => Main.LocalPlayer.ZoneSandstorm);
	public static readonly Condition InOldOneArmy =			new("Condition.InOldOneArmy",			() => Main.LocalPlayer.ZoneOldOneArmy);
	public static readonly Condition InGranite =			new("Condition.InGranite",				() => Main.LocalPlayer.ZoneGranite);
	public static readonly Condition InMarble =				new("Condition.InMarble",				() => Main.LocalPlayer.ZoneMarble);
	public static readonly Condition InHive =				new("Condition.InHive",					() => Main.LocalPlayer.ZoneHive);
	public static readonly Condition InGemCave =			new("Condition.InGemCave",				() => Main.LocalPlayer.ZoneGemCave);
	public static readonly Condition InLihzhardTemple =		new("Condition.InLihzardTemple",		() => Main.LocalPlayer.ZoneLihzhardTemple);
	public static readonly Condition InGraveyardBiome =		new("Condition.InGraveyardBiome",		() => Main.LocalPlayer.ZoneGraveyard);
	//WorldType
	public static readonly Condition CrimsonWorld =			new("Condition.CrimsonWorld",			() => WorldGen.crimson);
	public static readonly Condition CorruptWorld =			new("Condition.CorruptWorld",			() => !WorldGen.crimson);
	
	public static readonly Condition DrunkWorld =			new("Condition.WorldDrunk",				() => Main.drunkWorld);
	public static readonly Condition RemixWorld =			new("Condition.WorldRemix",				() => Main.remixWorld);
	public static readonly Condition ForTheWorthy =			new("Condition.WorldForTheWorthy",		() => Main.getGoodWorld);
	public static readonly Condition TenthAnniversary =		new("Condition.WorldAnniversary",		() => Main.tenthAnniversaryWorld);
	public static readonly Condition ZenithWorld =			new("Condition.WorldZenith",			() => Main.remixWorld && Main.getGoodWorld);
}
			