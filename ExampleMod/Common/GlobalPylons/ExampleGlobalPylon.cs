using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.Map;
using Terraria.ModLoader;

namespace ExampleMod.Common.GlobalPylons
{
	/// <summary>
	/// An example and showcase of some of the hooks within the GlobalPylon class, which allow us to change functionality
	/// for any type of pylon that we want. The changes we make in this class may not be the most practical, as they are more-so
	/// showing what you can do with the hooks.
	/// </summary>.
	public class ExampleGlobalPylon : GlobalPylon
	{
		public override bool? ValidTeleportCheck_PreNPCCount(TeleportPylonInfo pylonInfo, ref int defaultNecessaryNPCCount) {
			//Since we have the capabilities, we can allow players to teleport to any pylon even if there are no NPCs there during the night time.
			if (!Main.dayTime) {
				defaultNecessaryNPCCount = 0;
			}

			//Since we aren't preventing anything and just changing the NPC count, we can just return what the default method returns, which is null (AKA vanilla behavior)
			return base.ValidTeleportCheck_PreNPCCount(pylonInfo, ref defaultNecessaryNPCCount);
		}

		public override bool PreDrawMapIcon(ref MapOverlayDrawContext context, ref string mouseOverText, ref TeleportPylonInfo pylonInfo, ref bool isNearPylon, ref Color drawColor, ref float deselectedScale, ref float selectedScale) {
			//What if we want to change the color of all of the map icons?
			//If we aren't near a pylon, we're going to shift the color of all pylon icons to being more red
			if (!isNearPylon) {
				drawColor = Color.Lerp(drawColor, Color.Red, 0.75f);
			}

			//Since we aren't actually preventing the drawing of any map icons, we can just return the default value, which in this case is null (AKA vanilla behavior)
			return base.PreDrawMapIcon(ref context, ref mouseOverText, ref pylonInfo, ref isNearPylon, ref drawColor, ref deselectedScale, ref selectedScale);
		}

		public override bool? ValidTeleportCheck_PreBiomeRequirements(TeleportPylonInfo pylonInfo, SceneMetrics sceneData) {
			//What if we want to do something based on the type of pylon in particular? Well all we have to do is check the pylon's type!
			//Let's swap around the biome requirements for the Jungle Pylon and Snow Pylon, for example:
			TeleportPylonType pylonType = pylonInfo.TypeOfPylon;

			if (pylonType == TeleportPylonType.Jungle) {
				return sceneData.EnoughTilesForSnow;
			}

			if (pylonType == TeleportPylonType.Snow) {
				return sceneData.EnoughTilesForJungle;
			}

			return base.ValidTeleportCheck_PreBiomeRequirements(pylonInfo, sceneData);
		}

		public override void PostValidTeleportCheck(TeleportPylonInfo destinationPylonInfo, TeleportPylonInfo nearbyPylonInfo, ref bool destinationPylonValid, ref bool validNearbyPylonFound, ref string errorKey) {
			//Since there is not an explicit hook for it (since it's too specific), what if we wanted to nullify vanilla's check to prevent accessing the Lihzahrd Temple early with a pylon?

			//We just need to check that to see if the Lihzahrd Temple check is the actual error we got (not some other error) which in this case is done by checking the error key.
			//We also do another quick check to make sure that we are still near a valid pylon.
			//If that is true, we can set destinationPylonValid to true, overriding the teleportation prevention.
			if (validNearbyPylonFound && errorKey == "Net.CannotTeleportToPylonBecauseAccessingLihzahrdTempleEarly") {
				destinationPylonValid = true;
			}
		}
	}
}
