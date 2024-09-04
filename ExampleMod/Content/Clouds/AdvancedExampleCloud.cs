using Terraria.ModLoader;

namespace ExampleMod.Content.Clouds
{
    public class AdvancedExampleCloud : ModCloud
    {
		public override float SpawnChance(int cloudIndex) {
			if (cloudIndex % 2 == 0)
				return 1f;
			else if (cloudIndex % 3 == 0)
				return 0.5f;
			else
				return 0f;
		}
	}
}
