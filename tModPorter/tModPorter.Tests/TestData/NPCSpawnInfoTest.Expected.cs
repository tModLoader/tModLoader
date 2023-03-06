using Terraria.ModLoader;

public class NPCSpawnInfoTest
{
	void Method() {
		NPCSpawnInfo info = default;
		var a = info.DesertCave;
		var b = info.Granite;
		var c = info.Invasion;
		var d = info.Lihzahrd;
		var e = info.Marble;
		#if COMPILE_ERROR
		var f = info.PlanteraDefeated/* tModPorter Note: Removed. Use (NPC.downedPlantBoss && Main.hardMode) instead */;
		#endif
		var g = info.Player;
		var h = info.PlayerFloorX;
		var i = info.PlayerFloorY;
		var j = info.PlayerInTown;
		var k = info.PlayerSafe;
		var l = info.SafeRangeX;
		var m = info.Sky;
		var n = info.SpawnTileType;
		var o = info.SpawnTileX;
		var p = info.SpawnTileY;
		var q = info.SpiderCave;
		var r = info.Water;
	}
}