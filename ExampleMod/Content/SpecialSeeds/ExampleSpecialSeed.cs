using System.Collections.Generic;
using Terraria.ModLoader;

namespace ExampleMod.Content.SpecialSeeds;

public class ExampleSpecialSeed : ModSpecialSeed
{
	public override IEnumerable<string> SpecialSeedNames()
	{
		yield return "example";
	}
}
public class TestSpecialSeed : ModSpecialSeed
{
	public override IEnumerable<string> SpecialSeedNames()
	{
		yield return "test";
	}
}
