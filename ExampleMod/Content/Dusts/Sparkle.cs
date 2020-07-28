using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Content.Dusts
{
	public class Sparkle : ModDust
	{
		public override void OnSpawn(Dust dust) {
			dust.velocity *= 0.4f; // Velocity of the dust when spawned by anything. Remember: Positive floats make it go downward. Negative floats make it go upward.
			dust.noGravity = true; // Makes the dust have no gravity.
			dust.noLight = true; // Makes the dust emit no light.
			dust.scale *= 1.5f; // The scale of the dust.
		}

		public override bool Update(Dust dust) { // Calls every frame the dust is active
			dust.position += dust.velocity;
			dust.rotation += dust.velocity.X * 0.15f;
			dust.scale *= 0.99f;

			float light = 0.35f * dust.scale;

			Lighting.AddLight(dust.position, light, light, light);

			if (dust.scale < 0.5f) {
				dust.active = false;
			}

			return false; // Return false to prevent the vanilla behaviors to overrwrite the dust
		}
	}
}
