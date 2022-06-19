using Terraria.WorldBuilding;
using Terraria.IO;

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
		test.Apply(null); // don't change Apply to ApplyPass
		width = new GenBaseTest()._worldWidth; // leave this alone, may have side effects
#endif
	}
}

public class GenPassTest : GenPass
{
	// Mandatory
	public GenPassTest(string name, float loadWeight) : base(name, loadWeight) { /* Empty */ }

	protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration) { /* Empty */ }
}