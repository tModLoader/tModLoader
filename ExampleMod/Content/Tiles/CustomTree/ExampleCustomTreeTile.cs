using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Tiles.CustomTree
{
	public class ExampleCustomTreeTile : CustomTreeTile
	{
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();

			// Make tree breakable by hammer only
			Main.tileAxe[Type] = false;
			Main.tileHammer[Type] = true;

			// Map color with default name from localization file
			AddMapEntry(Color.Gray, DefaultMapNameLocalization);
		}

		public override bool CreateDust(int x, int y, ref int dustType) {
		    TreeTileInfo info = TreeTileInfo.GetInfo(x, y);
		    switch (info.Type)
		    {
		        case TreeTileType.LeafyBranch:
		            dustType = DustID.Clentaminator_Red;
		            break;
		        case TreeTileType.LeafyTop:
		            dustType = DustID.Clentaminator_Blue;
		            break;
		        default:
		            dustType = DustID.WhiteTorch;
		            break;
		    }
		    return true;
		}

		// Item drops for each tile broken
		// In this case, loot is determined by whether tile was a leafy top or not
		public override IEnumerable<Item> GetItemDrops(int x, int y) {
			Item item = new();

			if (TreeTileInfo.GetInfo(x, y).Type == TreeTileType.LeafyTop)
				item.SetDefaults(ModContent.ItemType<Content.Items.Placeable.ExampleBar>());
			else
				item.SetDefaults(ModContent.ItemType<Content.Items.Placeable.ExampleBlock>());

			return new[] { item };
		}
	}
}