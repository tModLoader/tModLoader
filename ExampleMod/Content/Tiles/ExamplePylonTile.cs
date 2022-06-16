using ExampleMod.Content.Items.Placeable;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.ObjectInteractions;
using Terraria.GameContent.Tile_Entities;
using Terraria.ID;
using Terraria.Localization;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.UI;

namespace ExampleMod.Content.Tiles
{
	/// <summary>
	/// An example for creating a Pylon, identical to how they function in Vanilla. Shows off <seealso cref="ModPylon"/>, an abstract
	/// extension of <seealso cref="ModTile"/> that has additional functionality for Pylon specific tiles.
	/// <br>
	/// If you are going to make multiple pylons that all act the same (like in Vanilla), it is recommended you make a base class
	/// with override functionality in order to prevent writing boilerplate. (For example, making a "CrystalTexture" property that you can
	/// override in order to streamline that process.)
	/// </br>
	/// </summary>
	public class ExamplePylonTile : ModPylon
	{
		public const int CrystalHorizontalFrameCount = 2;
		public const int CrystalVerticalFrameCount = 8;

		public Asset<Texture2D> crystalTexture;
		public Asset<Texture2D> mapIcon;

		public override void Load() {
			//We'll need these textures for later, it's best practice to cache them on load instead of continually requesting every draw call.
			crystalTexture = ModContent.Request<Texture2D>(Texture + "_Crystal");
			mapIcon = ModContent.Request<Texture2D>(Texture + "_MapIcon");
		}

		public override void SetStaticDefaults() {
			//This method is a nice shorthand to set all your defaults to exactly what vanilla uses for its Pylon tiles.
			SetToPylonDefaults();
		}

		//Must be true in order for our highlight texture to work.
		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) {
			return true;
		}

		public override bool RightClick(int i, int j) {
			//Just like vanilla pylons, upon right click, open the map and play a sound.
			Main.mapFullscreen = true;
			SoundEngine.PlaySound(SoundID.MenuOpen);
			return true;
		}

		public override void MouseOver(int i, int j) {
			//Show a little pylon icon on the mouse indicating we are hovering over it.
			Main.LocalPlayer.cursorItemIconEnabled = true;
			Main.LocalPlayer.cursorItemIconID = ModContent.ItemType<ExamplePylonItem>();
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			//We need to clean up after ourselves, since this is still a "unique" tile, separate from Vanilla Pylons, so we must kill the TileEntity.
			TETeleportationPylon.Kill(i, j);

			//Also, like other pylons, allow the 
			Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 3, 4, ModContent.ItemType<ExamplePylonItem>());
		}

		//Let's make it possible to teleport to this Pylon any time we want, as long as we're standing next to valid Pylon!
		public override bool ValidTeleportCheck_NPCCount(TeleportPylonInfo pylonInfo, int defaultNecessaryNPCCount) {
			return true;
		}

		//These two methods are the only ones we need to override, since they have default functionality that could potentially prevent teleportation.
		public override bool ValidTeleportCheck_AnyDanger(TeleportPylonInfo nearbyPylonInfo) {
			return true;
		}

		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
			//Basically, we only want the crystal to be drawn once, based off the top left corner, which is what this check is.
			//The top left corner of a Pylon will have its FrameX divisible by 54 (since it's 3 tiles wide, and each tile on the tile sheet takes up 18x18 pixels),
			//and its FrameY will be 0, since it's at the top of the tile sheet.
			if (drawData.tileFrameX % 54 == 0 && drawData.tileFrameY == 0) {
				//This method call basically says "Run SpecialDraw once at this position"
				Main.instance.TilesRenderer.AddSpecialLegacyPoint(i, j);
			}
		}

		public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch) {
			//This code is adapted vanilla code, the *exact* way that Vanilla draws the pylon crystals.

			//This is lighting-mode specific, always include this if you draw tiles manually
			Vector2 offScreen = new Vector2(Main.offScreenRange);
			if (Main.drawToScreen) {
				offScreen = Vector2.Zero;
			}

			//Take the tile, check if it actually exists
			Point pos = new Point(i, j);
			Tile tile = Main.tile[pos.X, pos.Y];
			if (tile == null || !tile.HasTile) {
				return;
			}

			//Here, lots of framing takes place. 
			Texture2D vanillaPylonCrystals = TextureAssets.Extra[181].Value; //The default textures for vanilla pylons, containing a "sheen" texture we need
			int frameY = (Main.tileFrameCounter[TileID.TeleportationPylon] + pos.X + pos.Y) % 64 / CrystalVerticalFrameCount; //Get the current frameY. All pylons share the same frameCounter
			//Next, frame the actual crystal texture, it's highlight texture, and the sheen texture, in that order.
			Rectangle crystalFrame = crystalTexture.Frame(CrystalHorizontalFrameCount, CrystalVerticalFrameCount, 0, frameY); 
			Rectangle highlightFrame = crystalTexture.Frame(CrystalHorizontalFrameCount, CrystalVerticalFrameCount, 1, frameY);
			vanillaPylonCrystals.Frame(CrystalHorizontalFrameCount, CrystalVerticalFrameCount, 0, frameY);
			//Calculate positional values in order to determine where to actually draw the pylon
			Vector2 origin = crystalFrame.Size() / 2f;
			Vector2 centerPos = pos.ToWorldCoordinates(24f, 64f);
			float centerDisplacement = (float)Math.Sin(Main.GlobalTimeWrappedHourly * ((float)Math.PI * 2f) / 5f);
			Vector2 drawPos = centerPos + offScreen + new Vector2(0f, -40f) + new Vector2(0f, centerDisplacement * 4f);
			//Randomly spawn dust, granted that the game is open an active
			if (!Main.gamePaused && Main.instance.IsActive && Main.rand.NextBool(40)) {
				Rectangle dustBox = Utils.CenteredRectangle(drawPos, crystalFrame.Size());

				//Customize this dust however you want, but this is the Vanilla Pylon dust spawning process.
				int dustIndex = Dust.NewDust(dustBox.TopLeft(), dustBox.Width, dustBox.Height, DustID.TintableDustLighted, 0f, 0f, 254, Color.Gray, 0.5f);
				Main.dust[dustIndex].velocity *= 0.1f;
				Main.dust[dustIndex].velocity.Y -= 0.2f;
			}

			//Next, calculate the color of the crystal and draw it. The color is determined by how lit the tile is at that position.
			Color crystalColor = Color.Lerp(Lighting.GetColor(pos.X, pos.Y), Color.White, 0.8f) ;
			Main.spriteBatch.Draw(crystalTexture.Value, drawPos - Main.screenPosition, crystalFrame, crystalColor * 0.7f, 0f, origin, 1f, SpriteEffects.None, 0f);

			//Next, calculate the color of the sheen texture and its scale, then draw it.
			float scale = (float)Math.Sin(Main.GlobalTimeWrappedHourly * (Math.PI * 2f) / 1f) * 0.2f + 0.8f;
			Color sheenColor = new Color(255, 255, 255, 0) * 0.1f * scale;
			for (float displacement = 0f; displacement < 1f; displacement += 355f / (678f * (float)Math.PI)) {
				Main.spriteBatch.Draw(crystalTexture.Value, drawPos - Main.screenPosition + ((float)Math.PI * 2f * displacement).ToRotationVector2() * (6f + centerDisplacement * 2f), crystalFrame, sheenColor, 0f, origin, 1f, SpriteEffects.None, 0f);
			}

			//Finally, everything below is for the smart cursor, drawing the highlight texture depending on whether or not either:
			//1) Smart Cursor is off, and no highlight texture is drawn at all (selectionType = 0)
			//2) Smart Cursor is on, but it is not currently focusing on the Pylon (selectionType = 1)
			//3) Smart Cursor is on, but it IS currently focusing on the Pylon (selectionType = 2)
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

			//Finally, draw the highlight texture, if applicable.
			int colorPotency = (crystalColor.R + crystalColor.G + crystalColor.B) / 3;
			if (colorPotency > 10) {
				Color selectionGlowColor = Colors.GetSelectionGlowColor(selectionType == 2, colorPotency);
				Main.spriteBatch.Draw(crystalTexture.Value, drawPos - Main.screenPosition, highlightFrame, selectionGlowColor, 0f, origin, 1f, SpriteEffects.None, 0f);
			}
		}

		public override bool DrawMapIcon(ref MapOverlayDrawContext context, ref string mouseOverText, TeleportPylonInfo pylonInfo, Color drawColor, float deselectedScale, float selectedScale) {
			bool isMouseOver = context.Draw(
				                          mapIcon.Value,
				                          pylonInfo.PositionInTiles.ToVector2() + new Vector2(1.5f, 2f),
				                          drawColor,
				                          new SpriteFrame(1, 1, 0, 0),
				                          deselectedScale,
				                          selectedScale,
				                          Alignment.Center
			                          )
			                          .IsMouseOver;

			//Only change the text if we are mousing over; we don't want the text to always be displaying.
			if (isMouseOver) {
				mouseOverText = Language.GetTextValue("Mods.ExampleMod.ItemName.ExamplePylonItem");
			}

			return isMouseOver;
		}
	}
}
