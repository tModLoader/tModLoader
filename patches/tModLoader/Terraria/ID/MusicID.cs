using System;
using ReLogic.Reflection;
using Terraria.ModLoader;

namespace Terraria.ID;

public class MusicID
{
	public static partial class Sets
	{
		public static SetFactory Factory = new SetFactory(MusicLoader.MusicCount, nameof(MusicID), Search);
		/// <summary>
		/// Skips Terraria's <see cref="Terraria.Audio.ASoundEffectBasedAudioTrack.ReMapVolumeToMatchXact(float)"/> function to make music play at its intended volume.
		/// <para/>This should be set in <see cref="ModType.SetStaticDefaults()"/>, preferably <see cref="ModSystem"/>
		/// </summary>
		public static bool[] SkipsVolumeRemap = Factory.CreateBoolSet(false);
	}
	// Names derived from the music box that plays each: https://terraria.wiki.gg/wiki/Music_Box
	public const short OverworldDay = 1;
	public const short Eerie = 2;
	public const short Night = 3;
	public const short Underground = 4;
	public const short Boss1 = 5;
	public const short Title = 6;
	public const short Jungle = 7;
	public const short Corruption = 8;
	public const short TheHallow = 9;
	public const short UndergroundCorruption = 10;
	public const short UndergroundHallow = 11;
	public const short Boss2 = 12;
	public const short Boss3 = 13;
	public const short Snow = 14;
	public const short Space = 15;
	public const short Crimson = 16;
	public const short Boss4 = 17;
	public const short AltOverworldDay = 18;
	public const short Rain = 19;
	public const short Ice = 20;
	public const short Desert = 21;
	public const short Ocean = 22;
	public const short Dungeon = 23;
	public const short Plantera = 24;
	public const short Boss5 = 25;
	public const short Temple = 26;
	public const short Eclipse = 27;
	public const short RainSoundEffect = 28;
	public const short Mushrooms = 29;
	public const short PumpkinMoon = 30;
	public const short AltUnderground = 31;
	public const short FrostMoon = 32;
	public const short UndergroundCrimson = 33;
	public const short TheTowers = 34;
	public const short PirateInvasion = 35;
	public const short Hell = 36;
	public const short MartianMadness = 37;
	public const short LunarBoss = 38;
	public const short GoblinInvasion = 39;
	public const short Sandstorm = 40;
	public const short OldOnesArmy = 41;
	public const short SpaceDay = 42;
	public const short OceanNight = 43;
	public const short WindyDay = 44;
	public const short TownDay = 46;
	public const short TownNight = 47;
	public const short SlimeRain = 48;
	public const short DayRemix = 49;
	public const short MenuMusic = 50;
	public const short Monsoon = 52;
	public const short Graveyard = 53;
	public const short JungleUnderground = 54;
	public const short JungleNight = 55;
	public const short QueenSlime = 56;
	public const short EmpressOfLight = 57;
	public const short DukeFishron = 58;
	public const short MorningRain = 59;
	public const short ConsoleMenu = 60;
	public const short UndergroundDesert = 61;
	public const short OtherworldlyRain = 62;
	public const short OtherworldlyDay = 63;
	public const short OtherworldlyNight = 64;
	public const short OtherworldlyUnderground = 65;
	public const short OtherworldlyDesert = 66;
	public const short OtherworldlyOcean = 67;
	public const short OtherworldlyMushrooms = 68;
	public const short OtherworldlyDungeon = 69;
	public const short OtherworldlySpace = 70;
	public const short OtherworldlyUnderworld = 71;
	public const short OtherworldlySnow = 72;
	public const short OtherworldlyCorruption = 73;
	public const short OtherworldlyUGCorrption = 74;
	public const short OtherworldlyCrimson = 75;
	public const short OtherworldlyUGCrimson = 76;
	public const short OtherworldlyIce = 77;
	public const short OtherworldlyUGHallow = 78;
	public const short OtherworldlyEerie = 79;
	public const short OtherworldlyBoss2 = 80;
	public const short OtherworldlyBoss1 = 81;
	public const short OtherworldlyInvasion = 82;
	public const short OtherworldlyTowers = 83;
	public const short OtherworldlyLunarBoss = 84;
	public const short OtherworldlyPlantera = 85;
	public const short OtherworldlyJungle = 86;
	public const short OtherworldlyWoF = 87;
	public const short OtherworldlyHallow = 88;
	public const short Credits = 89;
	public const short Deerclops = 90;
	public const short Shimmer = 91;
	public const short Count = 92;

	public static IdDictionary Search = IdDictionary.Create<MusicID, short>();
}
