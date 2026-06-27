using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ExampleMod.Content.Tiles
{
	// This tile serves as a showcase for TileObjectData.
	// In particular, this contrived example shows how styles are laid out in the spritesheet when multiple styles, multiple alternate placements, random style range, animations, and toggle states are all desired.
	// If you place this tile, you'll noticed that it has both left and right variants depending on the player direction. You'll also notice that there are 4 random style variations for left and right. Once placed, the tile will animate through 3 frames of animation. Right clicking on the tile will change the tile to an "off" state, halting the animation and showing the 4th frame of animation. There are 4 tile styles contained in this example as well.
	// The StyleMultiplier section of the Tile wiki page, https://github.com/tModLoader/tModLoader/wiki/Basic-Tile#stylemultiplier, has a simpler visualization only showing alternate placements and random style variations.
	// Please experiment by placing this tile using both the "TileObjectData Showcase Style 3 - ExampleBlock" item and one of the other TileObjectData Showcase items. This tile anchors to specific tiles to the left and right, you'll need to place this tile between pillars of those specific tiles. By doing this you should be able to visualize the full potential of TileObjectData.
	// Not many tiles will require such complicated layout, but this serves as example of how each feature affects the resulting spritesheet.
	// Since this tile is "StyleHorizontal = true", styles in the spritesheet are positioned left to right. Each alternate placement and
	// random style are also placed in-line with the styles. Toggled states and animations are placed vertically below their corresponding placement. In the corresponding spritesheet, the styles, alternate placements, and animation frames are all labeled to make this layout clearer.
	// After reaching the wrap limit, subsequent styles are placed on the next row.
	public class TileObjectDataShowcase : ModTile
	{
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;

			// First setup a basic 2x2 tile.
			TileObjectData.newTile.UsesCustomCanPlace = true;
			TileObjectData.newTile.Width = 2;
			TileObjectData.newTile.Height = 2;
			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.CoordinateHeights = [16, 16];
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.Origin = new Point16(0, 1);
			TileObjectData.newTile.CoordinatePadding = 0; // This is used to keep the spritesheet legible for this example.

			// These define how multiple styles and alternate placements will be located in the spritesheet
			TileObjectData.newTile.StyleMultiplier = 8; // Each style will occupy 8 placement styles
			TileObjectData.newTile.RandomStyleRange = 4; // We have a left and right placement, each has 4 random varieties. Look for "Alt 0", "Alt 1", "Alt 2", and "Alt 3" in the spritesheet.
			TileObjectData.newTile.StyleWrapLimit = 16; // We will wrap to the next line in the texture after 16 placement styles, or 2 styles.
			TileObjectData.newTile.StyleLineSkip = 4; // This gives extra lines in the spritesheet for animation or tile states.

			// Here we declare that the tile will be placeable when facing left.
			TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft;
			// The tile only anchors between specific tiles to the left and right. These are defined in each Subtile below.
			TileObjectData.newTile.AnchorLeft = new AnchorData(AnchorType.AlternateTile, TileObjectData.newTile.Height, 0);
			TileObjectData.newTile.AnchorRight = new AnchorData(AnchorType.AlternateTile, TileObjectData.newTile.Height, 0);

			TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(PostPlaceMethod, -1, 0, true); // Just for fun.

			// Now we make a copy newTile to populate an alternate placement. This faces right instead of left.
			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight;
			TileObjectData.addAlternate(4); // This alternate starts at placement style 4 because the left alternate has 4 random placements. These alternate placements will be "Alt 4", "Alt 5", "Alt 6", and "Alt 7" in the spritesheet.

			// These additional alternates reuse the same placement styles of the the normal placement and alternate placements above, but have a different Origin to make placing the tile easier. The tile placement preview will seem to "snap" to valid locations. This is completely optional and serves as an example of how multiple alternates can share placement styles. With these additional alternates, the player can position the tile by the bottom left or bottom right corner of the tile. Try it out for yourself in-game to see. This is similar to how doors can be placed by placing the mouse in any of the 3 tiles of a doorway.
			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.newAlternate.Origin = new Point16(1, 1);
			TileObjectData.addAlternate(0);
			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight;
			TileObjectData.newAlternate.Origin = new Point16(1, 1);
			TileObjectData.addAlternate(4);

			// Next, we initialize subtiles. These are tile style specific tile properties.
			// Each of these subtiles define their own AnchorAlternateTiles array to showcase this capability, but subtiles are typically just used for water and lava behaviors. This tile anchors to these specific tiles to the left and right, meaning that pillars of those tiles are needed to place this tile into the world.
			// Look for "Sty 0", "Sty 1", and "Sty 2" in the spritesheet.
			TileObjectData.newSubTile.CopyFrom(TileObjectData.newTile);
			TileObjectData.newSubTile.LinkedAlternates = true;
			TileObjectData.newSubTile.AnchorAlternateTiles = [TileID.Gold];
			TileObjectData.addSubTile(0);

			TileObjectData.newSubTile.CopyFrom(TileObjectData.newTile);
			TileObjectData.newSubTile.LinkedAlternates = true;
			TileObjectData.newSubTile.AnchorAlternateTiles = [TileID.Silver];
			TileObjectData.addSubTile(1);

			TileObjectData.newSubTile.CopyFrom(TileObjectData.newTile);
			TileObjectData.newSubTile.LinkedAlternates = true;
			TileObjectData.newSubTile.AnchorAlternateTiles = [TileID.Copper];
			TileObjectData.addSubTile(2);

			TileObjectData.newSubTile.CopyFrom(TileObjectData.newTile);
			TileObjectData.newSubTile.LinkedAlternates = true;
			TileObjectData.newSubTile.AnchorAlternateTiles = [ModContent.TileType<ExampleBlock>()];
			TileObjectData.addSubTile(3);

			TileObjectData.addTile(Type);

			// We can automatically set the animation frame height from CoordinateFullHeight for any typical tile that uses the expected layout.
			AnimationFrameHeight = TileObjectData.GetTileData(Type, 0).CoordinateFullHeight;
		}

		// Displays various info about the tile placement in chat.
		private int PostPlaceMethod(int x, int y, int type, int style, int direction, int alternate) {
			// Note that alternate here is the alternate index, not the alternate placement style. We'll use some math to calculate the random offset and placement style values
			var tileData = TileObjectData.GetTileData(type, style, alternate);

			int alternatePlacement = -1;
			int unused = -1;
			TileObjectData.GetTileInfo(Main.tile[x, y], ref unused, ref alternatePlacement);

			Main.NewText($"Style: {style}, Alternate Offset: {tileData.Style}, Random Offset: {alternatePlacement - tileData.Style}, Placement Style: {alternatePlacement}, Full Placement Style: {style * tileData.StyleMultiplier + alternatePlacement}, Direction: {direction}, Alternate Index: {alternate}, Origin: ({tileData.Origin.X}, {tileData.Origin.Y})");

			return 0;
		}

		// When this tile is right clicked, it changes to a new state by changing TileFrameY. This "off" state is the "Fra 3" sprites in the spritesheet.
		public override bool RightClick(int i, int j) {
			SoundEngine.PlaySound(SoundID.Mech, new Vector2(i * 16, j * 16));

			// This math finds the top left corner of the tile.
			Tile tile = Main.tile[i, j];
			(int topX, int topY) = TileObjectData.TopLeft(i, j);

			// 96 is the Y position of the "Fra 3" sprites in the spritesheet. (32 * 3)
			short frameAdjustment = (short)(tile.TileFrameY % 128 >= 96 ? -96 : 96);

			for (int x = topX; x < topX + 2; x++) {
				for (int y = topY; y < topY + 2; y++) {
					Main.tile[x, y].TileFrameY += frameAdjustment;
				}
			}

			if (Main.netMode != NetmodeID.SinglePlayer) {
				NetMessage.SendTileSquare(-1, topX, topY, 2, 2);
			}

			return true;
		}

		public override void AnimateTile(ref int frame, ref int frameCounter) {
			// Cycle between frames 0, 1, and 2 every 16 ticks
			if (++frameCounter >= 16) {
				frameCounter = 0;
				frame = ++frame % 3;
			}
		}

		public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset) {
			var tile = Main.tile[i, j];
			// If the tile is "on", then the tile will animate between the "Fra 0", "Fra 1", and "Fra 2" sprites. This is already applied because we set AnimationFrameHeight in SetStaticDefaults and adjust frame in AnimateTile.

			// If the tile is "off", however, then we set frameYOffset to 0 to disable the automatic animation for this specific tile. 96 is the Y position of the "Fra 3" sprites in the spritesheet. (32 * 3)
			if (tile.TileFrameY % 128 >= 96) {
				frameYOffset = 0;
			}
		}
	}

	// These items place the 4 styles of this showcase tile. Experiment with placing these items to see how the tile works.
	public class TileObjectDataShowcaseStyle0 : ModItem
	{
		public override string Texture => $"ExampleMod/Content/Tiles/TileObjectDataShowcaseItemA";

		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<TileObjectDataShowcase>(), 0);
		}
	}

	public class TileObjectDataShowcaseStyle1 : ModItem
	{
		public override string Texture => $"ExampleMod/Content/Tiles/TileObjectDataShowcaseItemA";

		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<TileObjectDataShowcase>(), 1);
		}
	}

	public class TileObjectDataShowcaseStyle2 : ModItem
	{
		public override string Texture => $"ExampleMod/Content/Tiles/TileObjectDataShowcaseItemA";

		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<TileObjectDataShowcase>(), 2);
		}
	}

	public class TileObjectDataShowcaseStyle3 : ModItem
	{
		public override string Texture => $"ExampleMod/Content/Tiles/TileObjectDataShowcaseItemB";

		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<TileObjectDataShowcase>(), 3);
		}
	}
}
