using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace ExampleMod.Content.SpecialSeeds;

public class ExampleSpecialSeed : ModSpecialSeed
{
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
		int index = tasks.FindIndex(i => i.Name.Equals("Guide"));
		if (index < 0)
			return;
		tasks.Insert(index,tasks[index]);
		tasks.Insert(index,tasks[index]);
	}
}