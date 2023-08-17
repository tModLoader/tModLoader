using Terraria.WorldBuilding;
// not-yet-implemented
using Terraria.IO;
// instead-expect

public class GenBaseTest : GenBase
{
	void Method() {
		// Changed from instanced to static
		var width = _worldWidth;
		var height = _worldHeight;
		var random = _random;
		var tiles = _tiles;
		// not-yet-implemented
		tiles = _tiles;
		// instead-expect
#if COMPILE_ERROR
		tiles = this._tiles;
#endif

		var test = new GenBaseTest();
		// not-yet-implemented
		width = _worldWidth;
		// instead-expect
#if COMPILE_ERROR
		width = test._worldWidth;
#endif

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

	// not-yet-implemented
	protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration) { /* Empty */ }
	// instead-expect
#if COMPILE_ERROR
	public override void Apply(GenerationProgress progress) { /* Empty */ }
#endif
}