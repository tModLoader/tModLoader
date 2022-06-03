using Terraria.ModLoader;

public class SimpleRenamedTMLMembersTest
{
	void Method() {
		bool textureExists = ModContent.TextureExists("1");
		textureExists = ModContent.TextureExists("1" + "2");

		var mod = new Mod();
		textureExists = mod.TextureExists("1");
		textureExists = mod.TextureExists("1" + "2");

		NPCSpawnInfo info = default;
		var a = info.desertCave;
		var b = info.granite;
		var c = info.invasion;
		var d = info.lihzahrd;
		var e = info.marble;
		var f = info.planteraDefeated;
		var g = info.player;
		var h = info.playerFloorX;
		var i = info.playerFloorY;
		var j = info.playerInTown;
		var k = info.playerSafe;
		var l = info.safeRangeX;
		var m = info.sky;
		var n = info.spawnTileType;
		var o = info.spawnTileX;
		var p = info.spawnTileY;
		var q = info.spiderCave;
		var r = info.water;
	}
}