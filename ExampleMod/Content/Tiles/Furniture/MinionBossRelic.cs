using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ExampleMod.Content.Tiles.Furniture
{
	// Common code for a Master Mode boss relic
	// Supports optional Item.placeStyle handling if you wish to add more relics but use the same tile type (then it would be wise to name this class something more generic like BossRelic)
	// If you want to add more relics but don't want to use the Item.placeStyle approach, see the inheritance example at the bottom of the file
	public class MinionBossRelic : ModTile
	{
		public const int FrameWidth = 18 * 3;
		public const int FrameHeight = 18 * 4;
		public const int HorizontalFrames = 1;
		public const int VerticalFrames = 1; // Optional: Increase this number to match the amount of relics you have on your extra sheet, if you choose to use the Item.placeStyle approach

		public Asset<Texture2D> RelicTexture;

		// Every relic has its own extra floating part, should be 50x50. Optional: Expand this sheet if you want to add more, stacked vertically
		// If you do not use the Item.placeStyle approach, and you extend from this class, you can override this to point to a different texture
		public virtual string RelicTextureName => "ExampleMod/Content/Tiles/Furniture/MinionBossRelic";

		// All relics use the same pedestal texture, this one is copied from vanilla
		public override string Texture => "ExampleMod/Content/Tiles/Furniture/RelicPedestal";

		public override void Load() {
			// Cache the extra texture displayed on the pedestal
			RelicTexture = ModContent.Request<Texture2D>(RelicTextureName);
		}

		public override void SetStaticDefaults() {
			Main.tileShine[Type] = 400; // Responsible for golden particles
			Main.tileFrameImportant[Type] = true; // Any multitile requires this
			TileID.Sets.InteractibleByNPCs[Type] = true; // Town NPCs will palm their hand at this tile

			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x4); // Relics are 3x4
			TileObjectData.newTile.LavaDeath = false; // Does not break when lava touches it
			TileObjectData.newTile.DrawYOffset = 2; // So the tile sinks into the ground
			TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft; // Player faces to the left
			TileObjectData.newTile.StyleHorizontal = false; // Based on how the alternate sprites are positioned on the sprite (by default, true)

			// This controls how styles are laid out in the texture file. This tile is special in that all styles will use the same texture section to draw the pedestal.
			TileObjectData.newTile.StyleWrapLimitVisualOverride = 2;
			TileObjectData.newTile.StyleMultiplier = 2;
			TileObjectData.newTile.StyleWrapLimit = 2;
			TileObjectData.newTile.styleLineSkipVisualOverride = 0; // This forces the tile preview to draw as if drawing the 1st style.

			// Register an alternate tile data with flipped direction
			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile); // Copy everything from above, saves us some code
			TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight; // Player faces to the right
			TileObjectData.addAlternate(1);

			// Register the tile data itself
			TileObjectData.addTile(Type);

			// Register map name and color
			// "MapObject.Relic" refers to the translation key for the vanilla "Relic" text
			AddMapEntry(new Color(233, 207, 94), Language.GetText("MapObject.Relic"));
		}

		public override bool CreateDust(int i, int j, ref int type) {
			return false;
		}

		public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
			// This forces the tile to draw the pedestal even if the placeStyle differs. 
			tileFrameX %= FrameWidth; // Clamps the frameX
			tileFrameY %= FrameHeight * 2; // Clamps the frameY (two horizontally aligned place styles, hence * 2)
		}

		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
			// Since this tile does not have the hovering part on its sheet, we have to animate it ourselves
			// Therefore we register the top-left of the tile as a "special point"
			// This allows us to draw things in SpecialDraw
			if (drawData.tileFrameX % FrameWidth == 0 && drawData.tileFrameY % FrameHeight == 0) {
				Main.instance.TilesRenderer.AddSpecialPoint(i, j, Terraria.GameContent.Drawing.TileDrawing.TileCounterType.CustomNonSolid); // This tile is not Main.tileSolid, so CustomNonSolid is the correct choice here.
			}
		}

		public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch) {
			// Take the tile, check if it actually exists
			Point p = new Point(i, j);
			Tile tile = Main.tile[p.X, p.Y];
			if (!tile.HasTile) {
				return;
			}

			// Get the initial draw parameters
			Texture2D texture = RelicTexture.Value;

			int frameY = tile.TileFrameX / FrameWidth; // Picks the frame on the sheet based on the placeStyle of the item
			Rectangle frame = texture.Frame(HorizontalFrames, VerticalFrames, 0, frameY);

			Vector2 origin = frame.Size() / 2f;
			Vector2 worldPos = p.ToWorldCoordinates(24f, 64f);

			Color color = Lighting.GetColor(p.X, p.Y);

			bool direction = tile.TileFrameY / FrameHeight != 0; // This is related to the alternate tile data we registered before
			SpriteEffects effects = direction ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

			// Some math magic to make it smoothly move up and down over time
			const float TwoPi = (float)Math.PI * 2f;
			float offset = (float)Math.Sin(Main.GlobalTimeWrappedHourly * TwoPi / 5f);
			Vector2 drawPos = worldPos - Main.screenPosition + new Vector2(0f, -40f) + new Vector2(0f, offset * 4f);

			// Draw the main texture
			spriteBatch.Draw(texture, drawPos, frame, color, 0f, origin, 1f, effects, 0f);

			// Draw the periodic glow effect
			float scale = (float)Math.Sin(Main.GlobalTimeWrappedHourly * TwoPi / 2f) * 0.3f + 0.7f;
			Color effectColor = color;
			effectColor.A = 0;
			effectColor = effectColor * 0.1f * scale;
			for (float num5 = 0f; num5 < 1f; num5 += 355f / (678f * (float)Math.PI)) {
				spriteBatch.Draw(texture, drawPos + (TwoPi * num5).ToRotationVector2() * (6f + offset * 2f), frame, effectColor, 0f, origin, 1f, effects, 0f);
			}
		}

		// One drawback of relic tiles is that the placement preview doesn't show the relic itself (only the pedestal) since the relic would normally be manually drawn. We can use PostDrawPlacementPreview to draw the relic during tile placement.
		public override void PostDrawPlacementPreview(int i, int j, SpriteBatch spriteBatch, Rectangle frame, Vector2 position, Color color, bool validPlacement, SpriteEffects spriteEffects) {
			// Adjust the draw coordinates in case the preview is drawing the placement facing right.
			bool facingRight = frame.Y / FrameHeight != 0;
			spriteEffects = facingRight ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			frame.Y %= FrameHeight;

			// Convert from tile coordinates with tile padding to the corresponding RelicTexture coordinates.
			frame.X = frame.X / 18 * 16;
			frame.Y = frame.Y / 18 * 16;
			if (facingRight && frame.X != 16) {
				// We also need to swap the 1st and 3rd columns to render correctly.
				frame.X = frame.X == 0 ? 32 : 0;
			}

			// Finally, manually draw a section of the relic texture. Note that color will be White or Red so the individual sections that conflict with existing tiles will show as Red, just like the normal placement preview using the actual tile texture.
			spriteBatch.Draw(RelicTexture.Value, position, frame, color, 0f, Vector2.Zero, 1f, spriteEffects, 0f);
		}
	}

	// If you want to make more relics but do not use the Item.placeStyle approach, you can use inheritance to avoid using duplicate code:
	// Your tile code would then inherit from the MinionBossRelic class (which you should make abstract) and should look like this:
	/*
	public class MyBossRelic : MinionBossRelic
	{
		public override string RelicTextureName => "ExampleMod/Content/Tiles/Furniture/MyBossRelic";

		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
		}
	}
	*/

	// Your item code would then just use the MyBossRelic tile type, and keep placeStyle on 0
	// The textures for MyBossRelic item/tile have to be supplied separately
}
