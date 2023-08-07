using System;
using System.Collections.Generic;
using Terraria.GameContent.Events;
using Terraria.ID;

namespace Terraria.ModLoader.Utilities;

internal static class NPCSpawnHelper
{
	internal static List<ISpawnTreeItem> baseConditions = new();

	internal static void Reset()
	{
		foreach (SpawnCondition condition in baseConditions) {
			condition.Reset();
		}
	}

	internal static void DoChecks(NPCSpawnInfo info)
	{
		float weight = 1f;
		foreach (SpawnCondition condition in baseConditions) {
			condition.Check(info, ref weight);
			if (Math.Abs(weight) < 5E-6) {
				break;
			}
		}
	}

	internal static void SetupParents(this ISpawnTreeItem child, params SpawnCondition[] parents)
	{
		if (parents?.Length != 0)
			foreach (SpawnCondition parent in parents)
				parent.Children.Add(child);
		else
			baseConditions.Add(child);
	}
}

public interface ISpawnTreeItem
{
	public float Chance { get; protected set; }
	internal virtual void Reset() => Chance = 0f;
	internal void Check(NPCSpawnInfo info, ref float remainingWeight);
}
public struct GenericWieghtSpawn : ISpawnTreeItem
{
	public float Weight { get; init; }
	public float Chance { get; set; } = 0f;
	public GenericWieghtSpawn(float weight = 1f, params SpawnCondition[] parents)
	{
		Weight = weight;
		this.SetupParents(parents);
	}

	public void Check(NPCSpawnInfo info, ref float remainingWeight)
	{
		Chance = Weight * remainingWeight;
		remainingWeight -= Chance;
	}
}

public struct CalculatedWieghtSpawn : ISpawnTreeItem
{
	public Func<NPCSpawnInfo, float> WeightFunc { get; init; }
	public float Chance { get; set; } = 0f;
	public CalculatedWieghtSpawn(Func<NPCSpawnInfo, float> weightFunc, params SpawnCondition[] parents)
	{
		WeightFunc = weightFunc;
		this.SetupParents(parents);
	}

	public void Check(NPCSpawnInfo info, ref float remainingWeight)
	{
		float chance = WeightFunc(info) * remainingWeight;
		remainingWeight -= chance;
		Chance += chance;
	}
}

//todo: further documentation
/// <summary>
/// This serves as a central class to help modders spawn their NPCs. It's basically the vanilla spawn code if-else chains condensed into objects. See ExampleMod for usages.
/// </summary>
public class SpawnCondition : ISpawnTreeItem
{
	private readonly Func<NPCSpawnInfo, bool> condition;
	public List<ISpawnTreeItem> Children { get; init; } = new();
	private Func<NPCSpawnInfo, float> weightFunc;
	public bool Active { get; protected set; }
	public float BlockWeight { get; protected set; }
	public float Chance { get; set; }

	private SpawnCondition(Func<NPCSpawnInfo, bool> condition, Func<NPCSpawnInfo, float> weightFunc, float blockWeight, params SpawnCondition[] parents)
	{
		this.condition = condition;
		BlockWeight = blockWeight;
		this.weightFunc = weightFunc;
		this.SetupParents(parents);
	}

	public SpawnCondition(Func<NPCSpawnInfo, bool> condition, Func<NPCSpawnInfo, float> weightFunc) : this(condition, weightFunc, 1f, null)
	{ }

	public SpawnCondition(Func<NPCSpawnInfo, bool> condition, float blockWeight = 1f) : this(condition, null, blockWeight, null)
	{ }

	public SpawnCondition(SpawnCondition parent, float blockWeight) : this((info) => true, null, blockWeight, parent)
	{ }

	public SpawnCondition(float blockWeight) : this(parent: null, blockWeight)
	{ }

	public SpawnCondition(SpawnCondition[] parents, Func<NPCSpawnInfo, bool> condition, float blockWeight = 1f) : this(condition, null, blockWeight, parents)
	{ }

	public SpawnCondition(params SpawnCondition[] parents) : this(parents, (info) => true)
	{ }

	public SpawnCondition(SpawnCondition parent, Func<NPCSpawnInfo, bool> condition, Func<NPCSpawnInfo, float> weightFunc) : this(condition, weightFunc, 1f, parent)
	{ }

	public SpawnCondition(SpawnCondition parent, Func<NPCSpawnInfo, bool> condition, float blockWeight = 1f) : this(condition, null, blockWeight, parent)
	{ }

	public void Reset()
	{
		Chance = 0f;
		Active = false;
		foreach (ISpawnTreeItem child in Children) {
			child.Reset();
		}
	}

	public void Check(NPCSpawnInfo info, ref float remainingWeight)
	{
		Active = true;
		if (condition(info)) {
			if (weightFunc != null) { // Calculate weight
				BlockWeight = weightFunc(info);
			}

			float childWeight = remainingWeight * BlockWeight; // If condition passes, calc as usual for this parent
			remainingWeight -= childWeight;
			Chance += childWeight;

			foreach (ISpawnTreeItem child in Children) {
				child.Check(info, ref childWeight);
				if (Math.Abs(childWeight) < 5E-6) {
					break;
				}
			}
		}
	}

	public static readonly SpawnCondition NebulaTower; //1
	public static readonly SpawnCondition VortexTower; //2
	public static readonly SpawnCondition StardustTower; //3
	public static readonly SpawnCondition SolarTower; //4
	public static readonly SpawnCondition Sky; //5
	public static readonly SpawnCondition Invasion; //6
	public static readonly SpawnCondition GoblinArmy;
	public static readonly SpawnCondition FrostLegion;
	public static readonly SpawnCondition Pirates;
	public static readonly SpawnCondition MartianMadness;
	public static readonly SpawnCondition LivingTree; //7
	public static readonly SpawnCondition Bartender; //8
	public static readonly SpawnCondition SpiderCave; //9
	public static readonly SpawnCondition DesertCave; //10
	public static readonly SpawnCondition HardmodeJungleWater; //1
	public static readonly SpawnCondition HardmodeCrimsonWater; //12, 13
	public static readonly SpawnCondition Ocean; //14
	public static readonly SpawnCondition OceanAngler; //14, 15
	public static readonly SpawnCondition OceanCritter;
	public static readonly SpawnCondition OceanMonster;
	public static readonly SpawnCondition BeachAngler; //16
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
	public static readonly SpawnCondition BoundGoblin;
	public static readonly SpawnCondition BoundWizard;
	public static readonly SpawnCondition BoundOldShakingChest;
	public static readonly SpawnCondition BoundCaveNPC = new(BoundCaveNPC, BoundWizard, BoundOldShakingChest);
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
	public static readonly SpawnCondition UndergroundFairy;
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
	public static readonly SpawnCondition Pigron;
	public static readonly SpawnCondition PurplePigron;
	public static readonly SpawnCondition BluePigron;
	public static readonly SpawnCondition PinkPigron;
	public static readonly SpawnCondition IceTortiose;
	public static readonly SpawnCondition DiggerWormFlinx;
	public static readonly SpawnCondition Flinx1;
	public static readonly SpawnCondition Flinx2;
	public static readonly SpawnCondition MotherSlimeBlueSlimeSpikedIceSlime;
	public static readonly SpawnCondition JungleSlimeBlackSlimeSpikedIceSlime;
	public static readonly SpawnCondition MiscCavern;
	public static readonly SpawnCondition SkeletonMerchant;
	public static readonly SpawnCondition LostGirl;
	public static readonly SpawnCondition RuneWizzard;
	public static readonly SpawnCondition Marble;
	public static readonly SpawnCondition Granite;
	public static readonly SpawnCondition Tim;
	private static readonly SpawnCondition ArmouredVikingIcyMermanSkeletonArcherArmouredSkeleton;
	public static readonly SpawnCondition UndeadMiner;
	private static readonly SpawnCondition UndeadVikingSnowFlinx;
	public static readonly SpawnCondition UndeadViking;
	public static readonly SpawnCondition Flinx3;
	public static readonly SpawnCondition Flinx4;
	public static readonly SpawnCondition GenericCavernMonster;
	public static readonly SpawnCondition SporeSkeletons;
	public static readonly SpawnCondition HalloweenSkeletons;
	public static readonly SpawnCondition ExpertSkeletons;
	public static readonly SpawnCondition NormalSkeletons;
	public static readonly SpawnCondition AllSkeletons = new(NormalSkeletons, ExpertSkeletons, HalloweenSkeletons, SporeSkeletons);
	public static readonly SpawnCondition Flinx = new(Flinx1, Flinx2, Flinx3, Flinx4);

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

		// Living Tree (Critters + Gnome)
		LivingTree = new SpawnCondition((info) => info.WallTileType == WallID.LivingWoodUnsafe && !Main.remixWorld);

		//Bartender
		Bartender = new SpawnCondition((info) => !NPC.savedBartender && DD2Event.ReadyToFindBartender
			&& !NPC.AnyNPCs(NPCID.BartenderUnconscious) && !info.Water, 1f / 80f);

		// Caves
		SpiderCave = new SpawnCondition((info) => GetTile(info).wall == WallID.SpiderUnsafe || info.SpiderCave);
		DesertCave = new SpawnCondition((info) => (NPC.SpawnTileOrAboveHasAnyWallInSet(info.SpawnTileX, info.SpawnTileY, WallID.Sets.AllowsUndergroundDesertEnemiesToSpawn)
			|| info.DesertCave) && WorldGen.checkUnderground(info.SpawnTileX, info.SpawnTileY));

		//Hardmode Water
		HardmodeJungleWater = new SpawnCondition((info) => Main.hardMode && info.Water && info.Player.ZoneJungle, 2f / 3f);
		HardmodeCrimsonWater = new SpawnCondition((info) => Main.hardMode && info.Water && info.Player.ZoneCrimson, 8f / 9f);

		//Ocean
		Ocean = new SpawnCondition((info) => (!info.PlayerInTown || (!NPC.savedAngler && !NPC.AnyNPCs(376)))
			&& info.Water && info.Ocean);
		OceanAngler = new(
			new SpawnCondition(Ocean, (info) => !NPC.savedAngler && !NPC.AnyNPCs(NPCID.SleepingAngler)
				&& WaterSurface(info) && (info.SpawnTileY < Main.worldSurface - 10.0 || Main.remixWorld)),
			new SpawnCondition((info) => !info.Water && !NPC.savedAngler && !NPC.AnyNPCs(NPCID.SleepingAngler)
				&& (info.SpawnTileX < WorldGen.beachDistance || info.SpawnTileX > Main.maxTilesX - WorldGen.beachDistance)
				&& Main.tileSand[info.ProperGroundTileType] && (info.SpawnTileY < Main.worldSurface || Main.remixWorld)));

		SpawnCondition oceanCreature = new SpawnCondition(Ocean, (info) => !info.SafeRangeX);

		List<SpawnCondition> oceanCritterBits = new() {
			new SpawnCondition(oceanCreature , (info) => WaterSurfaceAvoidHousing(info), 0.01f),
			new SpawnCondition(oceanCreature, 0.1f)
		};
		List<SpawnCondition> oceanMonsterBits = new() {
			new SpawnCondition(oceanCreature, 1f/40f) // Sea Snail
		};
		oceanCritterBits.Add(new SpawnCondition(oceanCreature, 1f / 18f)); // Squid
		oceanMonsterBits.Add(new SpawnCondition(oceanCreature));
		OceanCritter = new(oceanCritterBits.ToArray());
		OceanMonster = new(oceanMonsterBits.ToArray());

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

		// Bound NPCs
		BoundGoblin = new SpawnCondition((info) => NPC.downedGoblins && !info.Water && info.Caverns
			&& info.SpawnTileY < Main.maxTilesY - 210 && !NPC.savedGoblin && !NPC.AnyNPCs(105), GetPlayerRollWeightFunc(20));
		BoundWizard = new SpawnCondition((info) => Main.hardMode && !info.Water && info.Caverns
			&& info.SpawnTileY < Main.maxTilesY - 210 && !NPC.savedWizard && !NPC.AnyNPCs(106), GetPlayerRollWeightFunc(20));
		BoundOldShakingChest = new SpawnCondition((info) => NPC.downedBoss3 && !info.Water && info.Caverns
			&& info.SpawnTileY < Main.maxTilesY - 210 && !NPC.unlockedSlimeOldSpawn && !NPC.AnyNPCs(685), GetPlayerRollWeightFunc(20));

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
		TownOverworldWaterBeachCritter = new SpawnCondition(new SpawnCondition[] { TownWaterCritter, TownOverworldWaterCritter }, (info) => info.Beach);
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
		FrostMoon = new SpawnCondition((info) => (Main.remixWorld || info.SpawnTileY <= Main.worldSurface) && !Main.dayTime && Main.snowMoon);
		PumpkinMoon = new SpawnCondition((info) => (Main.remixWorld || info.SpawnTileY <= Main.worldSurface) && !Main.dayTime && Main.pumpkinMoon);
		SolarEclipse = new SpawnCondition((info) => ((Main.remixWorld && info.SpawnTileY > Main.rockLayer) || info.SpawnTileY <= Main.worldSurface) && Main.dayTime && Main.eclipse);

		UndergroundFairy = new SpawnCondition((info) => NPC.SpawnNPC_CheckToSpawnUndergroundFairy(info.SpawnTileX, info.SpawnTileY, info.Player.whoAmI));

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
			weightFunc = (info) => {
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
			weightFunc = (info) => {
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
			weightFunc = (info) => {
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
			weightFunc = (info) => 1f / NPC.fireFlyChance
		};

		//Overworld Monsters
		OverworldNightMonster = new SpawnCondition(OverworldNight);

		// Underground
		Underground = new SpawnCondition((info) => info.UnderGround);
		Underworld = new SpawnCondition((info) => info.SpawnTileY > Main.maxTilesY - 190);

		// Caverns
		Cavern = new SpawnCondition((info) => true);

		RockGolem = new SpawnCondition(Cavern, (info) => NPC.SpawnNPC_CheckToSpawnRockGolem(info.SpawnTileX, info.SpawnTileY, info.Player.whoAmI, info.ProperGroundTileType));
		DyeBeetle = new SpawnCondition(Cavern, 1f / 60f);
		ChaosElemental = new SpawnCondition(Cavern, (info) => Main.hardMode && !info.PlayerSafe
			&& (info.ProperGroundTileType == TileID.Pearlsand || info.ProperGroundTileType == TileID.Pearlstone || info.ProperGroundTileType == TileID.HallowedIce)
			, 1f / 8f);

		Pigron = new SpawnCondition(Cavern, (info) => (info.SpawnTileType == TileID.SnowBlock
			|| info.SpawnTileType == TileID.IceBlock || info.SpawnTileType == TileID.BreakableIce
			|| info.SpawnTileType == TileID.CorruptIce || info.SpawnTileType == TileID.HallowedIce || info.SpawnTileType == TileID.FleshIce)
			&& !info.PlayerSafe && Main.hardMode, 1f / 30f);
		PurplePigron = new SpawnCondition(Pigron, (info) => info.Player.ZoneCorrupt);
		BluePigron = new SpawnCondition(Pigron, (info) => info.Player.ZoneHallow);
		PinkPigron = new SpawnCondition(Pigron, (info) => info.Player.ZoneCrimson);

		IceTortiose = new SpawnCondition(Cavern, (info) => Main.hardMode && info.Player.ZoneSnow, 0.1f);
		DiggerWormFlinx = new SpawnCondition(Cavern, (info) => !info.PlayerSafe && info.Player.ZoneHallow, 0.01f);
		Flinx1 = new SpawnCondition(DiggerWormFlinx, (info) => !Main.hardMode && info.Player.ZoneSnow);
		Flinx2 = new SpawnCondition(Cavern, (info) => info.Player.ZoneSnow, 1f / 20f);
		MotherSlimeBlueSlimeSpikedIceSlime = new SpawnCondition(Cavern, (info) => true, (info) => Main.hardMode ? 1f / 20f : 1f / 10f);
		JungleSlimeBlackSlimeSpikedIceSlime = new SpawnCondition(Cavern, (info) => !Main.hardMode, 0.25f);
		MiscCavern = new SpawnCondition(Cavern, 0.5f);
		SkeletonMerchant = new SpawnCondition(Cavern, (info) => NPC.CountNPCS(453) == 0, 1f / 35f);
		LostGirl = new SpawnCondition(Cavern, 1f / 80f);
		RuneWizzard = new SpawnCondition(Cavern, (info) => Main.hardMode && (Main.remixWorld || info.SpawnTileY > (Main.rockLayer + Main.maxTilesY) / 2.0), 1f / 200f);
		Marble = new SpawnCondition(Cavern, (info) => info.Marble, 3f / 4f);
		Granite = new SpawnCondition(Cavern, (info) => info.Granite, 4f / 5f);

		static bool TimArmourCheck(NPCSpawnInfo info)
			=> (info.Player.armor[1].type == 4256 || (info.Player.armor[1].type >= 1282 && info.Player.armor[1].type <= 1287)) && info.Player.armor[0].type != 238;
		Tim = new SpawnCondition(Cavern, (info) => (Main.remixWorld || info.SpawnTileY > (Main.rockLayer + Main.maxTilesY) / 2.0),
			(info) => 1f / (TimArmourCheck(info) ? 50f : 200f));

		ArmouredVikingIcyMermanSkeletonArcherArmouredSkeleton = new SpawnCondition(Cavern, (info) => Main.hardMode, 0.9f);
		Ghost = new SpawnCondition(Cavern, (info) => (!info.PlayerSafe && (Main.halloween || info.Player.ZoneGraveyard)), 1f / 30f);
		UndeadMiner = new SpawnCondition(Cavern, 1f / 20f);
		UndeadVikingSnowFlinx = new SpawnCondition(Cavern, (info) => info.SpawnTileType == TileID.SnowBlock
			|| info.SpawnTileType == TileID.IceBlock || info.SpawnTileType == TileID.BreakableIce);
		Flinx3 = new SpawnCondition(UndeadVikingSnowFlinx, 1f / 15f);
		UndeadViking = new SpawnCondition(UndeadVikingSnowFlinx);
		Flinx4 = new SpawnCondition(Cavern, (info) => info.Player.ZoneSnow);
		GenericCavernMonster = new SpawnCondition(Cavern, 1f / 3f);
		SporeSkeletons = new SpawnCondition(Cavern, (info) => info.Player.ZoneGlowshroom
			&& (info.SpawnTileType == TileID.MushroomGrass || info.SpawnTileType == TileID.MushroomBlock));
		HalloweenSkeletons = new SpawnCondition(Cavern, (info) => Main.halloween, 0.5f);
		ExpertSkeletons = new SpawnCondition(Cavern, (info) => Main.expertMode, 1f / 3f);
		NormalSkeletons = new SpawnCondition(Cavern);
	}

	public static Tile GetTile(NPCSpawnInfo info)
	{
		return Main.tile[info.SpawnTileX, info.SpawnTileY];
	}

	private static int GetWaterSurface_Unchecked(NPCSpawnInfo info, int extraHeight = 2, int maxHeight = 50, int airGapHeight = 3)
	{
		int surfaceY = -1;
		for (int currentSurfaceY = info.SpawnTileY - 1; currentSurfaceY > info.SpawnTileY - maxHeight; currentSurfaceY--) {
			if (Main.tile[info.SpawnTileX, currentSurfaceY].liquid == 0 && !AnySolid(info.SpawnTileX, currentSurfaceY, airGapHeight)) {
				surfaceY = currentSurfaceY + extraHeight;
				break;
			}
		}
		return surfaceY;
	}

	/// <summary>
	/// Moves from one tile above <paramref name="info"/> to <paramref name="maxHeight"/> (vanilla default is 50) tiles above, finds the first value that has no liquid and fails <see cref="WorldGen.SolidTile(int, int, bool)"/>
	/// for itself and two tiles above it then adds <paramref name="extraHeight"/> above this value. If it finds no value, defaults to -1. Then caps the value at the initial height, and returns
	/// </summary>
	public static int GetWaterSurface(NPCSpawnInfo info, int extraHeight = 2, int maxHeight = 50, int airGapHeight = 3)
	{
		int surfaceY = GetWaterSurface_Unchecked(info, extraHeight, maxHeight, airGapHeight);
		return surfaceY > info.SpawnTileY ? info.SpawnTileY : surfaceY;
	}

	/// <summary>
	/// Checks for false <see cref="WorldGen.SolidTile(int, int, bool)"/>, in a rectangle around between <paramref name="x"/>, <paramref name="y"/>, <paramref name="width"/>, <paramref name="height"/>
	/// </summary>
	public static bool AnySolid(int x, int y, int height, int width = 1)
	{
		for (int posY = y; posY < y + height; posY++) {
			for (int posX = x; posX < x + width; posX++) {
				if (WorldGen.SolidTile(posX, posY))
					return false;
			}
		}
		return true;
	}

	public static bool WaterSurface(NPCSpawnInfo info)
		=> GetWaterSurface(info) > 0;
		//TODO: Check where this is used, double check new implementation is appropriate
		//if (info.SafeRangeX) {
		//	return false;
		//}
		//for (int heightCheck = info.SpawnTileY - 1; heightCheck > info.SpawnTileY - 50; heightCheck--) {
		//	if (Main.tile[info.SpawnTileX, heightCheck].liquid == 0 && !WorldGen.SolidTile(info.SpawnTileX, heightCheck)
		//		&& !WorldGen.SolidTile(info.SpawnTileX, heightCheck + 1) && !WorldGen.SolidTile(info.SpawnTileX, heightCheck + 2)) {
		//		return true;
		//	}
		//}
		//return false;

	public static bool WaterSurfaceAvoidHousing(NPCSpawnInfo info, int extraHeight = 2, int maxHeight = 50, int airGapHeight = 3)
	{
		int surfaceY = GetWaterSurface_Unchecked(info, extraHeight, maxHeight, airGapHeight);
		if (IsHouseWall(info.SpawnTileX, surfaceY))
			return false;
		return surfaceY > 0;
	}

	/// <summary>
	/// Shorthand for "Main.wallHouse[Main.tile[x, y].wall]"
	/// </summary>
	public static bool IsHouseWall(int x, int y)
		=> Main.wallHouse[Main.tile[x, y].wall];

	/// <summary>
	/// Checks if a martian probe can spawn
	/// </summary>
	public static bool MartianProbeHelper(NPCSpawnInfo info)
		=> MathF.Abs(info.SpawnTileX - Main.maxTilesX / 2) / (Main.maxTilesX / 2) > 0.33f && !NPC.AnyDanger();

	/// <summary>
	/// Checks if the spawn tile is in the middle third of the map
	/// </summary>
	public static bool InnerThird(NPCSpawnInfo info)
		=> Math.Abs(info.SpawnTileX - Main.spawnTileX) < Main.maxTilesX / 3;

	/// <summary>
	/// Checks if the spawn tile is in either of the outer sixths of the map
	/// </summary>
	public static bool OuterThird(NPCSpawnInfo info)
		=> Math.Abs(info.SpawnTileX - Main.spawnTileX) > Main.maxTilesX / 3;

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