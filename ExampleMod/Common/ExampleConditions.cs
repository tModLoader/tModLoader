using ExampleMod.Content.Biomes;
using Terraria.Localization;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Common
{
	public static class ExampleConditions
	{
		public static NPCShop.Condition InExampleBiome = new NPCShop.Condition(NetworkText.FromKey("Mods.ExampleMod.ShopConditions.InExampleBiome"), () => Main.LocalPlayer.InModBiome<ExampleSurfaceBiome>() || Main.LocalPlayer.InModBiome<ExampleUndergroundBiome>());
	}
}
