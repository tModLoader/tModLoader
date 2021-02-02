using ExampleMod.Content.Dusts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Content.Tiles
{
	public class ExampleBlock : ModTile
	{
		public override void SetDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileMergeDirt[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileLighted[Type] = true;
			DustType = ModContent.DustType<Sparkle>();
			ItemDrop = ModContent.ItemType<Items.Placeable.ExampleBlock>();
			AddMapEntry(new Color(200, 200, 200));

			// todo: implement
			// SetModTree(new Trees.ExampleTree());
		}

		public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;

		// Make this tile glow slighty white
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			r = 0.5f;
			g = 0.5f;
			b = 0.5f;
		}

		// todo: implement
		// public override void ChangeWaterfallStyle(ref int style) {
		// 	style = mod.GetWaterfallStyleSlot("ExampleWaterfallStyle");
		// }
		//
		// public override int SaplingGrowthType(ref int style) {
		// 	style = 0;
		// 	return TileType<ExampleSapling>();
		// }
	}
}