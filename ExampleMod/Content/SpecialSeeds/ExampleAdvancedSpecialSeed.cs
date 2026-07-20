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
	private Asset<Texture2D> iconCorruption;
	private Asset<Texture2D> iconHallowCorruption;
	private Asset<Texture2D> iconCrimson;
	private Asset<Texture2D> iconHallowCrimson;

	public override void SetStaticDefaults() {
		iconCorruption = ModContent.Request<Texture2D>($"ExampleMod/Content/SpecialSeeds/{Name}_IconCorruption");
		iconHallowCorruption = ModContent.Request<Texture2D>($"ExampleMod/Content/SpecialSeeds/{Name}_IconHallowCorruption");
		iconCrimson = ModContent.Request<Texture2D>($"ExampleMod/Content/SpecialSeeds/{Name}_IconCrimson");
		iconHallowCrimson = ModContent.Request<Texture2D>($"ExampleMod/Content/SpecialSeeds/{Name}_IconHallowCrimson");
	}

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

