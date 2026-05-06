using Terraria;
using Terraria.ModLoader;

public class NPCSpawnInfoTest
{
	void Method() {
		NPC.Spawner info = default;
		var a = info.spawnUndergroundDesert;
		var b = info.nearGranite;
		var c = info.invaders;
		var d = info.ZoneLihzhardTemple;
		var e = info.nearMarble;
		var f = info.hardDungeon;
		var g = info.Player;
#if COMPILE_ERROR
		var h = info.PlayerFloorX/* tModPorter Note: Removed. Player floor coordinates are no longer tracked by NPC.Spawner */;
		var i = info.PlayerFloorY/* tModPorter Note: Removed. Player floor coordinates are no longer tracked by NPC.Spawner */;
#endif
		var j = info.spawnFriendly;
		var k = info.noWorms;
		var l = info.SafeRangeX;
		var m = info.skyMob;
		var n = info.SpawnTileType;
		var o = info.SpawnTileX;
		var p = info.SpawnTileY;
		var q = info.spawnSpider;
		var r = info.waterTile;

		var a2 = info.spawnUndergroundDesert;
		var b2 = info.nearGranite;
		var c2 = info.invaders;
		var d2 = info.ZoneLihzhardTemple;
		var e2 = info.nearMarble;
		var f2 = info.hardDungeon;
		var g2 = info.Player;
#if COMPILE_ERROR
		var h2 = info.PlayerFloorX/* tModPorter Note: Removed. Player floor coordinates are no longer tracked by NPC.Spawner */;
		var i2 = info.PlayerFloorY/* tModPorter Note: Removed. Player floor coordinates are no longer tracked by NPC.Spawner */;
#endif
		var j2 = info.spawnFriendly;
		var k2 = info.noWorms;
		var l2 = info.SafeRangeX;
		var m2 = info.skyMob;
		var n2 = info.SpawnTileType;
		var o2 = info.SpawnTileX;
		var p2 = info.SpawnTileY;
		var q2 = info.spawnSpider;
		var r2 = info.waterTile;
	}
}
