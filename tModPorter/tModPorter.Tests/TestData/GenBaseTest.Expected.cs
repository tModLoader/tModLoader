using Terraria.WorldBuilding;

public class GenBaseTest : GenBase
{
	void Method() {
		// Changed from instanced to static
		var width = GenBase._worldWidth;
		var height = GenBase._worldHeight;
		var random = GenBase._random;
		var tiles = GenBase._tiles;
		tiles = GenBase._tiles;

		var test = new GenBaseTest();
		width = GenBase._worldWidth;
	}
}