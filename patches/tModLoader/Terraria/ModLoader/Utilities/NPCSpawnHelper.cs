using System;
using System.Collections.Generic;
using System.Linq;
using MonoMod.Utils;
using Terraria.GameContent.Events;
using Terraria.ID;
using static Terraria.ModLoader.Utilities.SubSpawnCondition.Enemy;
using static Terraria.ModLoader.Utilities.SubSpawnCondition.Common;
using static Terraria.ModLoader.Utilities.SubSpawnCondition.Unique;
using static Terraria.ModLoader.Utilities.SubSpawnCondition.Zone;
using static Terraria.ModLoader.Utilities.SubSpawnCondition.Tile;

namespace Terraria.ModLoader.Utilities;

public enum CompareType
{
	And,
	Or,
}

public interface ISubSpawnCondition
{
	public bool IsMet(NPCSpawnInfo info);

	public ISubSpawnCondition GetNot();
}

public sealed record class SubSpawnCondition(Func<NPCSpawnInfo, bool> Predicate, bool Not, string MetaData) : ISubSpawnCondition, IEquatable<SubSpawnCondition>, IEquatable<Condition>
{
	public SubSpawnCondition EqualsWhenNot { get; init; } = null;
	public SubSpawnCondition(Func<NPCSpawnInfo, bool> Predicate) : this(Predicate, false, null)
	{ }

	private static readonly Action<NPCSpawnInfo> setPlayer = (NPCSpawnInfo info) => {
		StorePlayer = Main.myPlayer;
		Main.myPlayer = info.Player.whoAmI;
	};
	public static int StorePlayer { get; private set; } = -1;
	private static readonly Action<NPCSpawnInfo> resetPlayer = (NPCSpawnInfo info) => {
		Main.myPlayer = StorePlayer;
		StorePlayer = -1;
	};
	public static SubSpawnCondition FromCondition(Condition condition, bool fixPlayer = false)
	{
		Func<NPCSpawnInfo, bool> func = fixPlayer ? info => {
			setPlayer.Invoke(info);
			bool returnVal = condition.IsMet();
			resetPlayer.Invoke(info);
			return returnVal;
		} : condition.Predicate.CastDelegate<Func<NPCSpawnInfo, bool>>();
		return new SubSpawnCondition(func, false, condition.Description.Key);
	}
	public bool Equals(Condition other)
	{
		return Not ? EqualsWhenNot.TrueEquals(other) : TrueEquals(other);
	}

	private bool TrueEquals(Condition other)
	{
		return other.Description.Key == MetaData;
	}

	public bool Equals(SubSpawnCondition other)
	{
		return Not && EqualsWhenNot != null && other.TrueEquals(EqualsWhenNot) || other.TrueEquals(this);
	}

	private bool TrueEquals(SubSpawnCondition other)
	{
		return other != null
			&& ((other.MetaData != string.Empty && MetaData != string.Empty) ? other.MetaData == MetaData : Predicate == other.Predicate)
			&& Not == other.Not;
	}

	//TODO
	public override int GetHashCode() => base.GetHashCode();

	//public bool Equals(SubSpawnCondition? other)
	//{
	//	return other.HasValue
	//		&& ((other.Value.MetaData != string.Empty && MetaData != string.Empty) ? other.Value.MetaData == MetaData : Predicate == other.Value.Predicate)
	//		&& Not == other.Value.Not;
	//}
	public bool IsMet(NPCSpawnInfo info) => (Predicate(info) ^ Not);
	public SubSpawnCondition GetNot() => !this;
	ISubSpawnCondition ISubSpawnCondition.GetNot() => GetNot();
	public static SubSpawnCondition operator !(SubSpawnCondition condition)
		=> condition with { Not = !condition.Not };
	public static IEnumerable<ISubSpawnCondition> operator +(SubSpawnCondition condition1, SubSpawnCondition condition2)
		=> new ISubSpawnCondition[] { condition1, condition2 };
	public static ConditionWrapper operator &(SubSpawnCondition condition1, SubSpawnCondition condition2)
		=> new(CompareType.And, condition1 + condition2);
	public static ConditionWrapper operator |(SubSpawnCondition condition1, SubSpawnCondition condition2)
		=> new(CompareType.Or, condition1 + condition2);

	public static implicit operator ConditionWrapper(SubSpawnCondition condition)
		=> new(CompareType.And, new ISubSpawnCondition[] { condition });
	public static class Zone
	{
		public static readonly SubSpawnCondition InDungeon = FromCondition(Condition.InDungeon, true) ;
		public static readonly SubSpawnCondition InCorrupt = FromCondition(Condition.InCorrupt, true);
		public static readonly SubSpawnCondition InHallow = FromCondition(Condition.InHallow, true);
		public static readonly SubSpawnCondition InMeteor = FromCondition(Condition.InMeteor, true);
		public static readonly SubSpawnCondition InJungle = FromCondition(Condition.InJungle, true);
		public static readonly SubSpawnCondition InSnow = FromCondition(Condition.InSnow, true);
		public static readonly SubSpawnCondition InCrimson = FromCondition(Condition.InCrimson, true);
		public static readonly SubSpawnCondition InWaterCandle = FromCondition(Condition.InWaterCandle, true);
		public static readonly SubSpawnCondition InPeaceCandle = FromCondition(Condition.InPeaceCandle, true);
		public static readonly SubSpawnCondition InTowerSolar = FromCondition(Condition.InTowerSolar, true);
		public static readonly SubSpawnCondition InTowerVortex = FromCondition(Condition.InTowerVortex, true);
		public static readonly SubSpawnCondition InTowerNebula = FromCondition(Condition.InTowerNebula, true);
		public static readonly SubSpawnCondition InTowerStardust = FromCondition(Condition.InTowerStardust, true);
		public static readonly SubSpawnCondition InDesert = FromCondition(Condition.InDesert, true);
		public static readonly SubSpawnCondition InGlowshroom = FromCondition(Condition.InGlowshroom, true);
		public static readonly SubSpawnCondition InUndergroundDesert = FromCondition(Condition.InUndergroundDesert, true);
		public static readonly SubSpawnCondition InSkyHeight = FromCondition(Condition.InSkyHeight, true);
		public static readonly SubSpawnCondition InOverworldHeight = FromCondition(Condition.InOverworldHeight, true);
		public static readonly SubSpawnCondition InDirtLayerHeight = FromCondition(Condition.InDirtLayerHeight, true);
		public static readonly SubSpawnCondition InRockLayerHeight = FromCondition(Condition.InRockLayerHeight, true);
		public static readonly SubSpawnCondition InUnderworldHeight = FromCondition(Condition.InUnderworldHeight, true);
		public static readonly SubSpawnCondition InBeach = FromCondition(Condition.InBeach, true);
		public static readonly SubSpawnCondition InRain = FromCondition(Condition.InRain, true);
		public static readonly SubSpawnCondition InSandstorm = FromCondition(Condition.InSandstorm, true);
		public static readonly SubSpawnCondition InOldOneArmy = FromCondition(Condition.InOldOneArmy, true);
		public static readonly SubSpawnCondition InGraveyard = FromCondition(Condition.InGraveyard, true);
		public static readonly SubSpawnCondition ZoneShadowCandle = new((info) => info.Player.ZoneShadowCandle);
		public static readonly SubSpawnCondition InAether = FromCondition(Condition.InAether, true);

		public static readonly SubSpawnCondition InfoPlayerInTown = new((info) => info.PlayerInTown);
	}

	public static class Unique
	{
		public static readonly SubSpawnCondition RockGolemCondition = new((info) => NPC.SpawnNPC_CheckToSpawnRockGolem(info.SpawnTileX, info.SpawnTileY, info.Player.whoAmI, info.ProperGroundTileType));
		public static readonly SubSpawnCondition TimArmourCheck = new((info) => (info.Player.armor[1].type == 4256 || (info.Player.armor[1].type >= 1282 && info.Player.armor[1].type <= 1287)) && info.Player.armor[0].type != 238);
		public static readonly SubSpawnCondition SpiderCaveCheck = new((info) => SpawnCondition.GetTile(info).wall == WallID.SpiderUnsafe || info.SpiderCave);

		public static readonly SubSpawnCondition DesertCaveConditions = new((info) => (NPC.SpawnTileOrAboveHasAnyWallInSet(info.SpawnTileX, info.SpawnTileY, WallID.Sets.AllowsUndergroundDesertEnemiesToSpawn) || info.DesertCave));
		public static readonly SubSpawnCondition CanGetBartender = new((info) => !NPC.savedBartender && DD2Event.ReadyToFindBartender);
		public static readonly ConditionWrapper BoundNPCBaseCondition = !InfoWater & InfoCaverns & CommonAboveHellHeightCheck;

		public static readonly SubSpawnCondition AnglerNotSavedOrSpawned = new((info) => !NPC.savedAngler && !NPC.AnyNPCs(NPCID.SleepingAngler));
		public static readonly SubSpawnCondition AnglerOceanSurface = new((info) => (info.SpawnTileY < Main.worldSurface - 10.0 || Main.remixWorld));

		public static readonly SubSpawnCondition WizardNotSavedOrSpawned = new((info) => !NPC.savedWizard && !NPC.AnyNPCs(NPCID.BoundWizard));
		public static readonly SubSpawnCondition GoblinNotSavedOrSpawned = new((info) => !NPC.savedGoblin && !NPC.AnyNPCs(NPCID.BoundGoblin));
		public static readonly SubSpawnCondition OldChestNotSavedOrSpawned = new((info) => !NPC.unlockedSlimeOldSpawn && !NPC.AnyNPCs(NPCID.BoundTownSlimeOld));

		public static readonly SubSpawnCondition BelowTile50 = BelowTile(50);
		public static readonly SubSpawnCondition IDontKnow = new((info) => info.SpawnTileY < (Main.rockLayer + Main.maxTilesY) / 2); //TODO: IDontKnow
		public static readonly SubSpawnCondition IDontKnow2 = new((info) => info.SpawnTileY > (Main.worldSurface + Main.rockLayer) / 2.0); //TODO: IDontKnow

		public static readonly SubSpawnCondition DungeonGuardianHeightOrDrunk = new((info) => (!Main.drunkWorld || (info.Player.position.Y / 16f < (Main.dungeonY + 40))));
		public static readonly SubSpawnCondition DungeonDefendersOngoing = new((info) => DD2Event.Ongoing);

		public static readonly SubSpawnCondition JungleBirdTime = new((info) => Main.dayTime && Main.time < 43200.00064373016);
		public static readonly SubSpawnCondition MartianProbePosition = new(SpawnCondition.MartianProbeHelper);
		public static readonly SubSpawnCondition UndergroundFairyCondition = new((info) => NPC.SpawnNPC_CheckToSpawnUndergroundFairy(info.SpawnTileX, info.SpawnTileY, info.Player.whoAmI));
		public static readonly SubSpawnCondition Spawning_SandstoneCheck = new((info) => NPC.Spawning_SandstoneCheck(info.SpawnTileX, info.SpawnTileY));
		public static readonly SubSpawnCondition FairyCritter = new((info) => Main.numClouds <= 55 && Main.cloudBGActive == 0f && Star.starfallBoost > 3f);
		public static readonly SubSpawnCondition DragonflyCattailTop = new((info) => NPC.FindCattailTop(info.SpawnTileX, info.SpawnTileY, out _, out _));
	}

	public static class Enemy
	{
		public static readonly SubSpawnCondition DownedSkeletron = FromCondition(Condition.DownedSkeletron);
		public static readonly SubSpawnCondition DownedGoblinArmy = FromCondition(Condition.DownedGoblinArmy);
		public static readonly SubSpawnCondition DownedGolem = FromCondition(Condition.DownedGolem);

		public static SubSpawnCondition InvasionIDHappening(int invasionID)
			=> new((info) => Main.invasionType == invasionID) { MetaData = nameof(InvasionIDHappening) + ":" + invasionID };

		public static readonly SubSpawnCondition GoblinArmyCondition = InvasionIDHappening(InvasionID.GoblinArmy);
		public static readonly SubSpawnCondition FrostLegionCondition = InvasionIDHappening(InvasionID.SnowLegion);
		public static readonly SubSpawnCondition PiratesCondition = InvasionIDHappening(InvasionID.PirateInvasion);
		public static readonly SubSpawnCondition MartianMadnessCondition = InvasionIDHappening(InvasionID.MartianMadness);
		public static SubSpawnCondition SpawnCap(int npcID, int count = 1)
			=> new((info) => IsNPCCountUnder(npcID, count)) { MetaData = nameof(SpawnCap) + ":" + npcID + ":" + count };
		private static bool IsNPCCountUnder(int npcID, int count)
		{
			for (int i = 0; i < 200; i++) {
				if (Main.npc[i].active && Main.npc[i].type == npcID && --count <= 0)
					return false;
			}

			return true;
		}
	}
	public static class Common
	{
		public static readonly SubSpawnCondition PlayerFloorInUnderworld = new((info) => info.PlayerFloorY <= Main.UnderworldLayer);

		public static readonly SubSpawnCondition TimeDay = FromCondition(Condition.TimeDay) with { EqualsWhenNot = TimeNight };
		public static readonly SubSpawnCondition TimeNight = FromCondition(Condition.TimeNight) with { EqualsWhenNot = TimeDay };
		public static SubSpawnCondition TimeLessThan(double time)
			=> new((info) => Main.time< time) { MetaData = nameof(TimeLessThan) + ":" + time };

		public static readonly SubSpawnCondition Raining = new((info) => Main.raining);
		public static readonly SubSpawnCondition SandstormHappening = new((info) => Sandstorm.Happening);

		public static readonly SubSpawnCondition HardMode = new((info) => Main.hardMode);
		public static readonly SubSpawnCondition ExpertMode = new((info) => Main.expertMode);
		public static readonly SubSpawnCondition MasterMode = new((info) => Main.masterMode);
		public static readonly SubSpawnCondition RemixWorld = new((info) => Main.remixWorld);

		public static readonly SubSpawnCondition TooWindyForButterflies = new((info) => NPC.TooWindyForButterflies);

		public static readonly SubSpawnCondition InfoWater = new((info) => info.Water);
		public static readonly SubSpawnCondition OnWaterSurface = new(SpawnCondition.WaterSurface);
		public static readonly SubSpawnCondition OnWaterSurfaceAvoidHousing = new((info) => SpawnCondition.WaterSurfaceAvoidHousing(info, 2, 50, 3));

		public static readonly SubSpawnCondition AboveWorldSurface = new((info) => info.SpawnTileY <= Main.worldSurface);
		public static readonly SubSpawnCondition CommonAboveHellHeightCheck = !BelowTileFromFloor(210);
		public static readonly SubSpawnCondition UnderworldHeight = BelowTileFromFloor(190);

		public static readonly SubSpawnCondition InBeachDistance = new((info) => (info.SpawnTileX < WorldGen.beachDistance || info.SpawnTileX > Main.maxTilesX - WorldGen.beachDistance));
		public static readonly SubSpawnCondition InfoInnerThird = new(SpawnCondition.InnerThird);
		public static readonly SubSpawnCondition InfoOuterThird = new(SpawnCondition.OuterThird);
		public static readonly SubSpawnCondition ProperGroundTileSand = new((info) => Main.tileSand[info.ProperGroundTileType]);

		public static readonly SubSpawnCondition InfoInvasion = new((info) => info.Invasion);
		public static readonly SubSpawnCondition Halloween = new((info) => Main.halloween);
		public static readonly SubSpawnCondition FrostMoonInvasion = new((info) => Main.snowMoon);
		public static readonly SubSpawnCondition PumpkinMoonInvasion = new((info) => Main.pumpkinMoon);
		public static readonly SubSpawnCondition Eclipse = new((info) => Main.eclipse);

		public static readonly SubSpawnCondition InfoOverWorld = new((info) => info.OverWorld);
		public static readonly SubSpawnCondition InfoUnderGround = new((info) => info.UnderGround);
		public static readonly SubSpawnCondition InfoMarble = new((info) => info.Marble);
		public static readonly SubSpawnCondition InfoGranite = new((info) => info.Granite);
		public static readonly SubSpawnCondition InfoBeach = new((info) => info.Beach);
		public static readonly SubSpawnCondition InfoOcean = new((info) => info.Ocean);
		public static readonly SubSpawnCondition InfoSky = new((info) => info.Sky);
		public static readonly SubSpawnCondition InfoCaverns = new((info) => info.Caverns);
		public static readonly SubSpawnCondition InfoLihzahrd = new((info) => info.Lihzahrd);

		public static readonly SubSpawnCondition InfoSafeRange = new((info) => info.SafeRangeX);
		public static readonly SubSpawnCondition InfoPlayerSafe = new((info) => info.PlayerSafe);

		public static readonly SubSpawnCondition CheckUnderground = new((info) => WorldGen.checkUnderground(info.SpawnTileX, info.SpawnTileY));

		public static SubSpawnCondition CloudAlphaAbove(float amount)
			=> new((info) => Main.cloudAlpha > amount) { MetaData = nameof(CloudAlphaAbove) + ":" + amount };
		public static SubSpawnCondition BelowTile(int height)
			=> new((info) => info.SpawnTileY > height) { MetaData = nameof(BelowTile) + ":" + height };

		public static SubSpawnCondition BelowTileFromFloor(int height)
			=> new((info) => info.SpawnTileY > Main.maxTilesY - height) { MetaData = nameof(BelowTileFromFloor) + ":" + height };
	}

	public static class Tile
	{
		/// <summary>
		/// wrap items from <see cref="TileID.Sets.Conversion"/>
		/// </summary>
		public static class Conversion
		{
			public static readonly SubSpawnCondition Stone = new((info) => TileID.Sets.Conversion.Stone[info.SpawnTileType]);
			public static readonly SubSpawnCondition Grass = new((info) => TileID.Sets.Conversion.Grass[info.SpawnTileType]);
			public static readonly SubSpawnCondition GolfGrass = new((info) => TileID.Sets.Conversion.GolfGrass[info.SpawnTileType]);
			public static readonly SubSpawnCondition JungleGrass = new((info) => TileID.Sets.Conversion.JungleGrass[info.SpawnTileType]);
			public static readonly SubSpawnCondition Snow = new((info) => TileID.Sets.Conversion.Snow[info.SpawnTileType]);
			public static readonly SubSpawnCondition Ice = new((info) => TileID.Sets.Conversion.Ice[info.SpawnTileType]);
			public static readonly SubSpawnCondition Sand = new((info) => TileID.Sets.Conversion.Sand[info.SpawnTileType]);
			public static readonly SubSpawnCondition HardenedSand = new((info) => TileID.Sets.Conversion.HardenedSand[info.SpawnTileType]);
			public static readonly SubSpawnCondition SandStone = new((info) => TileID.Sets.Conversion.Sandstone[info.SpawnTileType]);
			public static readonly SubSpawnCondition Moss = new((info) => TileID.Sets.Conversion.Moss[info.SpawnTileType]);
		}

		public static SubSpawnCondition InfoWallTile(int wallID)
			=> new((info) => info.WallTileType == wallID) { MetaData = nameof(InfoWallTile) + ":" + wallID };
		public static SubSpawnCondition WallTile(int wallID)
			=> new((info) => SpawnCondition.GetTile(info).wall == wallID) { MetaData = nameof(WallTile) + ":" + wallID };
		public static ConditionWrapper SpawnTile(params int[] tileIDs)
			=> new(CompareType.Or, tileIDs.Select<int, ISubSpawnCondition>((tileID) =>
				new SubSpawnCondition((info) => SpawnCondition.GetTile(info).type == tileID) { MetaData = nameof(SpawnTile) + ":" + tileID }));
		public static ConditionWrapper InfoSpawnTile(params int[] tileIDs)
			=> new(CompareType.Or, tileIDs.Select<int, ISubSpawnCondition>((tileID) =>
			new SubSpawnCondition((info) => info.SpawnTileType == tileID) { MetaData = nameof(InfoSpawnTile) + ":" + tileID }));
		public static ConditionWrapper InfoProperSpawnTile(params int[] tileIDs)
			=> new(CompareType.Or, tileIDs.Select<int, ISubSpawnCondition>((tileID) =>
			new SubSpawnCondition((info) => info.ProperGroundTileType == tileID) { MetaData = nameof(InfoProperSpawnTile) + ":" + tileID }));
	}
}

public readonly record struct ConditionWrapper(CompareType CurrentCompare, IEnumerable<ISubSpawnCondition> SpecificConditions) : ISubSpawnCondition
{
	public bool IsMet(NPCSpawnInfo info)
		=> CurrentCompare switch {
			CompareType.And => SpecificConditions.All((condition) => condition.IsMet(info)),
			CompareType.Or => SpecificConditions.Any((condition) => condition.IsMet(info)),
			_ => true
		};

	public static ConditionWrapper operator &(ConditionWrapper wrapper1, ConditionWrapper wrapper2)
		=> Operate(wrapper1, wrapper2, CompareType.And);

	public static ConditionWrapper operator |(ConditionWrapper wrapper1, ConditionWrapper wrapper2)
		=> Operate(wrapper1, wrapper2, CompareType.Or);

	//public static ConditionWrapper operator ^(ConditionWrapper wrapper1, ConditionWrapper wrapper2)
	//	=> Operate(wrapper1, wrapper2, CompareType.Xor);

	public static ConditionWrapper operator !(ConditionWrapper wrapper)
		=> wrapper.CurrentCompare switch {
			CompareType.And => new(CompareType.Or, wrapper.SpecificConditions.Select((ISubSpawnCondition) => ISubSpawnCondition.GetNot())),
			CompareType.Or => new(CompareType.And, wrapper.SpecificConditions.Select((ISubSpawnCondition) => ISubSpawnCondition.GetNot())),
			_ => throw new NotImplementedException("No valid not operation for: " + wrapper.CurrentCompare.ToString())
		};

	// AND => NOT AND => NOT (individual) OR
	//	 IF all true, true => If all true, false => If any false, false
	//	 !(A && B && C) == !A || !B || !C
	// OR => NOT OR => NOT (individual) AND:
	//	 If any true, true => If any true, false => If all false, false
	//	 !(A || B || C) == !A && !B && !C
	// To avoid multiple layers of not
	public ConditionWrapper GetNot() => !this;
	ISubSpawnCondition ISubSpawnCondition.GetNot() => GetNot();
	private static ConditionWrapper Operate(ConditionWrapper wrapper1, ConditionWrapper wrapper2, CompareType compareType)
	{
		int wrapper1Count = wrapper1.SpecificConditions.Count();
		int wrapper2Count = wrapper2.SpecificConditions.Count();
		if (wrapper1Count == 0)
			return wrapper2; // If one is empty ignore the assigned comparer and return the other
		if (wrapper2Count == 0)
			return wrapper1;
		//if (compareType == CompareType.Xor)
		//	return new ConditionWrapper(compareType, new ISubSpawnCondition[] { wrapper1, wrapper2 });
		if ((wrapper1.CurrentCompare == wrapper2.CurrentCompare && wrapper2.CurrentCompare == compareType)
			|| (wrapper1.CurrentCompare == compareType && wrapper2Count == 1) // If one has one element and the other matches the current comparetype
			|| (wrapper2.CurrentCompare == compareType && wrapper1Count == 1)) {
			return new ConditionWrapper(compareType, wrapper1.SpecificConditions.Concat(wrapper2.SpecificConditions));
		}
		return new ConditionWrapper(compareType, new ISubSpawnCondition[] { wrapper1, wrapper2 });
	}

	public static ConditionWrapper CreateConditional(ISubSpawnCondition condition, ISubSpawnCondition left, ISubSpawnCondition right)
		=> new(CompareType.Or, new ISubSpawnCondition[] {
			new ConditionWrapper(CompareType.And, new ISubSpawnCondition[] { condition, left }),
			new ConditionWrapper(CompareType.And, new ISubSpawnCondition[] { condition.GetNot(), right }),
		});
}

public readonly record struct EntrySumChance(IList<ISpawnTreeItem> Items)
{
	public EntrySumChance(params ISpawnTreeItem[] items) : this(items as IList<ISpawnTreeItem>)
	{ }
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

public interface IConditionedTreeItem : ISpawnTreeItem
{
	public ConditionWrapper Conditions { get; init; }

	public bool HasCondition(SubSpawnCondition condition)
		=> Conditions.SpecificConditions.Contains(condition);
}

public struct WeightedSpawnCondition : IConditionedTreeItem
{
	public ConditionWrapper Conditions { get; init; }
	private float Weight { get; init; }
	public float Chance { get; set; } = 0f;

	public WeightedSpawnCondition(float weight = 1f)
	{ Weight = weight; Conditions = new(CompareType.And, Array.Empty<ISubSpawnCondition>()); }

	public WeightedSpawnCondition(ConditionWrapper conditions, float weight = 1f) : this(weight)
	{ Conditions = conditions; }

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
	{ WeightFunc = weightFunc; Conditions = new(CompareType.And, Array.Empty<ISubSpawnCondition>()); }

	public CalculatedSpawnCondition(ConditionWrapper condition, Func<NPCSpawnInfo, float> weightFunc) : this(weightFunc)
	{ Conditions = condition; }

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
	public float BlockWeight { get; init; }
	public float Chance { get; set; } = 0f;

	public virtual void Reset()
	{
		Chance = 0f;
		foreach (ISpawnTreeItem child in Children) {
			child.Reset();
		}
	}

	public virtual void Check(NPCSpawnInfo info, ref float remainingWeight)
	{
		float childWeight = remainingWeight * BlockWeight;
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
		=> new(parent.Children.Append(child).ToArray());
}

public sealed class ConditionedSpawnTreeParent : SpawnTreeParent, IConditionedTreeItem
{
	public ConditionWrapper Conditions { get; init; }

	public ConditionedSpawnTreeParent(ConditionWrapper condition, float blockWeight, ISpawnTreeItem[] children) : base(blockWeight, children)
	{
		Conditions = condition;
	}

	public ConditionedSpawnTreeParent(ConditionWrapper condition, ISpawnTreeItem[] children) : this(condition, 1f, children)
	{ }

	public override void Check(NPCSpawnInfo info, ref float remainingWeight)
	{
		if (Conditions.IsMet(info))
			base.Check(info, ref remainingWeight);
	}
}

//TODO: further documentation
/// <summary>
/// This serves as a central class to help modders spawn their NPCs. It's basically the vanilla spawn code if-else chains condensed into objects. See ExampleMod for usages.
/// </summary>
public static class SpawnCondition
{
	internal static SpawnTreeParent baseCondition = new();

	public static readonly WeightedSpawnCondition NebulaTower; //1
	public static readonly WeightedSpawnCondition VortexTower; //2
	public static readonly WeightedSpawnCondition StardustTower; //3
	public static readonly WeightedSpawnCondition SolarTower; //4
	public static readonly WeightedSpawnCondition Sky; //5
	public static readonly ConditionedSpawnTreeParent Invasion; //6
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
	public static readonly ConditionedSpawnTreeParent Ocean; //14
	public static readonly WeightedSpawnCondition OceanAngler; //14, 15
	public static readonly EntrySumChance OceanCritter;
	public static readonly EntrySumChance OceanMonster;
	public static readonly WeightedSpawnCondition BeachAngler; //16
	public static readonly EntrySumChance Angler = new(BeachAngler, OceanAngler);
	public static readonly ConditionedSpawnTreeParent CaveOrJungleWater;
	public static readonly ConditionedSpawnTreeParent JungleWater;
	public static readonly WeightedSpawnCondition JungleWaterSurfaceCritter;
	public static readonly WeightedSpawnCondition JunglePiranha;
	public static readonly WeightedSpawnCondition CaveWater;
	public static WeightedSpawnCondition CavePiranha => CaveWater;
	public static readonly EntrySumChance Piranha = new(CavePiranha, JunglePiranha);
	public static readonly WeightedSpawnCondition CaveJellyfish;
	public static readonly ConditionedSpawnTreeParent WaterCritter;
	public static readonly WeightedSpawnCondition CorruptWaterCritter;
	public static readonly ConditionedSpawnTreeParent OverworldWaterCritter;
	public static readonly WeightedSpawnCondition OverworldWaterSurfaceCritter;
	public static readonly WeightedSpawnCondition OverworldUnderwaterCritter;
	public static readonly WeightedSpawnCondition DefaultWaterCritter;
	public static readonly CalculatedSpawnCondition BoundGoblin;
	public static readonly CalculatedSpawnCondition BoundWizard;
	public static readonly CalculatedSpawnCondition BoundOldShakingChest;
	public static readonly EntrySumChance BoundCaveNPC = new(BoundGoblin, BoundWizard, BoundOldShakingChest);
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
	[Obsolete("Does not correspond to a read vanilla NPC, to replicate the spawning of goldfish use TownDefaultWaterCritter, to replicate the spawning of pupfish use TownOverworldWaterBeachCritter.")]
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
	private static readonly WeightedSpawnCondition ArmouredVikingIcyMermanSkeletonArcherArmouredSkeleton;
	public static readonly WeightedSpawnCondition UndeadMiner;
	private static readonly ConditionedSpawnTreeParent UndeadVikingSnowFlinx;
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

	//public static readonly ConditionedSpawnTreeParent Gnome;
	public static readonly WeightedSpawnCondition Ghost;

	static SpawnCondition()
	{
		// Pillars
		baseCondition += NebulaTower = new(InTowerNebula);
		baseCondition += VortexTower = new(InTowerVortex);
		baseCondition += StardustTower = new(InTowerStardust);
		baseCondition += SolarTower = new(InTowerSolar);

		//Sky
		baseCondition += Sky = new(InfoSky);

		//Invasions
		baseCondition += Invasion = new(InfoInvasion, new ISpawnTreeItem[] {
			GoblinArmy = new(GoblinArmyCondition),
			FrostLegion = new(FrostLegionCondition),
			Pirates = new(PiratesCondition),
			MartianMadness = new(MartianMadnessCondition)
		});

		// Living Tree (Critters + Gnome)
		baseCondition += LivingTree = new(InfoWallTile(WallID.LivingWoodUnsafe) & RemixWorld);

		//Bartender
		baseCondition += Bartender = new(CanGetBartender & SpawnCap(NPCID.BartenderUnconscious, 1) & !InfoWater, 1f / 80f);

		// Caves
		baseCondition += SpiderCave = new(SpiderCaveCheck); //TODO: split to parts
		baseCondition += DesertCave = new(DesertCaveConditions & CheckUnderground);

		//Hardmode Water
		baseCondition += HardmodeJungleWater = new(HardMode & InfoWater & InJungle, 2f / 3f);
		baseCondition += HardmodeCrimsonWater = new(HardMode & InfoWater & InCrimson, 8f / 9f);

		//Ocean
		ConditionedSpawnTreeParent oceanCreature;
		OceanCritter = new(null, null, null);
		OceanMonster = new(null, null);

		baseCondition += Ocean = new((!InfoPlayerInTown | AnglerNotSavedOrSpawned) & InfoWater & InfoOcean, new ISpawnTreeItem[] {
			OceanAngler = new(AnglerNotSavedOrSpawned & OnWaterSurface & AnglerOceanSurface),
			oceanCreature = new(!InfoSafeRange, new ISpawnTreeItem[] {
				OceanCritter.Items[0] = new WeightedSpawnCondition(OnWaterSurfaceAvoidHousing, 0.01f),
				OceanCritter.Items[1] = new WeightedSpawnCondition(0.1f),
				OceanMonster.Items[0] = new WeightedSpawnCondition(1f / 40f), // Sea Snail
				OceanCritter.Items[2] = new WeightedSpawnCondition(1f / 18f), // Squid
				OceanMonster.Items[1] = new WeightedSpawnCondition()
			})
		});

		baseCondition += BeachAngler = new(!InfoWater & AnglerNotSavedOrSpawned & InBeachDistance & ProperGroundTileSand & (AboveWorldSurface | RemixWorld));

		//Misc Water
		baseCondition += CaveOrJungleWater = new(!InfoPlayerInTown & InfoWater, new ISpawnTreeItem[] {
			JungleWater = new(InfoSpawnTile(TileID.JungleGrass), new ISpawnTreeItem[] {
				JungleWaterSurfaceCritter = new(InfoOverWorld & BelowTile50 & TimeDay & OnWaterSurface, 1f / 3f),
				JunglePiranha = new()
			}),
			CaveWater = new(InfoCaverns, 0.5f)
		});
		baseCondition += CaveJellyfish = new(InfoPlayerInTown & InfoWater & !AboveWorldSurface, 1f / 3f);

		// Water Critters
		baseCondition += WaterCritter = new(InfoWater, 0.25f, new ISpawnTreeItem[] {
			CorruptWaterCritter = new(InCorrupt),
			OverworldWaterCritter = new(AboveWorldSurface & BelowTile50 & TimeDay, 2f / 3f, new ISpawnTreeItem[] {
				OverworldWaterSurfaceCritter = new(OnWaterSurface),
				OverworldUnderwaterCritter = new()
			}),
			DefaultWaterCritter = new()
		});

		// Bound NPCs
		baseCondition += BoundGoblin = new(BoundNPCBaseCondition & DownedGoblinArmy & GoblinNotSavedOrSpawned, GetPlayerRollWeightFunc(20));
		baseCondition += BoundWizard = new(BoundNPCBaseCondition & HardMode & WizardNotSavedOrSpawned, GetPlayerRollWeightFunc(20));
		baseCondition += BoundOldShakingChest = new(BoundNPCBaseCondition & DownedSkeletron & OldChestNotSavedOrSpawned, GetPlayerRollWeightFunc(20));

		// Town Critters
		baseCondition += TownCritter = new(InfoPlayerInTown, new ISpawnTreeItem[] {
			// Graveyard
			TownGraveyardCritter = new(InGraveyard, new ISpawnTreeItem[] {
				TownGraveyardWaterCritter = new(InfoWater) }),
			// Beach
			TownBeachCritter = new(!InfoSafeRange & InfoBeach,new ISpawnTreeItem[] {
				TownBeachWaterCritter = new(InfoWater) }),

			TownDragonFlyCritter = new(TimeDay & Raining & InfoSpawnTile(TileID.Grass, TileID.GolfGrass, TileID.Sand)  & !TooWindyForButterflies & (AboveWorldSurface | RemixWorld) & DragonflyCattailTop, 0.5f),

			// Water General
			TownWaterCritter = new(InfoWater, new ISpawnTreeItem[] {
				TownOverworldWaterCritter = new(InfoOverWorld & BelowTile50 & TimeDay, 2f / 3f, new ISpawnTreeItem[] {
					TownOverworldWaterSurfaceCritter = new(OnWaterSurface),
					TownOverworldWaterBeachCritter = new(InfoBeach),
					TownDefaultWaterCritter = new()
				}),
				TownOverworldWaterBeachCritter,
				TownDefaultWaterCritter
			}),
			TownSnowCritter = new(InfoSpawnTile(TileID.SnowBlock, TileID.IceBlock)),
			TownJungleCritter = new(InfoSpawnTile(TileID.JungleGrass)),
			TownDesertCritter = new(InfoSpawnTile(TileID.Sand)),
			TownGrassCritter = new(!AboveWorldSurface | InfoSpawnTile(TileID.Grass, TileID.GolfGrass, TileID.HallowedGrass, TileID.GolfGrassHallowed), new ISpawnTreeItem[] {
				TownRainingUnderGroundCritter = new(Raining & PlayerFloorInUnderworld, new ISpawnTreeItem[] {
					TownGemSquirrel = new(InfoCaverns, 0.2f),
					TownGemBunny = new(InfoCaverns, 0.2f),
					TownGeneralCritter = new()
				}),
				TownCritterGreenFairy = new(TimeDay & FairyCritter & InfoOverWorld, GetPlayerRollWeightFunc(2))})
		});

		// Dungeon
		baseCondition += Dungeon = new(InDungeon, new ISpawnTreeItem[] {
			DungeonGuardian = new(DownedSkeletron & DungeonGuardianHeightOrDrunk),
		DungeonNormal = new()
		});

		// Meteor
		baseCondition += Meteor = new(InMeteor);

		// Events
		baseCondition += OldOnesArmy = new(DungeonDefendersOngoing & InOldOneArmy);
		baseCondition += FrostMoon = new((RemixWorld | InfoSpawnTile(TileID.MushroomGrass)) & !TimeDay & FrostMoonInvasion);
		baseCondition += PumpkinMoon = new((RemixWorld | InfoSpawnTile(TileID.MushroomGrass)) & !TimeDay & PumpkinMoonInvasion);
		baseCondition += SolarEclipse = new((info) => ((Main.remixWorld && ) || info.SpawnTileY <= Main.worldSurface) & TimeDay & Eclipse);

		baseCondition += UndergroundFairy = new(UndergroundFairyCondition);

		// Mushroom
		baseCondition += HardmodeMushroomWater = new(HardMode & InfoSpawnTile(TileID.MushroomGrass) & InfoWater);
		baseCondition += OverworldMushroom = new(InfoSpawnTile(TileID.MushroomGrass) & AboveWorldSurface, 2f / 3f);
		baseCondition += UndergroundMushroom = new(InfoSpawnTile(TileID.MushroomGrass) & HardMode & !AboveWorldSurface, 2f / 3f);

		// Misc
		baseCondition += CorruptWorm = new(InCorrupt & !InfoPlayerSafe, 1f / 65f);
		baseCondition += UndergroundMimic = new(HardMode & !AboveWorldSurface, 1f / 70f);
		baseCondition += OverworldMimic = new(HardMode & WallTile(WallID.DirtUnsafe), 0.05f);
		baseCondition += Wraith = new(HardMode & AboveWorldSurface & TimeDay, (info) => 1f - (Main.moonPhase == 4 ? 0.8f * 0.9f : 0.95f));
		baseCondition += HoppinJack = new(HardMode & Halloween & AboveWorldSurface & !TimeDay, 0.1f);
		baseCondition += DoctorBones = new(InfoSpawnTile(TileID.JungleGrass) & TimeDay, GetPlayerRollWeightFunc(500));
		baseCondition += LacBeetle = new(InfoSpawnTile(TileID.JungleGrass) & !AboveWorldSurface, 1f / 60f);

		// Critters
		baseCondition += WormCritter = new(!AboveWorldSurface & CommonAboveHellHeightCheck & !InSnow & !InCrimson & !InCorrupt & !InJungle & !InHallow, 1f / 8f);
		baseCondition += MouseCritter = new(!AboveWorldSurface & CommonAboveHellHeightCheck & !InSnow & !InCrimson & !InCorrupt & !InJungle & !InHallow, 1f / 13f);
		baseCondition += SnailCritter = new(!AboveWorldSurface & IDontKnow & !InSnow & !InCrimson & !InCorrupt & !InHallow, 1f / 13f);
		baseCondition += JungleCritterBirdOrFrog = new(InfoOverWorld & InJungle & !InCrimson & !InCorrupt, 1f / 7f, new ISpawnTreeItem[] {
			JungleCritterBird = new(JungleBirdTime, 2f / 3f),
			FrogCritter = new()
		});

		baseCondition += Hive = new(InfoProperSpawnTile(TileID.Hive), 0.5f);

		// Jungle
		baseCondition += HardmodeJungle = new(InfoSpawnTile(TileID.JungleGrass) & HardMode, 2f / 3f);
		baseCondition += JungleTemple = new((InfoProperSpawnTile(TileID.LihzahrdBrick, TileID.WoodenSpikes) | RemixWorld) & InfoLihzahrd);
		baseCondition += HiveHornet = new(InfoWallTile(WallID.HiveUnsafe), 7f / 8f);
		baseCondition += UndergroundJungle = new(InfoSpawnTile(TileID.JungleGrass)
			& ConditionWrapper.CreateConditional(RemixWorld, AboveRockLayer, IDontKnow2), (info) => Main.remixWorld ? 0.5f : 1f);
		baseCondition += SurfaceJungle = new(InfoSpawnTile(TileID.JungleGrass), 11f / 32f);

		// Sandstorm
		baseCondition += SandstormEvent = new(SandstormHappening & InSandstorm
			& SubSpawnCondition.Tile.Conversion.Sand /*TODO: Check sand was right*/ & Spawning_SandstoneCheck);

		// Mummy
		baseCondition += Mummy = new(HardMode & InfoSpawnTile(TileID.Sand), 1f / 3f);
		baseCondition += DarkMummy = new(HardMode & InfoSpawnTile(TileID.Ebonsand), 0.5f);
		baseCondition += BloodMummy = new(HardMode & InfoSpawnTile(TileID.Crimsand), 0.5f);
		baseCondition += LightMummy = new(HardMode & InfoSpawnTile(TileID.Pearlsand), 0.5f);

		// Hallow
		baseCondition += OverworldHallow = new(HardMode & !InfoWater & InfoUnderGround & InfoProperSpawnTile(TileID.Pearlsand,
			TileID.Pearlstone, TileID.HallowedGrass, TileID.HallowedIce));
		baseCondition += EnchantedSword = new(!InfoPlayerSafe & HardMode & !InfoWater & InfoCaverns & InfoProperSpawnTile(TileID.Pearlsand,
			TileID.Pearlstone, TileID.HallowedGrass, TileID.HallowedIce), 0.02f);

		// Crimson
		baseCondition += Crimson = new((InfoProperSpawnTile(TileID.Crimtane) & InCrimson)
			| InfoProperSpawnTile(TileID.CrimsonGrass, TileID.FleshIce, TileID.Crimstone, TileID.Crimsand, TileID.CrimsonJungleGrass));

		// Corruption
		baseCondition += Corruption = new((InfoProperSpawnTile(TileID.Demonite) & InCorrupt)
			| InfoProperSpawnTile(TileID.CorruptGrass, TileID.CorruptIce, TileID.Ebonstone, TileID.Ebonsand, TileID.CorruptJungleGrass));

		// Overworld
		baseCondition += Overworld = new(InfoOverWorld, new ISpawnTreeItem[] {
			// Overworld Misc
			IceGolem = new(InSnow & HardMode & CloudAlphaAbove(0f) & SpawnCap(NPCID.IceGolem), 0.05f),
			RainbowSlime = new(InHallow & HardMode & CloudAlphaAbove(0f) & SpawnCap(NPCID.RainbowSlime), 0.05f),
			AngryNimbus = new(!InSnow & HardMode & CloudAlphaAbove(0f) & SpawnCap(NPCID.AngryNimbus, 2), 0.1f),
			MartianProbe = new(MartianProbePosition & HardMode & DownedGolem & SpawnCap(NPCID.MartianProbe),
				(info) => 1f - (!NPC.downedMartians ? (399f / 400f) * 0.99f : (399f / 400f))),

			// Overworld Typical
			OverworldDay = new(TimeDay, new ISpawnTreeItem[] {
				OverworldDaySnowCritter = new(InfoInnerThird & SpawnTile(TileID.SnowBlock, TileID.IceBlock), 1f / 15f),
				OverworldDayGrassCritter = new(InfoInnerThird & SpawnTile(TileID.Grass, TileID.HallowedGrass), 1f / 15f),
				OverworldDaySandCritter = new(InfoInnerThird & SpawnTile(TileID.Sand), 1f / 15f),
				OverworldMorningBirdCritter = new(InfoInnerThird & TimeLessThan(18000.0) & SpawnTile(TileID.Grass, TileID.HallowedGrass), 0.25f),
				OverworldDayBirdCritter = new(InfoInnerThird & SpawnTile(TileID.Grass, TileID.HallowedGrass, TileID.SnowBlock), 1f / 15f),

				// King Slime (Overworld)
				KingSlime = new(InfoOuterThird & SpawnTile(TileID.Grass) & SpawnCap(NPCID.KingSlime), 1f / 300f),

				OverworldDayDesert = new(SpawnTile(TileID.Sand) & InfoWater, 0.2f),

				// Overworld Goblin Scout
				GoblinScout = new(InfoOuterThird, (info) => 1f - ((!NPC.downedGoblins && WorldGen.shadowOrbSmashed) ? (6f / 7f) * (14f / 15f) : (14f / 15f))),

				// Overworld Typical
				OverworldDayRain = new(Raining, 2f / 3f),
				OverworldDaySlime = new()
			}),
			OverworldNight = new(new ISpawnTreeItem[] {
				OverworldFirefly = new(SpawnTile(TileID.Grass, TileID.HallowedGrass), (info) => 1f / NPC.fireFlyChance),
				OverworldNightMonster = new()
			})
		});

		// Underground
		baseCondition += Underground = new(InfoUnderGround);
		baseCondition += Underworld = new(UnderworldHeight);

		// Caverns

		baseCondition += Cavern = new(new ISpawnTreeItem[] {
			RockGolem = new(RockGolemCondition),
			DyeBeetle = new(1f / 60f),
			ChaosElemental = new(HardMode & !InfoPlayerSafe & InfoProperSpawnTile(TileID.Pearlsand, TileID.Pearlstone, TileID.HallowedIce), 1f / 8f),

			Pigron = new(SpawnTile(TileID.SnowBlock, TileID.IceBlock, TileID.BreakableIce, TileID.CorruptIce, TileID.HallowedIce, TileID.FleshIce)
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
			ArmouredVikingIcyMermanSkeletonArcherArmouredSkeleton = new(HardMode, 0.9f),
			Ghost = new(!InfoPlayerSafe & (Halloween | InGraveyard), 1f / 30f),
			UndeadMiner = new(1f / 20f),
			UndeadVikingSnowFlinx = new(InfoSpawnTile(TileID.SnowBlock, TileID.IceBlock, TileID.BreakableIce),new ISpawnTreeItem[] {
				Flinx3 = new(1f / 15f),
				UndeadViking = new()
			}),
			Flinx4 = new(InSnow),
			GenericCavernMonster = new(1f / 3f),
			SporeSkeletons = new(InGlowshroom & InfoSpawnTile(TileID.MushroomGrass, TileID.MushroomBlock)),
			HalloweenSkeletons = new(Halloween, 0.5f),
			ExpertSkeletons = new(ExpertMode, 1f / 3f),
			NormalSkeletons = new()
		});
	}

	/// <summary>
	/// Shorthand for "Main.tile[info.SpawnTileX, info.SpawnTileY]"
	/// </summary>
	public static Tile GetTile(NPCSpawnInfo info)
		=> Main.tile[info.SpawnTileX, info.SpawnTileY];

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