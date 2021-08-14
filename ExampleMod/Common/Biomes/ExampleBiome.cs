using ExampleMod.Common.Systems;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Common.Biomes
{
	public class ExampleBiome : ModBiome
	{
		public override bool IsBiomeActive(Player player) => ModContent.GetInstance<ExampleBiomeTileCount>().exampleBlockCount > 50;
	}
}