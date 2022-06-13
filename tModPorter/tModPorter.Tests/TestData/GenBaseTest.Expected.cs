using Terraria.WorldBuilding;

public class GenBaseTest : GenBase
{
	void Method() {
		// Changed from instanced to static
		var width = _worldWidth;
		var height = _worldHeight;
		var random = _random;
		var tiles = _tiles;
		tiles = _tiles;

		var test = new GenBaseTest();
		width = _worldWidth;

#if COMPILE_ERROR
		width = new GenBaseTest()._worldWidth; // leave this alone, may have side effects
#endif
	}
}