using ExampleMod.Content.Dusts;
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
            TileObjectData.newTile.CoordinateWidth = 16; // Width of tile on spritesheet
            TileObjectData.newTile.CoordinateHeights = new int[] { 16 }; // Sets an offset
            TileObjectData.newTile.CoordinatePadding = 2; // Sets an "empty space" that should be between each 16x16 sprite in spritesheet
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.addTile(Type);
            Main.tileFrameImportant[Type] = true;
            Main.tileObsidianKill[Type] = true;
            Main.tileShine2[Type] = true;
            Main.tileShine[Type] = 500;
            Main.tileSpelunker[Type] = true;
            ItemType = ModContent.ItemType<ExampleCrystalItem>();
            DustType = ModContent.DustType<Sparkle>();
        }

        public override ushort GetMapOption(int i, int j) 
			=> (ushort)(Main.tile[i, j].TileFrameX / 18); // this override is redundant, but in case if you want more map names option ig, then you can leave it alone?

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            Main.tile[i, j].TileType = TileID.Crystals;
            WorldGen.TileFrame(i, j, resetFrame, noBreak); // applies vanilla Crystal Shards behavior
            Main.tile[i, j].TileType = Type;
            return false;
        }
    }

    // We need a GlobalTile class to make sure our crystal can randomly spawn in the world
	// The logic is contained in the RandomUpdate function
    internal class ExampleCrystalGrow : GlobalTile
    {
        public override void RandomUpdate(int i, int j, int type)
        {
			// The crystal will grow only if: the world is in Hardmode, the tile is Stone, it's in the rock layer
			// and with same chance as the vanilla Crystal Shards have of spawning.
            if (Main.hardMode && type == TileID.Stone && j > Main.rockLayer && WorldGen.genRand.NextBool(110))       
            {
				// Randomly generate the direction the crystal spawns in
                int coordinateChance = WorldGen.genRand.Next(4);
                int additionalX = 0;
                int additionalY = 0;
				
                if (coordinateChance == 0)
                {
                    additionalX = -1;
                    additionalY = -1;
                }
                else if (coordinateChance == 1)
                {
                    additionalX = 1;
                    additionalY = 1;
                }
				
				// Checks if the position where the crystal should spawn doesn't already have a tile
                if (!Main.tile[i + additionalX, j + additionalY].HasTile)
                {
					// Count how many crystals there are in the area
                    int amountOfCrystals = 0;
                    int tilesToCheck = 6;
                    int tile = ModContent.TileType<ExampleCrystal>();
					
                    for (int k = i - tilesToCheck; k <= i + tilesToCheck; k++)
                    {
                        for (int l = j - tilesToCheck; l <= j + tilesToCheck; l++)
                        {
                            if (Main.tile[k, l].HasTile && Main.tile[k, l].TileType == tile)
                            {
                                amountOfCrystals++;
                            }
                        }
                    }
					
					// If there are less than 2 crystals, we can proceed to spawn one
                    if (amountOfCrystals < 2)
                    {
                        int style = (short)WorldGen.genRand.Next(1);
                        if (WorldGen.PlaceTile(i + additionalX, j + additionalY, TileID.Crystals, true, style: style)) // Places crystal shards first, because hardcoded!
                        {
                            Main.tile[i + additionalX, j + additionalY].TileType = ModContent.TileType<ExampleCrystal>(); // ... if succesfully placed, then replace with our crystal
                            Main.Map.Update(i + additionalX, j + additionalY, Main.Map[i + additionalX, j + additionalY].Light); // also don't forget to update it on map, so it won't appear as crystal shards
                        }
                        NetMessage.SendTileSquare(-1, i + additionalX, j + additionalY, TileChangeType.None);
                    }
                }
            }
        }
    }

	// The item for our custom crystal
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
