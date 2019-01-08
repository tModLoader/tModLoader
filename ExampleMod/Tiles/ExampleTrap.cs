using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

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

		public override void PlaceInWorld(int i, int j, Item item)
		{
			int style = Main.LocalPlayer.HeldItem.placeStyle;
			Tile tile = Main.tile[Player.tileTargetX, Player.tileTargetY];
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

		// We can use the Slope method to override what happens when this tile is hammered.
		public override bool Slope(int i, int j)
		{
			Tile tile = Main.tile[i, j];
			int nextFrameX = 0;
			int style = tile.frameY / 18;
			switch (style)
			{
				case 0:
				case 1:
					switch (tile.frameX / 18) // This progression matches vanilla tiles, you don't have to follow it if you don't want.
					{
						case 0:
							nextFrameX = 2;
							break;
						case 1:
							nextFrameX = 3;
							break;
						case 2:
							nextFrameX = 4;
							break;
						case 3:
							nextFrameX = 5;
							break;
						case 4:
							nextFrameX = 1;
							break;
						case 5:
							nextFrameX = 0;
							break;
					}
					break;
					// Some vanilla traps don't have 6 states, only 4. That logic can go here.
			}
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
			Vector2 spawnPosition = Vector2.Zero;
			float speedX = 0f;
			float speedY = 0f;
			int projectileType = 0;
			int damage = 0;
			switch (style)
			{
				// Each trap style within this Tile shoots different projectiles.
				case 0:
					if (Wiring.CheckMech(i, j, 10)) // 200 ticks is the cooldown. 200 is the dart/flame cooldown. Spear is 90, spiky ball is 300
					{
						int horizontalDirection = (tile.frameX == 0) ? -1 : ((tile.frameX == 18) ? 1 : 0);
						int verticalDirection = (tile.frameX < 36) ? 0 : ((tile.frameX < 72) ? -1 : 1);
						spawnPosition = new Vector2(i * 16 + 8 + 0 * horizontalDirection, j * 16 + 9 + verticalDirection * 0); //spawnPosition = new Vector2(i * 16 + 8 + 10 * horizontalDirection, j * 16 + 9 + verticalDirection * 9);
						float speed = 12f;
						projectileType = ProjectileID.IchorBullet;
						damage = 20;
						speedX = horizontalDirection * speed;
						speedY = verticalDirection * speed;
					}
					break;
				case 1:
					if (Wiring.CheckMech(i, j, 200)) // 200 ticks is the cooldown. 200 is the dart/flame cooldown. Spear is 90, spiky ball is 300
					{
						int horizontalDirection = (tile.frameX == 0) ? -1 : ((tile.frameX == 18) ? 1 : 0);
						int verticalDirection = (tile.frameX < 36) ? 0 : ((tile.frameX < 72) ? -1 : 1);
						spawnPosition = new Vector2(i * 16 + 8 + 0 * horizontalDirection, j * 16 + 9 + verticalDirection * 0); //spawnPosition = new Vector2(i * 16 + 8 + 10 * horizontalDirection, j * 16 + 9 + verticalDirection * 9);
						float speed = 12f;
						projectileType = ProjectileID.ChlorophyteBullet;
						damage = 40;
						speedX = horizontalDirection * speed;
						speedY = verticalDirection * speed;
					}
					break;
			}
			if (projectileType != 0)
			{
				// In reality you should be spawning projectiles that are both hostile and friendly to do damage to both players and NPC.
				Projectile.NewProjectile((int)spawnPosition.X, (int)spawnPosition.Y, speedX, speedY, projectileType, damage, 2f, Main.myPlayer, 0f, 0f);
			}
		}
	}
}