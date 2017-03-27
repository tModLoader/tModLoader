using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ExampleMod.Barrel
{
    public class BarrelTile : ModTile
    {
        private BarrelTE barrelTE;
     //   private bool place = false;

        public override void SetDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileSolid[Type] = false;
            Main.tileNoAttach[Type] = false;
            Main.tileWaterDeath[Type] = true;
            Main.tileLavaDeath[Type] = true;

            TileObjectData.newTile.Width = 2;
            TileObjectData.newTile.Height = 2;
            TileObjectData.newTile.Origin = new Point16(1, 1);
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 18 }; // Per Block Height
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(mod.GetTileEntity("BarrelTE").Hook_AfterPlacement, -1, 0, false);
            TileObjectData.addTile(Type);

            animationFrameHeight = 38;
            AddMapEntry(new Color(200, 200, 200), "Barrel");
            disableSmartCursor = true;
            dustType = mod.DustType("Pixel");
        }

        public override void AnimateTile(ref int frame, ref int frameCounter)
        {
           // if (place == true)
            {
           //     frame = barrelTE.fillStatus;
            }
        }
		 
        public override void RightClick(int x, int y)
        {
            //Tile tile = Main.tile[x, y];
            //int left = x - (tile.frameX / 18);
            //int top = y - (tile.frameY / 18);
            //int index = mod.GetTileEntity("BarrelTE").Find(left, top);
            //if (index != -1)
            //{
            //    barrelTE = (BarrelTE)TileEntity.ByID[index];
            //}
        }

        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;

            Tile tile = Main.tile[i, j];
            int left = i - (tile.frameX / 18);
            int top = j - (tile.frameY / 18);
            int index = mod.GetTileEntity("BarrelTE").Find(left, top);
            if (index != -1)
            {
                barrelTE = (BarrelTE)TileEntity.ByID[index];

                player.showItemIconText = "Filled: " + (barrelTE.fillStatus * 10).ToString() + "%";
                player.showItemIcon2 = -1;

                player.noThrow = 2;
                player.showItemIcon = true;
            }
        }
        
        //public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        //{
        //    Tile tile = Main.tile[i, j];
        //    Vector2 zero = new Vector2(Main.offScreenRange, Main.offScreenRange);
        //    if (Main.drawToScreen)
        //    {
        //        zero = Vector2.Zero;
        //    }
        //    int height = 16;
        //    int start;

        //    int left = i - (tile.frameX / 18);
        //    int top = j - (tile.frameY / 18);
        //    int index = mod.GetTileEntity("BarrelTE").Find(left, top);
        //    barrelTE = (BarrelTE)TileEntity.ByID[index];

        //    if (tile.frameY == 18)
        //    {
        //        start = (36 * barrelTE.fillStatus) + (2 * barrelTE.fillStatus) + 18;
        //    }
        //    else
        //    {
        //        start = (36 * barrelTE.fillStatus) + (2 * barrelTE.fillStatus);
        //    }

        //    Color color = Lighting.GetColor(i, j);

        //    Main.spriteBatch.Draw(mod.GetTexture("Barrel/BarrelTile"), new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + zero, new Rectangle(tile.frameX, start, 16, height), color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        //}


        //public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        //{
        //    if (place == false)
        //    {
        //        Tile tile = Main.tile[i, j];
        //        int left = i - (tile.frameX / 18);
        //        int top = j - (tile.frameY / 18);
        //        int index = mod.GetTileEntity("BarrelTE").Find(left, top);
        //        if (index != -1)
        //        {
        //            place = true;
        //            barrelTE = (BarrelTE)TileEntity.ByID[index];
        //        }
        //    }

        //    return true;
        //}

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }
        
        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(i * 16, j * 16, 48, 48, mod.ItemType("BarrelItem"));
            mod.GetTileEntity("BarrelTE").Kill(i, j);
        }
    }
}