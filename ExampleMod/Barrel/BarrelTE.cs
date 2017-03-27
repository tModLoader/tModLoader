using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ExampleMod.Barrel
{
    public class BarrelTE : ModTileEntity
    {
        internal int nextUpdate = 1000;
        internal int fillStatus = 0;
        internal bool createdOn = false;

        public override void Update()
        {
            if (Main.raining == true)
            {
                if (fillStatus < 8)
                {
                    nextUpdate--;
                    if (nextUpdate <= 0)
                    {
                        fillStatus++;
                        nextUpdate = 1000;
                    }
                }
            }
        }

        public override bool ValidTile(int i, int j)
        {
            Tile tile = Main.tile[i, j];
            return tile.active() && tile.type == mod.TileType("BarrelTile") && tile.frameX == 0 && tile.frameY == 0;
        }

        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction)
        {
            // i - 1 and j - 2 come from the fact that the origin of the tile is "new Point16(1, 2);", so we need to pass the coordinates back to the top left tile. If using a vanilla TileObjectData.Style, make sure you know the origin value.
            if (Main.netMode == 1)
            {
                NetMessage.SendTileSquare(Main.myPlayer, i - 1, j - 1, 3); // this is -1, -1, however, because -1, -1 places the 3 diameter square over all the tiles, which are sent to other clients as an update.
                NetMessage.SendData(87, -1, -1, "", i - 1, j - 1, 0f, 0f, 0, 0, 0);
                createdOn = true;
                return -1;
            }
            createdOn = true;

            return Place(i - 1, j - 1);
        }

        public override TagCompound Save()
        {
            return new TagCompound {
                {"createdOn", createdOn},
                {"fillStatus", fillStatus}
            };
        }

        public override void Load(TagCompound tag)
        {
            createdOn = tag.GetBool("createdOn");
            fillStatus = tag.GetInt("fillStatus");
        }
    }
}