using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod.Content.Tiles
{
	// This class shows off a number of less common ModTile methods. These methods help our trap tile behave like vanilla traps.
	// In particular, hammer behavior is particularly tricky. The logic here is setup for multiple styles as well.
	public class ExampleTrap : ModTile
	{
		public override void SetStaticDefaults() {
			TileID.Sets.DrawsWalls[Type] = true;
			TileID.Sets.DontDrawTileSliced[Type] = true;
			TileID.Sets.IgnoresNearbyHalfbricksWhenDrawn[Type] = true;
			TileID.Sets.IsAMechanism[Type] = true;

			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileFrameImportant[Type] = true;

			// These 2 AddMapEntry and GetMapOption show off multiple Map Entries per Tile. Delete GetMapOption and all but 1 of these for your own ModTile if you don't actually need it.
			AddMapEntry(new Color(21, 179, 192), Language.GetText("MapObject.Trap")); // localized text for "Trap"
			AddMapEntry(new Color(0, 141, 63), Language.GetText("MapObject.Trap"));
		}

		// Read the comments above on AddMapEntry.
		public override ushort GetMapOption(int i, int j) => (ushort)(Main.tile[i, j].TileFrameY / 18);

		public override bool IsTileDangerous(int i, int j, Player player) => true;

		// Because this tile does not use a TileObjectData, and consequently does not have "real" tile styles, the correct tile style value can't be determined automatically. This means that the correct item won't automatically drop, so we must use GetItemDrops to calculate the tile style to determine the item drop.
		public override IEnumerable<Item> GetItemDrops(int i, int j) {
			Tile t = Main.tile[i, j];
			int style = t.TileFrameY / 18;
			// It can be useful to share a single tile with multiple styles.
			yield return new Item(Mod.Find<ModItem>(Items.Placeable.ExampleTrap.GetInternalNameFromStyle(style)).Type);

			// Here is an alternate approach:
			// int dropItem = TileLoader.GetItemDropFromTypeAndStyle(Type, style);
			// yield return new Item(dropItem);
		}

		public override bool CreateDust(int i, int j, ref int type) {
			int style = Main.tile[i, j].TileFrameY / 18;
			if (style == 0) {
				type = DustID.Glass; // A blue dust to match the tile
			}
			if (style == 1) {
				type = DustID.JungleGrass; // A green dust for the 2nd style.
			}
			return true;
		}

		// PlaceInWorld is needed to facilitate styles and alternates since this tile doesn't use a TileObjectData. Placing left and right based on player direction is usually done in the TileObjectData, but the specifics of that don't work for how we want this tile to work.
		public override void PlaceInWorld(int i, int j, Item item) {
			int style = Main.LocalPlayer.HeldItem.placeStyle;
			Tile tile = Main.tile[i, j];
			tile.TileFrameY = (short)(style * 18);
			if (Main.LocalPlayer.direction == 1) {
				tile.TileFrameX += 18;
			}
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				NetMessage.SendTileSquare(-1, Player.tileTargetX, Player.tileTargetY, 1, TileChangeType.None);
			}
		}

		// This progression matches vanilla tiles, you don't have to follow it if you don't want. Some vanilla traps don't have 6 states, only 4. This can be implemented with different logic in Slope. Making 8 directions is also easily done in a similar manner.
		private static int[] frameXCycle = [2, 3, 4, 5, 1, 0];
		// We can use the Slope method to override what happens when this tile is hammered.
		public override bool Slope(int i, int j) {
			Tile tile = Main.tile[i, j];
			int nextFrameX = frameXCycle[tile.TileFrameX / 18];
			tile.TileFrameX = (short)(nextFrameX * 18);
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				NetMessage.SendTileSquare(-1, Player.tileTargetX, Player.tileTargetY, 1, TileChangeType.None);
			}
			return false;
		}

		public override void HitWire(int i, int j) {
			Tile tile = Main.tile[i, j];
			int style = tile.TileFrameY / 18;
			Vector2 spawnPosition;
			// This logic here corresponds to the orientation of the sprites in the spritesheet, change it if your tile is different in design.
			int horizontalDirection = (tile.TileFrameX == 0) ? -1 : ((tile.TileFrameX == 18) ? 1 : 0);
			int verticalDirection = (tile.TileFrameX < 36) ? 0 : ((tile.TileFrameX < 72) ? -1 : 1);
			// Each trap style within this Tile shoots different projectiles.
			if (style == 0) {
				// Wiring.CheckMech checks if the wiring cooldown has been reached. Put a longer number here for less frequent projectile spawns. 200 is the dart/flame cooldown. Spear is 90, spiky ball is 300
				if (Wiring.CheckMech(i, j, 60)) {
					spawnPosition = new Vector2(i * 16 + 8 + 0 * horizontalDirection, j * 16 + 9 + 0 * verticalDirection); // The extra numbers here help center the projectile spawn position if you need to.

					// In a real mod you should be spawning projectiles that are both hostile and friendly to do damage to both players and NPC, as Terraria traps do.
					// Make sure to change velocity, projectile, damage, and knockback.
					Projectile.NewProjectile(Wiring.GetProjectileSource(i, j), spawnPosition, new Vector2(horizontalDirection, verticalDirection) * 6f, ProjectileID.IchorBullet, 20, 2f, Main.myPlayer);
				}
			}
			else if (style == 1) {
				// A longer cooldown for ChlorophyteBullet trap.
				if (Wiring.CheckMech(i, j, 200)) {
					spawnPosition = new Vector2(i * 16 + 8 + 0 * horizontalDirection, j * 16 + 9 + 0 * verticalDirection); // The extra numbers here help center the projectile spawn position.
					Projectile.NewProjectile(Wiring.GetProjectileSource(i, j), spawnPosition, new Vector2(horizontalDirection, verticalDirection) * 8f, ProjectileID.ChlorophyteBullet, 40, 2f, Main.myPlayer);
				}
			}
		}
	}
}