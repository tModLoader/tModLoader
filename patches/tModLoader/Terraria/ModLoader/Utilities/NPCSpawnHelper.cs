using System;
using System.Collections.Generic;
using Terraria.GameContent.Events;
using Terraria.ID;

namespace Terraria.ModLoader.Utilities;

internal static class NPCSpawnHelper
{
	internal static List<SpawnCondition> conditions = new();

	internal static void Reset()
	{
		foreach (SpawnCondition condition in conditions) {
			condition.Reset();
		}
	}

	internal static void DoChecks(NPCSpawnInfo info)
	{
		float weight = 1f;
		foreach (SpawnCondition condition in conditions) {
			condition.Check(info, ref weight);
			if (Math.Abs(weight) < 5E-6) {
				break;
			}
		}
	}
}

//todo: further documentation
/// <summary>
/// This serves as a central class to help modders spawn their NPCs. It's basically the vanilla spawn code if-else chains condensed into objects. See ExampleMod for usages.
/// </summary>
public class SpawnCondition
{
	private readonly Func<NPCSpawnInfo, bool> condition;
	private readonly List<SpawnCondition> children = new();
	private float blockWeight;
	internal Func<NPCSpawnInfo, float> WeightFunc;

	private float chance;
	private bool active;

	internal IEnumerable<SpawnCondition> Children => children;
	public float BlockWeight => blockWeight;

	public float Chance => chance;
	public bool Active => active;
	private static SpawnCondition category = null;

	private SpawnCondition()
	{
		category.children.Add(this);
	}

	public SpawnCondition(Func<NPCSpawnInfo, bool> condition, float blockWeight = 1f) : this()
	{
		this.condition = condition;
		this.blockWeight = blockWeight;
		NPCSpawnHelper.conditions.Add(this);
	}

	public SpawnCondition(float blockWeight) : this((info) => true, blockWeight)
	{ }

	public SpawnCondition(Func<NPCSpawnInfo, bool> condition, Func<NPCSpawnInfo, float> weightFunc) : this(condition)
	{ WeightFunc = weightFunc; }

	public SpawnCondition(SpawnCondition parent, Func<NPCSpawnInfo, bool> condition, float blockWeight = 1f) : this(new[] { parent }, condition, blockWeight)
	{ }

	public SpawnCondition(params SpawnCondition[] parents) : this(parents, (info) => true)
	{ }

	public SpawnCondition(SpawnCondition parent, Func<NPCSpawnInfo, bool> condition, Func<NPCSpawnInfo, float> weightFunc) : this(parent, condition)
	{ WeightFunc = weightFunc; }

	public SpawnCondition(IEnumerable<SpawnCondition> parents, Func<NPCSpawnInfo, bool> condition, float blockWeight = 1f) : this()
	{
		this.condition = condition;
		this.blockWeight = blockWeight;
		foreach (SpawnCondition parent in parents)
			parent.children.Add(this);
	}

	internal void Reset()
	{
		chance = 0f;
		active = false;
		foreach (SpawnCondition child in children) {
			child.Reset();
		}
	}

	internal void Check(NPCSpawnInfo info, ref float remainingWeight)
	{
		if (WeightFunc != null) { // Calculate weight
			blockWeight = WeightFunc(info);
		}

		active = true;
		float saveChance = chance; // Save current chance for if this is not the first check for this condition
		chance = 0f;
		if (condition(info)) {
			chance = remainingWeight * blockWeight; // If condition passes, calc as usual for this parent
			float childWeight = chance;
			foreach (SpawnCondition child in children) {
				child.Check(info, ref childWeight);
				if (Math.Abs(childWeight) < 5E-6) {
					break;
				}
			}
			remainingWeight -= chance;
		}
		chance += saveChance; // Once finished, total up chance for this item
	}

	public static readonly SpawnCondition NebulaTower;
	public static readonly SpawnCondition VortexTower;
	public static readonly SpawnCondition StardustTower;
	public static readonly SpawnCondition SolarTower;
	public static readonly SpawnCondition Sky;
	public static readonly SpawnCondition Invasion;
	public static readonly SpawnCondition GoblinArmy;
	public static readonly SpawnCondition FrostLegion;
	public static readonly SpawnCondition Pirates;
	public static readonly SpawnCondition MartianMadness;
	public static readonly SpawnCondition Bartender;
	public static readonly SpawnCondition SpiderCave;
	public static readonly SpawnCondition DesertCave;
	public static readonly SpawnCondition HardmodeJungleWater;
	public static readonly SpawnCondition HardmodeCrimsonWater;
	public static readonly SpawnCondition Ocean;
	public static readonly SpawnCondition OceanAngler;
	public static readonly SpawnCondition OceanMonster;
	public static readonly SpawnCondition BeachAngler;
	public static readonly SpawnCondition CaveOrJungleWater;
	public static readonly SpawnCondition JungleWater;
	public static readonly SpawnCondition JungleWaterSurfaceCritter;
	public static readonly SpawnCondition JunglePiranha;
	public static readonly SpawnCondition CaveWater;
	public static readonly SpawnCondition CavePiranha;
	public static readonly SpawnCondition Piranha = new(CavePiranha, JunglePiranha);
	public static readonly SpawnCondition CaveJellyfish;
	public static readonly SpawnCondition WaterCritter;
	public static readonly SpawnCondition CorruptWaterCritter;
	public static readonly SpawnCondition OverworldWaterCritter;
	public static readonly SpawnCondition OverworldWaterSurfaceCritter;
	public static readonly SpawnCondition OverworldUnderwaterCritter;
	public static readonly SpawnCondition DefaultWaterCritter;
	public static readonly SpawnCondition BoundCaveNPC;
	public static readonly SpawnCondition TownCritter;
	public static readonly SpawnCondition TownGraveyardCritter;
	public static readonly SpawnCondition TownGraveyardWaterCritter; // No vanilla relation
	public static readonly SpawnCondition TownBeachCritter; // Just Seagull
	public static readonly SpawnCondition TownBeachWaterCritter;
	public static readonly SpawnCondition TownWaterCritter;
	public static readonly SpawnCondition TownOverworldWaterCritter;
	public static readonly SpawnCondition TownOverworldWaterSurfaceCritter;
	public static readonly SpawnCondition TownOverworldWaterBeachCritter;

	/// <summary>
	/// Currently Returns <see cref="TownDefaultWaterCritter"/>, replicating <see cref="NPCID.Goldfish"/> spawning behaviour. Use <see cref="TownDefaultWaterCritter"/>
	/// alongside <see cref="WaterSurface(NPCSpawnInfo)"/> for original behaviour
	/// </summary>
	[Obsolete("Does not correspond to a read vanilla NPC, to replicate the spawning of goldfish use TownDefaultWaterCritter, to replicate the spawning of pupfish use TownOverworldWaterBeachCritter.")]
	public static SpawnCondition TownOverworldUnderwaterCritter => TownDefaultWaterCritter;

	public static readonly SpawnCondition TownDefaultWaterCritter;
	public static readonly SpawnCondition TownSnowCritter;
	public static readonly SpawnCondition TownJungleCritter;
	public static readonly SpawnCondition TownDesertCritter;
	public static readonly SpawnCondition TownGrassCritter;
	public static readonly SpawnCondition TownRainingUnderGroundCritter;
	public static readonly SpawnCondition TownCritterGreenFairy;
	public static readonly SpawnCondition TownGemSquirrel;
	public static readonly SpawnCondition TownGemBunny;
	public static readonly SpawnCondition TownGeneralCritter;
	public static readonly SpawnCondition Dungeon;
	public static readonly SpawnCondition DungeonGuardian;
	public static readonly SpawnCondition DungeonNormal;
	public static readonly SpawnCondition Meteor;
	public static readonly SpawnCondition OldOnesArmy;
	public static readonly SpawnCondition FrostMoon;
	public static readonly SpawnCondition PumpkinMoon;
	public static readonly SpawnCondition SolarEclipse;
	public static readonly SpawnCondition HardmodeMushroomWater;
	public static readonly SpawnCondition OverworldMushroom;
	public static readonly SpawnCondition UndergroundMushroom;
	public static readonly SpawnCondition CorruptWorm;
	public static readonly SpawnCondition UndergroundMimic;
	public static readonly SpawnCondition OverworldMimic;
	public static readonly SpawnCondition Wraith;
	public static readonly SpawnCondition HoppinJack;
	public static readonly SpawnCondition DoctorBones;
	public static readonly SpawnCondition LacBeetle;
	public static readonly SpawnCondition WormCritter;
	public static readonly SpawnCondition MouseCritter;
	public static readonly SpawnCondition SnailCritter;
	public static readonly SpawnCondition JungleCritterBirdOrFrog;
	public static readonly SpawnCondition JungleCritterBird;
	public static readonly SpawnCondition FrogCritter;
	public static readonly SpawnCondition Hive;
	public static readonly SpawnCondition HardmodeJungle;
	public static readonly SpawnCondition JungleTemple;
	public static readonly SpawnCondition HiveHornet;
	public static readonly SpawnCondition UndergroundJungle;
	public static readonly SpawnCondition SurfaceJungle;
	public static readonly SpawnCondition SandstormEvent;
	public static readonly SpawnCondition Mummy;
	public static readonly SpawnCondition DarkMummy;
	public static readonly SpawnCondition BloodMummy;
	public static readonly SpawnCondition LightMummy;
	public static readonly SpawnCondition OverworldHallow;
	public static readonly SpawnCondition EnchantedSword;
	public static readonly SpawnCondition Crimson;
	public static readonly SpawnCondition Corruption;
	public static readonly SpawnCondition Overworld;
	public static readonly SpawnCondition IceGolem;
	public static readonly SpawnCondition RainbowSlime;
	public static readonly SpawnCondition AngryNimbus;
	public static readonly SpawnCondition MartianProbe;
	public static readonly SpawnCondition OverworldDay;
	public static readonly SpawnCondition OverworldDaySnowCritter;
	public static readonly SpawnCondition OverworldDayGrassCritter;
	public static readonly SpawnCondition OverworldDaySandCritter;
	public static readonly SpawnCondition OverworldMorningBirdCritter;
	public static readonly SpawnCondition OverworldDayBirdCritter;
	public static readonly SpawnCondition KingSlime;
	public static readonly SpawnCondition OverworldDayDesert;
	public static readonly SpawnCondition GoblinScout;
	public static readonly SpawnCondition OverworldDayRain;
	public static readonly SpawnCondition OverworldDaySlime;
	public static readonly SpawnCondition OverworldNight;
	public static readonly SpawnCondition OverworldFirefly;
	public static readonly SpawnCondition OverworldNightMonster;
	public static readonly SpawnCondition Underground;
	public static readonly SpawnCondition Underworld;
	public static readonly SpawnCondition Cavern;
	public static readonly SpawnCondition RockGolem;
	public static readonly SpawnCondition DyeBeetle;
	public static readonly SpawnCondition ChaosElemental;
	public static readonly SpawnCondition PurplePigron;
	public static readonly SpawnCondition BluePigron;
	public static readonly SpawnCondition PinkPigron;
	public static readonly SpawnCondition IceTortiose;
	public static readonly SpawnCondition DiggerWormFlinx;
	public static readonly SpawnCondition Flinx1;
	public static readonly SpawnCondition MotherSlimeBlueSlimeFlinx;
	public static readonly SpawnCondition JungleSlimeBlackSlimeFlinx;
	public static readonly SpawnCondition MiscCavern;
	public static readonly SpawnCondition SkeletonMerchant;
	public static readonly SpawnCondition LostGirl;
	public static readonly SpawnCondition RuneWizzard;
	public static readonly SpawnCondition Marble;
	public static readonly SpawnCondition Granite;
	public static readonly SpawnCondition Tim;
	public static readonly SpawnCondition ArmouredVikingIcyMermanSkeletonArcherArmouredSkeleton;
	public static readonly SpawnCondition UndeadMiner;
	public static readonly SpawnCondition UndeadVikingSnowFlinx;
	public static readonly SpawnCondition Flinx2;
	public static readonly SpawnCondition GenericCavernMonster;
	public static readonly SpawnCondition SporeSkeletons;
	public static readonly SpawnCondition HalloweenSkeletons;
	public static readonly SpawnCondition ExpertSkeletons;
	public static readonly SpawnCondition NormalSkeletons;
	public static readonly SpawnCondition AllSkeletons = new(NormalSkeletons, ExpertSkeletons, HalloweenSkeletons, SporeSkeletons);

	//public static readonly SpawnCondition Gnome;
	public static readonly SpawnCondition Ghost;

	static SpawnCondition()
	{
		// Pillars
		NebulaTower = new SpawnCondition((info) => info.Player.ZoneTowerNebula);
		VortexTower = new SpawnCondition((info) => info.Player.ZoneTowerVortex);
		StardustTower = new SpawnCondition((info) => info.Player.ZoneTowerStardust);
		SolarTower = new SpawnCondition((info) => info.Player.ZoneTowerSolar);

		//Sky
		Sky = new SpawnCondition((info) => info.Sky);

		//Invasions
		Invasion = new SpawnCondition((info) => info.Invasion);
		GoblinArmy = new SpawnCondition(Invasion, (info) => Main.invasionType == 1);
		FrostLegion = new SpawnCondition(Invasion, (info) => Main.invasionType == 2);
		Pirates = new SpawnCondition(Invasion, (info) => Main.invasionType == 3);
		MartianMadness = new SpawnCondition(Invasion, (info) => Main.invasionType == 4);

		//Bartender
		Bartender = new SpawnCondition((info) => !NPC.savedBartender && DD2Event.ReadyToFindBartender
			&& !NPC.AnyNPCs(NPCID.BartenderUnconscious) && !info.Water, 1f / 80f);

		SpiderCave = new SpawnCondition((info) => GetTile(info).wall == WallID.SpiderUnsafe || info.SpiderCave);

		DesertCave = new SpawnCondition((info) => (WallID.Sets.Conversion.HardenedSand[GetTile(info).wall]
			|| WallID.Sets.Conversion.Sandstone[GetTile(info).wall] || info.DesertCave)
			&& WorldGen.checkUnderground(info.SpawnTileX, info.SpawnTileY));

		//Hardmode Water
		HardmodeJungleWater = new SpawnCondition((info) => Main.hardMode && info.Water && info.Player.ZoneJungle, 2f / 3f);
		HardmodeCrimsonWater = new SpawnCondition((info) => Main.hardMode && info.Water && info.Player.ZoneCrimson, 8f / 9f);

		//Ocean
		Ocean = new SpawnCondition((info) => info.Water && (info.SpawnTileX < WorldGen.oceanDistance || info.SpawnTileX > Main.maxTilesX - WorldGen.oceanDistance)
			&& Main.tileSand[info.SpawnTileType] && info.SpawnTileY < Main.rockLayer);
		OceanAngler = new SpawnCondition(Ocean, (info) => !NPC.savedAngler && !NPC.AnyNPCs(NPCID.SleepingAngler)
			&& WaterSurface(info));
		OceanMonster = new SpawnCondition(Ocean, (info) => true);

		//Beach
		BeachAngler = new SpawnCondition((info) => !info.Water && !NPC.savedAngler && !NPC.AnyNPCs(NPCID.SleepingAngler)
			&& (info.SpawnTileX < WorldGen.beachDistance || info.SpawnTileX > Main.maxTilesX - WorldGen.beachDistance) && Main.tileSand[info.SpawnTileType]
			&& info.SpawnTileY < Main.worldSurface);

		//Misc Water
		CaveOrJungleWater = new SpawnCondition((info) => !info.PlayerInTown && info.Water);
		JungleWater = new SpawnCondition(CaveOrJungleWater, (info) => info.SpawnTileType == TileID.JungleGrass);
		JungleWaterSurfaceCritter = new SpawnCondition(JungleWater, (info) => info.OverWorld && info.SpawnTileY > 50 && Main.dayTime && WaterSurface(info), 1f / 3f);
		JunglePiranha = new SpawnCondition(JungleWater, (info) => true);
		CavePiranha = new SpawnCondition(CaveOrJungleWater, (info) => info.Caverns, 0.5f);
		CaveJellyfish = new SpawnCondition((info) => !info.PlayerInTown && info.Water && info.SpawnTileY > Main.worldSurface, 1f / 3f);

		// Water Critters
		WaterCritter = new SpawnCondition((info) => info.Water, 0.25f);
		CorruptWaterCritter = new SpawnCondition(WaterCritter, (info) => info.Player.ZoneCorrupt);
		OverworldWaterCritter = new SpawnCondition(WaterCritter, (info) => info.SpawnTileY < Main.worldSurface
			&& info.SpawnTileY > 50 && Main.dayTime, 2f / 3f);
		OverworldWaterSurfaceCritter = new SpawnCondition(OverworldWaterCritter, WaterSurface);
		OverworldUnderwaterCritter = new SpawnCondition(OverworldWaterCritter, (info) => true);
		DefaultWaterCritter = new SpawnCondition(WaterCritter, (info) => true);

		// Tinkerer/Wizard
		BoundCaveNPC = new SpawnCondition((info) => !info.Water && info.SpawnTileY >= Main.rockLayer
			&& info.SpawnTileY < Main.maxTilesY - 210, 1f / 20f);

		// Town Critters
		TownCritter = new SpawnCondition((info) => info.PlayerInTown);
		// Graveyard
		TownGraveyardCritter = new SpawnCondition(TownCritter, (info) => info.Player.ZoneGraveyard);
		TownGraveyardWaterCritter = new SpawnCondition(TownGraveyardCritter, (info) => info.Water);
		// Beach
		TownBeachCritter = new SpawnCondition(TownCritter, (info) => !info.SafeRangeX && info.Beach);
		TownBeachWaterCritter = new SpawnCondition(TownBeachCritter, (info) => info.Water);
		// Water General
		TownWaterCritter = new SpawnCondition(TownCritter, (info) => info.Water);
		TownOverworldWaterCritter = new SpawnCondition(TownWaterCritter, (info) => info.OverWorld
			&& info.SpawnTileY > 50 && Main.dayTime, 2f / 3f);
		TownOverworldWaterSurfaceCritter = new SpawnCondition(TownOverworldWaterCritter, WaterSurface);
		TownOverworldWaterBeachCritter = new SpawnCondition(new SpawnCondition[] { TownWaterCritter, TownOverworldWaterCritter }, (info) => Beach(info));
		TownDefaultWaterCritter = new SpawnCondition(new SpawnCondition[] { TownWaterCritter, TownOverworldWaterCritter }, (info) => true);
		TownSnowCritter = new SpawnCondition(TownCritter, (info) => info.SpawnTileType == TileID.SnowBlock
			|| info.SpawnTileType == TileID.IceBlock);
		TownJungleCritter = new SpawnCondition(TownCritter, (info) => info.SpawnTileType == TileID.JungleGrass);
		TownDesertCritter = new SpawnCondition(TownCritter, (info) => info.SpawnTileType == TileID.Sand);
		TownGrassCritter = new SpawnCondition(TownCritter, (info) => info.SpawnTileY > Main.worldSurface
			|| info.SpawnTileType == TileID.Grass || info.SpawnTileType == TileID.GolfGrass
			|| info.SpawnTileType == TileID.HallowedGrass || info.SpawnTileType == TileID.GolfGrassHallowed);
		TownRainingUnderGroundCritter = new SpawnCondition(TownCritter, (info) => Main.raining && info.PlayerFloorY <= Main.UnderworldLayer);
		TownGemSquirrel = new SpawnCondition(TownRainingUnderGroundCritter, (info) => info.Caverns, 0.2f);
		TownGemBunny = new SpawnCondition(TownRainingUnderGroundCritter, (info) => info.Caverns, 0.2f);
		TownGeneralCritter = new SpawnCondition(TownRainingUnderGroundCritter, (info) => true);
		TownCritterGreenFairy = new SpawnCondition(TownGrassCritter, (info) => !Main.dayTime && Main.numClouds <= 55
			&& Main.cloudBGActive == 0f && Star.starfallBoost > 3f && info.OverWorld, GetPlayerRollWeightFunc(2));

		// Dungeon
		Dungeon = new SpawnCondition((info) => info.Player.ZoneDungeon);
		DungeonGuardian = new SpawnCondition(Dungeon, (info) => !NPC.downedBoss3 && (!Main.drunkWorld || (info.Player.position.Y / 16f < (Main.dungeonY + 40))));
		DungeonNormal = new SpawnCondition(Dungeon, (info) => true);

		// Meteor
		Meteor = new SpawnCondition((info) => info.Player.ZoneMeteor);

		// Events
		OldOnesArmy = new SpawnCondition((info) => DD2Event.Ongoing && info.Player.ZoneOldOneArmy);
		FrostMoon = new SpawnCondition((info) => info.SpawnTileY <= Main.worldSurface && !Main.dayTime && Main.snowMoon);
		PumpkinMoon = new SpawnCondition((info) => info.SpawnTileY <= Main.worldSurface
			&& !Main.dayTime && Main.pumpkinMoon);
		SolarEclipse = new SpawnCondition((info) => info.SpawnTileY <= Main.worldSurface && Main.dayTime && Main.eclipse);

		// Mushroom
		HardmodeMushroomWater = new SpawnCondition((info) => Main.hardMode && info.SpawnTileType == TileID.MushroomGrass
			&& info.Water);
		OverworldMushroom = new SpawnCondition((info) => info.SpawnTileType == TileID.MushroomGrass
			&& info.SpawnTileY <= Main.worldSurface, 2f / 3f);
		UndergroundMushroom = new SpawnCondition((info) => info.SpawnTileType == TileID.MushroomGrass
			&& Main.hardMode && info.SpawnTileY >= Main.worldSurface, 2f / 3f);

		// Misc
		CorruptWorm = new SpawnCondition((info) => info.Player.ZoneCorrupt && !info.PlayerSafe, 1f / 65f);
		UndergroundMimic = new SpawnCondition((info) => Main.hardMode && info.SpawnTileY > Main.worldSurface, 1f / 70f);
		OverworldMimic = new SpawnCondition((info) => Main.hardMode && GetTile(info).wall == WallID.DirtUnsafe, 0.05f);
		Wraith = new SpawnCondition((info) => Main.hardMode && info.SpawnTileY <= Main.worldSurface
			&& !Main.dayTime, 0.05f) {
			WeightFunc = (info) => {
				float inverseChance = 0.95f;
				if (Main.moonPhase == 4) {
					inverseChance *= 0.8f;
				}
				return 1f - inverseChance;
			}
		};
		HoppinJack = new SpawnCondition((info) => Main.hardMode && Main.halloween
			&& info.SpawnTileY <= Main.worldSurface && !Main.dayTime, 0.1f);
		DoctorBones = new SpawnCondition((info) => info.SpawnTileType == TileID.JungleGrass && !Main.dayTime, GetPlayerRollWeightFunc(500));
		LacBeetle = new SpawnCondition((info) => info.SpawnTileType == TileID.JungleGrass
			&& info.SpawnTileY > Main.worldSurface, 1f / 60f);

		// Critters
		WormCritter = new SpawnCondition((info) => info.SpawnTileY > Main.worldSurface
			&& info.SpawnTileY < Main.maxTilesY - 210 && !info.Player.ZoneSnow && !info.Player.ZoneCrimson
			&& !info.Player.ZoneCorrupt && !info.Player.ZoneJungle && !info.Player.ZoneHallow, 1f / 8f);
		MouseCritter = new SpawnCondition((info) => info.SpawnTileY > Main.worldSurface
			&& info.SpawnTileY < Main.maxTilesY - 210 && !info.Player.ZoneSnow && !info.Player.ZoneCrimson
			&& !info.Player.ZoneCorrupt && !info.Player.ZoneJungle && !info.Player.ZoneHallow, 1f / 13f);
		SnailCritter = new SpawnCondition((info) => info.SpawnTileY > Main.worldSurface
			&& info.SpawnTileY < (Main.rockLayer + Main.maxTilesY) / 2 && !info.Player.ZoneSnow
			&& !info.Player.ZoneCrimson && !info.Player.ZoneCorrupt && !info.Player.ZoneHallow, 1f / 13f);
		JungleCritterBirdOrFrog = new SpawnCondition((info) => info.OverWorld && info.Player.ZoneJungle && !info.Player.ZoneCrimson && !info.Player.ZoneCorrupt, 1f / 7f);
		JungleCritterBird = new SpawnCondition(JungleCritterBirdOrFrog, (info) => Main.dayTime && Main.time < 43200.00064373016, 2f / 3f);
		FrogCritter = new SpawnCondition(JungleCritterBirdOrFrog, (info) => true);

		Hive = new SpawnCondition((info) => info.ProperGroundTileType == TileID.Hive, 0.5f);

		// Jungle
		HardmodeJungle = new SpawnCondition((info) => info.SpawnTileType == TileID.JungleGrass && Main.hardMode, 2f / 3f);
		JungleTemple = new SpawnCondition((info) => (info.SpawnTileType == TileID.LihzahrdBrick || info.ProperGroundTileType == TileID.WoodenSpikes || Main.remixWorld) && info.Lihzahrd);
		HiveHornet = new SpawnCondition((info) => info.WallTileType == WallID.HiveUnsafe, 7f / 8f);
		UndergroundJungle = new SpawnCondition((info) => info.SpawnTileType == TileID.JungleGrass
			&& (Main.remixWorld ? (info.SpawnTileY < Main.rockLayer) : info.SpawnTileY > (Main.worldSurface + Main.rockLayer) / 2.0), (info) => Main.remixWorld ? 0.5f : 1f);
		SurfaceJungle = new SpawnCondition((info) => info.SpawnTileType == TileID.JungleGrass, 11f / 32f);

		// Sandstorm
		SandstormEvent = new SpawnCondition((info) => Sandstorm.Happening && info.Player.ZoneSandstorm
			&& TileID.Sets.Conversion.Sand[info.SpawnTileType]
			&& NPC.Spawning_SandstoneCheck(info.SpawnTileX, info.SpawnTileY));

		// Mummy
		Mummy = new SpawnCondition((info) => Main.hardMode && info.ProperGroundTileType == TileID.Sand, 1f / 3f);
		DarkMummy = new SpawnCondition((info) => Main.hardMode && info.ProperGroundTileType == TileID.Ebonsand, 0.5f);
		BloodMummy = new SpawnCondition((info) => Main.hardMode && info.ProperGroundTileType == TileID.Crimsand, 0.5f);
		LightMummy = new SpawnCondition((info) => Main.hardMode && info.ProperGroundTileType == TileID.Pearlsand, 0.5f);

		// Hallow
		OverworldHallow = new SpawnCondition((info) => Main.hardMode && !info.Water && info.UnderGround
			&& (info.ProperGroundTileType == TileID.Pearlsand || info.ProperGroundTileType == TileID.Pearlstone
			|| info.ProperGroundTileType == TileID.HallowedGrass || info.ProperGroundTileType == TileID.HallowedIce));
		EnchantedSword = new SpawnCondition((info) => !info.PlayerSafe && Main.hardMode && !info.Water
			&& info.Caverns && (info.ProperGroundTileType == TileID.Pearlsand
			|| info.ProperGroundTileType == TileID.Pearlstone || info.ProperGroundTileType == TileID.HallowedGrass
			|| info.ProperGroundTileType == TileID.HallowedIce), 0.02f);

		// Crimson
		Crimson = new SpawnCondition((info) => (info.ProperGroundTileType == TileID.Crimtane && info.Player.ZoneCrimson)
			|| info.ProperGroundTileType == TileID.CrimsonGrass || info.ProperGroundTileType == TileID.FleshIce
			|| info.ProperGroundTileType == TileID.Crimstone || info.ProperGroundTileType == TileID.Crimsand
			|| info.ProperGroundTileType == TileID.CrimsonJungleGrass);

		// Corruption
		Corruption = new SpawnCondition((info) => (info.ProperGroundTileType == TileID.Demonite && info.Player.ZoneCorrupt)
			|| info.ProperGroundTileType == TileID.CorruptGrass || info.ProperGroundTileType == TileID.Ebonstone
			|| info.ProperGroundTileType == TileID.Ebonsand || info.ProperGroundTileType == TileID.CorruptIce
			|| info.ProperGroundTileType == TileID.CorruptJungleGrass);

		// Overworld
		Overworld = new SpawnCondition((info) => info.OverWorld);

		// Overworld Misc
		IceGolem = new SpawnCondition(Overworld, (info) => info.Player.ZoneSnow && Main.hardMode
			&& Main.cloudAlpha > 0f && !NPC.AnyNPCs(NPCID.IceGolem), 0.05f);
		RainbowSlime = new SpawnCondition(Overworld, (info) => info.Player.ZoneHallow && Main.hardMode
			&& Main.cloudAlpha > 0f && !NPC.AnyNPCs(NPCID.RainbowSlime), 0.05f);
		AngryNimbus = new SpawnCondition(Overworld, (info) => !info.Player.ZoneSnow && Main.hardMode
			&& Main.cloudAlpha > 0f && NPC.CountNPCS(NPCID.AngryNimbus) < 2, 0.1f);
		MartianProbe = new SpawnCondition(Overworld, (info) => MartianProbeHelper(info) && Main.hardMode
			&& NPC.downedGolemBoss && !NPC.AnyNPCs(NPCID.MartianProbe), 1f / 400f) {
			WeightFunc = (info) => {
				float inverseChance = 399f / 400f;
				if (!NPC.downedMartians) {
					inverseChance *= 0.99f;
				}
				return 1f - inverseChance;
			}
		};

		// Overworld Typical
		OverworldDay = new SpawnCondition(Overworld, (info) => Main.dayTime);
		OverworldDaySnowCritter = new SpawnCondition(OverworldDay, (info) => InnerThird(info)
			&& (GetTile(info).type == TileID.SnowBlock || GetTile(info).type == TileID.IceBlock), 1f / 15f);
		OverworldDayGrassCritter = new SpawnCondition(OverworldDay, (info) => InnerThird(info)
			&& (GetTile(info).type == TileID.Grass || GetTile(info).type == TileID.HallowedGrass), 1f / 15f);
		OverworldDaySandCritter = new SpawnCondition(OverworldDay, (info) => InnerThird(info)
			&& GetTile(info).type == TileID.Sand, 1f / 15f);
		OverworldMorningBirdCritter = new SpawnCondition(OverworldDay, (info) => InnerThird(info) && Main.time < 18000.0
		&& (GetTile(info).type == TileID.Grass || GetTile(info).type == TileID.HallowedGrass), 0.25f);
		OverworldDayBirdCritter = new SpawnCondition(OverworldDay, (info) => InnerThird(info)
			&& (GetTile(info).type == TileID.Grass || GetTile(info).type == TileID.HallowedGrass
			|| GetTile(info).type == TileID.SnowBlock), 1f / 15f);

		// King Slime (Overworld)
		KingSlime = new SpawnCondition(OverworldDay, (info) => OuterThird(info) && GetTile(info).type == TileID.Grass
			&& !NPC.AnyNPCs(NPCID.KingSlime), 1f / 300f);

		OverworldDayDesert = new SpawnCondition(OverworldDay, (info) => GetTile(info).type == TileID.Sand
			&& !info.Water, 0.2f);

		// Overworld Goblin Scout
		GoblinScout = new SpawnCondition(OverworldDay, (info) => OuterThird(info), 1f / 15f) {
			WeightFunc = (info) => {
				float inverseChance = 14f / 15f;
				if (!NPC.downedGoblins && WorldGen.shadowOrbSmashed) {
					return inverseChance *= (6f / 7f);
				}
				return 1f - inverseChance;
			}
		};

		// Overworld Typical
		OverworldDayRain = new SpawnCondition(OverworldDay, (info) => Main.raining, 2f / 3f);
		OverworldDaySlime = new SpawnCondition(OverworldDay);
		OverworldNight = new SpawnCondition(Overworld);
		OverworldFirefly = new SpawnCondition(OverworldNight, (info) => GetTile(info).type == TileID.Grass
			|| GetTile(info).type == TileID.HallowedGrass, 0.1f) {
			WeightFunc = (info) => 1f / NPC.fireFlyChance
		};

		//Overworld Monsters
		OverworldNightMonster = new SpawnCondition(OverworldNight);

		// Underground
		Underground = new SpawnCondition((info) => info.UnderGround);
		Underworld = new SpawnCondition((info) => info.SpawnTileY > Main.maxTilesY - 190);

		// Caverns
		category = Cavern = new SpawnCondition((info) => true);

		RockGolem = new SpawnCondition((info) => NPC.SpawnNPC_CheckToSpawnRockGolem(info.SpawnTileX, info.SpawnTileY, info.Player.whoAmI, info.ProperGroundTileType));
		DyeBeetle = new SpawnCondition(1f / 60f);
		ChaosElemental = new SpawnCondition((info) => Main.hardMode && !info.PlayerSafe
			&& (info.ProperGroundTileType == TileID.Pearlsand || info.ProperGroundTileType == TileID.Pearlstone || info.ProperGroundTileType == TileID.HallowedIce), 1f / 8f);

		PurplePigron = new SpawnCondition((info) => (info.SpawnTileType == TileID.SnowBlock
			|| info.SpawnTileType == TileID.IceBlock || info.SpawnTileType == TileID.BreakableIce
			|| info.SpawnTileType == TileID.CorruptIce || info.SpawnTileType == TileID.HallowedIce || info.SpawnTileType == TileID.FleshIce)
			&& !info.PlayerSafe && Main.hardMode && info.Player.ZoneCorrupt, 1f / 30f);

		BluePigron = new SpawnCondition((info) => (info.SpawnTileType == TileID.SnowBlock
			|| info.SpawnTileType == TileID.IceBlock || info.SpawnTileType == TileID.BreakableIce
			|| info.SpawnTileType == TileID.CorruptIce || info.SpawnTileType == TileID.HallowedIce || info.SpawnTileType == TileID.FleshIce)
			&& !info.PlayerSafe && Main.hardMode && info.Player.ZoneHallow, 1f / 30f);

		PinkPigron = new SpawnCondition((info) => (info.SpawnTileType == TileID.SnowBlock
			|| info.SpawnTileType == TileID.IceBlock || info.SpawnTileType == TileID.BreakableIce
			|| info.SpawnTileType == TileID.CorruptIce || info.SpawnTileType == TileID.HallowedIce || info.SpawnTileType == TileID.FleshIce)
			&& !info.PlayerSafe && Main.hardMode && info.Player.ZoneCrimson, 1f / 30f);

		IceTortiose = new SpawnCondition((info) => Main.hardMode && info.Player.ZoneSnow, 0.1f);
		DiggerWormFlinx = new SpawnCondition((info) => !info.PlayerSafe && info.Player.ZoneHallow, 0.01f);
		Flinx1 = new SpawnCondition((info) => info.Player.ZoneSnow, 1f / 20f);
		MotherSlimeBlueSlimeFlinx = new SpawnCondition((info) => true, (info) => Main.hardMode ? 1f / 20f : 1f / 10f);
		JungleSlimeBlackSlimeFlinx = new SpawnCondition((info) => !Main.hardMode, 0.25f);
		MiscCavern = new SpawnCondition(0.5f);
		SkeletonMerchant = new SpawnCondition((info) => NPC.CountNPCS(453) == 0, 1f / 35f);
		LostGirl = new SpawnCondition(1f / 80f);
		RuneWizzard = new SpawnCondition((info) => Main.hardMode && (Main.remixWorld || info.SpawnTileY > (Main.rockLayer + Main.maxTilesY) / 2.0), 1f / 200f);
		Marble = new SpawnCondition((info) => info.Marble, 3f / 4f);
		Granite = new SpawnCondition((info) => info.Granite, 4f / 5f);

		bool TimArmourCheck(NPCSpawnInfo info)
			=> (info.Player.armor[1].type == 4256 || (info.Player.armor[1].type >= 1282 && info.Player.armor[1].type <= 1287)) && info.Player.armor[0].type != 238;
		Tim = new SpawnCondition((info) => (Main.remixWorld || info.SpawnTileY > (Main.rockLayer + Main.maxTilesY) / 2.0)
		, (info) => 1f / (TimArmourCheck(info) ? 50f : 200f));

		ArmouredVikingIcyMermanSkeletonArcherArmouredSkeleton = new SpawnCondition((info) => Main.hardMode, 0.9f);
		Ghost = new SpawnCondition((info) => (!info.PlayerSafe && (Main.halloween || info.Player.ZoneGraveyard)), 1f / 30f);
		UndeadMiner = new SpawnCondition(1f / 20f);
		UndeadVikingSnowFlinx = new SpawnCondition((info) => info.SpawnTileType == TileID.SnowBlock
			|| info.SpawnTileType == TileID.IceBlock || info.SpawnTileType == TileID.BreakableIce);
		Flinx2 = new SpawnCondition((info) => info.Player.ZoneSnow);
		GenericCavernMonster = new SpawnCondition(1f / 3f);
		SporeSkeletons = new SpawnCondition((info) => info.Player.ZoneGlowshroom
			&& (info.SpawnTileType == TileID.MushroomGrass || info.SpawnTileType == TileID.MushroomBlock));
		HalloweenSkeletons = new SpawnCondition((info) => Main.halloween, 0.5f);
		ExpertSkeletons = new SpawnCondition((info) => Main.expertMode, 1f / 3f);
		NormalSkeletons = new SpawnCondition((info) => Main.expertMode);

		category = null;
	}

	public static Tile GetTile(NPCSpawnInfo info)
	{
		return Main.tile[info.SpawnTileX, info.SpawnTileY];
	}

	public static bool WaterSurface(NPCSpawnInfo info)
	{
		if (info.SafeRangeX) {
			return false;
		}
		for (int k = info.SpawnTileY - 1; k > info.SpawnTileY - 50; k--) {
			if (Main.tile[info.SpawnTileX, k].liquid == 0 && !WorldGen.SolidTile(info.SpawnTileX, k)
				&& !WorldGen.SolidTile(info.SpawnTileX, k + 1) && !WorldGen.SolidTile(info.SpawnTileX, k + 2)) {
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Checks if a martian probe can spawn
	/// </summary>
	public static bool MartianProbeHelper(NPCSpawnInfo info)
	{
		return MathF.Abs(info.SpawnTileX - Main.maxTilesX / 2) / (Main.maxTilesX / 2) > 0.33f
			&& !NPC.AnyDanger();
	}

	/// <summary>
	/// Checks if the spawn tile is in the middle third of the map
	/// </summary>
	public static bool InnerThird(NPCSpawnInfo info)
	{
		return Math.Abs(info.SpawnTileX - Main.spawnTileX) < Main.maxTilesX / 3;
	}

	/// <summary>
	/// Checks if the spawn tile is in either of the outer sixths of the map
	/// </summary>
	public static bool OuterThird(NPCSpawnInfo info)
	{
		return Math.Abs(info.SpawnTileX - Main.spawnTileX) > Main.maxTilesX / 3;
	}

	public static bool StormyOrRemixEquivilent() // From NPC.SpawnNPC() as Flag 24
		=> Main.remixWorld ? Main.raining : Main.cloudAlpha > 0f;

	public static bool Beach(NPCSpawnInfo info)
		=> info.SpawnTileType == TileID.Sand && info.SpawnTileX > WorldGen.beachDistance && info.SpawnTileX < Main.maxTilesX - WorldGen.beachDistance;

	public static Func<NPCSpawnInfo, float> GetPlayerRollWeightFunc(int range)
		=> (NPCSpawnInfo info) => {
			int denominator = Main.rand.Next(range);
			if (info.Player.luck > 0f && Main.rand.NextFloat() < info.Player.luck)
				denominator = Main.rand.Next(Main.rand.Next(range / 2, range));
			if (info.Player.luck < 0f && Main.rand.NextFloat() < 0f - info.Player.luck)
				denominator = Main.rand.Next(Main.rand.Next(range, range * 2));

			return 1f / denominator;
		};
} // TODO: flag 22, 20, 21, 22, 23 +, check flag 17 for all items, check windy day spawns, finish gem critter calculator, add rollluck items