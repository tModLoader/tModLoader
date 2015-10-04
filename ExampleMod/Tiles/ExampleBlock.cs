using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Tiles
{
	public class ExampleBlock : ModTile
	{
		public override void SetDefaults()
		{
			Main.tileSolid[Type] = true;
			Main.tileMergeDirt[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileLighted[Type] = true;
			drop = mod.ItemType("ExampleBlock");
			AddMapEntry(new Color(200, 200, 200));
		}

		public override void NumDust(int i, int j, bool fail, ref int num)
		{
			num = fail ? 1 : 3;
		}

		public override bool CreateDust(int i, int j, ref int type)
		{
			ModDust.NewDust(new Vector2(i, j) * 16f, 16, 16, mod, "Sparkle");
			return false;
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			r = 0.5f;
			g = 0.5f;
			b = 0.5f;
		}
	}
}