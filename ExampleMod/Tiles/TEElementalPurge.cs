using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Tiles
{
	public class TEElementalPurge : ModTileEntity
	{
		private const int range = 100;

		public override void Update() {
			int i = Position.X + Main.rand.Next(-range, range + 1);
			int j = Position.Y + Main.rand.Next(-range, range + 1);
			WorldGen.Convert(i, j, 0, 0);
		}

		public override bool ValidTile(int i, int j) {
			Tile tile = Main.tile[i, j];
			return tile.active() && tile.type == TileType<ElementalPurge>() && tile.frameX == 0 && tile.frameY == 0;
		}

		public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction) {
			// i - 1 and j - 2 come from the fact that the origin of the tile is "new Point16(1, 2);", so we need to pass the coordinates back to the top left tile. If using a vanilla TileObjectData.Style, make sure you know the origin value.
			if (Main.netMode == 1) {
				NetMessage.SendTileSquare(Main.myPlayer, i - 1, j - 1, 3); // this is -1, -1, however, because -1, -1 places the 3 diameter square over all the tiles, which are sent to other clients as an update.
				NetMessage.SendData(87, -1, -1, null, i - 1, j - 2, Type, 0f, 0, 0, 0);
				return -1;
			}
			return Place(i - 1, j - 2);
		}
	}
}