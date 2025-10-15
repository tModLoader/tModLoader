using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ExampleMod.Content.Tiles
{
	// This is an example of a pressure plate tile, specifically a "weighted" pressure plate. Due to how weighted pressure plates are implemented, they only work for player collisions. Comments below detail the small changes necessary to convert this to a non-weighted pressure plate tile.
	// Custom pressure plates are straightforward and are mostly useful for matching a biome color scheme.
	// If you are looking to make a tile with custom entity collision logic, see ExampleSlopeTile's SwitchTiles method.
	public class ExamplePressurePlate : ModTile
	{
		public override void SetStaticDefaults() {
			// Indicates that this is a weighted pressure plate, which will cause HitSwitch (below) to be called when a player collides with it. Other pressure plate behaviors have different values indicated in TileID.Sets.PressurePlate documentation.
			TileID.Sets.PressurePlate[Type] = -3;

			TileID.Sets.IsATrigger[Type] = true;
			Main.tileFrameImportant[Type] = true;
			Main.tileObsidianKill[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
			TileObjectData.newTile.CoordinateHeights = [18];
			TileObjectData.newTile.CoordinatePadding = 0;
			TileObjectData.newTile.DrawYOffset = 2;
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.addTile(Type);
		}

		public override bool IsTileDangerous(int i, int j, Player player) => true;

		public override void HitSwitch(int i, int j) {
			// HitSwitch in this case is being called since this tile uses TileID.Sets.PressurePlate. See ExampleSlopedTile for a manual example.
			SoundEngine.PlaySound(SoundID.Mech, new Vector2(i * 16, j * 16));
			Wiring.TripWire(i, j, 1, 1);
		}

		// To make a non-weighted pressure plate tile, remove the following methods and adjust the sprite. You'll also need to adjust TileID.Sets.PressurePlate according to its documentation.

		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) {
			if (!fail) {
				PressurePlateHelper.DestroyPlate(new Point(i, j)); // Handles sending a signal if mined while standing on it.
			}
		}

		public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
			// Draw 2 pixels lower. This affects the tile in the world, while TileObjectData.DrawYOffset affects both the world and placement preview. This gives the impression of the tile being embedded into the tile when placed, but still easily visible in the placement preview.
			offsetY += 2;
		}

		public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset) {
			if (PressurePlateHelper.PressurePlatesPressed.ContainsKey(new Point(i, j))) {
				frameXOffset += 18; // If currently pressed, draw the pressed sprite.
			}
		}
	}
}
