using Terraria.World.Generation;

public class GenBaseTest : GenBase
{
	void Method() {
		// Changed from instanced to static
		var width = _worldWidth;
		var height = _worldHeight;
		var random = _random;
		var tiles = _tiles;
		tiles = this._tiles;

		var test = new GenBaseTest();
		width = test._worldWidth;
	}
}