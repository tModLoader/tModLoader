using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Tiles.CustomTree
{
    public class ExampleCustomTree : ModCustomTree
    {
        public override string TileTexture => "Terraria/Images/Tiles_583";
        public override string TopTexture => "Terraria/Images/Tree_Tops_30";
        public override string BranchTexture => "Terraria/Images/Tree_Branches_26";

        //public override string LeafTexture => "CustomTreeLib/ExampleCustomTree/ExampleLeaf";

        //public override int[] ValidGroundTiles => new int[] { TileID.Grass, TileID.Dirt, TileID.Stone };

        //public override int SaplingStyles => 3;
        //public override int GrowChance => 1;

        //public override int MinHeight => 2;
        //public override int MaxHeight => 20;

        //public override Color? MapColor => Color.Yellow;
        //public override string MapName => "Example Tree";

        //public override Color? SaplingMapColor => Color.Orange;
        //public override string SaplingMapName => "Example Sapling";

        //public override bool Shake(int x, int y, ref bool createLeaves)
        //{
        //    createLeaves = true;
        //    Item.NewItem(WorldGen.GetItemSource_FromTreeShake(x, y), new Vector2(x, y) * 16, ItemID.StoneBlock);
        //    return false;
        //}
        //public override bool Drop(int x, int y)
        //{
        //    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(x, y), new Vector2(x, y) * 16, ItemID.DirtBlock);
        //    return false;
        //}
        //public override bool GetTreeFoliageData(int i, int j, int xoffset, ref int treeFrame, out int floorY, out int topTextureFrameWidth, out int topTextureFrameHeight)
        //{
        //    topTextureFrameWidth = 118;
        //    topTextureFrameHeight = 96;
        //    floorY = 0;
        //    return true;
        //}

        //public override bool TryGenerate(int x, int y)
        //{
        //    if (TreeGenPass.Active)
        //        return false; // Can be initiated only by command 

        //    return TreeGrowing.GrowTree(x, y, GetTreeSettings());
        //}

        //public override bool CreateDust(int x, int y, ref int dustType)
        //{
        //    TreeTileInfo info = TreeTileInfo.GetInfo(x, y);
        //    switch (info.Type)
        //    {
        //        case TreeTileType.LeafyBranch:
        //            dustType = DustID.Clentaminator_Red;
        //            break;
        //        case TreeTileType.LeafyTop:
        //            dustType = DustID.WoodFurniture;
        //            break;
        //        default:
        //            dustType = DustID.Stone;
        //            break;
        //    }
        //    return true;
        //}
    }
}