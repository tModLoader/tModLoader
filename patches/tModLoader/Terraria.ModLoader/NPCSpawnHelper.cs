using System;
using System.Collections.Generic;
using Terraria.GameContent.Events;
using Terraria.ID;

namespace Terraria.ModLoader
{
	//todo: further documentation
	/// <summary>
	/// This server as a central class to help modders spawn their npcs. It's basically the vanilla spawn code if-else chains condensed into objects. See ExampleMod for usages.
	/// </summary>
	public static class NPCSpawnHelper
	{
		internal static List<SpawnCondition> conditions = new List<SpawnCondition>();

		internal static void Reset() {
			foreach (SpawnCondition cond in conditions) {
				cond.Reset();
			}
		}

		internal static void DoChecks(NPCSpawnInfo info) {
			float weight = 1f;
			foreach (SpawnCondition cond in conditions) {
				cond.Check(info, ref weight);
				if (Math.Abs(weight) < 5E-6) {
					break;
				}
			}
		}
	}

	//todo: further documentation
	public class SpawnCondition
	{
		private Func<NPCSpawnInfo, bool> condition;
		private List<SpawnCondition> children;
		private float blockWeight;
		internal Func<float> WeightFunc;

		private float chance;
		private bool active;

		internal IEnumerable<SpawnCondition> Children => children;
		internal float BlockWeight => blockWeight;

		public float Chance => chance;
		public bool Active => active;

		internal SpawnCondition(Func<NPCSpawnInfo, bool> condition, float blockWeight = 1f) {
			this.condition = condition;
			this.children = new List<SpawnCondition>();
			this.blockWeight = blockWeight;
			NPCSpawnHelper.conditions.Add(this);
		}

		internal SpawnCondition(SpawnCondition parent, Func<NPCSpawnInfo, bool> condition, float blockWeight = 1f) {
			this.condition = condition;
			this.children = new List<SpawnCondition>();
			this.blockWeight = blockWeight;
			parent.children.Add(this);
		}

		internal void Reset() {
			chance = 0f;
			active = false;
			foreach (SpawnCondition child in children) {
				child.Reset();
			}
		}

		internal void Check(NPCSpawnInfo info, ref float remainingWeight) {
			if (WeightFunc != null) {
				blockWeight = WeightFunc();
			}
			active = true;
			if (condition(info)) {
				chance = remainingWeight * blockWeight;
				float childWeight = chance;
				foreach (SpawnCondition child in children) {
					child.Check(info, ref childWeight);
					if (Math.Abs(childWeight) < 5E-6) {
						break;
					}
				}
				remainingWeight -= chance;
			}
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
		public static readonly SpawnCondition JungleWater;
		public static readonly SpawnCondition CavePiranha;
		public static readonly SpawnCondition CaveJellyfish;
		public static readonly SpawnCondition WaterCritter;
		public static readonly SpawnCondition CorruptWaterCritter;
		public static readonly SpawnCondition OverworldWaterCritter;
		public static readonly SpawnCondition OverworldWaterSurfaceCritter;
		public static readonly SpawnCondition OverworldUnderwaterCritter;
		public static readonly SpawnCondition DefaultWaterCritter;
		public static readonly SpawnCondition BoundCaveNPC;
		public static readonly SpawnCondition TownCritter;
		public static readonly SpawnCondition TownWaterCritter;
		public static readonly SpawnCondition TownOverworldWaterCritter;
		public static readonly SpawnCondition TownOverworldWaterSurfaceCritter;
		public static readonly SpawnCondition TownOverworldUnderwaterCritter;
		public static readonly SpawnCondition TownDefaultWaterCritter;
		public static readonly SpawnCondition TownSnowCritter;
		public static readonly SpawnCondition TownJungleCritter;
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
		public static readonly SpawnCondition FrogCritter;
		public static readonly SpawnCondition HardmodeJungle;
		public static readonly SpawnCondition JungleTemple;
		public static readonly SpawnCondition UndergroundJungle;
		public static readonly SpawnCondition SurfaceJungle;
		public static readonly SpawnCondition SandstormEvent;
		public static readonly SpawnCondition Mummy;
		public static readonly SpawnCondition DarkMummy;
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

		static SpawnCondition() {
			NebulaTower = new SpawnCondition((info) => info.player.ZoneTowerNebula);
			VortexTower = new SpawnCondition((info) => info.player.ZoneTowerVortex);
			StardustTower = new SpawnCondition((info) => info.player.ZoneTowerStardust);
			SolarTower = new SpawnCondition((info) => info.player.ZoneTowerSolar);
			Sky = new SpawnCondition((info) => info.sky);
			Invasion = new SpawnCondition((info) => info.invasion);
			GoblinArmy = new SpawnCondition(Invasion, (info) => Main.invasionType == 1);
			FrostLegion = new SpawnCondition(Invasion, (info) => Main.invasionType == 2);
			Pirates = new SpawnCondition(Invasion, (info) => Main.invasionType == 3);
			MartianMadness = new SpawnCondition(Invasion, (info) => Main.invasionType == 4);
			Bartender = new SpawnCondition((info) => !NPC.savedBartender && DD2Event.ReadyToFindBartender
				&& !NPC.AnyNPCs(NPCID.BartenderUnconscious) && !info.water, 1f / 80f);
			SpiderCave = new SpawnCondition((info) => GetTile(info).wall == WallID.SpiderUnsafe || info.spiderCave);
			DesertCave = new SpawnCondition((info) => (WallID.Sets.Conversion.HardenedSand[GetTile(info).wall]
				|| WallID.Sets.Conversion.Sandstone[GetTile(info).wall] || info.desertCave)
				&& WorldGen.checkUnderground(info.spawnTileX, info.spawnTileY));
			HardmodeJungleWater = new SpawnCondition((info) => Main.hardMode && info.water && info.player.ZoneJungle, 2f / 3f);
			HardmodeCrimsonWater = new SpawnCondition((info) => Main.hardMode && info.water && info.player.ZoneCrimson, 8f / 9f);
			Ocean = new SpawnCondition((info) => info.water && (info.spawnTileX < 250 || info.spawnTileX > Main.maxTilesX - 250)
				&& Main.tileSand[info.spawnTileType] && info.spawnTileY < Main.rockLayer);
			OceanAngler = new SpawnCondition(Ocean, (info) => !NPC.savedAngler && !NPC.AnyNPCs(NPCID.SleepingAngler)
				&& WaterSurface(info));
			OceanMonster = new SpawnCondition(Ocean, (info) => true);
			BeachAngler = new SpawnCondition((info) => !info.water && !NPC.savedAngler && !NPC.AnyNPCs(NPCID.SleepingAngler)
				&& (info.spawnTileX < 340 || info.spawnTileX > Main.maxTilesX - 340) && Main.tileSand[info.spawnTileType]
				&& info.spawnTileY < Main.worldSurface);
			JungleWater = new SpawnCondition((info) => info.water && info.spawnTileType == TileID.JungleGrass);
			CavePiranha = new SpawnCondition((info) => info.water && info.spawnTileY > Main.rockLayer, 0.5f);
			CaveJellyfish = new SpawnCondition((info) => info.water && info.spawnTileY > Main.worldSurface, 1f / 3f);
			WaterCritter = new SpawnCondition((info) => info.water, 0.25f);
			CorruptWaterCritter = new SpawnCondition(WaterCritter, (info) => info.player.ZoneCorrupt);
			OverworldWaterCritter = new SpawnCondition(WaterCritter, (info) => info.spawnTileY < Main.worldSurface
				&& info.spawnTileY > 50 && Main.dayTime, 2f / 3f);
			OverworldWaterSurfaceCritter = new SpawnCondition(OverworldWaterCritter, WaterSurface);
			OverworldUnderwaterCritter = new SpawnCondition(OverworldWaterCritter, (info) => true);
			DefaultWaterCritter = new SpawnCondition(WaterCritter, (info) => true);
			BoundCaveNPC = new SpawnCondition((info) => !info.water && info.spawnTileY >= Main.rockLayer
				&& info.spawnTileY < Main.maxTilesY - 210, 1f / 20f);
			TownCritter = new SpawnCondition((info) => info.playerInTown);
			TownWaterCritter = new SpawnCondition(TownCritter, (info) => info.water);
			TownOverworldWaterCritter = new SpawnCondition(TownWaterCritter, (info) => info.spawnTileY < Main.worldSurface
				&& info.spawnTileY > 50 && Main.dayTime, 2f / 3f);
			TownOverworldWaterSurfaceCritter = new SpawnCondition(TownOverworldWaterCritter, WaterSurface);
			TownOverworldUnderwaterCritter = new SpawnCondition(TownOverworldWaterCritter, (info) => true);
			TownDefaultWaterCritter = new SpawnCondition(TownWaterCritter, (info) => true);
			TownSnowCritter = new SpawnCondition(TownCritter, (info) => info.spawnTileType == TileID.SnowBlock
				|| info.spawnTileType == TileID.IceBlock);
			TownJungleCritter = new SpawnCondition(TownCritter, (info) => info.spawnTileType == TileID.JungleGrass);
			TownGeneralCritter = new SpawnCondition(TownCritter, (info) => info.spawnTileType == TileID.Grass
				|| info.spawnTileType == TileID.HallowedGrass || info.spawnTileY > Main.worldSurface);
			Dungeon = new SpawnCondition((info) => info.player.ZoneDungeon);
			DungeonGuardian = new SpawnCondition(Dungeon, (info) => !NPC.downedBoss3);
			DungeonNormal = new SpawnCondition(Dungeon, (info) => true);
			Meteor = new SpawnCondition((info) => info.player.ZoneMeteor);
			OldOnesArmy = new SpawnCondition((info) => DD2Event.Ongoing && info.player.ZoneOldOneArmy);
			FrostMoon = new SpawnCondition((info) => info.spawnTileY <= Main.worldSurface && !Main.dayTime && Main.snowMoon);
			PumpkinMoon = new SpawnCondition((info) => info.spawnTileY <= Main.worldSurface
				&& !Main.dayTime && Main.pumpkinMoon);
			SolarEclipse = new SpawnCondition((info) => info.spawnTileY <= Main.worldSurface && Main.dayTime && Main.eclipse);
			HardmodeMushroomWater = new SpawnCondition((info) => Main.hardMode && info.spawnTileType == TileID.MushroomGrass
				&& info.water);
			OverworldMushroom = new SpawnCondition((info) => info.spawnTileType == TileID.MushroomGrass
				&& info.spawnTileY <= Main.worldSurface, 2f / 3f);
			UndergroundMushroom = new SpawnCondition((info) => info.spawnTileType == TileID.MushroomGrass
				&& Main.hardMode && info.spawnTileY >= Main.worldSurface, 2f / 3f);
			CorruptWorm = new SpawnCondition((info) => info.player.ZoneCorrupt && !info.playerSafe, 1f / 65f);
			UndergroundMimic = new SpawnCondition((info) => Main.hardMode && info.spawnTileY > Main.worldSurface, 1f / 70f);
			OverworldMimic = new SpawnCondition((info) => Main.hardMode && GetTile(info).wall == WallID.DirtUnsafe, 0.05f);
			Wraith = new SpawnCondition((info) => Main.hardMode && info.spawnTileY <= Main.worldSurface
				&& !Main.dayTime, 0.05f);
			Wraith.WeightFunc = () => {
				float inverseChance = 0.95f;
				if (Main.moonPhase == 4) {
					inverseChance *= 0.8f;
				}
				return 1f - inverseChance;
			};
			HoppinJack = new SpawnCondition((info) => Main.hardMode && Main.halloween
				&& info.spawnTileY <= Main.worldSurface && !Main.dayTime, 0.1f);
			DoctorBones = new SpawnCondition((info) => info.spawnTileType == TileID.JungleGrass && !Main.dayTime, 0.002f);
			LacBeetle = new SpawnCondition((info) => info.spawnTileType == TileID.JungleGrass
				&& info.spawnTileY > Main.worldSurface, 1f / 60f);
			WormCritter = new SpawnCondition((info) => info.spawnTileY > Main.worldSurface
				&& info.spawnTileY < Main.maxTilesY - 210 && !info.player.ZoneSnow && !info.player.ZoneCrimson
				&& !info.player.ZoneCorrupt && !info.player.ZoneJungle && !info.player.ZoneHoly, 1f / 8f);
			MouseCritter = new SpawnCondition((info) => info.spawnTileY > Main.worldSurface
				&& info.spawnTileY < Main.maxTilesY - 210 && !info.player.ZoneSnow && !info.player.ZoneCrimson
				&& !info.player.ZoneCorrupt && !info.player.ZoneJungle && !info.player.ZoneHoly, 1f / 13f);
			SnailCritter = new SpawnCondition((info) => info.spawnTileY > Main.worldSurface
				&& info.spawnTileY < (Main.rockLayer + Main.maxTilesY) / 2 && !info.player.ZoneSnow
				&& !info.player.ZoneCrimson && !info.player.ZoneCorrupt && !info.player.ZoneHoly, 1f / 13f);
			FrogCritter = new SpawnCondition((info) => info.spawnTileY < Main.worldSurface && info.player.ZoneJungle, 1f / 9f);
			HardmodeJungle = new SpawnCondition((info) => info.spawnTileType == TileID.JungleGrass && Main.hardMode, 2f / 3f);
			JungleTemple = new SpawnCondition((info) => info.spawnTileType == TileID.LihzahrdBrick && info.lihzahrd);
			UndergroundJungle = new SpawnCondition((info) => info.spawnTileType == TileID.JungleGrass
				&& info.spawnTileY > (Main.worldSurface + Main.rockLayer) / 2);
			SurfaceJungle = new SpawnCondition((info) => info.spawnTileType == TileID.JungleGrass, 11f / 32f);
			SandstormEvent = new SpawnCondition((info) => Sandstorm.Happening && info.player.ZoneSandstorm
				&& TileID.Sets.Conversion.Sand[info.spawnTileType]
				&& NPC.Spawning_SandstoneCheck(info.spawnTileX, info.spawnTileY));
			Mummy = new SpawnCondition((info) => Main.hardMode && info.spawnTileType == TileID.Sand, 1f / 3f);
			DarkMummy = new SpawnCondition((info) => Main.hardMode && (info.spawnTileType == TileID.Ebonsand
				|| info.spawnTileType == TileID.Crimsand), 0.5f);
			LightMummy = new SpawnCondition((info) => Main.hardMode && info.spawnTileType == TileID.Pearlsand, 0.5f);
			OverworldHallow = new SpawnCondition((info) => Main.hardMode && !info.water && info.spawnTileY < Main.rockLayer
				&& (info.spawnTileType == TileID.Pearlsand || info.spawnTileType == TileID.Pearlstone
				|| info.spawnTileType == TileID.HallowedGrass || info.spawnTileType == TileID.HallowedIce));
			EnchantedSword = new SpawnCondition((info) => !info.playerSafe && Main.hardMode && !info.water
				&& info.spawnTileY >= Main.rockLayer && (info.spawnTileType == TileID.Pearlsand
				|| info.spawnTileType == TileID.Pearlstone || info.spawnTileType == TileID.HallowedGrass
				|| info.spawnTileType == TileID.HallowedIce), 0.02f);
			Crimson = new SpawnCondition((info) => (info.spawnTileType == TileID.Crimtane && info.player.ZoneCrimson)
				|| info.spawnTileType == TileID.FleshGrass || info.spawnTileType == TileID.FleshIce
				|| info.spawnTileType == TileID.Crimstone || info.spawnTileType == TileID.Crimsand);
			Corruption = new SpawnCondition((info) => (info.spawnTileType == TileID.Demonite && info.player.ZoneCorrupt)
				|| info.spawnTileType == TileID.CorruptGrass || info.spawnTileType == TileID.Ebonstone
				|| info.spawnTileType == TileID.Ebonsand || info.spawnTileType == TileID.CorruptIce);
			Overworld = new SpawnCondition((info) => info.spawnTileY <= Main.worldSurface);
			IceGolem = new SpawnCondition(Overworld, (info) => info.player.ZoneSnow && Main.hardMode
				&& Main.cloudAlpha > 0f && !NPC.AnyNPCs(NPCID.IceGolem), 0.05f);
			RainbowSlime = new SpawnCondition(Overworld, (info) => info.player.ZoneHoly && Main.hardMode
				&& Main.cloudAlpha > 0f && !NPC.AnyNPCs(NPCID.RainbowSlime), 0.05f);
			AngryNimbus = new SpawnCondition(Overworld, (info) => !info.player.ZoneSnow && Main.hardMode
				&& Main.cloudAlpha > 0f && NPC.CountNPCS(NPCID.AngryNimbus) < 2, 0.1f);
			MartianProbe = new SpawnCondition(Overworld, (info) => MartianProbeHelper(info) && Main.hardMode
				&& NPC.downedGolemBoss && !NPC.AnyNPCs(NPCID.MartianProbe), 1f / 400f);
			MartianProbe.WeightFunc = () => {
				float inverseChance = 399f / 400f;
				if (!NPC.downedMartians) {
					inverseChance *= 0.99f;
				}
				return 1f - inverseChance;
			};
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
			KingSlime = new SpawnCondition(OverworldDay, (info) => OuterThird(info) && GetTile(info).type == TileID.Grass
				&& !NPC.AnyNPCs(NPCID.KingSlime), 1f / 300f);
			OverworldDayDesert = new SpawnCondition(OverworldDay, (info) => GetTile(info).type == TileID.Sand
				&& !info.water, 0.2f);
			GoblinScout = new SpawnCondition(OverworldDay, (info) => OuterThird(info), 1f / 15f);
			GoblinScout.WeightFunc = () => {
				float inverseChance = 14f / 15f;
				if (!NPC.downedGoblins && WorldGen.shadowOrbSmashed) {
					return inverseChance *= (6f / 7f);
				}
				return 1f - inverseChance;
			};
			OverworldDayRain = new SpawnCondition(OverworldDay, (info) => Main.raining, 2f / 3f);
			OverworldDaySlime = new SpawnCondition(OverworldDay, (info) => true);
			OverworldNight = new SpawnCondition(Overworld, (info) => true);
			OverworldFirefly = new SpawnCondition(OverworldNight, (info) => GetTile(info).type == TileID.Grass
				|| GetTile(info).type == TileID.HallowedGrass, 0.1f);
			OverworldFirefly.WeightFunc = () => 1f / (float)NPC.fireFlyChance;
			OverworldNightMonster = new SpawnCondition(OverworldNight, (info) => true);
			Underground = new SpawnCondition((info) => info.spawnTileY <= Main.rockLayer);
			Underworld = new SpawnCondition((info) => info.spawnTileY > Main.maxTilesY - 190);
			Cavern = new SpawnCondition((info) => true);
		}

		private static Tile GetTile(NPCSpawnInfo info) {
			return Main.tile[info.spawnTileX, info.spawnTileY];
		}

		private static bool WaterSurface(NPCSpawnInfo info) {
			if (info.safeRangeX) {
				return false;
			}
			for (int k = info.spawnTileY - 1; k > info.spawnTileY - 50; k--) {
				if (Main.tile[info.spawnTileX, k].liquid == 0 && !WorldGen.SolidTile(info.spawnTileX, k)
					&& !WorldGen.SolidTile(info.spawnTileX, k + 1) && !WorldGen.SolidTile(info.spawnTileX, k + 2)) {
					return true;
				}
			}
			return false;
		}

		private static bool MartianProbeHelper(NPCSpawnInfo info) {
			return (float)Math.Abs(info.spawnTileX - Main.maxTilesX / 2) / (float)(Main.maxTilesX / 2) > 0.33f
				&& !NPC.AnyDanger();
		}

		private static bool InnerThird(NPCSpawnInfo info) {
			return Math.Abs(info.spawnTileX - Main.spawnTileX) < Main.maxTilesX / 3;
		}

		private static bool OuterThird(NPCSpawnInfo info) {
			return Math.Abs(info.spawnTileX - Main.spawnTileX) > Main.maxTilesX / 3;
		}
	}
}
