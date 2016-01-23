using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader.Default;
using Terraria.World.Generation;

namespace Terraria.ModLoader
{
	public static class ModWorldHooks
	{
		private static readonly IList<ModWorld> modWorlds = new List<ModWorld>();

		internal static void Add(ModWorld modWorld)
		{
			modWorlds.Add(modWorld);
		}

		internal static void Unload()
		{
			modWorlds.Clear();
		}

		public static void WorldGenPostGen()
		{
			foreach (ModWorld modWorld in modWorlds)
			{
				modWorld.WorldGenPostGen();
			}
		}

		public static void WorldGenModifyTaskList(List<GenPass> _passes)
		{
			foreach (ModWorld modWorld in modWorlds)
			{
				modWorld.WorldGenModifyTaskList(_passes);
			}
		}
	}
}