using System;
using System.Collections.Generic;
using Terraria.GameContent;

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
			Array.Resize(ref TextureAssets.Background, nextBackground);
			Array.Resize(ref Main.backgroundHeight, nextBackground);
			Array.Resize(ref Main.backgroundWidth, nextBackground);

			foreach (string texture in backgrounds.Keys) {
				int slot = backgrounds[texture];
				var tex = ModContent.GetTexture(texture);

				TextureAssets.Background[slot] = tex;
				Main.backgroundWidth[slot] = tex.Width();
				Main.backgroundHeight[slot] = tex.Height();
			}
		}

		internal static void Unload() {
			nextBackground = vanillaBackgroundTextureCount;
			backgrounds.Clear();
		}
	}
}
