using Microsoft.Xna.Framework;
using Terraria.GameContent;
using Terraria.Map;

namespace Terraria.ModLoader
{
	/// <summary>
	/// Extension to <seealso cref="ModTile"/> that streamlines the process of creating a
	/// modded Pylon. Has all of ModTile's hooks for customization, but additional hooks for
	/// Pylon functionality.
	/// </summary>
	/// <remarks>
	/// One of the key features of this class is the <b>ValidTeleportCheck</b> process. At first glance it can look a bit
	/// messy, however here is a rough break-down of the call process to help you if you're lost:
	/// <br></br>
	/// 1) Game queries if the specified player is near a Pylon (<seealso cref="TeleportPylonsSystem.IsPlayerNearAPylon"/>)
	/// <br></br>
	/// 2) Assuming Step 1 has passed, game queries if the DESTINATION PYLON (the pylon the player CLICKED on the map) has enough NPCs nearby (NPCCount step)
	/// <br></br>
	/// 3) Assuming Step 2 has passed, game queries if there is ANY DANGER at ALL across the entire map, ignoring the lunar pillar event (AnyDanger step)
	/// <br></br>
	/// 4) Assuming Step 3 has passed, game queries if the DESTINATION PYLON is in the Lihzahrd Temple before Plantera is defeated.
	/// <br></br>
	/// 5) Assuming Step 4 has passed, game queries if the DESTINATION PYLON meets its biome specifications for whatever type of pylon it is (BiomeRequirements step)
	/// <br></br>
	/// 6) Regardless of all the past checks, if the DESTINATION PYLON is a modded one, <seealso cref="ValidTeleportCheck_DestinationPostCheck"/> is called on it.
	/// <br></br>
	/// 7) The game queries all pylons on the map and checks if any of them are in interaction distance with the player, and if so, checks Step 2 and 5 on them (NPCCount &amp; BiomeRequirements step)
	/// <br></br>
	/// 8) Given that Step 7 finds a valid nearby pylon that satisfied the conditions, if that nearby pylon is a modded one, <seealso cref="ValidTeleportCheck_NearbyPostCheck"/> is called on it.
	/// <br></br>
	/// 9) Any <seealso cref="GlobalPylon"/> instances run <seealso cref="GlobalPylon.PostValidTeleportCheck"/>.
	/// </remarks>
	public abstract class ModPylon : ModTile
	{

		/// <summary>
		/// What type of Pylon this ModPylon represents.
		/// </summary>
		/// <remarks>
		/// The TeleportPylonType enum only has string names up until Count (9). The very first modded pylon to be added will
		/// technically be accessible with the enum type of "Count" since that value isn't an actual "type" of pylon, and modded
		/// pylons are assigned IDs starting with the Count value (9). All other modded pylons added after 9 (i.e 10+) will have no
		/// enum name, and will only every be referred to by their number values.
		/// </remarks>
		public TeleportPylonType PylonType {
			get;
			internal set;
		}

		/// <summary>
		/// Whether or not this Pylon can even be placed.
		/// By default, it returns false if a Pylon of this type already exists in the world,
		/// otherwise true.
		/// </summary>
		/// <remarks>
		/// If you want to allow an infinite amount of these pylons to be placed, simply always return true.
		/// </remarks>
		public virtual bool CanPlacePylon() {
			return !Main.PylonSystem.HasPylonOfType(PylonType);
		}

		/// <summary>
		/// Whether or not this Pylon should be sold by the specified NPC type and with the given player.
		/// This should return the ITEM TYPE of the item that places this ModPylon, if one exists. If you don't
		/// want anything to be put up for sale, return null.
		/// <br>
		/// Returns null by default.
		/// </br>
		/// </summary>
		/// <param name="npcType"> The type of the NPC currently being spoken to to determine the shop of. </param>
		/// <param name="player"> The current player asking said NPC type what they have for sale. </param>
		/// <param name="isNPCHappyEnough">
		/// Whether or not this NPC is "happy enough", by vanilla standards. You can ignore this if you don't care about happiness.
		/// For reference, Vanilla defines "happy enough" as the player earning a 10% discount or more, or in code:
		/// <code>Main.LocalPlayer.currentShoppingSettings.PriceAdjustment &lt;= 0.8999999761581421;</code> 
		/// </param>
		public virtual int? IsPylonForSale(int npcType, Player player, bool isNPCHappyEnough) {
			return null;
		}

		/// <summary>
		/// Step 1 of the ValidTeleportCheck process. This is the first vanilla check that is called when
		/// checking both the destination pylon and any possible nearby pylons. This check should be where you check
		/// how many NPCs are nearby, returning false if the Pylon does not satisfy the conditions.
		/// By default, returns true if there are 2 or more (not-unhappy) NPCs nearby.
		/// </summary>
		/// <remarks>
		/// Note that it's important you put the right checks in the right ValidTeleportCheck step,
		/// as whatever one returns false (if any) will determine the error message sent to the player.
		/// <br></br>
		/// <b> If you're confused about the order of which the ValidTeleportCheck methods are called, check out the XML summary
		/// on the ModPylon class.</b>
		/// </remarks>
		/// <param name="pylonInfo"> The internal information pertaining to the current pylon being teleported to or from. </param>
		/// <param name="defaultNecessaryNPCCount"> The default amount of NPCs nearby required to satisfy a VANILLA pylon. </param>
		public virtual bool ValidTeleportCheck_NPCCount(TeleportPylonInfo pylonInfo, int defaultNecessaryNPCCount) {
			return TeleportPylonsSystem.DoesPositionHaveEnoughNPCs(defaultNecessaryNPCCount, pylonInfo.PositionInTiles);
		}

		/// <summary>
		/// Step 2 of the ValidTeleportCheck process. This is the second vanilla check that is called when
		/// checking the destination pylon. This check should be where you check
		/// if there is any "Danger" nearby, such as bosses or if there is an event happening.
		/// It is unlikely you will need to use this.
		/// By default, returns true if there are not any events happening (Lunar Pillars do not count)
		/// and there are no bosses currently alive.
		/// </summary>
		/// <remarks>
		/// Note that it's important you put the right checks in the right ValidTeleportCheck step,
		/// as whatever one returns false (if any) will determine the error message sent to the player.
		/// <br></br>
		/// <b> If you're confused about the order of which the ValidTeleportCheck methods are called, check out the XML summary
		/// on the ModPylon class.</b>
		/// </remarks>
		/// <param name="pylonInfo"> The internal information pertaining to the current pylon being teleported TO. </param>
		public virtual bool ValidTeleportCheck_AnyDanger(TeleportPylonInfo pylonInfo) {
			return !NPC.AnyDanger(false, true);
		}

		/// <summary>
		/// Step 3 of the ValidTeleportCheck process. This is the fourth vanilla check that is called when
		/// checking both the destination pylon and any possible nearby pylons. This check should be where you check biome related
		/// things, such as the simple check of whether or not the Pylon is in the proper biome.
		/// By default, returns true.
		/// </summary>
		/// <remarks>
		/// Note that it's important you put the right checks in the right ValidTeleportCheck step,
		/// as whatever one returns false (if any) will determine the error message sent to the player.
		/// <br></br>
		/// <b> If you're confused about the order of which the ValidTeleportCheck methods are called, check out the XML summary
		/// on the ModPylon class.</b>
		/// </remarks>
		/// <param name="pylonInfo"> The internal information pertaining to the current pylon being teleported to or from. </param>
		/// <param name="sceneData"> The scene metrics data AT THE LOCATION of the destination pylon, NOT the player. </param>
		public virtual bool ValidTeleportCheck_BiomeRequirements(TeleportPylonInfo pylonInfo, SceneMetrics sceneData) {
			return true;
		}

		/// <summary>
		/// The 4th check of the ValidTeleportCheck process. This check is for modded Pylons only, called after
		/// ALL other checks have completed pertaining the pylon clicked on the map (the destination pylon), but before
		/// any nearby pylon information is calculated. This is where you an do custom checks that don't pertain to the past destination checks,
		/// as well as customize the localization key to give custom messages to the player on teleportation failure. By default, does nothing.
		/// <br></br>
		/// <b> If you're confused about the order of which the ValidTeleportCheck methods are called, check out the XML summary
		/// on the ModPylon class.</b>
		/// </summary>
		/// <param name="destinationPylonInfo"> The Pylon information for the Pylon that the player is attempt to teleport to. </param>
		/// <param name="destinationPylonValid"> Whether or not after all of the checks, the destination Pylon is valid. </param>
		/// <param name="errorKey"> The localization key for the message sent to the player if destinationPylonValid is false. </param>
		public virtual void ValidTeleportCheck_DestinationPostCheck(TeleportPylonInfo destinationPylonInfo, ref bool destinationPylonValid, ref string errorKey) {}

		/// <summary>
		/// The 5th and final check of the ValidTeleportCheck process. This check is for modded Pylons only, called after
		/// ALL other checks have completed for the destination pylon and all normal checks have taken place for the nearby
		/// pylon, if applicable. This is where you can do custom checks that don't pertain to the past nearby pylon checks,
		/// as well as customize the localization key to give custom messages to the player on teleportation failure. By default, does nothing.
		/// <br></br>
		/// <b> If you're confused about the order of which the ValidTeleportCheck methods are called, check out the XML summary
		/// on the ModPylon class.</b>
		/// </summary>
		/// <param name="nearbyPylonInfo">
		/// The pylon information of the pylon the player in question is standing NEAR. This always has a value.
		/// </param>
		/// <param name="destinationPylonValid"> Whether or not after all of the checks, the destination Pylon is valid. </param>
		/// <param name="anyNearbyValidPylon"> Whether or not after all of the checks, there is a Pylon nearby to the player that is valid. </param>
		/// <param name="errorKey"> The localization key for the message sent to the player if destinationPylonValid is false. </param>
		public virtual void ValidTeleportCheck_NearbyPostCheck(TeleportPylonInfo nearbyPylonInfo, ref bool destinationPylonValid, ref bool anyNearbyValidPylon, ref string errorKey) { }

		/// <summary>
		/// Called when the map is visible, in order to draw the passed in Pylon on the map.
		/// In order to draw on the map, you must use <seealso cref="MapOverlayDrawContext"/>'s Draw Method. By default, doesn't draw anything.
		/// </summary>
		/// <param name="context"> The current map context on which you can draw. </param>
		/// <param name="mouseOverText"> The text that will overlay on the mouse when the icon is being hovered over. </param>
		/// <param name="pylonInfo"> The pylon that is currently needing its icon to be drawn. </param>
		/// <param name="isNearPylon"> Whether or not the player is currently near a pylon. </param>
		/// <param name="drawColor"> The draw color of the icon. This is bright white when the player is near a Pylon, but gray and translucent otherwise. </param>
		/// <param name="deselectedScale"> The scale of the icon if it is NOT currently being hovered over. In vanilla, this is 1f, or 100%. </param>
		/// <param name="selectedScale"> The scale of the icon if it IS currently being over. In vanilla, this is 2f, or 200%. </param>
		public virtual void DrawMapIcon(ref MapOverlayDrawContext context, ref string mouseOverText, TeleportPylonInfo pylonInfo, bool isNearPylon, Color drawColor, float deselectedScale, float selectedScale) { }
	}
}
