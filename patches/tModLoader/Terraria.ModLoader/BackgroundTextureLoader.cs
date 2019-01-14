using System;
using System.Collections.Generic;

namespace Terraria.ModLoader
{
	//todo: further documentation
	/// <summary>
	/// This is the class that keeps track of all modded background textures and their slots/IDs.
	/// </summary>
	public static class BackgroundTextureLoader
	{
		public const int vanillaBackgroundTextureCount = Main.maxBackgrounds;
		private static int nextBackground = vanillaBackgroundTextureCount;
		internal static IDictionary<string, int> backgrounds = new Dictionary<string, int>();

		internal static int ReserveBackgroundSlot() {
			int reserve = nextBackground;
			nextBackground++;
			return reserve;
		}

		/// <summary>
		/// Returns the slot/ID of the background texture with the given name.
		/// </summary>
		public static int GetBackgroundSlot(string texture) => backgrounds.TryGetValue(texture, out int slot) ? slot : -1;

		internal static void ResizeAndFillArrays() {
			Array.Resize(ref Main.backgroundTexture, nextBackground);
			Array.Resize(ref Main.backgroundHeight, nextBackground);
			Array.Resize(ref Main.backgroundWidth, nextBackground);
			Array.Resize(ref Main.backgroundLoaded, nextBackground);
			foreach (string texture in backgrounds.Keys) {
				int slot = backgrounds[texture];
				Main.backgroundTexture[slot] = ModContent.GetTexture(texture);
				Main.backgroundWidth[slot] = Main.backgroundTexture[slot].Width;
				Main.backgroundHeight[slot] = Main.backgroundTexture[slot].Height;
				Main.backgroundLoaded[slot] = true;
			}
		}

		internal static void Unload() {
			nextBackground = vanillaBackgroundTextureCount;
			backgrounds.Clear();
		}
	}
}
