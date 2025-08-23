using Microsoft.Xna.Framework;
using Terraria.GameContent;
using Terraria.Map;

namespace Terraria.ModLoader;

/// <summary>
/// Global Type that exists for pylons that allows for modification of teleportation steps, map drawing, and
/// other functionality for any pylon that exists, whether it be vanilla or modded.
/// </summary>
public abstract class GlobalPylon : ModType
{

	/// <summary>
	/// Called right before both modded and vanilla pylons draw their icons on the map. Returning false will prevent the passed in icon from being drawn on the map. Returns
	/// true by default.
	/// </summary>
	/// <remarks>
	/// Note that if you change the value of the isNearPylon parameter, the change will cascade and all pylons following this will be affected by the change.
	/// </remarks>
	/// <param name="context"> The current map context on which you can draw. </param>
	/// <param name="mouseOverText"> The text that will overlay on the mouse when the icon is being hovered over. </param>
	/// <param name="pylonInfo"> The pylon that is currently needing its icon to be drawn. </param>
	/// <param name="isNearPylon"> Whether or not the player is currently near a pylon. </param>
	/// <param name="drawColor"> The draw color of the icon. This is bright white when the player is near a Pylon, but gray and translucent otherwise. </param>
	/// <param name="deselectedScale"> The scale of the icon if it is NOT currently being hovered over. In vanilla, this is 1f, or 100%. </param>
	/// <param name="selectedScale"> The scale of the icon if it IS currently being over. In vanilla, this is 2f, or 200%. </param>
	public virtual bool PreDrawMapIcon(ref MapOverlayDrawContext context, ref string mouseOverText, ref TeleportPylonInfo pylonInfo, ref bool isNearPylon, ref Color drawColor, ref float deselectedScale, ref float selectedScale)
	{
		return true;
	}

	/// <summary>
	/// Called right before both modded and vanilla pylons determine whether or not they can be placed. Returning false will prevent vanilla checks from taking place,
	/// forcefully preventing the pylon from being placed. Returning null will enact vanilla behavior, and returning true will make the check forcefully succeed,
	/// regardless of vanilla behavior. Returns null by default.
	/// </summary>
	/// <param name="x"> The current X position of where the tile in question is asking it can be placed. </param>
	/// <param name="y"> The current Y position of where the tile in question is asking it can be placed. </param>
	/// <param name="tileType"> The actual type of tile currently asking if it can be placed. </param>
	/// <param name="pylonType"> The type of pylon belonging to this tile. </param>
	/// <remarks>
	/// Note that in Multiplayer environments, this is called first on the client, and then is subsequently called &amp; double checked on the server.
	/// <br>If the server disagrees with the client that the given pylon CANNOT be placed for any given reason, the server will reject the placement
	/// and subsequently break the associated tile.</br>
	/// </remarks>
	public virtual bool? PreCanPlacePylon(int x, int y, int tileType, TeleportPylonType pylonType)
	{
		return null;
	}

	/// <summary>
	/// Called before Step 1 of the ValidTeleportCheck process. This is the first vanilla check that is called when
	/// the player attempts to teleport FROM or TO a Pylon. This method is called before both vanilla
	/// and modded pylons check their NPC requirements, and returning false will prevent those checks from taking place,
	/// forcefully failing the NPCCount step. Returning null will enact vanilla behavior, and returning true will make the
	/// step succeed regardless. Returns null by default.
	/// </summary>
	/// <remarks>
	/// Note that you may also change the default npc count value if for some reason you wish to change the default
	/// amount of NPCs required to satisfy ALL pylons (if they adhere to what value it is).
	/// </remarks>
	/// <param name="pylonInfo"> The internal information pertaining to the current pylon being teleported to or from. </param>
	/// <param name="defaultNecessaryNPCCount"> The default amount of NPCs nearby required to satisfy a VANILLA pylon. </param>
	public virtual bool? ValidTeleportCheck_PreNPCCount(TeleportPylonInfo pylonInfo, ref int defaultNecessaryNPCCount)
	{
		return null;
	}

	/// <summary>
	/// Called before Step 2 of the ValidTeleportCheck process. This is the second vanilla check that is called when
	/// the player attempts to teleport TO a Pylon. This method is called before both vanilla and
	/// modded pylons check their Danger requirements, and returning false will prevent those checks from taking place,
	/// forcefully failing the AnyDanger step. Returning null will enact vanilla behavior, and returning true will make the
	/// step succeed regardless. Returns null by default.
	/// </summary>
	/// <remarks>
	/// Note that it's important you put the right checks in the right ValidTeleportCheck step,
	/// as whatever one returns false (if any) will determine the error message sent to the player.
	/// </remarks>
	/// <param name="pylonInfo"> The internal information pertaining to the current pylon being teleported TO. </param>
	public virtual bool? ValidTeleportCheck_PreAnyDanger(TeleportPylonInfo pylonInfo)
	{
		return null;
	}

	/// <summary>
	/// Called before Step 3 of the ValidTeleportCheck process. This is the fourth vanilla check that is called when
	/// the player attempts to teleport FROM or TO a Pylon. This method is called before both vanilla and
	/// modded pylons check their Biome requirements, and returning false will prevent those checks from taking place,
	/// forcefully failing the BiomeRequirements step. Returning null will enact vanilla behavior, and returning true will make the
	/// step succeed regardless. Returns null by default.
	/// </summary>
	/// <remarks>
	/// Note that it's important you put the right checks in the right ValidTeleportCheck step,
	/// as whatever one returns false (if any) will determine the error message sent to the player.
	/// </remarks>
	/// <param name="pylonInfo"> The internal information pertaining to the current pylon being teleported to or from. </param>
	/// <param name="sceneData"> The scene metrics data AT THE LOCATION of the destination pylon, NOT the player. </param>
	public virtual bool? ValidTeleportCheck_PreBiomeRequirements(TeleportPylonInfo pylonInfo, SceneMetrics sceneData)
	{
		return null;
	}

	/// <summary>
	/// Called after all checks have taken place for all vanilla and modded pylons when a teleportation request is sent (AKA when the player clicks
	/// on a pylon on the map). You can (for example) use this to allow teleportation to go through even if one the normal requirements haven't been
	/// met, or vice versa; to do this, change destinationPylonValid to true or false respectively (for the given example).
	/// </summary>
	/// <param name="destinationPylonInfo"> The pylon information for the DESTINATION pylon. </param>
	/// <param name="nearbyPylonInfo">
	/// The pylon information of the pylon the player in question is standing NEAR. This value is at its defaults if
	/// validNearbyPylonFound is false, but otherwise, will give information on the nearby pylon.
	/// </param>
	/// <param name="destinationPylonValid">
	/// Whether or not the normal requirements were satisfied for the DESTINATION pylon.
	/// Set this to true if you want the teleportation request to succeed, false if not.
	/// </param>
	/// <param name="validNearbyPylonFound"> Whether or not a valid pylon near the player satisfied its normal requirements. </param>
	/// <param name="errorKey">
	/// The localization key that will be used to sent text to the player when destinationPylonValid is false.
	/// Note that this parameter will already have a value if the method is called with destinationPylonValid being false.
	/// </param>
	public virtual void PostValidTeleportCheck(TeleportPylonInfo destinationPylonInfo, TeleportPylonInfo nearbyPylonInfo, ref bool destinationPylonValid, ref bool validNearbyPylonFound, ref string errorKey) { }

	protected sealed override void Register() => PylonLoader.globalPylons.Add(this);

	public sealed override void SetupContent() => SetStaticDefaults();
}
