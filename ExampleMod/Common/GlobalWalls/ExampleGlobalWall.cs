using ExampleMod.Content.Dusts;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Content.Walls
{
	public class ExampleGlobalWall : GlobalWall
	{
		public override bool WallFrame(int i, int j, int type, bool resetFrame, ref int style, ref int frameNumber) {
			// This example shows adding additional visual effects to wall framing.
			if(Main.rand.NextBool(20))
				Dust.NewDustPerfect(new Vector2(i * 16 + 8, j * 16 + 8), ModContent.DustType<Sparkle>(), Vector2.Zero);
			

			return base.WallFrame(i, j, type, resetFrame, ref style, ref frameNumber);
		}
	}
}