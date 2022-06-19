using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.Map;

namespace Terraria.ModLoader
{
	public static class PylonLoader
	{
		internal static readonly IList<GlobalPylon> globalPylons = new List<GlobalPylon>();

		internal static void Unload() {
			globalPylons.Clear();
		}

		public static void AddGlobalPylon(GlobalPylon pylon) {
			globalPylons.Add(pylon);
			ModTypeLookup<GlobalPylon>.Register(pylon);
		}

		public static bool PreDrawMapIcon(ref MapOverlayDrawContext context, ref string mouseOverText, TeleportPylonInfo pylonInfo, bool isNearPylon, Color drawColor, float deselectedScale, float selectedScale) {
			bool returnValue = true;
			foreach (GlobalPylon globalPylon in globalPylons) {
				returnValue &= globalPylon.PreDrawMapIcon(ref context, ref mouseOverText, pylonInfo, isNearPylon, drawColor, deselectedScale, selectedScale);
			}

			return returnValue;
		}

		public static bool ValidTeleportCheck_PreNPCCount(TeleportPylonInfo pylonInfo, ref int defaultNecessaryNPCCount) {
			bool returnValue = true;
			foreach (GlobalPylon globalPylon in globalPylons) {
				returnValue &= globalPylon.ValidTeleportCheck_PreNPCCount(pylonInfo, ref defaultNecessaryNPCCount);
			}

			return returnValue;
		}

		public static bool ValidTeleportCheck_PreAnyDanger(TeleportPylonInfo pylonInfo) {
			bool returnValue = true;
			foreach (GlobalPylon globalPylon in globalPylons) {
				returnValue &= globalPylon.ValidTeleportCheck_PreAnyDanger(pylonInfo);
			}

			return returnValue;
		}

		public static bool ValidTeleportCheck_PreBiomeRequirements(TeleportPylonInfo pylonInfo, SceneMetrics sceneData) {
			bool returnValue = true;
			foreach (GlobalPylon globalPylon in globalPylons) {
				returnValue &= globalPylon.ValidTeleportCheck_PreBiomeRequirements(pylonInfo, sceneData);
			}

			return returnValue;
		}

		public static void PostValidTeleportCheck(TeleportPylonInfo destinationPylonInfo, ref bool destinationPylonValid, bool validNearbyPylonFound, ref string errorKey) {
			foreach (GlobalPylon globalPylon in globalPylons) {
				globalPylon.PostValidTeleportCheck(destinationPylonInfo, ref destinationPylonValid, validNearbyPylonFound, ref errorKey);
			}
		}
	}
}
