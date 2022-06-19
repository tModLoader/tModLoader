using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.Map;

namespace Terraria.ModLoader
{
	internal class ModdedPylonsMapLayer : IMapLayer
	{
		public bool Visible { get; set; } = true;

		public void Draw(ref MapOverlayDrawContext context, ref string text) {
			List<TeleportPylonInfo> moddedPylons = Main.PylonSystem.ModdedPylons;
			float deselectedScale = 1f;
			float selectedScale = deselectedScale * 2f;
			bool isNearPylon = TeleportPylonsSystem.IsPlayerNearAPylon(Main.LocalPlayer);
			Color drawColor = isNearPylon ? Color.White : Color.Gray * 0.5f;

			foreach (TeleportPylonInfo info in moddedPylons) {
				if (!PylonLoader.PreDrawMapIcon(ref context, ref text, info, isNearPylon, drawColor, deselectedScale, selectedScale)) {
					continue;
				}
				if (ModContent.TryFind(info.ModName, info.ModPylonName, out ModPylon pylon)) {
					pylon.DrawMapIcon(ref context, ref text, info, isNearPylon, drawColor, deselectedScale, selectedScale);
				}
			}
		}
	}
}
