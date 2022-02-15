using ExampleMod.Content.Dusts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ExampleMod.Content.Tiles.Furniture
{
	public class ExampleChair : ModTile
	{
		public const int NextStyleHeight = 40; // Calculated by adding all CoordinateHeights + CoordinatePaddingFix.Y Applied to all of them + 2

		public override void SetStaticDefaults() {
			// Properties
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = true;
			TileID.Sets.HasOutlines[Type] = true;
			TileID.Sets.CanBeSatOnForNPCs[Type] = true; // Only supports 1x2 chairs/toilets. Doesn't work for anything else that deviates from the structure of these tiles
			TileID.Sets.CanBeSatOnForPlayers[Type] = true; // Needed so that ModifySittingTargetInfo is called
			TileID.Sets.DisableSmartCursor[Type] = true;

			AddToArray(ref TileID.Sets.RoomNeeds.CountsAsChair);

			DustType = ModContent.DustType<Sparkle>();
			AdjTiles = new int[] { TileID.Chairs };

			// Names
			AddMapEntry(new Color(200, 200, 200), Language.GetText("MapObject.Chair"));

			// Placement
			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2);
			TileObjectData.newTile.CoordinateHeights = new[] { 16, 18 };
			TileObjectData.newTile.CoordinatePaddingFix = new Point16(0, 2);
			TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft;
			// The following 3 lines are needed if you decide to add more styles and stack them vertically
			TileObjectData.newTile.StyleWrapLimit = 2;
			TileObjectData.newTile.StyleMultiplier = 2;
			TileObjectData.newTile.StyleHorizontal = true;

			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight;
			TileObjectData.addAlternate(1); // Facing right will use the second texture style
			TileObjectData.addTile(Type);
		}

		public override void NumDust(int i, int j, bool fail, ref int num) {
			num = fail ? 1 : 3;
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			Item.NewItem(i * 16, j * 16, 16, 32, ModContent.ItemType<Items.Placeable.Furniture.ExampleChair>());
		}

		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) {
			return settings.player.IsWithinSnappngRangeToTile(i, j, 40); // Avoid being able to trigger it from long range
		}

		public override void ModifySmartInteractCoords(ref int width, ref int height, ref int frameWidth, ref int frameHeight, ref int extraY) {
			// See ExampleBed tile for explanation
			extraY = 4;
		}

		public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
			if (tileFrameY % NextStyleHeight == 18) {
				height = 18;
			}
		}

		public override void ModifySittingTargetInfo(int i, int j, ref int sitX, ref int sitY, ref int directionOffset, ref int targetDirection, ref Vector2 seatDownOffset, ref Vector2 zero) {
			Tile tile = Framing.GetTileSafely(i, j);

			directionOffset = 6; // Default to 6
			seatDownOffset = Vector2.Zero; // Defaults to (0,0)

			targetDirection = -1;
			if (tile.TileFrameX != 0) {
				targetDirection = 1; // Facing right if sat down on the right alternate (added through addAlternate earlier)
			}

			if (tile.TileFrameY % NextStyleHeight != 0) {
				sitY--; // If clicked on anything but the top tile of the frame, move a tile up
			}
		}

		public override bool RightClick(int i, int j) {
			Player player = Main.LocalPlayer;

			if (player.IsWithinSnappngRangeToTile(i, j, 40)) { // Avoid being able to trigger it from long range
				player.GamepadEnableGrappleCooldown();
				player.sitting.SitDown(player, i, j);
			}

			return true;
		}

		public override void MouseOver(int i, int j) {
			Player player = Main.LocalPlayer;
			if (!player.IsWithinSnappngRangeToTile(i, j, 40)) { // Match condition in RightClick. Interaction should only show if clicking it does something
				return;
			}

			player.noThrow = 2;
			player.cursorItemIconEnabled = true;
			player.cursorItemIconID = ModContent.ItemType<Items.Placeable.Furniture.ExampleChair>();

			if (Main.tile[i, j].TileFrameX / 18 < 1) {
				player.cursorItemIconReversed = true;
			}
		}
	}
}
