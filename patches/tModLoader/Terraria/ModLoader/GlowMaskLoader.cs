using System;
using Terraria.GameContent;
using Terraria.ID;

namespace Terraria.ModLoader
{
	public static class GlowMaskLoader
	{
		private static short nextGlow = GlowMaskID.Count;

		internal static short ReserveGlowID() {
			short reserveID = nextGlow;
			nextGlow++;
			return reserveID;
		}

		internal static void ResizeArrays() {
			Array.Resize(ref TextureAssets.GlowMask, nextGlow);
		}

		internal static void Unload() {
			nextGlow = GlowMaskID.Count;
		}
	}
}
