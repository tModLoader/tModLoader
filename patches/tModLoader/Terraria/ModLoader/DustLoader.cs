using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.ID;

namespace Terraria.ModLoader
{
	public static class DustLoader
	{
		internal static readonly IList<ModDust> dusts = new List<ModDust>();

		internal static int DustCount { get; private set; } = DustID.Count;

		internal static int ReserveDustID() => DustCount++;

		internal static void ResizeArrays() {
			Array.Resize(ref ChildSafety.SafeDust, DustCount);

			for (int k = DustID.Count; k < DustCount; k++) {
				ChildSafety.SafeDust[k] = true;
			}
		}

		internal static void Unload() {
			dusts.Clear();

			DustCount = DustID.Count;
		}

		internal static void SetupDust(Dust dust) {
			if (ModContent.TryGet<ModDust>(dust.type, out var modDust)) {
				dust.frame.X = 0;
				dust.frame.Y %= 30;
				modDust.OnSpawn(dust);
			}
		}

		internal static void SetupUpdateType(Dust dust) {
			if (ModContent.TryGet<ModDust>(dust.type, out var modDust) && modDust.UpdateType >= 0) {
				dust.realType = dust.type;
				dust.type = modDust.UpdateType;
			}
		}

		internal static void TakeDownUpdateType(Dust dust) {
			if (dust.realType >= 0) {
				dust.type = dust.realType;
				dust.realType = -1;
			}
		}
	}
}
