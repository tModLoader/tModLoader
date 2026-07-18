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

public class ExampleSpecialSeed : ModSpecialSeed
{
	public override void SetStaticDefaults() {
		IncludeInZenith = true;
	}

	public override void PostAddSeeds() {
		SortBeforeVanillaSeed<WorldSeedOption_Skyblock>();
		// alternatively SortBefore(WorldGenerationOptions.Get<WorldSeedOption_Skyblock>());
	}

	public override IEnumerable<string> SpecialSeedNames()
	{
		yield return "example";
	}

	public override IEnumerable<int> SpecialSeedNumbers() {
		yield return 1337;
	}

	public override ModMenu WorldGenMenu => ModContent.GetInstance<ExampleModMenu>();

	public override Asset<Texture2D> IconCorruption =>
		ModContent.Request<Texture2D>($"ExampleMod/Content/SpecialSeeds/{Name}_IconCorruption");
	public override Asset<Texture2D> IconHallowCorruption =>
		ModContent.Request<Texture2D>($"ExampleMod/Content/SpecialSeeds/{Name}_IconHallowCorruption");
	public override Asset<Texture2D> IconCrimson =>
		ModContent.Request<Texture2D>($"ExampleMod/Content/SpecialSeeds/{Name}_IconCrimson");
	public override Asset<Texture2D> IconHallowCrimson =>
		ModContent.Request<Texture2D>($"ExampleMod/Content/SpecialSeeds/{Name}_IconHallowCrimson");

	public override void ModifyWorldGenTasks(List<GenPass> tasks) {
		//Add a GenPass immediately after the "Grass" pass. ExampleOreSystem explains this approach in more detail.
		int index = tasks.FindIndex(i => i.Name.Equals("Grass"));

		if (index != -1) {
			tasks.Insert(index+1,new ExampleSpecialSeedPass("Example Special Seed Changes", 200f));
		}
	}
}
