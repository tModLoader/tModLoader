using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Dusts
{
	// This dust shows off custom drawing. By default, the dust sprite is drawn once. This example uses custom drawing to draw a trail, it is an exact clone of DustID.Electric, aside from some code cleanup. One place Terraria uses DustID.Electric is when a player is suffering from BuffID.Electrified.
	public class ExampleCustomDrawDust : ModDust
	{
		public override string Texture => null; // Set to null to use the vanilla texture instead of a custom texture

		public override void OnSpawn(Dust dust) {
			int desiredVanillaDustTexture = DustID.Electric;
			int frameX = desiredVanillaDustTexture * 10 % 1000;
			int frameY = desiredVanillaDustTexture * 10 / 1000 * 30 + Main.rand.Next(3) * 10;
			dust.frame = new Rectangle(frameX, frameY, 8, 8);
		}

		public override Color? GetAlpha(Dust dust, Color lightColor) {
			lightColor = Color.Lerp(lightColor, Color.White, 0.8f);
			return new Color(lightColor.R, lightColor.G, lightColor.B, 25);
		}

		public override bool PreDraw(Dust dust) {
			// Here we draw a trail by drawing the dust many times at different scales and offsets.
			if (dust.fadeIn == 0f) {
				float trailLength = Math.Abs(dust.velocity.X) + Math.Abs(dust.velocity.Y);
				trailLength *= 3f;
				if (trailLength > 10f)
					trailLength = 10f;

				Color drawColor = Lighting.GetColor((int)(dust.position.X + 4) / 16, (int)(dust.position.Y + 4) / 16);
				drawColor = dust.GetAlpha(drawColor);
				for (int i = 0; i < trailLength; i++) {
					Vector2 trailPosition = dust.position - dust.velocity * i;
					float trailScale = dust.scale * (1f - i / 10f);
					Main.spriteBatch.Draw(Texture2D.Value, trailPosition - Main.screenPosition, dust.frame, drawColor, dust.rotation, new Vector2(4f, 4f), trailScale, SpriteEffects.None, 0f);
				}
			}

			// By returning true, the default dust drawing will occur, drawing the final full scale dust.
			return true;
		}
	}
}
