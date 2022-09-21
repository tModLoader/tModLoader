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

			// GlassBlock is special in how walls interact with it, this replicates that logic
			/*
			if (style < 15) {
				if (Main.tile[i, j - 1].HasTile && Main.tile[i, j - 1].TileType == ModContent.TileType<ExampleGlassBlock>())
					style |= 1;
				if (Main.tile[i - 1, j].HasTile && Main.tile[i - 1, j].TileType == ModContent.TileType<ExampleGlassBlock>())
					style |= 2;
				if (Main.tile[i + 1, j].HasTile && Main.tile[i + 1, j].TileType == ModContent.TileType<ExampleGlassBlock>())
					style |= 4;
				if (Main.tile[i, j + 1].HasTile && Main.tile[i, j + 1].TileType == ModContent.TileType<ExampleGlassBlock>())
					style |= 8;

				if (style == 15)
					style += Framing.centerWallFrameLookup[i % 3][j % 3];
			}
			*/

			return base.WallFrame(i, j, type, resetFrame, ref style, ref frameNumber);
		}
	}
}