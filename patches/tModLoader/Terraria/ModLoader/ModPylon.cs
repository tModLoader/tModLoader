using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.ObjectInteractions;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.Map;
using Terraria.ObjectData;
using Terraria.UI;

namespace Terraria.ModLoader;

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
/// 7) The game queries all pylons on the map and checks if any of them are in interaction distance with the player (<seealso cref="Player.InInteractionRange"/>), and if so, checks Step 2 on it. If Step 2 passes, Step 5 is then called on it as well (NPCCount &amp; BiomeRequirements step).
/// If Step 5 also passes, the loop breaks and no further pylons are checked, and for the next steps, the pylon that succeeded will be the designated NEARBY PYLON.
/// <br></br>
/// 8) Regardless of all the past checks, if the designated NEARBY PYLON is a modded one, <seealso cref="ValidTeleportCheck_NearbyPostCheck"/> is called on it.
/// <br></br>
/// 9) Any <seealso cref="GlobalPylon"/> instances run <seealso cref="GlobalPylon.PostValidTeleportCheck"/>.
/// <br></br>
/// 10) Finally, if all previous checks pass AND the DESTINATION pylon is a modded one, <seealso cref="ModifyTeleportationPosition"/> is called on it, right before the player is teleported.
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
	/// otherwise true. If you want to allow an infinite amount of these pylons to be placed,
	/// simply always return true.
	/// </summary>
	/// <remarks>
	/// Note that in Multiplayer environments, granted that any GlobalPylon instances do not return false in <seealso cref="GlobalPylon.PreCanPlacePylon"/>,
	/// this is called first on the client, and then is subsequently called &amp; double checked on the server.
	/// <br>If the server disagrees with the client that the given pylon CANNOT be placed for any given reason, the server will reject the placement
	/// and subsequently break the associated tile.</br>
	/// </remarks>
	public virtual bool CanPlacePylon()
	{
		return !Main.PylonSystem.HasPylonOfType(PylonType);
	}

	/// <summary>
	/// Creates the npc shop entry which will be registered to the shops of all NPCs which can sell pylons. <br/>
	/// Override this to change the sold item type, or alter the conditions of sale. <br/>
	/// Return null to prevent automatically registering this pylon in shops. <br/>
	/// By default, the pylon will be sold in all shops when the provided conditions are met, if the pylon has a non-zero item drop.<br/>
	/// <br/>
	/// The standard pylon conditions are <see cref="Condition.HappyEnoughToSellPylons"/>, <see cref="Condition.AnotherTownNPCNearby"/>, <see cref="Condition.NotInEvilBiome"/>
	/// </summary>
	public virtual NPCShop.Entry GetNPCShopEntry()
	{
		// TODO: Handle this correctly once pylons support multiple styles.
		int drop = TileLoader.GetItemDropFromTypeAndStyle(Type);
		if (drop == 0)
			return null;

		return new NPCShop.Entry(drop, Condition.HappyEnoughToSellPylons, Condition.AnotherTownNPCNearby, Condition.NotInEvilBiome);
	}

	/// <summary>
	/// Step 1 of the ValidTeleportCheck process. This is the first vanilla check that is called when
	/// checking both the destination pylon and any possible nearby pylons. This check should be where you check
	/// how many NPCs are nearby, returning false if the Pylon does not satisfy the conditions.
	/// By default, returns true if there are 2 or more NPCs nearby.
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
	public virtual bool ValidTeleportCheck_NPCCount(TeleportPylonInfo pylonInfo, int defaultNecessaryNPCCount)
	{
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
	public virtual bool ValidTeleportCheck_AnyDanger(TeleportPylonInfo pylonInfo)
	{
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
	public virtual bool ValidTeleportCheck_BiomeRequirements(TeleportPylonInfo pylonInfo, SceneMetrics sceneData)
	{
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
	public virtual void ValidTeleportCheck_DestinationPostCheck(TeleportPylonInfo destinationPylonInfo, ref bool destinationPylonValid, ref string errorKey) { }

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
	/// Called right BEFORE the teleportation of the player occurs, when all checks succeed during the ValidTeleportCheck process. Allows the modification
	/// of where the player ends up when the teleportation takes place. Remember that the teleport location is in WORLD coordinates, not tile coordinates.
	/// </summary>
	/// <remarks>
	/// You shouldn't need to use this method if your pylon is the same size as a normal vanilla pylons (3x4 tiles).
	/// </remarks>
	/// <param name="destinationPylonInfo"> The information of the pylon the player intends to teleport to. </param>
	/// <param name="teleportationPosition"> The position (IN WORLD COORDINATES) of where the player ends up when the teleportation occurs. </param>
	public virtual void ModifyTeleportationPosition(TeleportPylonInfo destinationPylonInfo, ref Vector2 teleportationPosition) { }

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

	public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
	{
		// Adapted vanilla code from TETeleportationPylon in order to line up with vanilla's functionality.
		if (WorldGen.destroyObject)
			return false;

		int topLeftX = i;
		int topLeftY = j;
		Tile tileSafely = Framing.GetTileSafely(i, j);
		TileObjectData tileData = TileObjectData.GetTileData(tileSafely);
		bool shouldBreak = false;

		topLeftX -= tileSafely.frameX / tileData.CoordinateWidth % tileData.Width;
		topLeftY -= tileSafely.frameY / 18 % tileData.Height;

		int rightX = topLeftX + tileData.Width;
		int bottomY = topLeftY + tileData.Height;

		for (int x = topLeftX; x < rightX; x++) {
			for (int y = topLeftY; y < bottomY; y++) {
				Tile tile = Main.tile[x, y];
				if (!tile.HasTile || tile.type != Type) {
					shouldBreak = true;
					break;
				}
			}
		}

		for (int x = topLeftX; x < rightX; x++) {
			if (!WorldGen.SolidTileAllowBottomSlope(x, bottomY)) {
				shouldBreak = true;
				break;
			}
		}

		if (!shouldBreak) {
			noBreak = true;
			return true;
		}

		WorldGen.KillTile_DropItems(i, j, tileSafely, includeLargeObjectDrops: true, includeAllModdedLargeObjectDrops: true); // include all drops.
		KillMultiTile(topLeftX, topLeftY, tileSafely.TileFrameX, tileSafely.TileFrameY);
		WorldGen.destroyObject = true;
		for (int x = topLeftX; x < rightX; x++) {
			for (int y = topLeftY; y < bottomY; y++) {
				Tile tile = Main.tile[x, y];
				if (tile.HasTile && tile.TileType == Type)
					WorldGen.KillTile(x, y);
			}
		}
		WorldGen.destroyObject = false;

		return true;
	}

	public override bool RightClick(int i, int j)
	{
		// Vanilla has a very handy function we can use that automatically opens the map, closes the inventory, plays a sound, etc:
		Main.LocalPlayer.TryOpeningFullscreenMap();
		return true;
	}

	// Must be true in order for our highlight texture to work.
	public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
	{
		return true;
	}

	public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
	{
		// Basically, we only want the crystal to be drawn once, based off the top left corner, which is what this check is.
		// The top left corner of a Pylon will have its FrameX divisible by its full pixel width,
		// and its FrameY will be 0, since it's at the top of the tile sheet.
		if (drawData.tileFrameX % TileObjectData.GetTileData(drawData.tileCache).CoordinateFullWidth == 0 && drawData.tileFrameY == 0) {
			// This method call basically says "Run SpecialDraw once at this position"
			Main.instance.TilesRenderer.AddSpecialLegacyPoint(i, j);
		}
	}


	[Obsolete("Parameters have changed; parameters crystalDrawColor, frameHeight, and crystalHorizontalFrameCount no longer exist. There are 5 new parameters: crystalHighlightTexture, crystalOffset, pylonShadowColor, dustColor, and dustChanceDenominator.", true)]
	public void DefaultDrawPylonCrystal(SpriteBatch spriteBatch, int i, int j, Asset<Texture2D> crystalTexture, Color crystalDrawColor, int frameHeight, int crystalHorizontalFrameCount, int crystalVerticalFrameCount)
	{
		DefaultDrawPylonCrystal(spriteBatch, i, j, crystalTexture, crystalTexture, new Vector2(0, -12f), Color.White * 0.1f, Color.White, 4, crystalVerticalFrameCount);
	}

	/// <summary>
	/// Draws the passed in pylon crystal texture the exact way that vanilla draws it. This MUST be called in SpecialDraw in order
	/// to function properly.
	/// </summary>
	/// <param name="spriteBatch"> The sprite batch that will draw the crystal. </param>
	/// <param name="i"> The X tile coordinate to start drawing from. </param>
	/// <param name="j"> The Y tile coordinate to start drawing from. </param>
	/// <param name="crystalTexture"> The texture of the crystal that will actually be drawn. </param>
	/// <param name="crystalHighlightTexture"> The texture of the smart cursor highlight for the corresponding crystal texture. </param>
	/// <param name="crystalOffset">
	/// The offset of the actual position of the crystal. Assuming that a pylon tile itself and the crystals are equivalent to
	/// vanilla's sizes, this value should be Vector2(0, -12).
	/// </param>
	/// <param name="pylonShadowColor"> The color of the "shadow" that is drawn on top of the crystal texture. </param>
	/// <param name="dustColor"> The color of the dust that emanates from the crystal. </param>
	/// <param name="dustChanceDenominator"> Every draw call, this is this the denominator value of a Main.rand.NextBool() (1/denominator chance) check for whether or not a dust particle will spawn. 4 is the value vanilla uses. </param>
	/// <param name="crystalVerticalFrameCount"> How many vertical frames the crystal texture has. </param>
	public void DefaultDrawPylonCrystal(SpriteBatch spriteBatch, int i, int j, Asset<Texture2D> crystalTexture, Asset<Texture2D> crystalHighlightTexture, Vector2 crystalOffset, Color pylonShadowColor, Color dustColor, int dustChanceDenominator, int crystalVerticalFrameCount)
	{
		// Gets offscreen vector for different lighting modes
		Vector2 offscreenVector = new Vector2(Main.offScreenRange);
		if (Main.drawToScreen) {
			offscreenVector = Vector2.Zero;
		}

		// Double check that the tile exists
		Point point = new Point(i, j);
		Tile tile = Main.tile[point.X, point.Y];
		if (tile == null || !tile.HasTile) {
			return;
		}

		TileObjectData tileData = TileObjectData.GetTileData(tile);

		// Calculate frame based on vanilla counters in order to line up the animation
		int frameY = Main.tileFrameCounter[TileID.TeleportationPylon] / crystalVerticalFrameCount;

		// Frame our modded crystal sheet accordingly for proper drawing
		Rectangle crystalFrame = crystalTexture.Frame(1, crystalVerticalFrameCount, 0, frameY);
		Rectangle smartCursorGlowFrame = crystalHighlightTexture.Frame(1, crystalVerticalFrameCount, 0, frameY);
		// I have no idea what is happening here; but it fixes the frame bleed issue. All I know is that the vertical sinusoidal motion has something to with it.
		// If anyone else has a clue as to why, please do tell. - MutantWafflez
		crystalFrame.Height -= 1;
		smartCursorGlowFrame.Height -= 1;

		// Calculate positional variables for actually drawing the crystal
		Vector2 origin = crystalFrame.Size() / 2f;
		Vector2 tileOrigin = new Vector2(tileData.CoordinateFullWidth / 2f, tileData.CoordinateFullHeight / 2f);
		Vector2 crystalPosition = point.ToWorldCoordinates(tileOrigin.X - 2f, tileOrigin.Y) + crystalOffset;

		// Calculate additional drawing positions with a sine wave movement
		float sinusoidalOffset = (float)Math.Sin(Main.GlobalTimeWrappedHourly * (Math.PI * 2) / 5);
		Vector2 drawingPosition = crystalPosition + offscreenVector + new Vector2(0f, sinusoidalOffset * 4f);

		// Do dust drawing
		if (!Main.gamePaused && Main.instance.IsActive && (!Lighting.UpdateEveryFrame || Main.rand.NextBool(4)) && Main.rand.NextBool(dustChanceDenominator)) {
			Rectangle dustBox = Utils.CenteredRectangle(crystalPosition, crystalFrame.Size());
			int numForDust = Dust.NewDust(dustBox.TopLeft(), dustBox.Width, dustBox.Height, DustID.TintableDustLighted, 0f, 0f, 254, dustColor, 0.5f);
			Dust obj = Main.dust[numForDust];
			obj.velocity *= 0.1f;
			Main.dust[numForDust].velocity.Y -= 0.2f;
		}

		// Get color value and draw the crystal
		Color color = Lighting.GetColor(point.X, point.Y);
		color = Color.Lerp(color, Color.White, 0.8f);
		spriteBatch.Draw(crystalTexture.Value, drawingPosition - Main.screenPosition, crystalFrame, color * 0.7f, 0f, origin, 1f, SpriteEffects.None, 0f);

		// Draw the shadow effect for the crystal
		float scale = (float)Math.Sin(Main.GlobalTimeWrappedHourly * ((float)Math.PI * 2f) / 1f) * 0.2f + 0.8f;
		Color shadowColor = pylonShadowColor * scale;
		for (float shadowPos = 0f; shadowPos < 1f; shadowPos += 1f / 6f) {
			spriteBatch.Draw(crystalTexture.Value, drawingPosition - Main.screenPosition + ((float)Math.PI * 2f * shadowPos).ToRotationVector2() * (6f + sinusoidalOffset * 2f), crystalFrame, shadowColor, 0f, origin, 1f, SpriteEffects.None, 0f);
		}

		// Interpret smart cursor outline color & draw it
		int selectionLevel = 0;
		if (Main.InSmartCursorHighlightArea(point.X, point.Y, out bool actuallySelected)) {
			selectionLevel = 1;
			if (actuallySelected) {
				selectionLevel = 2;
			}
		}

		if (selectionLevel == 0) {
			return;
		}

		int averageBrightness = (color.R + color.G + color.B) / 3;

		if (averageBrightness <= 10) {
			return;
		}

		Color selectionGlowColor = Colors.GetSelectionGlowColor(selectionLevel == 2, averageBrightness);
		spriteBatch.Draw(crystalHighlightTexture.Value, drawingPosition - Main.screenPosition, smartCursorGlowFrame, selectionGlowColor, 0f, origin, 1f, SpriteEffects.None, 0f);
	}

	/// <summary>
	/// Draws the passed in map icon texture for pylons the exact way that vanilla would draw it. Note that this method
	/// assumes that the texture is NOT framed, i.e there is only a single sprite that is not animated.
	/// Returns whether or not the player is currently hovering over the icon.
	/// </summary>
	/// <param name="context"> The draw context that will allow for drawing on thj</param>
	/// <param name="mapIcon"> The icon that is to be drawn on the map. </param>
	/// <param name="drawCenter"> The position in TILE coordinates for where the CENTER of the map icon should be. </param>
	/// <param name="drawColor"> The color to draw the icon as. </param>
	/// <param name="deselectedScale"> The scale to draw the map icon when it is not selected (not being hovered over). </param>
	/// <param name="selectedScale"> The scale to draw the map icon when it IS selected (being hovered over). </param>
	public bool DefaultDrawMapIcon(ref MapOverlayDrawContext context, Asset<Texture2D> mapIcon, Vector2 drawCenter, Color drawColor, float deselectedScale, float selectedScale)
	{
		return context.Draw(
						  mapIcon.Value,
						  drawCenter,
						  drawColor,
						  new SpriteFrame(1, 1, 0, 0),
						  deselectedScale,
						  selectedScale,
						  Alignment.Center
						  )
					  .IsMouseOver;
	}

	/// <summary>
	/// Handles mouse clicking on the map icon the exact way that vanilla handles it. In normal circumstances, this should be called
	/// directly after DefaultDrawMapIcon.
	/// </summary>
	/// <param name="mouseIsHovering"> Whether or not the map icon is currently being hovered over. </param>
	/// <param name="pylonInfo"> The information pertaining to the current pylon being drawn. </param>
	/// <param name="hoveringTextKey">
	/// The localization key that will be used to display text on the mouse, granted the mouse is currently hovering over the map icon.
	/// </param>
	/// <param name="mouseOverText"> The reference to the string value that actually changes the mouse text value. </param>
	public void DefaultMapClickHandle(bool mouseIsHovering, TeleportPylonInfo pylonInfo, string hoveringTextKey, ref string mouseOverText)
	{
		// We only want these things to happen if the mouse is hovering, thus the check:
		if (!mouseIsHovering) {
			return;
		}

		Main.cancelWormHole = true;
		mouseOverText = Language.GetTextValue(hoveringTextKey);

		// If clicking, then teleport!
		if (Main.mouseLeft && Main.mouseLeftRelease) {
			Main.mouseLeftRelease = false;
			Main.mapFullscreen = false;
			PlayerInput.LockGamepadButtons("MouseLeft");
			Main.PylonSystem.RequestTeleportation(pylonInfo, Main.LocalPlayer);
		}
	}
}
