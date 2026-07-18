using ExampleMod.Content.SpecialSeeds;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace ExampleMod.Common.Systems;

public class ExampleSpecialSeedSystem : ModSystem
{
	public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor) {
		if (!SpecialSeedLoader.SeedEnabled<ExampleSpecialSeed>())
			return;
		backgroundColor = Color.Lerp(backgroundColor, Color.Black, 0.8f);
	}
}