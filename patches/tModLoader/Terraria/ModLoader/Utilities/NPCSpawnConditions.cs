using System;
using System.Collections.Generic;
using System.Linq;
using MonoMod.Utils;
using Terraria.GameContent.Events;
using Terraria.ID;
using static Terraria.ModLoader.Utilities.SubSpawnCondition.Common;
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

	public static IEnumerable<ISubSpawnCondition> operator +(ISubSpawnCondition condition1, ISubSpawnCondition condition2)
		=> new ISubSpawnCondition[] { condition1, condition2 };

	public static IEnumerable<ISubSpawnCondition> operator +(ISubSpawnCondition condition)
		=> new ISubSpawnCondition[] { condition };
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
		}
		: condition.Predicate.CastDelegate<Func<NPCSpawnInfo, bool>>();
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

	public static ConditionWrapper operator &(SubSpawnCondition condition1, SubSpawnCondition condition2)
		=> new(CompareType.And, condition1 + (condition2 as ISubSpawnCondition));
	public static ConditionWrapper operator |(SubSpawnCondition condition1, SubSpawnCondition condition2)
		=> new(CompareType.Or, condition1 + (condition2 as ISubSpawnCondition));

	public static implicit operator ConditionWrapper(SubSpawnCondition condition)
		=> new(CompareType.And, +(condition as ISubSpawnCondition));
	public static class Zone
	{
		public static readonly SubSpawnCondition InDungeon = FromCondition(Condition.InDungeon, true);
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

		public static readonly SubSpawnCondition DesertCaveWallCheck = new((info) => NPC.SpawnTileOrAboveHasAnyWallInSet(info.SpawnTileX, info.SpawnTileY, WallID.Sets.AllowsUndergroundDesertEnemiesToSpawn));
		public static readonly SubSpawnCondition CanGetBartender = new((info) => !NPC.savedBartender && DD2Event.ReadyToFindBartender);

		public static readonly SubSpawnCondition SavedAngler = new((info) => NPC.savedAngler);
		public static readonly SubSpawnCondition AnglerOceanSurface = new((info) => (info.SpawnTileY < Main.worldSurface - 10.0 || Main.remixWorld));

		/// <summary> Shorthand for: !InfoWater &amp; InfoCaverns &amp; CommonAboveHellHeightCheck </summary>
		public static readonly ConditionWrapper BoundNPCBaseCondition = !InfoWater & InfoCaverns & CommonAboveHellHeightCheck;
		public static readonly SubSpawnCondition WizardNotSavedOrSpawned = new((info) => !NPC.savedWizard && !NPC.AnyNPCs(NPCID.BoundWizard));
		public static readonly SubSpawnCondition GoblinNotSavedOrSpawned = new((info) => !NPC.savedGoblin && !NPC.AnyNPCs(NPCID.BoundGoblin));
		public static readonly SubSpawnCondition OldChestNotSavedOrSpawned = new((info) => !NPC.unlockedSlimeOldSpawn && !NPC.AnyNPCs(NPCID.BoundTownSlimeOld));

		public static readonly SubSpawnCondition GnomeSurfaceCheck = new((info) => info.SpawnTileY >= (Main.worldSurface * 0.800000011920929) & info.SpawnTileY < (Main.worldSurface * 1.100000023841858));

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
			=> new((info) => Main.time < time) { MetaData = nameof(TimeLessThan) + ":" + time };

		public static readonly SubSpawnCondition Raining = new((info) => Main.raining);
		public static readonly SubSpawnCondition SandstormHappening = new((info) => Sandstorm.Happening);

		public static readonly SubSpawnCondition HardMode = new((info) => Main.hardMode);
		public static readonly SubSpawnCondition ExpertMode = new((info) => Main.expertMode);
		public static readonly SubSpawnCondition MasterMode = new((info) => Main.masterMode);
		public static readonly SubSpawnCondition RemixWorld = new((info) => Main.remixWorld);
		public static readonly SubSpawnCondition GetGoodWorld = new((info) => Main.getGoodWorld);

		public static readonly SubSpawnCondition TooWindyForButterflies = new((info) => NPC.TooWindyForButterflies);

		public static readonly SubSpawnCondition InfoWater = new((info) => info.Water);
		public static readonly SubSpawnCondition OnWaterSurface = new(SpawnCondition.WaterSurface);
		public static readonly SubSpawnCondition OnWaterSurfaceAvoidHousing = new((info) => SpawnCondition.WaterSurfaceAvoidHousing(info, 2, 50, 3));

		/// <summary> <see cref="NPCSpawnInfo.SpawnTileY"/> &lt;= <see cref="Main.worldSurface"/> </summary>
		public static readonly SubSpawnCondition AboveOrWorldSurface = new((info) => info.SpawnTileY <= Main.worldSurface);

		/// <summary> <see cref="NPCSpawnInfo.SpawnTileY"/> &lt; <see cref="Main.worldSurface"/> </summary>
		public static readonly SubSpawnCondition AboveWorldSurface = new((info) => info.SpawnTileY < Main.worldSurface);

		/// <summary> <see cref="NPCSpawnInfo.SpawnTileY"/> &gt; <see cref="Main.rockLayer"/> </summary>
		public static readonly SubSpawnCondition BelowRockLayer = new((info) => info.SpawnTileY < Main.worldSurface);
		public static readonly SubSpawnCondition BelowOrRockLayer = new((info) => info.SpawnTileY <= Main.worldSurface);

		/// <summary> Shorthand for: AboveTileFromFloor(210) </summary>
		public static readonly SubSpawnCondition CommonAboveHellHeightCheck = AboveTileFromFloor(210);
		public static readonly SubSpawnCondition UnderworldHeight = BelowTileFromFloor(190);

		public static readonly SubSpawnCondition InBeachDistance = new((info) => info.SpawnTileX < WorldGen.beachDistance || info.SpawnTileX > Main.maxTilesX - WorldGen.beachDistance);
		public static readonly SubSpawnCondition NotInOceanDistance = new((info) => info.SpawnTileX > WorldGen.oceanDistance && info.SpawnTileX < Main.maxTilesX - WorldGen.oceanDistance);
		public static readonly SubSpawnCondition InfoInnerThird = new(SpawnCondition.InnerThird);
		public static readonly SubSpawnCondition InfoOuterThird = new(SpawnCondition.OuterThird);
		public static readonly SubSpawnCondition ProperGroundSand = new((info) => Main.tileSand[info.ProperGroundTileType]);

		public static readonly SubSpawnCondition InfoInvasion = new((info) => info.Invasion);
		public static readonly SubSpawnCondition Halloween = new((info) => Main.halloween);
		public static readonly SubSpawnCondition FrostMoonInvasion = new((info) => Main.snowMoon);
		public static readonly SubSpawnCondition PumpkinMoonInvasion = new((info) => Main.pumpkinMoon);
		public static readonly SubSpawnCondition Eclipse = new((info) => Main.eclipse);
		public static readonly SubSpawnCondition BloodMoon = new((info) => Main.bloodMoon);

		/// <summary> flag20 </summary>
		public static readonly SubSpawnCondition InfoOverWorld = new((info) => info.OverWorld);
		public static readonly SubSpawnCondition InfoUnderGround = new((info) => info.UnderGround);
		public static readonly SubSpawnCondition InfoMarble = new((info) => info.Marble);
		public static readonly SubSpawnCondition InfoGranite = new((info) => info.Granite);

		/// <summary> flag23 </summary>
		/// //TODO: Probably can be moved out of SpawnInfo
		public static readonly SubSpawnCondition InfoBeach = new((info) => info.Beach);
		public static readonly SubSpawnCondition InfoOcean = new((info) => info.Ocean);
		public static readonly SubSpawnCondition InfoSky = new((info) => info.Sky);
		/// <summary> flag21 </summary>
		public static readonly SubSpawnCondition InfoCaverns = new((info) => info.Caverns);
		public static readonly SubSpawnCondition InfoLihzahrd = new((info) => info.Lihzahrd);
		public static readonly SubSpawnCondition InfoSpider = new((info) => info.SpiderCave);
		public static readonly SubSpawnCondition InfoDesertCave = new((info) => info.DesertCave);

		public static readonly SubSpawnCondition InfoSafeRange = new((info) => info.SafeRangeX);
		public static readonly SubSpawnCondition InfoPlayerSafe = new((info) => info.PlayerSafe);

		public static readonly SubSpawnCondition WorldGenCheckUnderground = new((info) => WorldGen.checkUnderground(info.SpawnTileX, info.SpawnTileY));

		public static SubSpawnCondition CloudAlphaAbove(float amount)
			=> new((info) => Main.cloudAlpha > amount) { MetaData = nameof(CloudAlphaAbove) + ":" + amount };
	}
	public static class Tile
	{
		/// <summary> wrap items from <see cref="TileID.Sets.Conversion"/> </summary>
		public static class Conversion
		{
			public static readonly SubSpawnCondition Stone = new((info) => TileID.Sets.Conversion.Stone[info.ProperGroundTileType]);
			public static readonly SubSpawnCondition Grass = new((info) => TileID.Sets.Conversion.Grass[info.ProperGroundTileType]);
			public static readonly SubSpawnCondition GolfGrass = new((info) => TileID.Sets.Conversion.GolfGrass[info.ProperGroundTileType]);
			public static readonly SubSpawnCondition JungleGrass = new((info) => TileID.Sets.Conversion.JungleGrass[info.ProperGroundTileType]);
			public static readonly SubSpawnCondition Snow = new((info) => TileID.Sets.Conversion.Snow[info.ProperGroundTileType]);
			public static readonly SubSpawnCondition Ice = new((info) => TileID.Sets.Conversion.Ice[info.ProperGroundTileType]);
			public static readonly SubSpawnCondition Sand = new((info) => TileID.Sets.Conversion.Sand[info.ProperGroundTileType]);
			public static readonly SubSpawnCondition HardenedSand = new((info) => TileID.Sets.Conversion.HardenedSand[info.ProperGroundTileType]);
			public static readonly SubSpawnCondition SandStone = new((info) => TileID.Sets.Conversion.Sandstone[info.ProperGroundTileType]);
			public static readonly SubSpawnCondition Moss = new((info) => TileID.Sets.Conversion.Moss[info.ProperGroundTileType]);
		}

		public static readonly SubSpawnCondition HasTrueWallTile = new((info) => Main.tile[info.SpawnTileX, info.SpawnTileY].wall > 0);

		#region TileType

		public static ConditionWrapper InfoWallTile(params int[] wallIDs)
			=> new(CompareType.Or, wallIDs.Select<int, ISubSpawnCondition>((wallID) =>
				new SubSpawnCondition((info) => info.WallTileType == wallID) { MetaData = nameof(InfoWallTile) + ":" + wallID }));

		public static ConditionWrapper TrueWallTile(params int[] wallIDs)
			=> new(CompareType.Or, wallIDs.Select<int, ISubSpawnCondition>((wallID) =>
				new SubSpawnCondition((info) => info.GetTile().wall == wallID) { MetaData = nameof(TrueWallTile) + ":" + wallID }));

		/// <summary> GetTile(num, num2) </summary>
		public static ConditionWrapper TrueSpawnTile(params int[] tileIDs)
			=> new(CompareType.Or, tileIDs.Select<int, ISubSpawnCondition>((tileID) =>
				new SubSpawnCondition((info) => info.GetTile().type == tileID) { MetaData = nameof(TrueSpawnTile) + ":" + tileID }));
		/// <summary> num3 </summary>
		public static ConditionWrapper SpawnTile(params int[] tileIDs)
			=> new(CompareType.Or, tileIDs.Select<int, ISubSpawnCondition>((tileID) =>
				new SubSpawnCondition((info) => info.SpawnTileType == tileID) { MetaData = nameof(SpawnTile) + ":" + tileID }));
		/// <summary> num49 </summary>
		public static ConditionWrapper ProperGroundSpawnTile(params int[] tileIDs)
			=> new(CompareType.Or, tileIDs.Select<int, ISubSpawnCondition>((tileID) =>
				new SubSpawnCondition((info) => info.ProperGroundTileType == tileID) { MetaData = nameof(ProperGroundSpawnTile) + ":" + tileID }));

		public static ConditionWrapper PlayerCenterSpawnTile(params int[] tileIDs)
			=> new(CompareType.Or, tileIDs.Select<int, ISubSpawnCondition>((tileID) =>
				new SubSpawnCondition((info) => info.GetPlayerCentreTile().wall == tileID) { MetaData = nameof(ProperGroundSpawnTile) + ":" + tileID }));

		#endregion TileType

		#region TileHeight

		/// <summary> Checks the spawn tile is at least <paramref name="depth"/> below the world's roof </summary>
		public static SubSpawnCondition BelowTile(int depth)
			=> new((info) => info.SpawnTileY > depth) { MetaData = nameof(BelowTile) + ":" + depth };

		/// <summary> Checks the spawn tile is at least <paramref name="amount"/> below the world's surface </summary>
		public static SubSpawnCondition BelowSurfaceBy(int amount)
			=> new((info) => info.SpawnTileY > Main.worldSurface + amount) { MetaData = nameof(BelowSurfaceBy) + ":" + amount };

		/// <summary> Checks the spawn tile is between the world's floor and <paramref name="height"/> from the floor </summary>
		public static SubSpawnCondition BelowTileFromFloor(int height, bool inclusive = false)
		{
			if (inclusive)
				height -= 1;
			return new((info) => info.SpawnTileY > Main.maxTilesY - height) { MetaData = nameof(BelowTileFromFloor) + ":" + height };
		}

		public static SubSpawnCondition AboveTileFromFloor(int height, bool inclusive = false)
			=> !BelowTileFromFloor(height, !inclusive);

		#endregion TileHeight
	}
}

public record class ConditionWrapper(CompareType CurrentCompare, IEnumerable<ISubSpawnCondition> SpecificConditions) : ISubSpawnCondition
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

	// AND => NOT AND => NOT (individual) OR IF all true, true => If all true, false => If any false, false !(A && B && C) == !A || !B || !C OR => NOT OR => NOT
	// (individual) AND: If any true, true => If any true, false => If all false, false !(A || B || C) == !A && !B && !C To avoid multiple layers of not
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
			|| (wrapper1.CurrentCompare == compareType && wrapper2Count == 1) // If one has one element and the other matches the current CompareType
			|| (wrapper2.CurrentCompare == compareType && wrapper1Count == 1)) {
			return new ConditionWrapper(compareType, wrapper1.SpecificConditions.Concat(wrapper2.SpecificConditions));
		}
		return new ConditionWrapper(compareType, wrapper1 + (wrapper2 as ISubSpawnCondition));
	}

	public static ConditionWrapper CreateConditional(ISubSpawnCondition condition, ISubSpawnCondition whenTrue, ISubSpawnCondition whenFalse)
		=> new(CompareType.Or, new ISubSpawnCondition[] {
			new ConditionWrapper(CompareType.And, condition+ whenTrue ),
			new ConditionWrapper(CompareType.And, condition.GetNot() + whenFalse ),
		});
}