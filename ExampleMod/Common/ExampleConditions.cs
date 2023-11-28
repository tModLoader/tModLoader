using ExampleMod.Content.Biomes;
using Terraria;

namespace ExampleMod.Common
{
	public static class ExampleConditions
	{
		public static Condition InExampleBiome = new Condition("Mods.ExampleMod.Conditions.InExampleBiome", () => Main.LocalPlayer.InModBiome<ExampleSurfaceBiome>() || Main.LocalPlayer.InModBiome<ExampleUndergroundBiome>());
	}
}
