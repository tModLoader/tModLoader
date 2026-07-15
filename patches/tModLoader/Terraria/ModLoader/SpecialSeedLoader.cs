using System.Collections.Generic;

namespace Terraria.ModLoader;

public static class SpecialSeedLoader
{
	public static bool ShouldSeedMenuScroll => false;

	internal static readonly IList<ModSpecialSeed> specialSeeds = new List<ModSpecialSeed>();

	internal static void Add(ModSpecialSeed modSpecialSeed)
	{
		specialSeeds.Add(modSpecialSeed);
	}

	internal static void Unload()
	{
		specialSeeds.Clear();
	}
}