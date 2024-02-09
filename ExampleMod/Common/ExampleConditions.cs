using ExampleMod.Common.Systems;
using ExampleMod.Content.Biomes;
using Terraria;

namespace ExampleMod.Common
{
	// This class contains conditions that will be reused in multiple places in ExampleMod.
	// There is nothing wrong with making a new Condition where is it used, such as is shown in ExampleNPCShop and ExamplePerson,
	// but it is a good idea to place Conditions used multiple times in a central location to avoid typos and other bugs.
	// Storing the Condition in a field also exposes these conditions for easier cross-mod compatibility.
	// For more information on using the Condition class, see https://github.com/tModLoader/tModLoader/wiki/Conditions
	public static class ExampleConditions
	{
		public static Condition InExampleBiome = new Condition("Mods.ExampleMod.Conditions.InExampleBiome", () => Main.LocalPlayer.InModBiome<ExampleSurfaceBiome>() || Main.LocalPlayer.InModBiome<ExampleUndergroundBiome>());
		public static Condition DownedMinionBoss = new("Mods.ExampleMod.Conditions.DownedMinionBoss", () => DownedBossSystem.downedMinionBoss);
	}
}
