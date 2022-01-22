using System;
using ExampleMod.Content.Tiles;
using Terraria.ModLoader;

namespace ExampleMod.Common.Systems
{
	public class ExampleBiomeTileCount : ModSystem
	{
		public int exampleBlockCount;

		public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts) {
			exampleBlockCount = tileCounts[ModContent.TileType<ExampleBlock>()];
		}
	}
}
