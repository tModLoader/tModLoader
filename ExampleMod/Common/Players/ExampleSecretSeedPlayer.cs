using ExampleMod.Content.SecretSeeds;
using Terraria.ModLoader;

namespace ExampleMod.Common.Players;

public class ExampleSecretSeedPlayer : ModPlayer
{
	public override void PostUpdateBuffs() {
		if (SeedLoader.SeedEnabled<ExampleSecretSeed>()) {
			Player.gravity *= 0.3f;
		}
	}
}