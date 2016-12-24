using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Tiles
{
	public class TEElementalPurge : ModTileEntity
	{
		private const int range = 100;

		public override void Update()
		{
			int i = Position.X + Main.rand.Next(-range, range + 1);
			int j = Position.Y + Main.rand.Next(-range, range + 1);
			WorldGen.Convert(i, j, 0, 0);
		}

		public override bool ValidTile(int i, int j)
		{
			Tile tile = Main.tile[i, j];
			return tile.active() && tile.type == mod.TileType("ElementalPurge") && tile.frameX == 0 && tile.frameY == 0;
		}

		public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction)
		{
			if (Main.netMode == 1)
			{
				NetMessage.SendTileSquare(Main.myPlayer, i - 1, j - 1, 3);
				NetMessage.SendData(87, -1, -1, "", i - 1, j - 2, 0f, 0f, 0, 0, 0);
				return -1;
			}
			return Place(i - 1, j - 2);
		}
	}
}