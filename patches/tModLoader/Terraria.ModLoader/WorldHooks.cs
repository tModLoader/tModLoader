using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader.Default;
using Terraria.World.Generation;

namespace Terraria.ModLoader
{
	public static class WorldHooks
	{
		private static readonly IList<ModWorld> worlds = new List<ModWorld>();

		internal static void Add(ModWorld modWorld)
		{
			worlds.Add(modWorld);
		}

		internal static void Unload()
		{
			worlds.Clear();
		}

		public static void ModifyWorldGenTasks(List<GenPass> passes, ref float totalWeight)
		{
			foreach (ModWorld modWorld in worlds)
			{
				modWorld.ModifyWorldGenTasks(passes, ref totalWeight);
			}
		}

		public static void PostWorldGen()
		{
			foreach (ModWorld modWorld in worlds)
			{
				modWorld.PostWorldGen();
			}
		}
	}
}