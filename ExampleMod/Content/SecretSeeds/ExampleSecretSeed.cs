using ExampleMod.Content.Tiles;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace ExampleMod.Content.SecretSeeds;

public class ExampleSecretSeed : ModSecretSeed
{
	public override void SetStaticDefaults() {
		SeedCode = "examplesecret";
		Known = true;
	}

	//Fill the world with large circles made of ExampleBlock.
	public override void PostWorldGen() {
		// "8E-04" is "scientific notation". It simply means 0.0008 but in some ways is easier to read.
		// This example uses procedural syntax to place the circles: https://github.com/tModLoader/tModLoader/wiki/World-Generation#procedural-syntax
		for (int k = 0; k < (int)(Main.maxTilesX * Main.maxTilesY * 8E-04); k++) {
			int x = WorldGen.genRand.Next(15, Main.maxTilesX - 15);

			int y = WorldGen.genRand.Next(15, Main.maxTilesY - 15);

			int radius = WorldGen.genRand.Next(5, 14);

			Point point = new Point(x, y);
			//Check if the desired area already contains ExampleBlocks in order to prevent overlap.
			Dictionary<ushort, int> exampleTileCount = new Dictionary<ushort,int>();
			WorldUtils.Gen(point, new Shapes.Circle(radius, radius), new Actions.TileScanner((ushort)ModContent.TileType<ExampleBlock>()).Output(exampleTileCount));
			if (exampleTileCount[(ushort)ModContent.TileType<ExampleBlock>()] > 0) {
				continue;
			}
			WorldUtils.Gen(point, new Shapes.Circle(radius, radius), new Actions.SetTile((ushort)ModContent.TileType<ExampleBlock>()));
		}
	}
}
