using System;
using System.Collections.Generic;

namespace Terraria.ModLoader.Core
{
	internal static class GlobalUtils
	{
		public static T Instance<T>(Instanced<T>[] globals, ushort index) where T : GlobalType {
			for (int i = 0; i < globals.Length; i++) {
				var g = globals[i];

				if (g.index == index) {
					return g.instance;
				}
			}

			return null;
		}
	}
}
