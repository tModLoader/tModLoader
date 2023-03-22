using System;
using System.Diagnostics;
using System.Reflection;
using Terraria.ModLoader.Core;

namespace Terraria.ModLoader;

public static partial class Logging
{
	private static void TryFreeingMemory()
	{
		// In case of OOM, unload the Main.tile array and do immediate garbage collection.
		// If we don't do this, there will be a big chance that this method will fail to even quit the game, due to another OOM exception being thrown.

		Main.tile = new Tilemap(0, 0);
		GC.Collect();
	}
}
