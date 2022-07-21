using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.ObjectInteractions;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.Map;
using Terraria.ObjectData;
using Terraria.UI;

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
		/// otherwise true. If you want to allow an infinite amount of these pylons to be placed,
		/// simply always return true.
		/// </summary>
		/// <remarks>
		/// Note that in Multiplayer environments, granted that any GlobalPylon instances do not return false in <seealso cref="GlobalPylon.PreCanPlacePylon"/>,
		/// this is called first on the client, and then is subsequently called &amp; double checked on the server.
		/// <br>If the server disagrees with the client that the given pylon CANNOT be placed for any given reason, the server will reject the placement
		/// and subsequently break the associated tile.</br>
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

		public override bool RightClick(int i, int j) {
			// Vanilla has a very handy function we can use that automatically opens the map, closes the inventory, plays a sound, etc:
			Main.LocalPlayer.TryOpeningFullscreenMap();
			return true;
		}

		// Must be true in order for our highlight texture to work.
		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) {
			return true;
		}

		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
			// Basically, we only want the crystal to be drawn once, based off the top left corner, which is what this check is.
			// The top left corner of a Pylon will have its FrameX divisible by its full pixel width,
			// and its FrameY will be 0, since it's at the top of the tile sheet.
			if (drawData.tileFrameX % TileObjectData.GetTileData(drawData.tileCache).CoordinateFullWidth == 0 && drawData.tileFrameY == 0) {
				//This method call basically says "Run SpecialDraw once at this position"
				Main.instance.TilesRenderer.AddSpecialLegacyPoint(i, j);
			}
		}

		/// <summary>
		/// Draws the passed in pylon crystal texture the exact way that vanilla draws it. This MUST be called in SpecialDraw in order
		/// to function properly.
		/// </summary>
		/// <param name="spriteBatch"> The sprite batch that will draw the crystal. </param>
		/// <param name="i"> The X tile coordinate to start drawing from. </param>
		/// <param name="j"> The Y tile coordinate to star drawing from. </param>
		/// <param name="crystalTexture"> The texture of the crystal that will actually be drawn. </param>
		/// <param name="crystalDrawColor"> The color to draw the crystal as. </param>
		/// <param name="frameHeight"> The height of a singular frame of the crystal. 64 for a normal pylon crystal in vanilla.</param>
		/// <param name="crystalHorizontalFrameCount">The total frames wide the crystal texture has. </param>
		/// <param name="crystalVerticalFrameCount"> The total frames high the crystal texture has. </param>
		public void DefaultDrawPylonCrystal(SpriteBatch spriteBatch, int i, int j, Asset<Texture2D> crystalTexture, Color crystalDrawColor, int frameHeight, int crystalHorizontalFrameCount, int crystalVerticalFrameCount) {
			// This is lighting-mode specific, always include this if you draw tiles manually
			Vector2 offScreen = new Vector2(Main.offScreenRange);
			if (Main.drawToScreen) {
				offScreen = Vector2.Zero;
			}

			// Take the tile, check if it actually exists
			Point pos = new Point(i, j);
			Tile tile = Main.tile[pos.X, pos.Y];
			if (tile == null || !tile.HasTile) {
				return;
			}
			TileObjectData tileData = TileObjectData.GetTileData(tile);

			// Here, lots of framing takes place. 
			Texture2D vanillaPylonCrystals = TextureAssets.Extra[181].Value; //The default textures for vanilla pylons, containing a "sheen" texture we need
			int frameY = (Main.tileFrameCounter[TileID.TeleportationPylon] + pos.X + pos.Y) % frameHeight / crystalVerticalFrameCount; // Get the current frameY. All pylons share the same frameCounter
			// Next, frame the actual crystal texture, it's highlight texture, and the sheen texture, in that order.
			Rectangle crystalFrame = crystalTexture.Frame(crystalHorizontalFrameCount, crystalVerticalFrameCount, 0, frameY); 
			Rectangle highlightFrame = crystalTexture.Frame(crystalHorizontalFrameCount, crystalVerticalFrameCount, 1, frameY);
			vanillaPylonCrystals.Frame(crystalHorizontalFrameCount, crystalVerticalFrameCount, 0, frameY);
			// Calculate positional values in order to determine where to actually draw the pylon
			Vector2 origin = crystalFrame.Size() / 2f;
			Vector2 centerPos = pos.ToWorldCoordinates(0f, 0f) + new Vector2(tileData.Width / 2f * 16f, (tileData.Height / 2f + 1.5f) * 16f);
			float centerDisplacement = (float)Math.Sin(Main.GlobalTimeWrappedHourly * ((float)Math.PI * 2f) / 5f);
			Vector2 drawPos = centerPos + offScreen + new Vector2(0f, -40f) + new Vector2(0f, centerDisplacement * 4f);
			// Randomly spawn dust, granted that the game is open an active
			if (!Main.gamePaused && Main.instance.IsActive && Main.rand.NextBool(40)) {
				Rectangle dustBox = Utils.CenteredRectangle(drawPos, crystalFrame.Size());

				int dustIndex = Dust.NewDust(dustBox.TopLeft(), dustBox.Width, dustBox.Height, DustID.TintableDustLighted, 0f, 0f, 254, Color.Gray, 0.5f);
				Main.dust[dustIndex].velocity *= 0.1f;
				Main.dust[dustIndex].velocity.Y -= 0.2f;
			}

			// Next, calculate the color of the crystal and draw it. The color is determined by how lit the tile is at that position.
			Color crystalColor = Color.Lerp(Lighting.GetColor(pos.X, pos.Y), crystalDrawColor, 0.8f) ;
			spriteBatch.Draw(crystalTexture.Value, drawPos - Main.screenPosition, crystalFrame, crystalColor * 0.7f, 0f, origin, 1f, SpriteEffects.None, 0f);

			// Next, calculate the color of the sheen texture and its scale, then draw it.
			float scale = (float)Math.Sin(Main.GlobalTimeWrappedHourly * (Math.PI * 2f) / 1f) * 0.2f + 0.8f;
			Color sheenColor = new Color(255, 255, 255, 0) * 0.1f * scale;
			for (float displacement = 0f; displacement < 1f; displacement += 355f / (678f * (float)Math.PI)) {
				spriteBatch.Draw(crystalTexture.Value, drawPos - Main.screenPosition + ((float)Math.PI * 2f * displacement).ToRotationVector2() * (6f + centerDisplacement * 2f), crystalFrame, sheenColor, 0f, origin, 1f, SpriteEffects.None, 0f);
			}

			// Finally, everything below is for the smart cursor, drawing the highlight texture depending on whether or not either:
			// 1) Smart Cursor is off, and no highlight texture is drawn at all (selectionType = 0)
			// 2) Smart Cursor is on, but it is not currently focusing on the Pylon (selectionType = 1)
			// 3) Smart Cursor is on, but it IS currently focusing on the Pylon (selectionType = 2)
			int selectionType = 0;
			if (Main.InSmartCursorHighlightArea(pos.X, pos.Y, out bool actuallySelected)) {
				selectionType = 1;

				if (actuallySelected) {
					selectionType = 2;
				}
			}

			if (selectionType == 0) {
				return;
			}

			// Finally, draw the highlight texture, if applicable.
			int colorPotency = (crystalColor.R + crystalColor.G + crystalColor.B) / 3;
			if (colorPotency > 10) {
				Color selectionGlowColor = Colors.GetSelectionGlowColor(selectionType == 2, colorPotency);
				spriteBatch.Draw(crystalTexture.Value, drawPos - Main.screenPosition, highlightFrame, selectionGlowColor, 0f, origin, 1f, SpriteEffects.None, 0f);
			}
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
		public bool DefaultDrawMapIcon(ref MapOverlayDrawContext context, Asset<Texture2D> mapIcon, Vector2 drawCenter, Color drawColor, float deselectedScale, float selectedScale) {
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
		public void DefaultMapClickHandle(bool mouseIsHovering, TeleportPylonInfo pylonInfo, string hoveringTextKey, ref string mouseOverText) {
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
				SoundEngine.PlaySound(SoundID.Item6);
			}
		}
	}
}
