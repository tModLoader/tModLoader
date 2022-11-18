using System.Collections.Generic;

namespace Terraria.ModLoader;

public static class ResourceDisplaySetLoader
{
	public static int DisplaySetCount => moddedDisplaySets.Count;

	internal static readonly IList<ModResourceDisplaySet> moddedDisplaySets = new List<ModResourceDisplaySet>();

	internal static int Add(ModResourceDisplaySet displaySet)
	{
		moddedDisplaySets.Add(displaySet);
		return DisplaySetCount - 1;
	}

	public static ModResourceDisplaySet GetDisplaySet(int type)
	{
		return type >= 0 && type < DisplaySetCount ? moddedDisplaySets[type] : null;
	}

	internal static void Unload()
	{
		moddedDisplaySets.Clear();

		Main.ResourceSetsManager.ResetToVanilla();
	}
}
