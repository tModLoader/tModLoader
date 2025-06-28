using ExampleMod.Content.Tiles;
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
			// Since we have the capabilities, we can allow players to teleport to any pylon even if there are no NPCs there during the night time.
			if (!Main.dayTime) {
				defaultNecessaryNPCCount = 0;
			}

			// Since we aren't preventing anything and just changing the NPC count, we can just return what the default method returns, which is null (AKA vanilla behavior)
			return base.ValidTeleportCheck_PreNPCCount(pylonInfo, ref defaultNecessaryNPCCount);
		}

		public override bool PreDrawMapIcon(ref MapOverlayDrawContext context, ref string mouseOverText, ref TeleportPylonInfo pylonInfo, ref bool isNearPylon, ref Color drawColor, ref float deselectedScale, ref float selectedScale) {
			// What if we want to change the color of all of the map icons?
			// If we aren't near a pylon, we're going to shift the color of all pylon icons to being more red
			if (!isNearPylon) {
				drawColor = Color.Lerp(drawColor, Color.Red, 0.75f);
			}

			// Since we aren't actually preventing the drawing of any map icons, we can just return the default value, which in this case is null (AKA vanilla behavior)
			return base.PreDrawMapIcon(ref context, ref mouseOverText, ref pylonInfo, ref isNearPylon, ref drawColor, ref deselectedScale, ref selectedScale);
		}

		public override bool? PreCanPlacePylon(int x, int y, int tileType, TeleportPylonType pylonType) {
			// What if we want to override the functionality for pylon placement?
			// For example, let's always allow the players to place universal pylons, even if they already exist in the world:
			if (pylonType == TeleportPylonType.Victory) {
				return true;
			}
			// What if we wanted to change something for a modded type? If you have strong reference to the modded pylon in question,
			// you can simply use the class:
			if (pylonType == ModContent.PylonType<ExamplePylonTileAdvanced>()) {
				return null; // We don't want to *actually* change any functionality of the advanced pylon, so we return null.
				// Obviously, if you wanted to actually change something about the modded pylon, you'd return something other than null here.
			}

			return base.PreCanPlacePylon(x, y, tileType, pylonType);
		}

		public override bool? ValidTeleportCheck_PreBiomeRequirements(TeleportPylonInfo pylonInfo, SceneMetrics sceneData) {
			// What if we want to do something based on the type of pylon in particular? Well all we have to do is check the pylon's type!
			// Let's allow the Jungle Pylon to work in the snow, for example:
			if (pylonInfo.TypeOfPylon == TeleportPylonType.Jungle) {
				// If another mod tries to mess with Jungle pylons, we don't want to return a forceful false, if applicable. If Jungle AND snow
				// are both false, we will return null to allow for other mods to try and change things based on the Jungle pylon.
				// Granted that no other mod does anything to change the null value, the teleportation process will fail, under the above circumstances.
				return sceneData.EnoughTilesForJungle || sceneData.EnoughTilesForSnow ? true : null;
			}

			return base.ValidTeleportCheck_PreBiomeRequirements(pylonInfo, sceneData);
		}

		public override void PostValidTeleportCheck(TeleportPylonInfo destinationPylonInfo, TeleportPylonInfo nearbyPylonInfo, ref bool destinationPylonValid, ref bool validNearbyPylonFound, ref string errorKey) {
			// Since there is not an explicit hook for it (since it's too specific), what if we wanted to nullify vanilla's check to prevent accessing the Lihzahrd Temple early with a pylon?

			// We just need to check that to see if the Lihzahrd Temple check is the actual error we got (not some other error) which in this case is done by checking the error key.
			// We also do another quick check to make sure that we are still near a valid pylon.
			// If that is true, we can set destinationPylonValid to true, overriding the teleportation prevention.
			if (validNearbyPylonFound && errorKey == "Net.CannotTeleportToPylonBecauseAccessingLihzahrdTempleEarly") {
				destinationPylonValid = true;
			}
		}
	}
}
