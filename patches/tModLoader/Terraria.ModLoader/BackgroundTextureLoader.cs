using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;

namespace Terraria.ModLoader
{
	public static class BackgroundTextureLoader
	{
		public const int vanillaBackgroundTextureCount = Main.maxBackgrounds;
		private static int nextBackground = vanillaBackgroundTextureCount;
		internal static IDictionary<string, int> backgrounds = new Dictionary<string, int>();

		internal static int ReserveBackgroundSlot()
		{
			int reserve = nextBackground;
			nextBackground++;
			return reserve;
		}

		public static int GetBackgroundSlot(string texture)
		{
			if (backgrounds.ContainsKey(texture))
			{
				return backgrounds[texture];
			}
			else
			{
				return -1;
			}
		}

		internal static void ResizeAndFillArrays()
		{
			Array.Resize(ref Main.backgroundTexture, nextBackground);
			Array.Resize(ref Main.backgroundHeight, nextBackground);
			Array.Resize(ref Main.backgroundWidth, nextBackground);
			Array.Resize(ref Main.backgroundLoaded, nextBackground);
			foreach (string texture in backgrounds.Keys)
			{
				int slot = backgrounds[texture];
				Main.backgroundTexture[slot] = ModLoader.GetTexture(texture);
				Main.backgroundWidth[slot] = Main.backgroundTexture[slot].Width;
				Main.backgroundHeight[slot] = Main.backgroundTexture[slot].Height;
				Main.backgroundLoaded[slot] = true;
			}
		}

		internal static void Unload()
		{
			nextBackground = vanillaBackgroundTextureCount;
			backgrounds.Clear();
		}
	}
}
