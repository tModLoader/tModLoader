using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;

namespace Terraria.ModLoader
{
	public static class UndergroundBackgroundStyleLoader
	{
		public static readonly int vanillaUndergroundBackgroundStyleCount = 18;
		private static int nextUndergroundBackgroundStyle = vanillaUndergroundBackgroundStyleCount;
		internal static IDictionary<string, int> undergroundBackgroundStyles = new Dictionary<string, int>();

		internal static int ReserveBackgroundSlot()
		{
			int reserve = nextUndergroundBackgroundStyle;
			nextUndergroundBackgroundStyle++;
			return reserve;
		}

		public static int GetBackgroundSlot(string texture)
		{
			if (undergroundBackgroundStyles.ContainsKey(texture))
			{
				return undergroundBackgroundStyles[texture];
			}
			else
			{
				return -1;
			}
		}

		internal static void ResizeAndFillArrays()
		{
		}

		internal static void Unload()
		{
			nextUndergroundBackgroundStyle = vanillaUndergroundBackgroundStyleCount;
			undergroundBackgroundStyles.Clear();
		}
	}

	public static class SurfaceBackgroundStyleLoader
	{
	}
}
