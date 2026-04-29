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
		#if COMPILE_ERROR
		var f = info.PlanteraDefeated/* tModPorter Note: Removed. Use (NPC.downedPlantBoss && Main.hardMode) instead */;
		#endif
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
	}
}
