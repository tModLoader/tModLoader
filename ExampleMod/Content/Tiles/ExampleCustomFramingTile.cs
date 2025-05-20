using ExampleMod.Content.Items;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Tiles
{
	public class ExampleCustomFramingTile : ModTile
	{
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			// This indicates the tile's spritesheet has extra frames for merging with adjacent tiles
			// Main.tileMergeDirt[Type] = true can be used for merging with dirt automatically, but here we will be looking at making the tile merge with snow instead
			TileID.Sets.ChecksForMerge[Type] = true;
			// This is so that snow blocks try to connect to this tile
			// Our custom framing code will make our tile have custom merging with snow, but we also need to specify this so the merging happens on the snow block side as well
			Main.tileMerge[TileID.SnowBlock][Type] = true;
			AddMapEntry(new Color(200, 200, 200));
		}

		public override void ModifyFrameMerge(int i, int j, ref int up, ref int down, ref int left, ref int right, ref int upLeft, ref int upRight, ref int downLeft, ref int downRight) {
			// We use this method to set the merge values of the adjacent tiles to -2 if the tile nearby is a snow block
			// -2 is what terraria uses to designate the tiles that will merge with ours using the custom frames
			WorldGen.TileMergeAttempt(-2, TileID.SnowBlock, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
		}

		public override void PostTileFrame(int i, int j, int up, int down, int left, int right, int upLeft, int upRight, int downLeft, int downRight) {
			// For every even X and Y coordinate, we will offset the tile's horizontal and vertical frame by the size of the sheet so the tile's frame ends up using the alternate version on the sheet, making a pattern that spans 2x2 tiles
			Tile t = Main.tile[i, j];
			if (i % 2 == 0) {
				t.TileFrameX += 288;
			}
			if (j % 2 == 0) {
				t.TileFrameY += 270;
			}
		}
	}

	internal class ExampleCustomFramingTileItem : ModItem
	{
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<ExampleCustomFramingTile>());
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.AddTile<Furniture.ExampleWorkbench>()
				.Register();
		}
	}
}