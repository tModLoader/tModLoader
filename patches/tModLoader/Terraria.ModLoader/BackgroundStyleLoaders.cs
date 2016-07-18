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
		public static readonly int vanillaSurfaceBackgroundStyleCount = 10;
		private static int nextSurfaceBackgroundStyle = vanillaSurfaceBackgroundStyleCount;
		internal static IDictionary<string, int> surfaceBackgroundStyles = new Dictionary<string, int>();

		//public static int SurfaceStyleCount => nextSurfaceBackgroundStyle;

		internal static int ReserveBackgroundSlot()
		{
			int reserve = nextSurfaceBackgroundStyle;
			nextSurfaceBackgroundStyle++;
			return reserve;
		}

		public static int GetBackgroundSlot(string texture)
		{
			if (surfaceBackgroundStyles.ContainsKey(texture))
			{
				return surfaceBackgroundStyles[texture];
			}
			else
			{
				return -1;
			}
		}

		internal static void ResizeAndFillArrays()
		{
			Array.Resize(ref Main.bgAlpha, nextSurfaceBackgroundStyle);
			Array.Resize(ref Main.bgAlpha2, nextSurfaceBackgroundStyle);
		}

		internal static void Unload()
		{
			nextSurfaceBackgroundStyle = vanillaSurfaceBackgroundStyleCount;
			surfaceBackgroundStyles.Clear();
		}
	}
}
