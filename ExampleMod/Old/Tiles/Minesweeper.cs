using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Tiles
{
	// This class replicates the behavior of the game Minesweeper within a ModTile.
	// This contrived example serves to teach modders about what TileFrame is capable of. Usually ModTiles are "framed" according to vanilla patterns. We override this behavior as a teaching example.
	public class Minesweeper : ModTile
	{
		public override void SetDefaults() {
			// Most 1x1 tiles without a TileObjectData don't set tileFrameImportant because FrameTile will reconstruct the frame automatically. 
			// This tile is special because we need it to preserve the hidden mine tiles.
			Main.tileFrameImportant[Type] = true;
			Main.tileSolid[Type] = true; // TODO: tModLoader hook for allowing non solid tiles to be hammer-able.
			drop = ItemType<MinesweeperItem>();
		}

		public override bool Dangersense(int i, int j, Player player) => IsMine(i, j);

		public override void PlaceInWorld(int i, int j, Item item) {
			Tile tile = Main.tile[i, j];
			if (Main.rand.NextBool(4)) // 1 in 4 placed Tiles will be a Mine
			{
				tile.frameX = 18;
				TileFrame8Neighbors(i, j);
				if (Main.netMode == NetmodeID.MultiplayerClient) // If we are a multiplayer client, we need to inform the server of the changes we've made to the Tile.
					NetMessage.SendTileSquare(-1, i, j, 1, TileChangeType.None);
			}
		}

		// When a tile is hammered, we need to reveal it and possibly update nearby tiles. 
		public override bool Slope(int i, int j) {
			Tile tile = Main.tile[i, j];
			bool IsBomb = (tile.frameX == 18 || tile.frameX == 5 * 18) && tile.frameY == 0;

			if (IsBomb) {
				// Spawning a Grenade projectile that dies quickly is the simplest way to get this effect
				int projectile = Projectile.NewProjectile(i * 16 + 8, j * 16 + 8, 0, 0, ProjectileID.Grenade, 30, 1, Main.myPlayer);
				Main.projectile[projectile].timeLeft = 2;
				Main.projectile[projectile].netUpdate = true;
				tile.frameX = 5 * 18;

				if (Main.netMode == NetmodeID.MultiplayerClient) // Slope is called on Clients, so we need to inform the server of changes.
					NetMessage.SendTileSquare(-1, i, j, 1, TileChangeType.None);
			}
			else {
				short mineCount = NearbyMines(i, j);
				if (mineCount == 0)
					RevealNeighbors(i, j);
				tile.frameX = 0; // TileFrame will take care of setting this correctly. 
				tile.frameY = 18;

				WorldGen.TileFrame(i, j);
				TileFrame8Neighbors(i, j);
			}
			// By returning false, we tell Terraria to skip the default sloping behavior
			return false;
		}

		// By using ModTile.TileFrame, we can have tiles adapt to nearby tiles however we like.
		// TileFrame is called to correct the frameX and frameY values of this Tile. Usually this happens when a Tile is placed nearby or when the world is first loaded.
		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
			Tile tile = Main.tile[i, j];
			bool changed = false;
			// frameX and frameY correspond to the top left corner of the sprite in the tile spritesheet.
			bool revealed = !((tile.frameX == 18 || tile.frameX == 0) && tile.frameY == 0);
			bool revealedBomb = tile.frameX == 5 * 18 && tile.frameY == 0;
			if (revealed && !revealedBomb) {
				short mineCount = NearbyMines(i, j);
				if (tile.frameX != (mineCount + 1) * 18 || tile.frameY != 18)
					changed = true;
				tile.frameX = (short)((mineCount + 1) * 18);
				tile.frameY = 18;
			}
			if (changed) {
				if (Main.netMode == NetmodeID.MultiplayerClient)
					NetMessage.SendTileSquare(-1, i, j, 1, TileChangeType.None);

				// Since this tile changed, we will change other nearby tiles. This isn't typical but is suitable for minesweeper. 
				TileFrame8Neighbors(i, j);
			}
			return false;
		}

		// A recursive method that visits nearby Minesweeper tiles and reveals them, continuing to reveal if there are no nearby mines.
		void RevealNeighbors(int i, int j) {
			Tile tile = Framing.GetTileSafely(i, j);
			if (tile.active() && tile.type == Type && (tile.frameY != 18 /*|| (tile.frameX == 0 && tile.frameY == 18)*/)) {
				// revealed, not right number, TileFrame will fix
				tile.frameX = 0;
				tile.frameY = 18;

				if (NearbyMines(i, j) == 0) {
					RevealNeighbors(i + 1, j);
					RevealNeighbors(i - 1, j);
					RevealNeighbors(i, j - 1);
					RevealNeighbors(i, j + 1);
				}
			}
		}

		bool IsMine(int i, int j) => IsMine(Main.tile[i, j]);

		bool IsMine(Tile tile) => tile.type == Type && tile.frameX != 0 && tile.frameY == 0;

		short NearbyMines(int i, int j) => (short)(new bool[] { IsMine(i - 1, j - 1), IsMine(i - 1, j), IsMine(i - 1, j + 1), IsMine(i, j - 1), IsMine(i, j + 1), IsMine(i + 1, j - 1), IsMine(i + 1, j), IsMine(i + 1, j + 1), }.Count(b => b));

		private void TileFrame8Neighbors(int i, int j) {
			WorldGen.TileFrame(i + 1, j);
			WorldGen.TileFrame(i - 1, j);
			WorldGen.TileFrame(i, j + 1);
			WorldGen.TileFrame(i, j - 1);
			WorldGen.TileFrame(i + 1, j + 1);
			WorldGen.TileFrame(i - 1, j - 1);
			WorldGen.TileFrame(i - 1, j + 1);
			WorldGen.TileFrame(i + 1, j - 1);
		}
	}

	public class MinesweeperItem : ModItem
	{
		public override string Texture => "ExampleMod/Items/ExampleItem";

		public override void SetDefaults() {
			item.CloneDefaults(ItemID.DirtBlock);
			item.createTile = TileType<Minesweeper>();
		}
	}
}