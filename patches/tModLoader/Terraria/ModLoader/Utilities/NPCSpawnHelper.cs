using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent.Events;
using Terraria.ID;

namespace Terraria.ModLoader.Utilities;

public readonly record struct EntrySumChance(IList<ISpawnTreeItem> Items)
{
	public EntrySumChance(params ISpawnTreeItem[] items) : this(items as IList<ISpawnTreeItem>)
	{}
	public readonly float Chance => Items.Sum((item) => item.Chance);

	public static EntrySumChance operator +(EntrySumChance parent, ISpawnTreeItem child)
		=> parent with { Items = parent.Items.Append(child).ToList() };
}

public interface ISpawnTreeItem
{
	public float Chance { get; protected set; }

	internal virtual void Reset() => Chance = 0f;

	internal void Check(NPCSpawnInfo info, ref float remainingWeight);
}

public struct WeightedSpawnCondition : ISpawnTreeItem
{
	private Func<NPCSpawnInfo, bool> Condition { get; init; } = null;

	private float Weight { get; init; }
	public float Chance { get; set; } = 0f;

	public WeightedSpawnCondition(float weight = 1f)
	{ Weight = weight; }

	public WeightedSpawnCondition(Func<NPCSpawnInfo, bool> condition, float weight = 1f) : this(weight)
	{ Condition = condition; }

	public void Check(NPCSpawnInfo info, ref float remainingWeight)
	{
		if (Condition?.Invoke(info) ?? true) {
			float chance = Weight * remainingWeight;
			remainingWeight -= chance;
			Chance += chance;
		}
	}
}

public struct CalculatedSpawnCondition : ISpawnTreeItem
{
	private Func<NPCSpawnInfo, bool> Condition { get; init; } = null;
	private Func<NPCSpawnInfo, float> WeightFunc { get; init; }
	public float Chance { get; set; } = 0f;

	public CalculatedSpawnCondition(Func<NPCSpawnInfo, float> weightFunc)
	{ WeightFunc = weightFunc; }

	public CalculatedSpawnCondition(Func<NPCSpawnInfo, bool> condition, Func<NPCSpawnInfo, float> weightFunc) : this(weightFunc)
	{ Condition = condition; }

	public void Check(NPCSpawnInfo info, ref float remainingWeight)
	{
		if (Condition?.Invoke(info) ?? true) {
			float chance = WeightFunc(info) * remainingWeight;
			remainingWeight -= chance;
			Chance += chance;
		}
	}
}

public class SpawnTreeParent : ISpawnTreeItem
{
	public List<ISpawnTreeItem> Children { get; init; }
	public float Chance { get; set; } = 1f;

	public virtual void Reset()
	{
		Chance = 0f;
		foreach (ISpawnTreeItem child in Children) {
			child.Reset();
		}
	}

	public virtual void Check(NPCSpawnInfo info, ref float remainingWeight)
	{
		Chance += remainingWeight;
		foreach (ISpawnTreeItem child in Children) {
			child.Check(info, ref remainingWeight);
			if (Math.Abs(remainingWeight) < 5E-6) {
				break;
			}
		}
	}

	public SpawnTreeParent(params ISpawnTreeItem[] children)
	{
		Children = children.ToList();
	}

	public static SpawnTreeParent operator +(SpawnTreeParent parent, ISpawnTreeItem child)
	{
		parent.Children.Add(child);
		return parent;
	}
}

//todo: further documentation
/// <summary>
/// This serves as a central class to help modders spawn their NPCs. It's basically the vanilla spawn code if-else chains condensed into objects. See ExampleMod for usages.
/// </summary>
public class SpawnCondition : SpawnTreeParent // Better name: ConditionalSpawnParent
{
	internal static SpawnTreeParent baseCondition = new();

	private Func<NPCSpawnInfo, bool> Condition { get; init; }
	public float BlockWeight { get; init; }

	public SpawnCondition(Func<NPCSpawnInfo, bool> condition, float blockWeight, params ISpawnTreeItem[] children) : base(children)
	{
		Condition = condition;
		BlockWeight = blockWeight;
	}

	public SpawnCondition(float blockWeight, params ISpawnTreeItem[] children) : this(null, blockWeight, children)
	{ }

	public SpawnCondition(Func<NPCSpawnInfo, bool> condition, params ISpawnTreeItem[] children) : this(condition, 1f, children)
	{ }

	public override void Check(NPCSpawnInfo info, ref float remainingWeight)
	{
		if (Condition?.Invoke(info) ?? true) {
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

	public static readonly WeightedSpawnCondition NebulaTower; //1
	public static readonly WeightedSpawnCondition VortexTower; //2
	public static readonly WeightedSpawnCondition StardustTower; //3
	public static readonly WeightedSpawnCondition SolarTower; //4
	public static readonly WeightedSpawnCondition Sky; //5
	public static readonly SpawnCondition Invasion; //6
	public static readonly WeightedSpawnCondition GoblinArmy;
	public static readonly WeightedSpawnCondition FrostLegion;
	public static readonly WeightedSpawnCondition Pirates;
	public static readonly WeightedSpawnCondition MartianMadness;
	public static readonly WeightedSpawnCondition LivingTree; //7
	public static readonly WeightedSpawnCondition Bartender; //8
	public static readonly WeightedSpawnCondition SpiderCave; //9
	public static readonly WeightedSpawnCondition DesertCave; //10
	public static readonly WeightedSpawnCondition HardmodeJungleWater; //11
	public static readonly WeightedSpawnCondition HardmodeCrimsonWater; //12, 13
	public static readonly SpawnCondition Ocean; //14
	public static readonly WeightedSpawnCondition OceanAngler; //14, 15
	public static readonly EntrySumChance OceanCritter;
	public static readonly EntrySumChance OceanMonster;
	public static readonly WeightedSpawnCondition BeachAngler; //16
	public static readonly EntrySumChance Angler = new(BeachAngler, OceanAngler);
	public static readonly SpawnCondition CaveOrJungleWater;
	public static readonly SpawnCondition JungleWater;
	public static readonly WeightedSpawnCondition JungleWaterSurfaceCritter;
	public static readonly WeightedSpawnCondition JunglePiranha;
	public static readonly WeightedSpawnCondition CaveWater;
	public static WeightedSpawnCondition CavePiranha => CaveWater;
	public static readonly EntrySumChance Piranha = new(CavePiranha, JunglePiranha);
	public static readonly WeightedSpawnCondition CaveJellyfish;
	public static readonly SpawnCondition WaterCritter;
	public static readonly WeightedSpawnCondition CorruptWaterCritter;
	public static readonly SpawnCondition OverworldWaterCritter;
	public static readonly WeightedSpawnCondition OverworldWaterSurfaceCritter;
	public static readonly WeightedSpawnCondition OverworldUnderwaterCritter;
	public static readonly WeightedSpawnCondition DefaultWaterCritter;
	public static readonly CalculatedSpawnCondition BoundGoblin;
	public static readonly CalculatedSpawnCondition BoundWizard;
	public static readonly CalculatedSpawnCondition BoundOldShakingChest;
	public static readonly EntrySumChance BoundCaveNPC = new(BoundGoblin, BoundWizard, BoundOldShakingChest);
	public static readonly SpawnCondition TownCritter;
	public static readonly SpawnCondition TownGraveyardCritter;
	public static readonly WeightedSpawnCondition TownGraveyardWaterCritter; // No vanilla relation
	public static readonly SpawnCondition TownBeachCritter; // Just Seagull
	public static readonly WeightedSpawnCondition TownBeachWaterCritter;
	public static readonly SpawnCondition TownWaterCritter;
	public static readonly SpawnCondition TownOverworldWaterCritter;
	public static readonly WeightedSpawnCondition TownOverworldWaterSurfaceCritter;
	public static readonly WeightedSpawnCondition TownOverworldWaterBeachCritter;

	/// <summary>
	/// Currently Returns <see cref="TownDefaultWaterCritter"/>, replicating <see cref="NPCID.Goldfish"/> spawning behaviour. Use <see cref="TownDefaultWaterCritter"/>
	/// alongside <see cref="WaterSurface(NPCSpawnInfo)"/> for original behaviour
	/// </summary>
	[Obsolete("Does not correspond to a read vanilla NPC, to replicate the spawning of goldfish use TownDefaultWaterCritter, to replicate the spawning of pupfish use TownOverworldWaterBeachCritter.")]
	public static WeightedSpawnCondition TownOverworldUnderwaterCritter => TownDefaultWaterCritter;

	public static readonly WeightedSpawnCondition TownDefaultWaterCritter;
	public static readonly WeightedSpawnCondition TownSnowCritter;
	public static readonly WeightedSpawnCondition TownJungleCritter;
	public static readonly WeightedSpawnCondition TownDesertCritter;
	public static readonly SpawnCondition TownGrassCritter;
	public static readonly SpawnCondition TownRainingUnderGroundCritter;
	public static readonly CalculatedSpawnCondition TownCritterGreenFairy;
	public static readonly WeightedSpawnCondition TownGemSquirrel;
	public static readonly WeightedSpawnCondition TownGemBunny;
	public static readonly WeightedSpawnCondition TownGeneralCritter;
	public static readonly SpawnCondition Dungeon;
	public static readonly WeightedSpawnCondition DungeonGuardian;
	public static readonly WeightedSpawnCondition DungeonNormal;
	public static readonly WeightedSpawnCondition Meteor;
	public static readonly WeightedSpawnCondition OldOnesArmy;
	public static readonly WeightedSpawnCondition FrostMoon;
	public static readonly WeightedSpawnCondition PumpkinMoon;
	public static readonly WeightedSpawnCondition SolarEclipse;
	public static readonly WeightedSpawnCondition UndergroundFairy;
	public static readonly WeightedSpawnCondition HardmodeMushroomWater;
	public static readonly WeightedSpawnCondition OverworldMushroom;
	public static readonly WeightedSpawnCondition UndergroundMushroom;
	public static readonly WeightedSpawnCondition CorruptWorm;
	public static readonly WeightedSpawnCondition UndergroundMimic;
	public static readonly WeightedSpawnCondition OverworldMimic;
	public static readonly CalculatedSpawnCondition Wraith;
	public static readonly WeightedSpawnCondition HoppinJack;
	public static readonly CalculatedSpawnCondition DoctorBones;
	public static readonly WeightedSpawnCondition LacBeetle;
	public static readonly WeightedSpawnCondition WormCritter;
	public static readonly WeightedSpawnCondition MouseCritter;
	public static readonly WeightedSpawnCondition SnailCritter;
	public static readonly SpawnCondition JungleCritterBirdOrFrog;
	public static readonly WeightedSpawnCondition JungleCritterBird;
	public static readonly WeightedSpawnCondition FrogCritter;
	public static readonly WeightedSpawnCondition Hive;
	public static readonly WeightedSpawnCondition HardmodeJungle;
	public static readonly WeightedSpawnCondition JungleTemple;
	public static readonly WeightedSpawnCondition HiveHornet;
	public static readonly CalculatedSpawnCondition UndergroundJungle;
	public static readonly WeightedSpawnCondition SurfaceJungle;
	public static readonly WeightedSpawnCondition SandstormEvent;
	public static readonly WeightedSpawnCondition Mummy;
	public static readonly WeightedSpawnCondition DarkMummy;
	public static readonly WeightedSpawnCondition BloodMummy;
	public static readonly WeightedSpawnCondition LightMummy;
	public static readonly WeightedSpawnCondition OverworldHallow;
	public static readonly WeightedSpawnCondition EnchantedSword;
	public static readonly WeightedSpawnCondition Crimson;
	public static readonly WeightedSpawnCondition Corruption;
	public static readonly SpawnCondition Overworld;
	public static readonly WeightedSpawnCondition IceGolem;
	public static readonly WeightedSpawnCondition RainbowSlime;
	public static readonly WeightedSpawnCondition AngryNimbus;
	public static readonly CalculatedSpawnCondition MartianProbe;
	public static readonly SpawnCondition OverworldDay;
	public static readonly WeightedSpawnCondition OverworldDaySnowCritter;
	public static readonly WeightedSpawnCondition OverworldDayGrassCritter;
	public static readonly WeightedSpawnCondition OverworldDaySandCritter;
	public static readonly WeightedSpawnCondition OverworldMorningBirdCritter;
	public static readonly WeightedSpawnCondition OverworldDayBirdCritter;
	public static readonly WeightedSpawnCondition KingSlime;
	public static readonly WeightedSpawnCondition OverworldDayDesert;
	public static readonly CalculatedSpawnCondition GoblinScout;
	public static readonly WeightedSpawnCondition OverworldDayRain;
	public static readonly WeightedSpawnCondition OverworldDaySlime;
	public static readonly SpawnTreeParent OverworldNight;
	public static readonly CalculatedSpawnCondition OverworldFirefly;
	public static readonly WeightedSpawnCondition OverworldNightMonster;
	public static readonly WeightedSpawnCondition Underground;
	public static readonly WeightedSpawnCondition Underworld;
	public static readonly SpawnTreeParent Cavern;
	public static readonly WeightedSpawnCondition RockGolem;
	public static readonly WeightedSpawnCondition DyeBeetle;
	public static readonly WeightedSpawnCondition ChaosElemental;
	public static readonly SpawnCondition Pigron;
	public static readonly WeightedSpawnCondition PurplePigron;
	public static readonly WeightedSpawnCondition BluePigron;
	public static readonly WeightedSpawnCondition PinkPigron;
	public static readonly WeightedSpawnCondition IceTortiose;
	public static readonly SpawnCondition DiggerWormFlinx;
	public static readonly WeightedSpawnCondition Flinx1;
	public static readonly WeightedSpawnCondition Flinx2;
	public static readonly CalculatedSpawnCondition MotherSlimeBlueSlimeSpikedIceSlime;
	public static readonly WeightedSpawnCondition JungleSlimeBlackSlimeSpikedIceSlime;
	public static readonly WeightedSpawnCondition MiscCavern;
	public static readonly WeightedSpawnCondition SkeletonMerchant;
	public static readonly WeightedSpawnCondition LostGirl;
	public static readonly WeightedSpawnCondition RuneWizzard;
	public static readonly WeightedSpawnCondition Marble;
	public static readonly WeightedSpawnCondition Granite;
	public static readonly CalculatedSpawnCondition Tim;
	private static readonly WeightedSpawnCondition ArmouredVikingIcyMermanSkeletonArcherArmouredSkeleton;
	public static readonly WeightedSpawnCondition UndeadMiner;
	private static readonly SpawnCondition UndeadVikingSnowFlinx;
	public static readonly WeightedSpawnCondition UndeadViking;
	public static readonly WeightedSpawnCondition Flinx3;
	public static readonly WeightedSpawnCondition Flinx4;
	public static readonly WeightedSpawnCondition GenericCavernMonster;
	public static readonly WeightedSpawnCondition SporeSkeletons;
	public static readonly WeightedSpawnCondition HalloweenSkeletons;
	public static readonly WeightedSpawnCondition ExpertSkeletons;
	public static readonly WeightedSpawnCondition NormalSkeletons;
	public static readonly EntrySumChance AllSkeletons = new(NormalSkeletons, ExpertSkeletons, HalloweenSkeletons, SporeSkeletons);
	public static readonly EntrySumChance Flinx = new(Flinx1, Flinx2, Flinx3, Flinx4);

	//public static readonly SpawnCondition Gnome;
	public static readonly SpawnCondition Ghost;

	static SpawnCondition()
	{
		// Pillars
		baseCondition += NebulaTower = new((info) => info.Player.ZoneTowerNebula);
		baseCondition += VortexTower = new((info) => info.Player.ZoneTowerVortex);
		baseCondition += StardustTower = new((info) => info.Player.ZoneTowerStardust);
		baseCondition += SolarTower = new((info) => info.Player.ZoneTowerSolar);

		//Sky
		baseCondition += Sky = new((info) => info.Sky);

		//Invasions
		baseCondition += Invasion = new((info) => info.Invasion,
			GoblinArmy = new((info) => Main.invasionType == 1),
			FrostLegion = new((info) => Main.invasionType == 2),
			Pirates = new((info) => Main.invasionType == 3),
			MartianMadness = new((info) => Main.invasionType == 4));

		// Living Tree (Critters + Gnome)
		baseCondition += LivingTree = new((info) => info.WallTileType == WallID.LivingWoodUnsafe && !Main.remixWorld);

		//Bartender
		baseCondition += Bartender = new((info) => !NPC.savedBartender && DD2Event.ReadyToFindBartender
			&& !NPC.AnyNPCs(NPCID.BartenderUnconscious) && !info.Water, 1f / 80f);

		// Caves
		baseCondition += SpiderCave = new((info) => GetTile(info).wall == WallID.SpiderUnsafe || info.SpiderCave);
		baseCondition += DesertCave = new((info) => (NPC.SpawnTileOrAboveHasAnyWallInSet(info.SpawnTileX, info.SpawnTileY, WallID.Sets.AllowsUndergroundDesertEnemiesToSpawn)
			|| info.DesertCave) && WorldGen.checkUnderground(info.SpawnTileX, info.SpawnTileY));

		//Hardmode Water
		baseCondition += HardmodeJungleWater = new((info) => Main.hardMode && info.Water && info.Player.ZoneJungle, 2f / 3f);
		baseCondition += HardmodeCrimsonWater = new((info) => Main.hardMode && info.Water && info.Player.ZoneCrimson, 8f / 9f);

		//Ocean
		SpawnCondition oceanCreature;
		OceanCritter = new(null, null, null);
		OceanMonster = new(null, null);

		baseCondition += Ocean = new((info) => (!info.PlayerInTown || (!NPC.savedAngler && !NPC.AnyNPCs(376))) && info.Water && info.Ocean,
			OceanAngler = new((info) => !NPC.savedAngler && !NPC.AnyNPCs(NPCID.SleepingAngler)
				&& WaterSurface(info) && (info.SpawnTileY < Main.worldSurface - 10.0 || Main.remixWorld)),
			oceanCreature = new((info) => !info.SafeRangeX,
				OceanCritter.Items[0] = new WeightedSpawnCondition((info) => WaterSurfaceAvoidHousing(info), 0.01f),
				OceanCritter.Items[1] = new WeightedSpawnCondition(0.1f),
				OceanMonster.Items[0] = new WeightedSpawnCondition(1f / 40f), // Sea Snail
				OceanCritter.Items[2] = new WeightedSpawnCondition(1f / 18f), // Squid
				OceanMonster.Items[1] = new WeightedSpawnCondition()));

		baseCondition += BeachAngler = new((info) => !info.Water && !NPC.savedAngler && !NPC.AnyNPCs(NPCID.SleepingAngler)
				&& (info.SpawnTileX < WorldGen.beachDistance || info.SpawnTileX > Main.maxTilesX - WorldGen.beachDistance)
				&& Main.tileSand[info.ProperGroundTileType] && (info.SpawnTileY < Main.worldSurface || Main.remixWorld));

		//Misc Water
		baseCondition += CaveOrJungleWater = new((info) => !info.PlayerInTown && info.Water,
			JungleWater = new((info) => info.SpawnTileType == TileID.JungleGrass,
				JungleWaterSurfaceCritter = new((info) => info.OverWorld && info.SpawnTileY > 50 && Main.dayTime && WaterSurface(info), 1f / 3f),
				JunglePiranha = new(),
			CaveWater = new((info) => info.Caverns, 0.5f)));
		baseCondition += CaveJellyfish = new((info) => !info.PlayerInTown && info.Water && info.SpawnTileY > Main.worldSurface, 1f / 3f);

		// Water Critters
		baseCondition += WaterCritter = new((info) => info.Water, 0.25f,
			CorruptWaterCritter = new((info) => info.Player.ZoneCorrupt),
			OverworldWaterCritter = new((info) => info.SpawnTileY < Main.worldSurface && info.SpawnTileY > 50 && Main.dayTime, 2f / 3f,
				OverworldWaterSurfaceCritter = new(WaterSurface),
				OverworldUnderwaterCritter = new((info) => true)),
			DefaultWaterCritter = new((info) => true));

		// Bound NPCs
		baseCondition += BoundGoblin = new((info) => NPC.downedGoblins && !info.Water && info.Caverns
			&& info.SpawnTileY < Main.maxTilesY - 210 && !NPC.savedGoblin && !NPC.AnyNPCs(105), GetPlayerRollWeightFunc(20));
		baseCondition += BoundWizard = new((info) => Main.hardMode && !info.Water && info.Caverns
			&& info.SpawnTileY < Main.maxTilesY - 210 && !NPC.savedWizard && !NPC.AnyNPCs(106), GetPlayerRollWeightFunc(20));
		baseCondition += BoundOldShakingChest = new((info) => NPC.downedBoss3 && !info.Water && info.Caverns
			&& info.SpawnTileY < Main.maxTilesY - 210 && !NPC.unlockedSlimeOldSpawn && !NPC.AnyNPCs(685), GetPlayerRollWeightFunc(20));

		// Town Critters
		baseCondition += TownCritter = new((info) => info.PlayerInTown,
		// Graveyard
			TownGraveyardCritter = new((info) => info.Player.ZoneGraveyard,
				TownGraveyardWaterCritter = new((info) => info.Water)),
		// Beach
			TownBeachCritter = new((info) => !info.SafeRangeX && info.Beach,
				TownBeachWaterCritter = new((info) => info.Water)),
		// Water General
			TownWaterCritter = new((info) => info.Water,
				TownOverworldWaterCritter = new((info) => info.OverWorld && info.SpawnTileY > 50 && Main.dayTime, 2f / 3f,
					TownOverworldWaterSurfaceCritter = new(WaterSurface),
					TownOverworldWaterBeachCritter = new((info) => info.Beach),
					TownDefaultWaterCritter = new((info) => true)),
				TownOverworldWaterBeachCritter,
				TownDefaultWaterCritter),
			TownSnowCritter = new((info) => info.SpawnTileType == TileID.SnowBlock || info.SpawnTileType == TileID.IceBlock),
			TownJungleCritter = new((info) => info.SpawnTileType == TileID.JungleGrass),
			TownDesertCritter = new((info) => info.SpawnTileType == TileID.Sand),
			TownGrassCritter = new((info) => info.SpawnTileY > Main.worldSurface
				|| info.SpawnTileType == TileID.Grass || info.SpawnTileType == TileID.GolfGrass
				|| info.SpawnTileType == TileID.HallowedGrass || info.SpawnTileType == TileID.GolfGrassHallowed),
				TownRainingUnderGroundCritter = new((info) => Main.raining && info.PlayerFloorY <= Main.UnderworldLayer,
					TownGemSquirrel = new((info) => info.Caverns, 0.2f),
					TownGemBunny = new((info) => info.Caverns, 0.2f),
					TownGeneralCritter = new(),
				TownCritterGreenFairy = new((info) => !Main.dayTime && Main.numClouds <= 55
					&& Main.cloudBGActive == 0f && Star.starfallBoost > 3f && info.OverWorld, GetPlayerRollWeightFunc(2))));


		// Dungeon
		baseCondition += Dungeon = new((info) => info.Player.ZoneDungeon,
			DungeonGuardian = new((info) => !NPC.downedBoss3 && (!Main.drunkWorld || (info.Player.position.Y / 16f < (Main.dungeonY + 40)))),
			DungeonNormal = new());

		// Meteor
		baseCondition += Meteor = new((info) => info.Player.ZoneMeteor);

		// Events
		baseCondition += OldOnesArmy = new((info) => DD2Event.Ongoing && info.Player.ZoneOldOneArmy);
		baseCondition += FrostMoon = new((info) => (Main.remixWorld || info.SpawnTileY <= Main.worldSurface) && !Main.dayTime && Main.snowMoon);
		baseCondition += PumpkinMoon = new((info) => (Main.remixWorld || info.SpawnTileY <= Main.worldSurface) && !Main.dayTime && Main.pumpkinMoon);
		baseCondition += SolarEclipse = new((info) => ((Main.remixWorld && info.SpawnTileY > Main.rockLayer) || info.SpawnTileY <= Main.worldSurface) && Main.dayTime && Main.eclipse);

		baseCondition += UndergroundFairy = new((info) => NPC.SpawnNPC_CheckToSpawnUndergroundFairy(info.SpawnTileX, info.SpawnTileY, info.Player.whoAmI));

		// Mushroom
		baseCondition += HardmodeMushroomWater = new((info) => Main.hardMode && info.SpawnTileType == TileID.MushroomGrass && info.Water);
		baseCondition += OverworldMushroom = new((info) => info.SpawnTileType == TileID.MushroomGrass && info.SpawnTileY <= Main.worldSurface, 2f / 3f);
		baseCondition += UndergroundMushroom = new((info) => info.SpawnTileType == TileID.MushroomGrass && Main.hardMode && info.SpawnTileY >= Main.worldSurface, 2f / 3f);

		// Misc
		baseCondition += CorruptWorm = new((info) => info.Player.ZoneCorrupt && !info.PlayerSafe, 1f / 65f);
		baseCondition += UndergroundMimic = new((info) => Main.hardMode && info.SpawnTileY > Main.worldSurface, 1f / 70f);
		baseCondition += OverworldMimic = new((info) => Main.hardMode && GetTile(info).wall == WallID.DirtUnsafe, 0.05f);
		baseCondition += Wraith = new((info) => Main.hardMode && info.SpawnTileY <= Main.worldSurface && !Main.dayTime, (info) => 1f - (Main.moonPhase == 4 ? 0.8f * 0.9f : 0.95f));
		baseCondition += HoppinJack = new((info) => Main.hardMode && Main.halloween && info.SpawnTileY <= Main.worldSurface && !Main.dayTime, 0.1f);
		baseCondition += DoctorBones = new((info) => info.SpawnTileType == TileID.JungleGrass && !Main.dayTime, GetPlayerRollWeightFunc(500));
		baseCondition += LacBeetle = new((info) => info.SpawnTileType == TileID.JungleGrass && info.SpawnTileY > Main.worldSurface, 1f / 60f);

		// Critters
		baseCondition += WormCritter = new((info) => info.SpawnTileY > Main.worldSurface
			&& info.SpawnTileY < Main.maxTilesY - 210 && !info.Player.ZoneSnow && !info.Player.ZoneCrimson
			&& !info.Player.ZoneCorrupt && !info.Player.ZoneJungle && !info.Player.ZoneHallow, 1f / 8f);
		baseCondition += MouseCritter = new((info) => info.SpawnTileY > Main.worldSurface
			&& info.SpawnTileY < Main.maxTilesY - 210 && !info.Player.ZoneSnow && !info.Player.ZoneCrimson
			&& !info.Player.ZoneCorrupt && !info.Player.ZoneJungle && !info.Player.ZoneHallow, 1f / 13f);
		baseCondition += SnailCritter = new((info) => info.SpawnTileY > Main.worldSurface
			&& info.SpawnTileY < (Main.rockLayer + Main.maxTilesY) / 2 && !info.Player.ZoneSnow
			&& !info.Player.ZoneCrimson && !info.Player.ZoneCorrupt && !info.Player.ZoneHallow, 1f / 13f);
		baseCondition += JungleCritterBirdOrFrog = new((info) => info.OverWorld && info.Player.ZoneJungle && !info.Player.ZoneCrimson && !info.Player.ZoneCorrupt, 1f / 7f,
			JungleCritterBird = new((info) => Main.dayTime && Main.time < 43200.00064373016, 2f / 3f),
			FrogCritter = new());

		baseCondition += Hive = new((info) => info.ProperGroundTileType == TileID.Hive, 0.5f);

		// Jungle
		baseCondition += HardmodeJungle = new((info) => info.SpawnTileType == TileID.JungleGrass && Main.hardMode, 2f / 3f);
		baseCondition += JungleTemple = new((info) => (info.SpawnTileType == TileID.LihzahrdBrick || info.ProperGroundTileType == TileID.WoodenSpikes || Main.remixWorld) && info.Lihzahrd);
		baseCondition += HiveHornet = new((info) => info.WallTileType == WallID.HiveUnsafe, 7f / 8f);
		baseCondition += UndergroundJungle = new CalculatedSpawnCondition((info) => info.SpawnTileType == TileID.JungleGrass
			&& (Main.remixWorld ? (info.SpawnTileY < Main.rockLayer) : info.SpawnTileY > (Main.worldSurface + Main.rockLayer) / 2.0), (info) => Main.remixWorld ? 0.5f : 1f);
		baseCondition += SurfaceJungle = new((info) => info.SpawnTileType == TileID.JungleGrass, 11f / 32f);

		// Sandstorm
		baseCondition += SandstormEvent = new((info) => Sandstorm.Happening && info.Player.ZoneSandstorm
			&& TileID.Sets.Conversion.Sand[info.SpawnTileType]
			&& NPC.Spawning_SandstoneCheck(info.SpawnTileX, info.SpawnTileY));

		// Mummy
		baseCondition += Mummy = new((info) => Main.hardMode && info.ProperGroundTileType == TileID.Sand, 1f / 3f);
		baseCondition += DarkMummy = new((info) => Main.hardMode && info.ProperGroundTileType == TileID.Ebonsand, 0.5f);
		baseCondition += BloodMummy = new((info) => Main.hardMode && info.ProperGroundTileType == TileID.Crimsand, 0.5f);
		baseCondition += LightMummy = new((info) => Main.hardMode && info.ProperGroundTileType == TileID.Pearlsand, 0.5f);

		// Hallow
		baseCondition += OverworldHallow = new((info) => Main.hardMode && !info.Water && info.UnderGround
			&& (info.ProperGroundTileType == TileID.Pearlsand || info.ProperGroundTileType == TileID.Pearlstone
			|| info.ProperGroundTileType == TileID.HallowedGrass || info.ProperGroundTileType == TileID.HallowedIce));
		baseCondition += EnchantedSword = new((info) => !info.PlayerSafe && Main.hardMode && !info.Water
			&& info.Caverns && (info.ProperGroundTileType == TileID.Pearlsand
			|| info.ProperGroundTileType == TileID.Pearlstone || info.ProperGroundTileType == TileID.HallowedGrass
			|| info.ProperGroundTileType == TileID.HallowedIce), 0.02f);

		// Crimson
		baseCondition += Crimson = new((info) => (info.ProperGroundTileType == TileID.Crimtane && info.Player.ZoneCrimson)
			|| info.ProperGroundTileType == TileID.CrimsonGrass || info.ProperGroundTileType == TileID.FleshIce
			|| info.ProperGroundTileType == TileID.Crimstone || info.ProperGroundTileType == TileID.Crimsand
			|| info.ProperGroundTileType == TileID.CrimsonJungleGrass);

		// Corruption
		baseCondition += Corruption = new((info) => (info.ProperGroundTileType == TileID.Demonite && info.Player.ZoneCorrupt)
			|| info.ProperGroundTileType == TileID.CorruptGrass || info.ProperGroundTileType == TileID.Ebonstone
			|| info.ProperGroundTileType == TileID.Ebonsand || info.ProperGroundTileType == TileID.CorruptIce
			|| info.ProperGroundTileType == TileID.CorruptJungleGrass);

		// Overworld
		baseCondition += Overworld = new((info) => info.OverWorld,

			// Overworld Misc
			IceGolem = new((info) => info.Player.ZoneSnow && Main.hardMode && Main.cloudAlpha > 0f && !NPC.AnyNPCs(NPCID.IceGolem), 0.05f),
			RainbowSlime = new((info) => info.Player.ZoneHallow && Main.hardMode && Main.cloudAlpha > 0f && !NPC.AnyNPCs(NPCID.RainbowSlime), 0.05f),
			AngryNimbus = new((info) => !info.Player.ZoneSnow && Main.hardMode && Main.cloudAlpha > 0f && NPC.CountNPCS(NPCID.AngryNimbus) < 2, 0.1f),
			MartianProbe = new((info) => MartianProbeHelper(info) && Main.hardMode && NPC.downedGolemBoss && !NPC.AnyNPCs(NPCID.MartianProbe),
				(info) => 1f - (!NPC.downedMartians ? (399f / 400f) * 0.99f : (399f / 400f))),

			// Overworld Typical
			OverworldDay = new((info) => Main.dayTime,
				OverworldDaySnowCritter = new((info) => InnerThird(info)
					&& (GetTile(info).type == TileID.SnowBlock || GetTile(info).type == TileID.IceBlock), 1f / 15f),
				OverworldDayGrassCritter = new((info) => InnerThird(info)
					&& (GetTile(info).type == TileID.Grass || GetTile(info).type == TileID.HallowedGrass), 1f / 15f),
				OverworldDaySandCritter = new((info) => InnerThird(info) && GetTile(info).type == TileID.Sand, 1f / 15f),
				OverworldMorningBirdCritter = new((info) => InnerThird(info) && Main.time < 18000.0
					&& (GetTile(info).type == TileID.Grass || GetTile(info).type == TileID.HallowedGrass), 0.25f),
				OverworldDayBirdCritter = new((info) => InnerThird(info)
					&& (GetTile(info).type == TileID.Grass || GetTile(info).type == TileID.HallowedGrass || GetTile(info).type == TileID.SnowBlock), 1f / 15f),

				// King Slime (Overworld)
				KingSlime = new((info) => OuterThird(info) && GetTile(info).type == TileID.Grass && !NPC.AnyNPCs(NPCID.KingSlime), 1f / 300f),

				OverworldDayDesert = new((info) => GetTile(info).type == TileID.Sand && !info.Water, 0.2f),

				// Overworld Goblin Scout
				GoblinScout = new(OuterThird,
					(info) => 1f - ((!NPC.downedGoblins && WorldGen.shadowOrbSmashed) ? (6f / 7f) * (14f / 15f) : (14f / 15f))),

				// Overworld Typical
				OverworldDayRain = new((info) => Main.raining, 2f / 3f),
				OverworldDaySlime = new()),
			OverworldNight = new(
				OverworldFirefly = new((info) => GetTile(info).type == TileID.Grass || GetTile(info).type == TileID.HallowedGrass, (info) => 1f / NPC.fireFlyChance),
				OverworldNightMonster = new()));

		// Underground
		baseCondition += Underground = new((info) => info.UnderGround);
		baseCondition += Underworld = new((info) => info.SpawnTileY > Main.maxTilesY - 190);

		// Caverns

		static bool TimArmourCheck(NPCSpawnInfo info)
			=> (info.Player.armor[1].type == 4256 || (info.Player.armor[1].type >= 1282 && info.Player.armor[1].type <= 1287)) && info.Player.armor[0].type != 238;

		baseCondition += Cavern = new(
			RockGolem = new((info) => NPC.SpawnNPC_CheckToSpawnRockGolem(info.SpawnTileX, info.SpawnTileY, info.Player.whoAmI, info.ProperGroundTileType)),
			DyeBeetle = new(1f / 60f),
			ChaosElemental = new((info) => Main.hardMode && !info.PlayerSafe
				&& (info.ProperGroundTileType == TileID.Pearlsand || info.ProperGroundTileType == TileID.Pearlstone || info.ProperGroundTileType == TileID.HallowedIce), 1f / 8f),

			Pigron = new((info) => (info.SpawnTileType == TileID.SnowBlock
				|| info.SpawnTileType == TileID.IceBlock || info.SpawnTileType == TileID.BreakableIce
				|| info.SpawnTileType == TileID.CorruptIce || info.SpawnTileType == TileID.HallowedIce || info.SpawnTileType == TileID.FleshIce)
				&& !info.PlayerSafe && Main.hardMode, 1f / 30f,
				PurplePigron = new((info) => info.Player.ZoneCorrupt),
				BluePigron = new((info) => info.Player.ZoneHallow),
				PinkPigron = new((info) => info.Player.ZoneCrimson)),

			IceTortiose = new((info) => Main.hardMode && info.Player.ZoneSnow, 0.1f),
			DiggerWormFlinx = new((info) => !info.PlayerSafe && info.Player.ZoneHallow, 0.01f,
				Flinx1 = new((info) => !Main.hardMode && info.Player.ZoneSnow)),
			Flinx2 = new((info) => info.Player.ZoneSnow, 1f / 20f),
			MotherSlimeBlueSlimeSpikedIceSlime = new((info) => Main.hardMode ? 1f / 20f : 1f / 10f),
			JungleSlimeBlackSlimeSpikedIceSlime = new((info) => !Main.hardMode, 0.25f),
			MiscCavern = new( 0.5f),
			SkeletonMerchant = new((info) => NPC.CountNPCS(453) == 0, 1f / 35f),
			LostGirl = new(1f / 80f),
			RuneWizzard = new((info) => Main.hardMode && (Main.remixWorld || info.SpawnTileY > (Main.rockLayer + Main.maxTilesY) / 2.0), 1f / 200f),
			Marble = new((info) => info.Marble, 3f / 4f),
			Granite = new((info) => info.Granite, 4f / 5f),
			Tim = new((info) => (Main.remixWorld || info.SpawnTileY > (Main.rockLayer + Main.maxTilesY) / 2.0), (info) => 1f / (TimArmourCheck(info) ? 50f : 200f)),
			ArmouredVikingIcyMermanSkeletonArcherArmouredSkeleton = new((info) => Main.hardMode, 0.9f),
			Ghost = new((info) => (!info.PlayerSafe && (Main.halloween || info.Player.ZoneGraveyard)), 1f / 30f),
			UndeadMiner = new(1f / 20f),
			UndeadVikingSnowFlinx = new((info) => info.SpawnTileType == TileID.SnowBlock || info.SpawnTileType == TileID.IceBlock || info.SpawnTileType == TileID.BreakableIce,
				Flinx3 = new(1f / 15f),
				UndeadViking = new()),
			Flinx4 = new((info) => info.Player.ZoneSnow),
			GenericCavernMonster = new(1f / 3f),
			SporeSkeletons = new((info) => info.Player.ZoneGlowshroom && (info.SpawnTileType == TileID.MushroomGrass || info.SpawnTileType == TileID.MushroomBlock)),
			HalloweenSkeletons = new((info) => Main.halloween, 0.5f),
			ExpertSkeletons = new((info) => Main.expertMode, 1f / 3f),
			NormalSkeletons = new());
	}
	/// <summary>
	/// Shorthand for "Main.tile[info.SpawnTileX, info.SpawnTileY]"
	/// </summary>
	public static Tile GetTile(NPCSpawnInfo info)
		=> Main.tile[info.SpawnTileX, info.SpawnTileY];

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
	/// Moves from one tile above <paramref name="info"/> to <paramref name="maxHeight"/> (vanilla default is 50) tiles above, finds the first value that has no liquid
	/// and fails <see cref="WorldGen.SolidTile(int, int, bool)"/> for itself and two tiles above it then adds <paramref name="extraHeight"/> above this value. If it
	/// finds no value, defaults to -1. Then caps the value at the initial height, and returns
	/// </summary>
	public static int GetWaterSurface(NPCSpawnInfo info, int extraHeight = 2, int maxHeight = 50, int airGapHeight = 3)
	{
		int surfaceY = GetWaterSurface_Unchecked(info, extraHeight, maxHeight, airGapHeight);
		return surfaceY > info.SpawnTileY ? info.SpawnTileY : surfaceY;
	}

	/// <summary>
	/// Checks for false <see cref="WorldGen.SolidTile(int, int, bool)"/>, in a rectangle around between <paramref name="x"/>, <paramref name="y"/>,
	/// <paramref name="width"/>, <paramref name="height"/>
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