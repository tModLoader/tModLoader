using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ID;
using static Terraria.ModLoader.Utilities.SubSpawnCondition.Common;
using static Terraria.ModLoader.Utilities.SubSpawnCondition.Enemy;
using static Terraria.ModLoader.Utilities.SubSpawnCondition.Tile;
using static Terraria.ModLoader.Utilities.SubSpawnCondition.Unique;
using static Terraria.ModLoader.Utilities.SubSpawnCondition.Zone;

namespace Terraria.ModLoader.Utilities;

public interface ISpawnTreeItem
{
	public float Chance { get; protected set; }

	internal virtual void Reset() => Chance = 0f;

	internal void Check(NPCSpawnInfo info, ref float remainingWeight);
}

public interface IConditionedTreeItem : ISpawnTreeItem
{
	public ConditionWrapper Conditions { get; init; }
}

public struct WeightedSpawnCondition : IConditionedTreeItem
{
	public ConditionWrapper Conditions { get; init; }
	private float Weight { get; init; }
	public float Chance { get; set; } = 0f;
	public WeightedSpawnCondition() : this(1f) { }
	public WeightedSpawnCondition(float weight)
	{ Weight = weight; Conditions = new(CompareType.And, Array.Empty<ISubSpawnCondition>()); }

	public WeightedSpawnCondition(ConditionWrapper conditions, float weight = 1f) : this(weight)
	{
		ArgumentNullException.ThrowIfNull(conditions);
		Conditions = conditions;
	}

	public void Check(NPCSpawnInfo info, ref float remainingWeight)
	{
		if (Conditions.IsMet(info)) {
			float chance = Weight * remainingWeight;
			remainingWeight -= chance;
			Chance += chance;
		}
	}
}

public struct CalculatedSpawnCondition : IConditionedTreeItem
{
	public ConditionWrapper Conditions { get; init; }
	private Func<NPCSpawnInfo, float> WeightFunc { get; init; }
	public float Chance { get; set; } = 0f;

	public CalculatedSpawnCondition(Func<NPCSpawnInfo, float> weightFunc)
	{
		ArgumentNullException.ThrowIfNull(weightFunc);
		WeightFunc = weightFunc;
		Conditions = new(CompareType.And, Array.Empty<ISubSpawnCondition>());
	}

	public CalculatedSpawnCondition(ConditionWrapper condition, Func<NPCSpawnInfo, float> weightFunc) : this(weightFunc)
	{
		ArgumentNullException.ThrowIfNull(condition);
		Conditions = condition;
	}

	public void Check(NPCSpawnInfo info, ref float remainingWeight)
	{
		if (Conditions.IsMet(info)) {
			float chance = WeightFunc(info) * remainingWeight;
			remainingWeight -= chance;
			Chance += chance;
		}
	}
}

public class SpawnTreeParent : ISpawnTreeItem
{
	public ISpawnTreeItem[] Children { get; init; }
	private float BlockWeight { get; init; }
	public float Chance { get; set; } = 0f;

	public virtual float GetWeight(NPCSpawnInfo info)
		=> BlockWeight;

	public virtual void Reset()
	{
		Chance = 0f;
		foreach (ISpawnTreeItem child in Children) {
			child.Reset();
		}
	}

	/// <summary>
	/// Adds the 
	/// </summary>
	/// <param name="item"></param>
	/// <param name="after"></param>
	public void InjectAfter(ISpawnTreeItem item, ISpawnTreeItem after)
	{

	}

	public virtual void Check(NPCSpawnInfo info, ref float remainingWeight)
	{
		float childWeight = remainingWeight * GetWeight(info);
		remainingWeight -= childWeight;
		Chance += childWeight;

		foreach (ISpawnTreeItem child in Children) {
			child.Check(info, ref childWeight);
			if (Math.Abs(childWeight) < 5E-6) {
				break;
			}
		}
	}

	public SpawnTreeParent(float weight, ISpawnTreeItem[] children)
	{
		ArgumentNullException.ThrowIfNull(children);
		Children = children;
		BlockWeight = weight;
	}

	public SpawnTreeParent(ISpawnTreeItem[] children) : this(1f, children)
	{
	}

	internal SpawnTreeParent() : this(Array.Empty<ISpawnTreeItem>())
	{
	}

	public static SpawnTreeParent operator +(SpawnTreeParent parent, ISpawnTreeItem child)
	{
		return new(parent.Children.Append(child).ToArray());
	}
}

public class ConditionedSpawnTreeParent : SpawnTreeParent, IConditionedTreeItem
{
	public ConditionWrapper Conditions { get; init; }

	public ConditionedSpawnTreeParent(ConditionWrapper conditions, float blockWeight, ISpawnTreeItem[] children) : base(blockWeight, children)
	{
		ArgumentNullException.ThrowIfNull(conditions);
		Conditions = conditions;
	}

	public ConditionedSpawnTreeParent(ConditionWrapper conditions, ISpawnTreeItem[] children) : this(conditions, 1f, children)
	{ }

	public override void Check(NPCSpawnInfo info, ref float remainingWeight)
	{
		if (Conditions.IsMet(info))
			base.Check(info, ref remainingWeight);
	}
}

// TODO: I don't like that we've ended up here again, think of something to keep it all separate
public sealed class DualConditionedSpawnTreeParent : ConditionedSpawnTreeParent
{
	private Func<NPCSpawnInfo, float> WeightFunc { get; init; }

	public DualConditionedSpawnTreeParent(ConditionWrapper conditions, Func<NPCSpawnInfo, float> weightFunc, ISpawnTreeItem[] children) : base(conditions, -1f, children)
	{
		ArgumentNullException.ThrowIfNull(weightFunc);
		WeightFunc = weightFunc;
	}

	public override float GetWeight(NPCSpawnInfo info) => WeightFunc.Invoke(info);
}

public class MultiEntrySum
{
	private IEnumerable<ISpawnTreeItem> items;

	/// <param name="items"> The array of items summed here </param>
	/// <exception cref="ArgumentException"> Thrown when <paramref name="items"/> is empty. </exception>
	/// <exception cref="ArgumentNullException"> Thrown when <paramref name="items"/> is null. </exception>
	public MultiEntrySum(params ISpawnTreeItem[] items)
	{
		ArgumentNullException.ThrowIfNull(items);
		if (items.Length == 0)
			throw new ArgumentException("Array was empty.", nameof(items));
		this.items = items;
	}

	/// <summary>
	/// For internal use with <see cref="AddAndReturn(ISpawnTreeItem)"/> only
	/// </summary>
	internal MultiEntrySum()
	{
		items = Array.Empty<ISpawnTreeItem>();
	}

	/// <summary>
	/// The sum of each elements <see cref="ISpawnTreeItem.Chance"/>
	/// </summary>
	public float Chance => items.Sum((item) => item.Chance);

	/// <summary>
	/// Appends <paramref name="item"/> to the internal array and returns <paramref name="item"/>, internal since externally this should look immutable, these are just for convenience here.
	/// </summary>
	internal ISpawnTreeItem AddAndReturn(ISpawnTreeItem item)
	{
		items = items.Append(item);
		return item;
	}
}

//TODO: further documentation
/// <summary>
/// This serves as a central class to help modders spawn their NPCs. It's basically the vanilla spawn code if-else chains condensed into objects. See ExampleMod for usages.
/// </summary>
public static class SpawnCondition
{
	internal static SpawnTreeParent baseCondition = new();

	public static readonly WeightedSpawnCondition NebulaTower;
	public static readonly WeightedSpawnCondition VortexTower;
	public static readonly WeightedSpawnCondition StardustTower;
	public static readonly WeightedSpawnCondition SolarTower;
	public static readonly WeightedSpawnCondition Sky;
	public static readonly ConditionedSpawnTreeParent Invasion;
	public static readonly WeightedSpawnCondition GoblinArmy;
	public static readonly WeightedSpawnCondition FrostLegion;
	public static readonly WeightedSpawnCondition Pirates;
	public static readonly WeightedSpawnCondition MartianMadness;
	public static readonly WeightedSpawnCondition LivingTree;
	public static readonly WeightedSpawnCondition Bartender;
	public static readonly WeightedSpawnCondition SpiderCave;
	public static readonly WeightedSpawnCondition DesertCave;
	public static readonly WeightedSpawnCondition HardmodeJungleWater;
	public static readonly MultiEntrySum HardmodeCrimsonWater;
	public static readonly ConditionedSpawnTreeParent Ocean;
	public static readonly WeightedSpawnCondition OceanAngler;
	public static readonly MultiEntrySum OceanCritter;
	public static readonly MultiEntrySum OceanMonster;
	public static readonly WeightedSpawnCondition BeachAngler;
	public static readonly MultiEntrySum Angler = new(BeachAngler, OceanAngler);
	public static readonly DualConditionedSpawnTreeParent CaveOrJungleWater;
	public static readonly ConditionedSpawnTreeParent JungleWater;

	/// <summary> In vanilla: <see cref="NPCID.TurtleJungle"/>, <see cref="NPCID.WaterStrider"/>, <see cref="NPCID.GoldWaterStrider"/> </summary>
	public static readonly WeightedSpawnCondition JungleWaterSurfaceCritter;

	/// <summary> In vanilla: <see cref="NPCID.AnglerFish"/>, <see cref="NPCID.Piranha"/> </summary>
	public static readonly WeightedSpawnCondition JunglePiranha;

	public static readonly WeightedSpawnCondition CaveWater;
	public static WeightedSpawnCondition CavePiranha => CaveWater;
	public static readonly MultiEntrySum Piranha = new(CavePiranha, JunglePiranha);
	public static readonly WeightedSpawnCondition CaveJellyfish;
	public static readonly ConditionedSpawnTreeParent WaterCritter;

	/// <summary> In vanilla: <see cref="NPCID.CorruptGoldfish"/> </summary>
	public static readonly WeightedSpawnCondition CorruptWaterCritter;

	/// <summary> In vanilla: <see cref="NPCID.Goldfish"/> </summary>
	public static readonly WeightedSpawnCondition CrimsonWaterCritter;

	public static readonly ConditionedSpawnTreeParent OverworldWaterCritter;
	public static readonly WeightedSpawnCondition OverworldWaterSurfaceCritter;
	public static readonly WeightedSpawnCondition OverworldUnderwaterCritter;

	/// <summary> In vanilla: <see cref="NPCID.Pupfish"/>, <see cref="NPCID.GoldGoldfish"/>, <see cref="NPCID.Goldfish"/> </summary>
	public static readonly WeightedSpawnCondition DefaultWaterCritter;

	public static readonly CalculatedSpawnCondition BoundGoblin;
	public static readonly CalculatedSpawnCondition BoundWizard;
	public static readonly CalculatedSpawnCondition BoundOldShakingChest;
	public static readonly MultiEntrySum BoundCaveNPC = new(BoundGoblin, BoundWizard, BoundOldShakingChest);
	public static readonly ConditionedSpawnTreeParent TownCritter;
	public static readonly ConditionedSpawnTreeParent TownGraveyardCritter;
	public static readonly WeightedSpawnCondition TownGraveyardWaterCritter; // No vanilla relation
	public static readonly ConditionedSpawnTreeParent TownBeachCritter; // Just Seagull
	public static readonly WeightedSpawnCondition TownBeachWaterCritter;
	public static readonly WeightedSpawnCondition TownDragonFlyCritter;
	public static readonly ConditionedSpawnTreeParent TownWaterCritter;
	public static readonly ConditionedSpawnTreeParent TownOverworldWaterCritter;
	public static readonly WeightedSpawnCondition TownOverworldWaterSurfaceCritter;
	public static readonly WeightedSpawnCondition TownOverworldWaterBeachCritter;

	/// <summary>
	/// Currently Returns <see cref="TownDefaultWaterCritter"/>, replicating <see cref="NPCID.Goldfish"/> spawning behaviour. Use <see cref="TownDefaultWaterCritter"/>
	/// alongside <see cref="WaterSurface(NPCSpawnInfo)"/> for original behaviour
	/// </summary>
	[Obsolete("Does not correspond to a real vanilla NPC, to replicate the spawning of goldfish use TownDefaultWaterCritter, to replicate the spawning of pupfish use TownOverworldWaterBeachCritter.")]
	public static WeightedSpawnCondition TownOverworldUnderwaterCritter => TownDefaultWaterCritter;

	public static readonly WeightedSpawnCondition TownDefaultWaterCritter;
	public static readonly WeightedSpawnCondition TownSnowCritter;
	public static readonly WeightedSpawnCondition TownJungleCritter;
	public static readonly WeightedSpawnCondition TownDesertCritter;
	public static readonly ConditionedSpawnTreeParent TownGrassCritter;
	public static readonly ConditionedSpawnTreeParent TownRainingUnderGroundCritter;
	public static readonly CalculatedSpawnCondition TownCritterGreenFairy;
	public static readonly WeightedSpawnCondition TownGemSquirrel;
	public static readonly WeightedSpawnCondition TownGemBunny;
	public static readonly WeightedSpawnCondition TownGeneralCritter;
	public static readonly ConditionedSpawnTreeParent Dungeon;
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

	/// <summary> In vanilla: <see cref="NPCID.DevourerHead"/>, <see cref="NPCID.SeekerHead"/> (World Feeder) </summary>
	public static readonly CalculatedSpawnCondition CorruptWorm;

	public static readonly MultiEntrySum UndergroundMimic = new();
	public static readonly WeightedSpawnCondition OverworldMimic;
	public static readonly CalculatedSpawnCondition Wraith;
	public static readonly WeightedSpawnCondition HoppinJack;
	public static readonly CalculatedSpawnCondition DoctorBones;
	public static readonly WeightedSpawnCondition LacBeetle;
	public static readonly WeightedSpawnCondition WormCritter;
	public static readonly WeightedSpawnCondition MouseCritter;
	public static readonly WeightedSpawnCondition SnailCritter;
	public static readonly ConditionedSpawnTreeParent JungleCritterBirdOrFrog;
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
	public static readonly ConditionedSpawnTreeParent Overworld;
	public static readonly WeightedSpawnCondition IceGolem;
	public static readonly WeightedSpawnCondition RainbowSlime;
	public static readonly WeightedSpawnCondition AngryNimbus;
	public static readonly CalculatedSpawnCondition MartianProbe;
	public static readonly ConditionedSpawnTreeParent OverworldDay;
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
	public static readonly ConditionedSpawnTreeParent Pigron;
	public static readonly WeightedSpawnCondition PurplePigron;
	public static readonly WeightedSpawnCondition BluePigron;
	public static readonly WeightedSpawnCondition PinkPigron;
	public static readonly WeightedSpawnCondition IceTortoise;
	public static readonly ConditionedSpawnTreeParent DiggerWormFlinx;
	public static readonly WeightedSpawnCondition Flinx1;
	public static readonly WeightedSpawnCondition Flinx2;
	public static readonly CalculatedSpawnCondition MotherSlimeBlueSlimeSpikedIceSlime;
	public static readonly WeightedSpawnCondition JungleSlimeBlackSlimeSpikedIceSlime;
	public static readonly WeightedSpawnCondition MiscCavern;
	public static readonly WeightedSpawnCondition SkeletonMerchant;
	public static readonly WeightedSpawnCondition LostGirl;
	public static readonly WeightedSpawnCondition RuneWizard;
	public static readonly WeightedSpawnCondition Marble;
	public static readonly WeightedSpawnCondition Granite;
	public static readonly CalculatedSpawnCondition Tim;
	public static readonly WeightedSpawnCondition UndeadMiner;
	public static readonly WeightedSpawnCondition UndeadViking;
	public static readonly WeightedSpawnCondition Flinx3;
	public static readonly WeightedSpawnCondition Flinx4;
	public static readonly WeightedSpawnCondition GenericCavernMonster;
	public static readonly WeightedSpawnCondition SporeSkeletons;
	public static readonly WeightedSpawnCondition HalloweenSkeletons;
	public static readonly WeightedSpawnCondition ExpertSkeletons;
	public static readonly WeightedSpawnCondition NormalSkeletons;
	public static readonly MultiEntrySum AllSkeletons = new(NormalSkeletons, ExpertSkeletons, HalloweenSkeletons, SporeSkeletons);

	/// <summary>
	/// The current spawn probability for the Flinx. The flinx has four spawn seperate spawning chances; <see cref="Flinx1"/>, <see cref="Flinx2"/>, <see cref="Flinx3"/>, <see cref="Flinx4"/>
	/// </summary>
	public static readonly MultiEntrySum Flinx = new(Flinx1, Flinx2, Flinx3, Flinx4);

	public static readonly MultiEntrySum Gnome;
	public static readonly WeightedSpawnCondition Ghost;

	static SpawnCondition() // Numbers corresponds to line number within SpawnNPC() if-else chain
	{
		// Pillars
		baseCondition += NebulaTower = new(InTowerNebula); //1: 61989
		baseCondition += VortexTower = new(InTowerVortex); //2
		baseCondition += StardustTower = new(InTowerStardust); //3
		baseCondition += SolarTower = new(InTowerSolar); //4

		//Sky
		baseCondition += Sky = new(InfoSky); //5 (spawnInfo.Sky = flag3)

		//Invasions
		baseCondition += Invasion = new(InfoInvasion, new ISpawnTreeItem[] { // 6 (spawnInfo.Invasion = flag6)
			GoblinArmy = new(GoblinArmyCondition),
			FrostLegion = new(FrostLegionCondition),
			Pirates = new(PiratesCondition),
			MartianMadness = new(MartianMadnessCondition)
		});

		// Living Tree (Critters + Gnome)
		baseCondition += LivingTree = new(InfoWallTile(WallID.LivingWoodUnsafe) & RemixWorld); // 7 (num50 == 244 && !Main.remixWorld)

		//Bartender
		baseCondition += Bartender = new(CanGetBartender & SpawnCap(NPCID.BartenderUnconscious, 1) & !InfoWater, 1f / 80f); // 8 (!savedBartender && DD2Event.ReadyToFindBartender && !AnyNPCs(579) && Main.rand.Next(80) == 0 && !flag7)

		// Caves
		baseCondition += SpiderCave = new(TrueWallTile(WallID.SpiderUnsafe) | InfoSpider); // 9 (Main.tile[num, num2].wall == 62 || flag11)
		baseCondition += DesertCave = new((DesertCaveWallCheck | InfoDesertCave) & WorldGenCheckUnderground); // 10: 62232 ((SpawnTileOrAboveHasAnyWallInSet(num, num2, WallID.Sets.AllowsUndergroundDesertEnemiesToSpawn) || flag13) && WorldGen.checkUnderground(num, num2))

		//Hardmode Water
		baseCondition += HardmodeJungleWater = new(HardMode & InfoWater & InJungle, 2f / 3f); // 11 (Main.hardMode && flag7 && Main.player[k].ZoneJungle && Main.rand.Next(3) != 0)

		// Separated in case I'd like to go deeper here
		HardmodeCrimsonWater = new(); // 12 (Main.hardMode && flag7 && Main.player[k].ZoneCrimson && Main.rand.Next(3) != 0)
									  // 13 (Main.hardMode && flag7 && Main.player[k].ZoneCrimson && Main.rand.Next(3) != 0)
		baseCondition += HardmodeCrimsonWater.AddAndReturn(new WeightedSpawnCondition(HardMode & InfoWater & InCrimson, 2f / 3f));
		baseCondition += HardmodeCrimsonWater.AddAndReturn(new WeightedSpawnCondition(HardMode & InfoWater & InCrimson, 2f / 3f));

		//Ocean
		ConditionedSpawnTreeParent oceanCreature;
		OceanCritter = new();
		OceanMonster = new();

		// 14 ((!flag12 || (!savedAngler && !AnyNPCs(376))) && flag7 && flag22)
		baseCondition += Ocean = new((!InfoPlayerInTown | (!SavedAngler & SpawnCap(NPCID.SleepingAngler))) & InfoWater & InfoOcean, new ISpawnTreeItem[] {
			OceanAngler = new(SavedAngler & OnWaterSurface & AnglerOceanSurface),
			oceanCreature = new(!InfoSafeRange, new ISpawnTreeItem[] {
				OceanCritter.AddAndReturn(new WeightedSpawnCondition(OnWaterSurfaceAvoidHousing, 0.01f)),
				OceanCritter.AddAndReturn(new WeightedSpawnCondition(0.1f)),
				OceanMonster.AddAndReturn(new WeightedSpawnCondition(1f / 40f)), // Sea Snail
				OceanCritter.AddAndReturn(new WeightedSpawnCondition(1f / 18f)), // Squid
				OceanMonster.AddAndReturn(new WeightedSpawnCondition())
			})
		});

		// 15 (!flag7 && !savedAngler && !AnyNPCs(376) && (num < WorldGen.beachDistance || num > Main.maxTilesX - WorldGen.beachDistance) && Main.tileSand[num49] &&
		// ((double)num2 < Main.worldSurface || Main.remixWorld))
		baseCondition += BeachAngler = new(!InfoWater & !SavedAngler & SpawnCap(NPCID.SleepingAngler) & InBeachDistance & ProperGroundSand & (AboveOrWorldSurface | RemixWorld));

		//Misc Water
		// TODO: redo this after refactor, currently does not work correctly due to (guess???) vanilla code being cooked.
		baseCondition += CaveOrJungleWater = new(!InfoPlayerInTown & InfoWater & (InfoCaverns | ProperGroundSpawnTile(TileID.JungleGrass)), (info) => (InfoCaverns & !ProperGroundSpawnTile(TileID.JungleGrass)).IsMet(info) ? 0.5f : 1f, new ISpawnTreeItem[] {
			// 16 (!flag12 && flag7 && ((flag21 && Main.rand.Next(2) == 0) || num49 == 60)) Factor out "!flag12 && flag7"
			JungleWater = new(ProperGroundSpawnTile(TileID.JungleGrass) & InfoOverWorld & BelowTile(50) & TimeDay & OnWaterSurface & !InfoSafeRange, new ISpawnTreeItem[] {
				// This is only valid because it's the only
				JungleWaterSurfaceCritter = new(1f / 3f),
				JunglePiranha = new()
			}),
			CaveWater = new(InfoCaverns) // (flag21 && Main.rand.Next(2) == 0)
		});

		// 17 (!flag12 && flag7 && (double)num2 > Main.worldSurface && Main.rand.Next(3) == 0)
		baseCondition += CaveJellyfish = new(!InfoPlayerInTown & InfoWater & !AboveOrWorldSurface, 1f / 3f);

		// Water Critters 18 (flag7 && Main.rand.Next(4) == 0 && ((num > WorldGen.oceanDistance && num < Main.maxTilesX - WorldGen.oceanDistance) || (double)num2 >
		// Main.worldSurface + 50.0))
		baseCondition += WaterCritter = new(InfoWater & (NotInOceanDistance | BelowSurfaceBy(50)), 0.25f, new ISpawnTreeItem[] {
			CorruptWaterCritter = new(InCorrupt), //18.1
			CrimsonWaterCritter = new(InCrimson), // 18.2
			OverworldWaterCritter = new(AboveWorldSurface & BelowTile(50) & TimeDay, 2f / 3f, new ISpawnTreeItem[] { // 18.3
				OverworldWaterSurfaceCritter = new(OnWaterSurface & !InfoSafeRange),
				OverworldUnderwaterCritter = new()
			}),
			DefaultWaterCritter = new()// 18.4, .5, .6
		});

		// Bound NPCs
		baseCondition += BoundGoblin = new(BoundNPCBaseCondition & DownedGoblinArmy & GoblinNotSavedOrSpawned, GetPlayerRollWeightFunc(20)); // 19
		baseCondition += BoundWizard = new(BoundNPCBaseCondition & HardMode & WizardNotSavedOrSpawned, GetPlayerRollWeightFunc(20)); // 20: 62511
		baseCondition += BoundOldShakingChest = new(BoundNPCBaseCondition & DownedSkeletron & OldChestNotSavedOrSpawned, GetPlayerRollWeightFunc(20)); // 21

		// Town Critters
		baseCondition += TownCritter = new(InfoPlayerInTown, new ISpawnTreeItem[] { // 22
			// Graveyard
			TownGraveyardCritter = new(InGraveyard, new ISpawnTreeItem[] {
				TownGraveyardWaterCritter = new(InfoWater) }),
			// Beach
			TownBeachCritter = new(!InfoSafeRange & InfoBeach,new ISpawnTreeItem[] {
				TownBeachWaterCritter = new(InfoWater) }),

			TownDragonFlyCritter = new(TimeDay & Raining & SpawnTile(TileID.Grass, TileID.GolfGrass, TileID.Sand)  & !TooWindyForButterflies & (AboveOrWorldSurface | RemixWorld) & DragonflyCattailTop, 0.5f),

			// Water General
			TownWaterCritter = new(InfoWater, new ISpawnTreeItem[] {
				TownOverworldWaterCritter = new(InfoOverWorld & BelowTile(50) & TimeDay, 2f / 3f, new ISpawnTreeItem[] {
					TownOverworldWaterSurfaceCritter = new(OnWaterSurface),
					TownOverworldWaterBeachCritter = new(InfoBeach),
					TownDefaultWaterCritter = new()
				}),
				TownOverworldWaterBeachCritter,
				TownDefaultWaterCritter
			}),
			TownSnowCritter = new(SpawnTile(TileID.SnowBlock, TileID.IceBlock)),
			TownJungleCritter = new(SpawnTile(TileID.JungleGrass)),
			TownDesertCritter = new(SpawnTile(TileID.Sand)),
			TownGrassCritter = new(!AboveOrWorldSurface | SpawnTile(TileID.Grass, TileID.GolfGrass, TileID.HallowedGrass, TileID.GolfGrassHallowed), new ISpawnTreeItem[] {
				TownRainingUnderGroundCritter = new(Raining & PlayerFloorInUnderworld, new ISpawnTreeItem[] {
					TownGemSquirrel = new(InfoCaverns, 0.2f),
					TownGemBunny = new(InfoCaverns, 0.2f),
					TownGeneralCritter = new()
				}),
				TownCritterGreenFairy = new(TimeDay & FairyCritter & InfoOverWorld, GetPlayerRollWeightFunc(2))})
		});

		// Dungeon
		baseCondition += Dungeon = new(InDungeon, new ISpawnTreeItem[] { // 23
			DungeonGuardian = new(DownedSkeletron & DungeonGuardianHeightOrDrunk),
			DungeonNormal = new()
		});

		// Meteor
		baseCondition += Meteor = new(InMeteor); // 24

		// Events
		baseCondition += OldOnesArmy = new(DungeonDefendersOngoing & InOldOneArmy); // 25
		baseCondition += FrostMoon = new((RemixWorld | AboveOrWorldSurface) & !TimeDay & FrostMoonInvasion); // 25
		baseCondition += PumpkinMoon = new((RemixWorld | AboveOrWorldSurface) & !TimeDay & PumpkinMoonInvasion); // 26
		baseCondition += SolarEclipse = new((AboveOrWorldSurface | (RemixWorld & BelowRockLayer)) & TimeDay & Eclipse); // 27

		baseCondition += UndergroundFairy = new(UndergroundFairyCondition); // 28

		Gnome = new();
		//(!Main.remixWorld && !flag7 && (!Main.dayTime || Main.tile[num, num2].wall > 0) && Main.tile[num8, num9].wall == 244 && !Main.eclipse && !Main.bloodMoon && Main.player[k].RollLuck(30) == 0 && CountNPCS(624) <= Main.rand.Next(3))
		baseCondition += Gnome.AddAndReturn(new CalculatedSpawnCondition(!RemixWorld & !InfoWater & (!TimeDay | HasTrueWallTile) & PlayerCenterSpawnTile(WallID.LivingWoodUnsafe) & !Eclipse & !BloodMoon & SpawnCap(NPCID.Gnome, 3),
			(info) => (3 - NPC.CountNPCS(NPCID.Gnome)) / 3f * GetPlayerRollWeight(info, 30))); //29

		//ReLogic??? I will cry on you and that is a threat
		//(!Main.player[k].ZoneCorrupt && !Main.player[k].ZoneCrimson && !flag7 && !Main.eclipse && !Main.bloodMoon && Main.player[k].RollLuck(range) == 0 && ((!Main.remixWorld && (double)num2 >= Main.worldSurface * 0.800000011920929 && (double)num2 < Main.worldSurface * 1.100000023841858) || (Main.remixWorld && (double)num2 > Main.rockLayer && num2 < Main.maxTilesY - 350)) && CountNPCS(624) <= Main.rand.Next(3) && (!Main.dayTime || Main.tile[num, num2].wall > 0) && (Main.tile[num, num2].wall == 63 || Main.tile[num, num2].wall == 2 || Main.tile[num, num2].wall == 196 || Main.tile[num, num2].wall == 197 || Main.tile[num, num2].wall == 198 || Main.tile[num, num2].wall == 199))
		baseCondition += Gnome.AddAndReturn(
			new CalculatedSpawnCondition(!InCorrupt & !InCrimson & !InfoWater & !Eclipse & !BloodMoon & (!TimeDay | HasTrueWallTile) & SpawnCap(NPCID.Gnome, 3) & ConditionWrapper.CreateConditional(RemixWorld, BelowRockLayer & AboveTileFromFloor(350), GnomeSurfaceCheck) & TrueWallTile(63, 2, 196, 197, 198, 199),
				(info) => (3 - NPC.CountNPCS(NPCID.Gnome)) / 3f * GetPlayerRollWeight(info, Main.remixWorld ? 5 : 10))); //30

		// Mushroom
		baseCondition += HardmodeMushroomWater = new(HardMode & SpawnTile(TileID.MushroomGrass) & InfoWater); //31
		baseCondition += OverworldMushroom = new(SpawnTile(TileID.MushroomGrass) & AboveOrWorldSurface, 2f / 3f); //32
		baseCondition += UndergroundMushroom = new(SpawnTile(TileID.MushroomGrass) & HardMode & !AboveWorldSurface & (!RemixWorld | GetGoodWorld | AboveTileFromFloor(360)), 2f / 3f); //33

		// Misc
		baseCondition += CorruptWorm = new(InCorrupt & !InfoPlayerSafe,
			// Local variable "maxValue"
			(info) => (Main.remixWorld && (double)(info.Player.position.Y / 16f) < Main.worldSurface && (info.Player.ZoneCorrupt || info.Player.ZoneCrimson)) ? (1 / 25f) : (1 / 65f)); //35

		baseCondition += UndergroundMimic.AddAndReturn(new CalculatedSpawnCondition(RemixWorld & !HardMode & !AboveOrWorldSurface, GetPlayerRollWeightFunc(100))); // 36
		baseCondition += UndergroundMimic.AddAndReturn(new CalculatedSpawnCondition(HardMode & !AboveOrWorldSurface, (info) => GetPlayerRollWeight(info, Main.tenthAnniversaryWorld ? 25 : 75))); // 37
		baseCondition += OverworldMimic = new(HardMode & TrueWallTile(WallID.DirtUnsafe), 1f / 20f); //38

		baseCondition += Wraith = new(HardMode & AboveOrWorldSurface & TimeDay, //39
			(info) => (1 / 20f) + (Main.moonPhase == 4 ? 1f / 5f : 0f));
		// P(A U B) = P(A) + P(B) - P(A n B) for independent event, so for independent events (where P(A n B) = 0), P(A U B) = P(A) + P(B)
		baseCondition += HoppinJack = new(HardMode & Halloween & AboveOrWorldSurface & !TimeDay, 0.1f); //40
		baseCondition += DoctorBones = new(ProperGroundSpawnTile(TileID.JungleGrass) & TimeDay, GetPlayerRollWeightFunc(500)); // 41
		baseCondition += LacBeetle = new(SpawnTile(TileID.JungleGrass) & !AboveOrWorldSurface, 1f / 60f); // 42

		// Critters
		baseCondition += WormCritter = new(!AboveOrWorldSurface & CommonAboveHellHeightCheck & !InSnow & !InCrimson & !InCorrupt & !InJungle & !InHallow, 1f / 8f); // 43
		baseCondition += MouseCritter = new(!AboveOrWorldSurface & CommonAboveHellHeightCheck & !InSnow & !InCrimson & !InCorrupt & !InJungle & !InHallow, 1f / 13f); // 44
		baseCondition += SnailCritter = new(!AboveOrWorldSurface & IDontKnow & !InSnow & !InCrimson & !InCorrupt & !InHallow, 1f / 13f); // 45
		baseCondition += JungleCritterBirdOrFrog = new(InfoOverWorld & InJungle & !InCrimson & !InCorrupt, 1f / 7f, new ISpawnTreeItem[] { // 46 (flag20 && Main.player[k].ZoneJungle && !Main.player[k].ZoneCrimson && !Main.player[k].ZoneCorrupt && Main.rand.Next(7) == 0)
			JungleCritterBird = new(JungleBirdTime, 2f / 3f),
			FrogCritter = new()
		});

		baseCondition += Hive = new(ProperGroundSpawnTile(TileID.Hive), 0.5f); // 47

		// Jungle
		baseCondition += HardmodeJungle = new(SpawnTile(TileID.JungleGrass) & HardMode, 2f / 3f); // 48
		baseCondition += JungleTemple = new((ProperGroundSpawnTile(TileID.LihzahrdBrick, TileID.WoodenSpikes) | RemixWorld) & InfoLihzahrd); // 49
		baseCondition += HiveHornet = new(InfoWallTile(WallID.HiveUnsafe), 7f / 8f); // 50

		// 51 (num49 == 60 && ((!Main.remixWorld && (double)num2 > (Main.worldSurface + Main.rockLayer) / 2.0) || (Main.remixWorld && ((double)num2 < Main.rockLayer ||
		// Main.rand.Next(2) == 0))))
		baseCondition += UndergroundJungle = new(SpawnTile(TileID.JungleGrass)
			& ConditionWrapper.CreateConditional(RemixWorld, !BelowOrRockLayer, IDontKnow2), (info) => Main.remixWorld ? 0.5f : 1f);

		baseCondition += SurfaceJungle = new(SpawnTile(TileID.JungleGrass), 3f / 8f); // 52 (num49 == 60 && Main.rand.Next(4) == 0)
																					  // 53 (num49 == 60 && Main.rand.Next(8) == 0)

		// Sandstorm
		baseCondition += SandstormEvent = new(SandstormHappening & InSandstorm & Conversion.Sand & Spawning_SandstoneCheck); // 54

		// Mummy
		baseCondition += Mummy = new(HardMode & SpawnTile(TileID.Sand), 1f / 3f); // 55
		baseCondition += DarkMummy = new(HardMode & SpawnTile(TileID.Ebonsand), 0.5f); // 56
		baseCondition += BloodMummy = new(HardMode & SpawnTile(TileID.Crimsand), 0.5f); // 57
		baseCondition += LightMummy = new(HardMode & SpawnTile(TileID.Pearlsand), 0.5f); // 58

		// Hallow
		baseCondition += OverworldHallow = new(HardMode & !InfoWater & InfoUnderGround & ProperGroundSpawnTile(TileID.Pearlsand,
			TileID.Pearlstone, TileID.HallowedGrass, TileID.HallowedIce)); // 59
		baseCondition += EnchantedSword = new(!InfoPlayerSafe & HardMode & !InfoWater & InfoCaverns & ProperGroundSpawnTile(TileID.Pearlsand,
			TileID.Pearlstone, TileID.HallowedGrass, TileID.HallowedIce), 1f / 50f); // 60

		// World Evil
		baseCondition += Crimson = new((ProperGroundSpawnTile(TileID.Crimtane) & InCrimson) | ProperGroundSpawnTile(TileID.CrimsonGrass, TileID.FleshIce, TileID.Crimstone, TileID.Crimsand, TileID.CrimsonJungleGrass)); // 61
		baseCondition += Corruption = new((ProperGroundSpawnTile(TileID.Demonite) & InCorrupt) | ProperGroundSpawnTile(TileID.CorruptGrass, TileID.CorruptIce, TileID.Ebonstone, TileID.Ebonsand, TileID.CorruptJungleGrass)); // 62

		// Overworld
		baseCondition += Overworld = new(InfoOverWorld, new ISpawnTreeItem[] { // 63
			// Overworld Misc
			IceGolem = new(InSnow & HardMode & CloudAlphaAbove(0f) & SpawnCap(NPCID.IceGolem), 0.05f),
			RainbowSlime = new(InHallow & HardMode & CloudAlphaAbove(0f) & SpawnCap(NPCID.RainbowSlime), 0.05f),
			AngryNimbus = new(!InSnow & HardMode & CloudAlphaAbove(0f) & SpawnCap(NPCID.AngryNimbus, 2), 0.1f),
			MartianProbe = new(MartianProbePosition & HardMode & DownedGolem & SpawnCap(NPCID.MartianProbe),
				(info) => 1f - (!NPC.downedMartians ? 399f / 400f * 0.99f : (399f / 400f))),

			// Overworld Typical
			OverworldDay = new(TimeDay, new ISpawnTreeItem[] {
				OverworldDaySnowCritter = new(InfoInnerThird & TrueSpawnTile(TileID.SnowBlock, TileID.IceBlock), 1f / 15f),
				OverworldDayGrassCritter = new(InfoInnerThird & TrueSpawnTile(TileID.Grass, TileID.HallowedGrass), 1f / 15f),
				OverworldDaySandCritter = new(InfoInnerThird & TrueSpawnTile(TileID.Sand), 1f / 15f),
				OverworldMorningBirdCritter = new(InfoInnerThird & TimeLessThan(18000.0) & TrueSpawnTile(TileID.Grass, TileID.HallowedGrass), 0.25f),
				OverworldDayBirdCritter = new(InfoInnerThird & TrueSpawnTile(TileID.Grass, TileID.HallowedGrass, TileID.SnowBlock), 1f / 15f),

				// King Slime (Overworld)
				KingSlime = new(InfoOuterThird & TrueSpawnTile(TileID.Grass) & SpawnCap(NPCID.KingSlime), 1f / 300f),

				OverworldDayDesert = new(TrueSpawnTile(TileID.Sand) & InfoWater, 0.2f),

				// Overworld Goblin Scout
				GoblinScout = new(InfoOuterThird, (info) => 1f - ((!NPC.downedGoblins && WorldGen.shadowOrbSmashed) ? 6f / 7f * (14f / 15f) : (14f / 15f))),

				// Overworld Typical
				OverworldDayRain = new(Raining, 2f / 3f),
				OverworldDaySlime = new()
			}),
			OverworldNight = new(new ISpawnTreeItem[] {
				OverworldFirefly = new(TrueSpawnTile(TileID.Grass, TileID.HallowedGrass), (info) => 1f / NPC.fireFlyChance),
				OverworldNightMonster = new()
			})
		});

		// Underground
		baseCondition += Underground = new(InfoUnderGround); // 64
		baseCondition += Underworld = new(UnderworldHeight); // 65

		// Caverns

		baseCondition += Cavern = new(new ISpawnTreeItem[] { // 66 => 93 //TODO: Make into Multi? Kinda unnecessary as overflow isn't an issue here
			RockGolem = new(RockGolemCondition),
			DyeBeetle = new(1f / 60f),
			ChaosElemental = new(HardMode & !InfoPlayerSafe & ProperGroundSpawnTile(TileID.Pearlsand, TileID.Pearlstone, TileID.HallowedIce), 1f / 8f),

			Pigron = new(TrueSpawnTile(TileID.SnowBlock, TileID.IceBlock, TileID.BreakableIce, TileID.CorruptIce, TileID.HallowedIce, TileID.FleshIce)
			& !InfoSafeRange & HardMode, 1f / 30f, new ISpawnTreeItem[] {
				PurplePigron = new(InCorrupt),
				BluePigron = new(InHallow),
				PinkPigron = new(InCrimson),
			}),

			IceTortoise = new(HardMode & InSnow, 0.1f),
			DiggerWormFlinx = new(!InfoSafeRange & InHallow, 0.01f, new ISpawnTreeItem[] {
				Flinx1 = new(HardMode & InSnow) }),
			Flinx2 = new(InSnow, 1f / 20f),
			MotherSlimeBlueSlimeSpikedIceSlime = new((info) => Main.hardMode ? 1f / 20f : 1f / 10f),
			JungleSlimeBlackSlimeSpikedIceSlime = new(HardMode, 0.25f),
			MiscCavern = new(0.5f),
			SkeletonMerchant = new(SpawnCap(453, 1), 1f / 35f),
			LostGirl = new(1f / 80f),
			RuneWizard = new(HardMode & (RemixWorld | !IDontKnow), 1f / 200f),
			Marble = new(InfoMarble, 3f / 4f),
			Granite = new(InfoGranite, 4f / 5f),
			Tim = new(RemixWorld | !IDontKnow, (info) => 1f / (TimArmourCheck.IsMet(info) ? 50f : 200f)),
			new WeightedSpawnCondition(HardMode, 0.9f),
			Ghost = new(!InfoPlayerSafe & (Halloween | InGraveyard), 1f / 30f),
			UndeadMiner = new(1f / 20f),
			new ConditionedSpawnTreeParent(SpawnTile(TileID.SnowBlock, TileID.IceBlock, TileID.BreakableIce),new ISpawnTreeItem[] {
				Flinx3 = new(1f / 15f),
				UndeadViking = new()
			}),
			Flinx4 = new(InSnow),
			GenericCavernMonster = new(1f / 3f),
			SporeSkeletons = new(InGlowshroom & SpawnTile(TileID.MushroomGrass, TileID.MushroomBlock)),
			HalloweenSkeletons = new(Halloween, 0.5f),
			ExpertSkeletons = new(ExpertMode, 1f / 3f),
			NormalSkeletons = new()
		});
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
		return !IsHouseWall(info.SpawnTileX, surfaceY) && surfaceY > 0;
	}

	/// <summary> Shorthand for "Main.wallHouse[Main.tile[x, y].wall]" </summary>
	public static bool IsHouseWall(int x, int y)
		=> Main.wallHouse[Main.tile[x, y].wall];

	/// <summary> Checks if a martian probe can spawn </summary>
	public static bool MartianProbeHelper(NPCSpawnInfo info)
		=> MathF.Abs(info.SpawnTileX - (Main.maxTilesX / 2)) / (Main.maxTilesX / 2) > 0.33f && !NPC.AnyDanger();

	/// <summary> Checks if the spawn tile is in the middle third of the map </summary>
	public static bool InnerThird(NPCSpawnInfo info)
		=> Math.Abs(info.SpawnTileX - Main.spawnTileX) < Main.maxTilesX / 3;

	/// <summary> Checks if the spawn tile is in either of the outer sixths of the map </summary>
	public static bool OuterThird(NPCSpawnInfo info)
		=> Math.Abs(info.SpawnTileX - Main.spawnTileX) > Main.maxTilesX / 3;

	public static Func<NPCSpawnInfo, float> GetPlayerRollWeightFunc(int range)
		=> (info) => GetPlayerRollWeight(info, range);

	public static float GetPlayerRollWeight(NPCSpawnInfo info, int range)
		=> 1f / (info.Player.luck > 0f && Main.rand.NextFloat() < info.Player.luck
			? Main.rand.Next(Main.rand.Next(range / 2, range))
			: info.Player.luck < 0f && Main.rand.NextFloat() < 0f - info.Player.luck
			? Main.rand.Next(Main.rand.Next(range, range * 2))
			: Main.rand.Next(range));

	public static bool ContainsOrIsCondition(this ISubSpawnCondition thisCondition, ISubSpawnCondition condition)
		=> thisCondition == condition || (thisCondition is ConditionWrapper conditionWrapper && conditionWrapper.SpecificConditions.Any(innerCondition => innerCondition.ContainsOrIsCondition(condition)));

	public static bool ContainsCondition(this IConditionedTreeItem thisConditionItem, ISubSpawnCondition condition)
		=> thisConditionItem.Conditions.ContainsOrIsCondition(condition);
}

// TODO: Add names for windy day spawns,
// TODO: Finish gem critter calculator,
// TODO: obsolete inaccurate names, return correct
// TODO: explore viability of larger category based abstraction layer to wrap vanilla a little better (Something like ~Biome => Liquid? => Other Conditions => Gold Critter? => Spawn)
// TODO: Poke at ModBiome for spawn blocking stuff?