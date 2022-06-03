using Terraria.ModLoader;

public class SimpleRenamedTMLMembersTest
{
	void Method() {
		bool textureExists = ModContent.HasAsset("1");
		textureExists = ModContent.HasAsset("1" + "2");

		var mod = new Mod();
		textureExists = mod.HasAsset("1");
		textureExists = mod.HasAsset("1" + "2");

		NPCSpawnInfo info = default;
		var a = info.DesertCave;
		var b = info.Granite;
		var c = info.Invasion;
		var d = info.Lihzahrd;
		var e = info.Marble;
		var f = info.PlanteraDefeated;
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