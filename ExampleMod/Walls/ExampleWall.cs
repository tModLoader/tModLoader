using ExampleMod.Dusts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Walls
{
	public class ExampleWall : ModWall
	{
		public override void SetDefaults() {
			Main.wallHouse[Type] = true;
			dustType = DustType<Sparkle>();
			drop = ItemType<Items.Placeable.ExampleWall>();
			AddMapEntry(new Color(150, 150, 150));
		}

		public override void NumDust(int i, int j, bool fail, ref int num) {
			num = fail ? 1 : 3;
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			r = 0.4f;
			g = 0.4f;
			b = 0.4f;
		}
	}
}