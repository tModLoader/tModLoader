using ExampleMod.Content.Dusts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Content.Walls
{
	public class ExampleGlobalWall : GlobalWall
	{
		public override bool WallFrame(int i, int j, int type, ref bool resetFrame) {
			Dust.NewDustPerfect(new Vector2(i * 16, j * 16), ModContent.DustType<Dusts.Sparkle>(), Vector2.Zero);
			
			return base.WallFrame(i, j, type, ref resetFrame);
		}
	}
}