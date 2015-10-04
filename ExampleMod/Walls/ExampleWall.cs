using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Walls
{
	public class ExampleWall : ModWall
	{
		public override void SetDefaults()
		{
			Main.wallHouse[Type] = true;
			drop = mod.ItemType("ExampleWall");
			AddMapEntry(new Color(150, 150, 150));
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
			r = 0.4f;
			g = 0.4f;
			b = 0.4f;
		}
	}
}