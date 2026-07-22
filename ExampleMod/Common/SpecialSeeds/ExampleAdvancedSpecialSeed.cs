using ExampleMod.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace ExampleMod.Common.SpecialSeeds;

public class ExampleAdvancedSpecialSeed : ModSpecialSeed
{
	private Asset<Texture2D> icon;

	public override void SetStaticDefaults() {
		icon = ModContent.Request<Texture2D>($"ExampleMod/Common/SpecialSeeds/{Name}_Icon");
	}

	public override void PostSetupContent() {
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

	public override void OnSeedButtonPress() {
		if (UIOption.Enabled) {
			SoundEngine.PlaySound(SoundID.BestReforge);
		}
		else {
			SoundEngine.PlaySound(SoundID.Item37); //Reforging sound effect.
		}
	}

	public override ModMenu WorldGenMenu => ModContent.GetInstance<ExampleModMenu>();

	public override Texture2D GetSeedTexture(bool isCorruption, bool isHardMode, ref Rectangle frame) {
		frame = new Rectangle(0, 0, 60, 58);
		if (!isCorruption) {
			frame.Y = 60;
		}
		if (isHardMode) {
			frame.X = 62;
		}
		return icon.Value;
	}

	public override void ModifyWorldGenTasks(List<GenPass> tasks) {
		//Add a GenPass immediately after the "Grass" pass. ExampleOreSystem explains this approach in more detail.
		int index = tasks.FindIndex(i => i.Name.Equals("Grass"));

		if (index != -1) {
			tasks.Insert(index+1,new ExampleSpecialSeedPass("Example Special Seed Changes", 200f));
		}
	}


	public override void ModifyLoadingTips(ref string text, ref Color textColor) {
		textColor = Main.DiscoColor;
	}

	public override void ModifyWorldProgressText(ref string text, ref Color textColor) {
		text = "Generating example bees";
		textColor = Main.DiscoColor;
	}
}

