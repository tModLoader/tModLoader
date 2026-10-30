using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Content.Dusts
{
	public class ExampleLiquidSplash : ModDust
	{
		public override void OnSpawn(Dust dust) {
			dust.velocity *= 0.1f;
			dust.velocity.Y = -0.5f;
		}

		public override bool MidUpdate(Dust dust) {
			if (dust.noGravity) {
				dust.scale += 0.03f;
				if (dust.scale < 1f) {
					dust.velocity.Y += 0.075f;
				}
				dust.velocity.X *= 1.08f;
				if (dust.velocity.X > 0f) {
					dust.rotation += 0.01f;
				}
				else {
					dust.rotation -= 0.01f;
				}
			}
			else {
				if (!Collision.WetCollision(new Vector2(dust.position.X, dust.position.Y - 8f), 4, 4)) {
					dust.scale = 0f;
				}
				else {
					dust.alpha += Main.rand.Next(2);
					if (dust.alpha > 255) {
						dust.scale = 0f;
					}
					dust.velocity.Y = -0.5f;

					dust.alpha++;
					dust.scale -= 0.01f;
					dust.velocity.Y = -0.2f;

					dust.velocity.X += Main.rand.Next(-10, 10) * 0.002f;
					if (dust.velocity.X < -0.25) {
						dust.velocity.X = -0.25f;
					}
					if (dust.velocity.X > 0.25) {
						dust.velocity.X = 0.25f;
					}
				}
			}
			return true;
		}
	}
}
