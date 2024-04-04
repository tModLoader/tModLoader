using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Liquids
{
	public class ExampleLiquid : ModLiquid
	{
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			WaterfallLength = 16;
			DefaultOpacity = 0.6f;
		}

		public override void PreUpdate(int x, int y) {
			Tile tile = Main.tile[x, y];

			if (y < 200 && tile.LiquidAmount > 0) {
				byte b = 2;
				if (tile.LiquidAmount < b)
					b = tile.LiquidAmount;

				tile.LiquidAmount -= b;
			}
		}

		public override void Merge(int otherLiquid, bool[] liquidNearby, ref int liquidMergeTileType, ref int liquidMergeType) {
			liquidMergeTileType = 0;

			if (otherLiquid == ModContent.GetInstance<ExampleLiquid2>().Type) {
				liquidMergeTileType = TileID.LunarOre;
			}
			else if (otherLiquid == LiquidID.Shimmer) {
				liquidMergeTileType = TileID.ShimmerBlock;
			}

			liquidMergeType = 4;
		}
	}
}
