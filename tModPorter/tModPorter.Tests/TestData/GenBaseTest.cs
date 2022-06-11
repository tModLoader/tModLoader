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

#if COMPILE_ERROR
		width = new GenBaseTest()._worldWidth; // leave this alone, may have side effects
#endif
	}
}

#if COMPILE_ERROR
public class GenPassTest : GenPass
{
	// Mandatory
	public GenPassTest(string name, float loadWeight) : base(name, loadWeight) { /* Empty */ }

	// do NOT port to: protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration), breaks invocation sites
	public override void Apply(GenerationProgress progress) { /* Empty */ }
}
#endif