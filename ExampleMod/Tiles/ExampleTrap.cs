using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Tiles
{
	// This class shows off a number of less common ModTile methods. These methods help our trap tile behave like vanilla traps. 
	// In particular, hammer behavior is particularly tricky. The logic here is setup for multiple styles as well.
	public class ExampleTrap : ModTile
	{
		public override void SetDefaults()
		{
			TileID.Sets.DrawsWalls[Type] = true;

			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileFrameImportant[Type] = true;

			// These 2 AddMapEntry and GetMapOption show off multiple Map Entries per Tile. Delete GetMapOption and all but 1 of these for your own ModTile if you don't actually need it.
			AddMapEntry(new Color(21, 179, 192), Language.GetText("MapObject.Trap")); // localized text for "Trap"
			AddMapEntry(new Color(0, 141, 63), Language.GetText("MapObject.Trap"));
		}

		// Read the comments above on AddMapEntry.
		public override ushort GetMapOption(int i, int j) => (ushort)(Main.tile[i, j].frameY / 18);

		public override bool Dangersense(int i, int j, Player player) => true;

		public override bool Drop(int i, int j)
		{
			Tile t = Main.tile[i, j];
			int style = t.frameY / 18;
			// It can be useful to share a single tile with multiple styles.
			if (style == 0)
				Item.NewItem(i * 16, j * 16, 16, 16, mod.ItemType(Items.Placeable.ExampleTrap.ExampleTrapA));
			if (style == 1)
				Item.NewItem(i * 16, j * 16, 16, 16, mod.ItemType(Items.Placeable.ExampleTrap.ExampleTrapB));
			return base.Drop(i, j);
		}

		public override bool CreateDust(int i, int j, ref int type)
		{
			int style = Main.tile[i, j].frameY / 18;
			if (style == 0)
				type = 13; // A blue dust to match the tile 
			if (style == 1)
				type = 39; // A green dust for the 2nd style.
			return true;
		}

		// PlaceInWorld is needed to facilitate styles and alternates since this tile doesn't use a TileObjectData. Placing left and right based on player direction is usually done in the TileObjectData, but the specifics of that didn't work for how we want this tile to work. 
		public override void PlaceInWorld(int i, int j, Item item)
		{
			int style = Main.LocalPlayer.HeldItem.placeStyle;
			Tile tile = Main.tile[i, j];
			tile.frameY = (short)(style * 18);
			if (Main.LocalPlayer.direction == 1)
			{
				tile.frameX += 18;
			}
			if (Main.netMode == 1)
			{
				NetMessage.SendTileSquare(-1, Player.tileTargetX, Player.tileTargetY, 1, TileChangeType.None);
			}
		}

		// This progression matches vanilla tiles, you don't have to follow it if you don't want. Some vanilla traps don't have 6 states, only 4. This can be implemented with different logic in Slope. Making 8 directions is also easily done in a similar manner.
		static int[] frameXCycle = { 2, 3, 4, 5, 1, 0 };
		// We can use the Slope method to override what happens when this tile is hammered.
		public override bool Slope(int i, int j)
		{
			Tile tile = Main.tile[i, j];
			int style = tile.frameY / 18;
			int nextFrameX = frameXCycle[tile.frameX / 18];
			tile.frameX = (short)(nextFrameX * 18);
			if (Main.netMode == 1)
			{
				NetMessage.SendTileSquare(-1, Player.tileTargetX, Player.tileTargetY, 1, TileChangeType.None);
			}
			return false;
		}

		public override void HitWire(int i, int j)
		{
			Tile tile = Main.tile[i, j];
			int style = tile.frameY / 18;
			Vector2 spawnPosition;
			// This logic here corresponds to the orientation of the sprites in the spritesheet, change it if your tile is different in design.
			int horizontalDirection = (tile.frameX == 0) ? -1 : ((tile.frameX == 18) ? 1 : 0);
			int verticalDirection = (tile.frameX < 36) ? 0 : ((tile.frameX < 72) ? -1 : 1);
			// Each trap style within this Tile shoots different projectiles.
			if (style == 0)
			{
				// Wiring.CheckMech checks if the wiring cooldown has been reached. Put a longer number here for less frequent projectile spawns. 200 is the dart/flame cooldown. Spear is 90, spiky ball is 300
				if (Wiring.CheckMech(i, j, 60))
				{
					spawnPosition = new Vector2(i * 16 + 8 + 0 * horizontalDirection, j * 16 + 9 + 0 * verticalDirection); // The extra numbers here help center the projectile spawn position if you need to.

					// In reality you should be spawning projectiles that are both hostile and friendly to do damage to both players and NPC.
					// Make sure to change velocity, projectile, damage, and knockback.
					Projectile.NewProjectile(spawnPosition, new Vector2(horizontalDirection, verticalDirection) * 6f, ProjectileID.IchorBullet, 20, 2f, Main.myPlayer);
				}
			}
			else if (style == 1)
			{
				if (Wiring.CheckMech(i, j, 200)) // A longer cooldown for ChlorophyteBullet trap.
				{
					spawnPosition = new Vector2(i * 16 + 8 + 0 * horizontalDirection, j * 16 + 9 + 0 * verticalDirection); // The extra numbers here help center the projectile spawn position.
					Projectile.NewProjectile(spawnPosition, new Vector2(horizontalDirection, verticalDirection) * 8f, ProjectileID.ChlorophyteBullet, 40, 2f, Main.myPlayer);
				}
			}
		}
	}
}