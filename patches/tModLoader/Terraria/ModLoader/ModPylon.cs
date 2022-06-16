using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Tile_Entities;
using Terraria.ID;
using Terraria.Map;
using Terraria.ObjectData;

namespace Terraria.ModLoader
{
	/// <summary>
	/// Extension to <seealso cref="ModTile"/> that streamlines the process of creating a
	/// modded Pylon. Has all of ModTile's hook for customization, but additional hooks for
	/// Pylon functionality.
	/// </summary>
	public abstract class ModPylon : ModTile
	{

		/// <summary>
		/// Whether or not this Pylon can even be placed.
		/// By default, it returns false if a Pylon of this type already exists in the world,
		/// otherwise true.
		/// <remarks>
		/// If you want an infinite amount of these pylons to be placed, simply always return true.
		/// </remarks>
		/// </summary>
		public virtual bool CanPlacePylon() {
			return !Main.PylonSystem.HasPylonOfType(TeleportPylonType.Modded, Type);
		}

		/// <summary>
		/// Step 1 of the ValidTeleportCheck process. This is the first vanilla check that is called when
		/// the player attempts to teleport FROM or TO a Pylon. This check should be where you check
		/// how many NPCs are nearby, returning false if the Pylon does not satisfy the conditions.
		/// By default, returns true if there are 2 or more (not-unhappy) NPCs nearby.
		/// </summary>
		/// <remarks>
		/// Note that it's important you put the right checks in the right ValidTeleportCheck step,
		/// as whatever one returns false (if any) will determine the error message sent to the player.
		/// </remarks>
		/// <param name="pylonInfo"> The internal information pertaining to the current pylon being teleported to or from. </param>
		/// <param name="defaultNecessaryNPCCount"> The default amount of NPCs nearby required to satisfy a VANILLA pylon. </param>
		public virtual bool ValidTeleportCheck_NPCCount(TeleportPylonInfo pylonInfo, int defaultNecessaryNPCCount) {
			return TeleportPylonsSystem.DoesPositionHaveEnoughNPCs(defaultNecessaryNPCCount, pylonInfo.PositionInTiles);
		}

		/// <summary>
		/// Step 2 of the ValidTeleportCheck process. This is the second vanilla check that is called when
		/// the player attempts to teleport TO a Pylon. This check should be where you check
		/// if there is any "Danger" nearby, such as bosses or if there is an event happening.
		/// It is unlikely you will need to use this.
		/// By default, returns true if there are not any events happening (Lunar Pillars do not count)
		/// and there are no bosses currently alive.
		/// </summary>
		/// <remarks>
		/// Note that it's important you put the right checks in the right ValidTeleportCheck step,
		/// as whatever one returns false (if any) will determine the error message sent to the player.
		/// </remarks>
		/// <param name="nearbyPylonInfo"> The internal information pertaining to the current pylon being teleported FROM. </param>
		public virtual bool ValidTeleportCheck_AnyDanger(TeleportPylonInfo nearbyPylonInfo) {
			return !NPC.AnyDanger(false, true);
		}

		/// <summary>
		/// Step 3 of the ValidTeleportCheck process. This is the fourth vanilla check that is called when
		/// the player attempts to teleport FROM or TO a Pylon. This check should be where you check biome related
		/// things, such as the simple check of whether or not the Pylon is in the proper biome.
		/// By default, returns true.
		/// </summary>
		/// <remarks>
		/// Note that it's important you put the right checks in the right ValidTeleportCheck step,
		/// as whatever one returns false (if any) will determine the error message sent to the player.
		/// </remarks>
		/// <param name="pylonInfo"> The internal information pertaining to the current pylon being teleported to or from. </param>
		/// <param name="sceneData"> The scene metrics data AT THE LOCATION of this parameter, NOT the player. </param>
		public virtual bool ValidTeleportCheck_BiomeRequirements(TeleportPylonInfo pylonInfo, SceneMetrics sceneData) {
			return true;
		}

		/// <summary>
		/// The 4th and final check of the ValidTeleportCheck process. This check is for modded Pylons only, called after
		/// ALL other checks have completed when the player attempts to teleport TO a Pylon. This is where you
		/// can do custom checks that don't pertain to the past checks, as well as customize the localization key to give
		/// custom messages to the player on teleportation failure. By default, does nothing.
		/// </summary>
		/// <param name="destinationPylonInfo"> The Pylon information for the Pylon that the player is attempt to teleport to. </param>
		/// <param name="destinationPylonValid"> Whether or not after all of the checks, the destination Pylon is valid. </param>
		/// <param name="errorKey"> The localization key for the message sent to the player if destinationPylonValid is false. </param>
		public virtual void ValidTeleportCheck_PostCheck(TeleportPylonInfo destinationPylonInfo, ref bool destinationPylonValid, ref string errorKey) { }

		/// <summary>
		/// Called when the map is visible, in order to draw the passed in Pylon on the map, and returns whether or not the mouse is hovering over the icon.
		/// In order to draw on the map, you must use <seealso cref="MapOverlayDrawContext"/>'s Draw Method. By default, doesn't draw anything and returns false.
		/// </summary>
		/// <param name="context"> The current map context on which you can draw. </param>
		/// <param name="mouseOverText"> The text that will overlay on the mouse when the icon is being hovered over. </param>
		/// <param name="pylonInfo"> The pylon that is currently needing its icon to be drawn. </param>
		/// <param name="drawColor"> The draw color of the icon. This is bright white when the player is near a Pylon, but gray and translucent otherwise. </param>
		/// <param name="deselectedScale"> The scale of the icon if it is NOT currently being hovered over. In vanilla, this is 1f, or 100%. </param>
		/// <param name="selectedScale"> The scale of the icon if it IS currently being over. In vanilla, this is 2f, or 200%. </param>
		public virtual bool DrawMapIcon(ref MapOverlayDrawContext context, ref string mouseOverText, TeleportPylonInfo pylonInfo, Color drawColor, float deselectedScale, float selectedScale) {
			return false;
		}

		/// <summary>
		/// Sets all tile values (Main.tileLights, TileObjectData, TileID.Sets, AddMapEntry, etc.) to the *exact*
		/// defaults that a Vanilla Pylon uses.
		/// </summary>
		public void SetToPylonDefaults() {
			Main.tileLighted[Type] = true;
			Main.tileFrameImportant[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x4);
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.newTile.DrawYOffset = 2;
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.HookCheckIfCanPlace = new PlacementHook(TETeleportationPylon.PlacementPreviewHook_CheckIfCanPlace, 1, 0, true);
			TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(TETeleportationPylon.PlacementPreviewHook_AfterPlacement, -1, 0, false);

			TileObjectData.addTile(Type);

			TileID.Sets.InteractibleByNPCs[Type] = true;
			TileID.Sets.PreventsSandfall[Type] = true;

			AddToArray(ref TileID.Sets.CountsAsPylon);

			ModTranslation pylonName = CreateMapEntryName(); //Name is in the localization file
			AddMapEntry(Color.White, pylonName);
		}
	}
}
