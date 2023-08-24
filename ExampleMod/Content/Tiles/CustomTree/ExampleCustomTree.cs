using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Tiles.CustomTree
{
    public class ExampleCustomTree : ModCustomTree
    {
        public override string LeafTexture => "ExampleMod/Content/Tiles/CustomTree/ExampleCustomTree_Leaf";

        public override int[] ValidGroundTiles => new int[] { TileID.Grass, TileID.Dirt, TileID.Stone };

        public override int SaplingStyles => 3;
        public override int GrowChance => 1;

        public override int MinHeight => 10;
        public override int MaxHeight => 50;

		public override TreeTypes TreeType => TreeTypes.Ash;

		public override Color? TileMapColor => Color.Gray;
		public override Color? SaplingMapColor => Color.Gray;

		public override int BranchChance => 2;
		public override int NotLeafyBranchChance => 2;

		public override bool Shake(int x, int y, ref bool createLeaves) {
            createLeaves = true;
            Item.NewItem(WorldGen.GetItemSource_FromTreeShake(x, y), new Vector2(x, y) * 16, ItemID.StoneBlock);
            return false;
        }

		public override IEnumerable<Item> GetItemDrops(int x, int y) {
			Item item = new();

			if (TreeTileInfo.GetInfo(x, y).Type == TreeTileType.LeafyTop)
				item.SetDefaults(ModContent.ItemType<Content.Items.Placeable.ExampleBar>());
			else
				item.SetDefaults(ModContent.ItemType<Content.Items.Placeable.ExampleBlock>());

			return new[] { item };
		}

		public override int GetStyle(int x, int y) {
			return WorldGen.TreeTops.GetTreeStyle(0);
		}

		public override bool TryGenerate(int x, int y) {
            return CustomTreeGen.GrowTree(x, y, GetTreeSettings());
        }

        public override bool CreateDust(int x, int y, ref int dustType) {
            TreeTileInfo info = TreeTileInfo.GetInfo(x, y);
            switch (info.Type)
            {
                case TreeTileType.LeafyBranch:
                    dustType = DustID.Clentaminator_Red;
                    break;
                case TreeTileType.LeafyTop:
                    dustType = DustID.WoodFurniture;
                    break;
                default:
                    dustType = DustID.Stone;
                    break;
            }
            return true;
        }

		public override Asset<Texture2D> GetFoliageTexture(int style, bool branch) {
			style = style % 2 + 1;

			string path;
			if (branch) {
				path = $"ExampleMod/Content/Tiles/CustomTree/ExampleCustomTree_Branch{style}";
			}
			else {
				path = $"ExampleMod/Content/Tiles/CustomTree/ExampleCustomTree_Top{style}";
			}
			return ModContent.Request<Texture2D>(path);
		}
	}
}