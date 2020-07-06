using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Dusts
{
	public class PuriumFlame : ModDust
	{
		public override void OnSpawn(Dust dust) {
			dust.color = new Color(120, 255, 90);
			dust.alpha = 1;
			dust.scale = 1.1f;
			dust.velocity *= 0.2f;
			dust.noGravity = true;
			dust.noLight = true;
		}

		public override bool Update(Dust dust) {
			dust.rotation += dust.velocity.X / 3f;
			dust.position += dust.velocity;
			int oldAlpha = dust.alpha;
			dust.alpha = (int)(dust.alpha * 1.2);
			if (dust.alpha == oldAlpha) {
				dust.alpha++;
			}
			if (dust.alpha >= 255) {
				dust.alpha = 255;
				dust.active = false;
			}
			return false;
		}
	}
}