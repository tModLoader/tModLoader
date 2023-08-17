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

		test.Apply(null); // don't change Apply to ApplyPass
		width = new GenBaseTest()._worldWidth; // leave this alone, may have side effects
	}
}

public class GenPassTest : GenPass
{
	// Mandatory
	public GenPassTest(string name, float loadWeight) : base(name, loadWeight) { /* Empty */ }

	public override void Apply(GenerationProgress progress) { /* Empty */ }
}