using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ExampleMod.Content.Tiles
{
    internal class ExampleCrystal : ModTile
    {
        public override void SetStaticDefaults()
        {
            TileObjectData.newTile.Width = 1;
            TileObjectData.newTile.Height = 1;
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinateHeights = new int[] { 16 };
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.addTile(Type);
            Main.tileFrameImportant[Type] = true;
            Main.tileObsidianKill[Type] = true;
            Main.tileShine2[Type] = true;
            Main.tileShine[Type] = 500;
            Main.tileSpelunker[Type] = true;
        }

        public override ushort GetMapOption(int i, int j) => (ushort)(Main.tile[i, j].TileFrameX / 18);

        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            int toDrop = 0;
            switch (Main.tile[i, j].TileFrameX / 18)
            {
                case 0:
                    toDrop = Mod.Find<ModItem>("ExampleCrystalItem").Type;
                    DustType = Mod.Find<ModDust>("Sparkle").Type;
                    break;
            }
            if (toDrop > 0) Item.NewItem(Entity.GetSource_None(), i * 16, j * 16, 16, 16, toDrop);
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            Main.tile[i, j].TileType = TileID.Crystals;
            WorldGen.TileFrame(i, j, resetFrame, noBreak); // applies vanilla Crystal Shards behavior
            Main.tile[i, j].TileType = Type;
            return false;
        }
    }
    // this class does some spreading
    internal class ExampleCrystalGrow : GlobalTile
    {
        public override void RandomUpdate(int i, int j, int type)
        {
            if (Main.hardMode && type == TileID.Stone && j > Main.rockLayer && WorldGen.genRand.NextBool(110))
                // this code line above means, that your crystal will grow only if: it's Hardmode, on Stone, in rock layer, and with same chance as vanilla Crystal Shards.
            {
                int num = WorldGen.genRand.Next(4);
                int num2 = 0;
                int num3 = 0;
                if (num == 0)
                {
                    num2 = -1;
                }
                else if (num == 1)
                {
                    num2 = 1;
                }
                else if (num == 0)
                {
                    num3 = -1;
                }
                else
                {
                    num3 = 1;
                }
                if (!Main.tile[i + num2, j + num3].HasTile)
                {
                    int num4 = 0;
                    int num5 = 6;
                    int tile = ModContent.TileType<ExampleCrystal>();
                    for (int k = i - num5; k <= i + num5; k++)
                    {
                        for (int l = j - num5; l <= j + num5; l++)
                        {
                            if (Main.tile[k, l].HasTile && Main.tile[k, l].TileType == tile)
                            {
                                num4++;
                            }
                        }
                    }
                    if (num4 < 2)
                    {
                        int style = (short)WorldGen.genRand.Next(1); // chooses style(s) that you want
                        if (WorldGen.PlaceTile(i + num2, j + num3, TileID.Crystals, true, style: style)) // Places crystal shards first, because hardcoded!
                        {
                            Main.tile[i + num2, j + num3].TileType = Mod.Find<ModTile>("ExampleCrystal").Type; // ... if succesfully placed, then replace with our crystal
                            Main.Map.Update(i + num2, j + num3, Main.Map[i + num2, j + num3].Light); // also don't forget to update it on map, so it won't appear as crystal shards
                        }
                        NetMessage.SendTileSquare(-1, i + num2, j + num3, TileChangeType.None);
                    }
                }
            }
        }
    }

    internal class ExampleCrystalItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = 99;
        }

        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.CrystalShard);
            Item.createTile = ModContent.TileType<ExampleCrystal>();
        }
    }
}
