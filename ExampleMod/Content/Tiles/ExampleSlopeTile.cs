using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Tiles
{
	// A tile demonstrating a non-solid non-terrain tile that can be slopped with hammers.
	// This tile also acts as a one-way sensor, sending a wire signal when a player passes through it in the indicated direction.
	public class ExampleSlopeTile : ModTile
	{
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			TileID.Sets.CanBeSloped[Type] = true; // allow this tile to be sloped, because it isn't solid
		}

		public override bool Slope(int i, int j) {
			Tile tile = Framing.GetTileSafely(i, j);
			tile.TileFrameX = (short)((tile.TileFrameX + 18) % 72);
			SoundEngine.PlaySound(SoundID.MenuTick);
			return false;
		}

		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
			return false;
		}

		// This method demonstrates the SwitchTiles hook and is not necessary for a basic non-Solid tile that can be sloped.
		public override bool SwitchTiles(int i, int j, Entity entity, Vector2 position, int width, int height, Vector2 oldPosition, int objType) {
			// This example demonstrates using SwitchTiles for tile collision interaction. This example sends a wire signal when a player travels through this tile in the direction of the arrow.

			if (entity is not Player player) {
				// Player only. We could also just check if (objType != 1), but we use the Player instance in this example.
				return false;
			}

			if (player.invis) {
				return false; // If the Invisibility buff is active, don't trigger
			}

			var tileCoordinates = new Rectangle(i * 16, j * 16, 16, 16);
			var oldPlayerHitbox = new Rectangle((int)oldPosition.X, (int)oldPosition.Y, width, height);
			var playerHitbox = new Rectangle((int)position.X, (int)position.Y, width, height);

			// If the player wasn't in the tile last update, don't do anything.
			if (!tileCoordinates.Intersects(oldPlayerHitbox)) {
				return false;
			}

			// Check if the player is now past the bounds of this tile
			bool success = (Main.tile[i, j].TileFrameX / 18) switch {
				0 => playerHitbox.Left >= tileCoordinates.Right,
				1 => playerHitbox.Top >= tileCoordinates.Bottom,
				2 => playerHitbox.Right <= tileCoordinates.Left,
				3 => playerHitbox.Bottom <= tileCoordinates.Top,
				_ => false,
			};

			if (success) {
				// Since SwitchTiles is called locally, rather than calling Wiring.TripWire(i, j, 1, 1); here, we delegate that work to HitSwitch which is called on the server and all clients if we use Wiring.HitSwitchAndSync.
				Wiring.HitSwitchAndSync(i, j);
				return true;
			}

			return false;
		}

		public override void HitSwitch(int i, int j) {
			// These will run on the server and all clients, to sync their effects.
			Wiring.TripWire(i, j, 1, 1); // TripWire only has effect on the server, but it is fine to call it regardless.
			Dust.NewDust(new Vector2(i * 16, j * 16), 16, 16, DustID.GreenFairy);
			SoundEngine.PlaySound(SoundID.Unlock, new Vector2(i * 16, j * 16));
		}
	}
}
