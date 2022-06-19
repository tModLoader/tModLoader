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

		public static bool PreDrawMapIcon(ref MapOverlayDrawContext context, ref string mouseOverText, ref TeleportPylonInfo pylonInfo, ref bool isNearPylon, ref Color drawColor, ref float deselectedScale, ref float selectedScale) {
			bool returnValue = true;
			foreach (GlobalPylon globalPylon in globalPylons) {
				returnValue &= globalPylon.PreDrawMapIcon(ref context, ref mouseOverText, ref pylonInfo, ref isNearPylon, ref drawColor, ref deselectedScale, ref selectedScale);
			}

			return returnValue;
		}

		public static bool? ValidTeleportCheck_PreNPCCount(TeleportPylonInfo pylonInfo, ref int defaultNecessaryNPCCount) {
			bool? returnValue = null;

			foreach (GlobalPylon globalPylon in globalPylons) {
				bool? shouldSucceed = globalPylon.ValidTeleportCheck_PreNPCCount(pylonInfo, ref defaultNecessaryNPCCount);

				if (shouldSucceed.HasValue) {
					if (!shouldSucceed.Value) {
						return false;
					}

					returnValue = true;
				}
			}

			return returnValue;
		}

		public static bool? ValidTeleportCheck_PreAnyDanger(TeleportPylonInfo pylonInfo) {
			bool? returnValue = null;

			foreach (GlobalPylon globalPylon in globalPylons) {
				bool? shouldSucceed = globalPylon.ValidTeleportCheck_PreAnyDanger(pylonInfo);

				if (shouldSucceed.HasValue) {
					if (!shouldSucceed.Value) {
						return false;
					}

					returnValue = true;
				}
			}

			return returnValue;
		}

		public static bool? ValidTeleportCheck_PreBiomeRequirements(TeleportPylonInfo pylonInfo, SceneMetrics sceneData) {
			bool? returnValue = null;

			foreach (GlobalPylon globalPylon in globalPylons) {
				bool? shouldSucceed = globalPylon.ValidTeleportCheck_PreBiomeRequirements(pylonInfo, sceneData);

				if (shouldSucceed.HasValue) {
					if (!shouldSucceed.Value) {
						return false;
					}

					returnValue = true;
				}
			}

			return returnValue;
		}

		public static void PostValidTeleportCheck(TeleportPylonInfo destinationPylonInfo, TeleportPylonInfo nearbyPylonInfo, ref bool destinationPylonValid, ref bool validNearbyPylonFound, ref string errorKey) {
			foreach (GlobalPylon globalPylon in globalPylons) {
				globalPylon.PostValidTeleportCheck(destinationPylonInfo, nearbyPylonInfo, ref destinationPylonValid, ref validNearbyPylonFound, ref errorKey);
			}
		}
	}
}
