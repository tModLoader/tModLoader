using Terraria.ModLoader;

namespace ExampleMod.Content.Walls
{
	public class ExampleGlobalWall : GlobalWall
	{
		public override bool WallFrame(int i, int j, int type, bool randomizeFrame, ref int style, ref int frameNumber) {
			// This code is a learning tool to visualize when this hook is called. Uncomment the code and rebuild to test.
			/*
			if (Main.rand.NextBool(20)) {
				Dust.NewDustPerfect(new Vector2(i * 16 + 8, j * 16 + 8), ModContent.DustType<Sparkle>(), Vector2.Zero);
			}
			*/

			// A more typical usage of this hook would be to implement a custom wall framing pattern.

			return base.WallFrame(i, j, type, randomizeFrame, ref style, ref frameNumber);
		}
	}
}