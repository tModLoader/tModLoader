using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Content.Dusts
{
	public class ExampleHoveringMinionDust : ModDust
	{
		public override void OnSpawn(Dust dust) {
			dust.color = new Color(120, 255, 90); // Sets the dust's color
			dust.alpha = 1; // Defines the initial alpha of the dust
			dust.scale = 1.1f;
			dust.velocity *= 0.2f; // Set's the dust's velocity
			dust.noGravity = true; // The dust is not affected by gravity
			dust.noLight = true; // The dust does not emit light
		}

		public override bool Update(Dust dust) {

			// Defines the movement of the dust
			dust.rotation += dust.velocity.X / 3f;
			dust.position += dust.velocity;

			// Increases the alpha of the dust
			int oldAlpha = dust.alpha;
			dust.alpha = (int)(dust.alpha * 1.2);
			if (dust.alpha == oldAlpha) {
				dust.alpha++;
			}
			if (dust.alpha >= 255) {
				dust.alpha = 255;
				dust.active = false; // When the alpha of the dust is 255 or greater, the dust is despawned
			}
			return false;
		}
	}
}