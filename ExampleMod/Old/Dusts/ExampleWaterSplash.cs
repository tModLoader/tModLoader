using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Dusts
{
	public class ExampleWaterSplash : ModDust
	{
		public override void SetDefaults() {
			updateType = 33;
		}

		public override void OnSpawn(Dust dust) {
			dust.alpha = 170;
			dust.velocity *= 0.5f;
			dust.velocity.Y += 1f;
		}
	}
}