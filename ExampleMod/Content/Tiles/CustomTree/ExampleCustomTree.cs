using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod.Content.Tiles.CustomTree
{
	public class ExampleCustomTree : ModCustomTree
    {
		// Point it to leaf texture, so it registers it
        public override string LeafTexture => "ExampleMod/Content/Tiles/CustomTree/ExampleCustomTree_Leaf";

		// Provide custom tile and sapling
		public override CustomTreeTile Tile { get; set; } = new ExampleCustomTreeTile();
		public override CustomTreeSapling Sapling { get; set; } = new ExampleCustomTreeSapling();

		// Make tree grow on grass, dirt and stone tiles
		public override int[] ValidGroundTiles => new int[] { TileID.Grass, TileID.Dirt, TileID.Stone };

		// How many styles sapling texture has
        public override int SaplingStyles => 3;

		// 1 in X chance to grow, bigger value -> less chance
        public override int GrowChance => 1;

		// Minimum and maximum tree height
        public override int MinHeight => 10;
        public override int MaxHeight => 50;

		// Ash loot table for shaking
		public override TreeTypes TreeType => TreeTypes.Ash;

		// Generator settings
		public override int BranchChance => 2;
		public override int NotLeafyBranchChance => 2;

		// Tree shake behavior, you can spawn items, NPCs and projectiles here
		public override bool Shake(int x, int y, ref bool createLeaves) {
            createLeaves = true;
            Item.NewItem(WorldGen.GetItemSource_FromTreeShake(x, y), new Vector2(x, y) * 16, ItemID.StoneBlock);
            return false;
        }

		// Gets style id here, which will be passed to GetFoliageTexture later
		public override int GetStyle(int x, int y) {
			return WorldGen.TreeTops.GetTreeStyle(0) % 2;
		}

		// Called on world generation in attempt to generate tree
		public override bool TryGenerate(int x, int y) {
            return CustomTreeGenerator.GrowTree(x, y, GetTreeSettings());
        }

		// Gets foliage texture for each style. Style is passed from GetStyle
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