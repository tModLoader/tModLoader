using ExampleMod.Content.Tiles;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace ExampleMod.Content.SpecialSeeds;

public class ExampleAdvancedSpecialSeed : ModSpecialSeed
{
	public override void PostAddSeeds() {
		SortAfterModdedSeed<ExampleSpecialSeed>();
		// alternatively SortAfter(ModContent.GetInstance<ExampleSpecialSeed>());
	}

	public override IEnumerable<string> SpecialSeedNames()
	{
		yield return "advanced";
	}

	public override IEnumerable<AWorldGenerationOption> GetIncompatibilities() {
		yield return GetModdedSeedOption<ExampleSpecialSeed>();
	}

	public override IEnumerable<AWorldGenerationOption> GetDependencies() {
		yield return WorldGenerationOptions.Get<WorldSeedOption_NotTheBees>();
		yield return WorldGenerationOptions.Get<WorldSeedOption_Anniversary>();
	}

	//public override AWorldGenerationOption SortAfter => ModContent.GetInstance<ExampleSpecialSeed>().UIOption;

	public override ModMenu WorldGenMenu => ModContent.GetInstance<ExampleModMenu>();

	public override Asset<Texture2D> IconCorruption => ModContent.Request<Texture2D>($"ExampleMod/Content/SpecialSeeds/{Name}_IconCorruption");
	public override Asset<Texture2D> IconHallowCorruption => ModContent.Request<Texture2D>($"ExampleMod/Content/SpecialSeeds/{Name}_IconHallowCorruption");
	public override Asset<Texture2D> IconCrimson => ModContent.Request<Texture2D>($"ExampleMod/Content/SpecialSeeds/{Name}_IconCrimson");
	public override Asset<Texture2D> IconHallowCrimson => ModContent.Request<Texture2D>($"ExampleMod/Content/SpecialSeeds/{Name}_IconHallowCrimson");

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
