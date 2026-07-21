using ExampleMod.Content;
using ExampleMod.Content.Tiles;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace ExampleMod.Common.SpecialSeeds;

// See ExampleSpecialSeedSystem for an example of this special seed's flag being used in a world.
public class ExampleSpecialSeed : ModSpecialSeed
{
	private Asset<Texture2D> iconCorruption;
	private Asset<Texture2D> iconHallowCorruption;
	private Asset<Texture2D> iconCrimson;
	private Asset<Texture2D> iconHallowCrimson;

	public override void SetStaticDefaults() {
		IncludeInZenith = true;
		iconCorruption = ModContent.Request<Texture2D>($"ExampleMod/Common/SpecialSeeds/{Name}_IconCorruption");
		iconHallowCorruption = ModContent.Request<Texture2D>($"ExampleMod/Common/SpecialSeeds/{Name}_IconHallowCorruption");
		iconCrimson = ModContent.Request<Texture2D>($"ExampleMod/Common/SpecialSeeds/{Name}_IconCrimson");
		iconHallowCrimson = ModContent.Request<Texture2D>($"ExampleMod/Common/SpecialSeeds/{Name}_IconHallowCrimson");
	}

	public override IEnumerable<string> SpecialSeedNames()
	{
		yield return "example";
	}

	public override IEnumerable<int> SpecialSeedNumbers() {
		yield return 1337;
	}

	public override ModMenu WorldGenMenu => ModContent.GetInstance<ExampleModMenu>();

	public override Asset<Texture2D> GetSeedTexture(bool isCorruption, bool isHardMode) {
		if (isCorruption) {
			return isHardMode ? iconHallowCorruption : iconCorruption;
		}
		else {
			return isHardMode ? iconHallowCrimson : iconCrimson;
		}
	}

	public override void ModifyWorldGenTasks(List<GenPass> tasks) {
		//Add a GenPass immediately after the "Grass" pass. ExampleOreSystem explains this approach in more detail.
		int index = tasks.FindIndex(i => i.Name.Equals("Grass"));

		if (index != -1) {
			tasks.Insert(index+1,new ExampleSpecialSeedPass("Example Special Seed Changes", 200f));
		}
	}
}

public class ExampleSpecialSeedPass : GenPass
{
	public ExampleSpecialSeedPass(string name, float loadWeight) : base(name, loadWeight) {
	}

	protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration) {
		progress.Message = "Applying Example Special Seed changes";

		for (int i = 0; i < Main.maxTilesX; i++) {
			for (int j = 0; j < Main.worldSurface; j++) {
				Tile tile = Main.tile[i, j];
				if (!TileID.Sets.Grass[tile.TileType] && !TileID.Sets.Dirt[tile.TileType]) {
					continue;
				}

				tile.TileType = (ushort)ModContent.TileType<ExampleBlock>();
			}
		}
	}
}