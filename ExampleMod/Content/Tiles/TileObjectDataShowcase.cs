using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ExampleMod.Content.Tiles
{
	// This tile serves as a showcase for TileObjectData.
	// In particular, this contrived example shows how styles are laid out in the spritesheet when multiple styles, multiple alternate placements, random style range, animations, and toggle states are all desired.
	// Not many tiles will require such complicated layout, but this serves as example of how each feature affects the resulting spritesheet.
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
			TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16 };
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.CoordinatePadding = 0; // This is used to keep the spritesheet legible for this example.

			// These define how multiple styles and alternate placements will be located in the spritesheet
			TileObjectData.newTile.StyleMultiplier = 8; // Each style will occupy 8 place styles
			TileObjectData.newTile.RandomStyleRange = 4; // We have a left and right placement, each have 4 random varieties. Look for "Alt 0", "Alt 1", "Alt 2", and "Alt 3" in the spritesheet.
			TileObjectData.newTile.StyleWrapLimit = 16; // We will wrap to the next line in the texture after 16 place styles, or 2 styles.
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
			TileObjectData.addAlternate(4); // This alternate starts at placestyle 4 because the left alternate has 4 random placements. These alternate placements will be "Alt 4", "Alt 5", "Alt 6", and "Alt 7" in the spritesheet.

			// Next, we initialize subtiles. These are tile style specific tile properties.
			// Each of these subtiles define their own AnchorAlternateTiles array, but subtiles are typically just used for water and lava behaviors.
			// Look for "Sty 0", "Sty 1", and "Sty 2" in the spritesheet.
			TileObjectData.newSubTile.CopyFrom(TileObjectData.newTile);
			TileObjectData.newSubTile.LinkedAlternates = true;
			TileObjectData.newSubTile.AnchorAlternateTiles = new int[] { TileID.Gold };
			TileObjectData.addSubTile(0);

			TileObjectData.newSubTile.CopyFrom(TileObjectData.newTile);
			TileObjectData.newSubTile.LinkedAlternates = true;
			TileObjectData.newSubTile.AnchorAlternateTiles = new int[] { TileID.Silver };
			TileObjectData.addSubTile(1);

			TileObjectData.newSubTile.CopyFrom(TileObjectData.newTile);
			TileObjectData.newSubTile.LinkedAlternates = true;
			TileObjectData.newSubTile.AnchorAlternateTiles = new int[] { TileID.Copper };
			TileObjectData.addSubTile(2);

			TileObjectData.addTile(Type);
		}

		private int PostPlaceMethod(int x, int y, int type, int style, int direction, int alternate) {
			Main.NewText($"Style: {style}, Direction: {direction}, Alternate: {alternate},");
			return 0;
		}

		// When this tile is right clicked, it changes to a new state by changing TileFrameY. This off state is the "Fra 3" sprites in the spritesheet.
		public override bool RightClick(int i, int j) {
			SoundEngine.PlaySound(SoundID.Mech, new Vector2(i * 16, j * 16));

			Tile tile = Main.tile[i, j];
			int topX = i - (tile.TileFrameX % 256) % 32 / 16;
			int topY = j - (tile.TileFrameY % 128) % 32 / 16;

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
			if (++frameCounter >= 16) {
				frameCounter = 0;
				frame = ++frame % 3;
			}
		}
		public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset) {
			var tile = Main.tile[i, j];
			if (tile.TileFrameY % 128 < 32) {
				// If the tile is "on", then the tile will animate between the "Fra 0", "Fra 1", and "Fra 2" sprites.
				frameYOffset = Main.tileFrame[type] * 32;
			}
		}
	}

	// These items place the 3 styles of this showcase tile.
	public class TileObjectDataShowcaseStyle0 : ModItem
	{
		public override string Texture => $"Terraria/Images/Item_{ItemID.RainbowBrick}";
		public override LocalizedText Tooltip => LocalizedText.Empty;
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<TileObjectDataShowcase>(), 0);
		}
	}

	public class TileObjectDataShowcaseStyle1 : ModItem
	{
		public override string Texture => $"Terraria/Images/Item_{ItemID.RainbowBrick}";
		public override LocalizedText Tooltip => LocalizedText.Empty;
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<TileObjectDataShowcase>(), 1);
		}
	}

	public class TileObjectDataShowcaseStyle2 : ModItem
	{
		public override string Texture => $"Terraria/Images/Item_{ItemID.RainbowBrick}";
		public override LocalizedText Tooltip => LocalizedText.Empty;
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<TileObjectDataShowcase>(), 2);
		}
	}
}
