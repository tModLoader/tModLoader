using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Projectiles.PuritySpirit
{
	public class PuritySphere : ModProjectile
	{
		public const float radius = 240f;
		public const int strikeTime = 20;
		private int timer = -60;
		public int maxTimer;

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Purity Eye");
			Main.projFrames[projectile.type] = 4;
			ProjectileID.Sets.TrailingMode[projectile.type] = 0;
			ProjectileID.Sets.TrailCacheLength[projectile.type] = 20;
		}

		public override void SetDefaults() {
			projectile.width = 40;
			projectile.height = 40;
			projectile.penetrate = -1;
			projectile.magic = true;
			projectile.tileCollide = false;
			projectile.ignoreWater = true;
			projectile.alpha = 120;
			cooldownSlot = 1;
		}

		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(projectile.localAI[0]);
			writer.Write(projectile.localAI[1]);
			writer.Write(maxTimer);
		}

		public override void ReceiveExtraAI(BinaryReader reader) {
			projectile.localAI[0] = reader.ReadSingle();
			projectile.localAI[1] = reader.ReadSingle();
			maxTimer = reader.ReadInt32();
		}

		public override void AI() {
			if (timer < 0) {
				projectile.alpha = -timer * 3;
			}
			else {
				projectile.alpha = 0;
				projectile.hostile = true;
			}
			if (projectile.localAI[0] != 255f) {
				Player player = Main.player[(int)projectile.localAI[0]];
				if (!player.active || player.dead) {
					projectile.localAI[0] = 255f;
				}
			}
			Vector2 center = new Vector2(projectile.ai[0], projectile.ai[1]);
			if (timer < 0 && projectile.localAI[0] != 255f) {
				Vector2 newCenter = Main.player[(int)projectile.localAI[0]].Center;
				projectile.position += newCenter - center;
				projectile.ai[0] = newCenter.X;
				projectile.ai[1] = newCenter.Y;
				center = newCenter;
			}
			float rotateSpeed = 2f * (float)Math.PI / 60f / 4f * projectile.localAI[1];
			if (timer < maxTimer) {
				projectile.Center = projectile.Center.RotatedBy(rotateSpeed, center);
			}
			else {
				Vector2 offset = projectile.Center - center;
				offset.Normalize();
				offset *= radius * ((float)strikeTime + maxTimer - timer) / (float)strikeTime;
				projectile.Center = center + offset;
			}
			if (timer == maxTimer) {
				ExamplePlayer modPlayer = Main.LocalPlayer.GetModPlayer<ExamplePlayer>();
				if (modPlayer.heroLives > 0) {
					SoundEngine.PlaySound(SoundID.Item12);
				}
				else {
					SoundEngine.PlaySound(SoundID.Item12, projectile.position);
				}
				projectile.hostile = true;
			}
			if (timer >= maxTimer + strikeTime) {
				projectile.Kill();
			}
			timer++;
			projectile.rotation += rotateSpeed * -5f * projectile.localAI[1];
			projectile.spriteDirection = projectile.localAI[1] < 0 ? -1 : 1;
			if (projectile.frame < 4) {
				projectile.frameCounter++;
				if (projectile.frameCounter >= 8) {
					projectile.frameCounter = 0;
					projectile.frame++;
					projectile.frame %= 4;
				}
			}
		}

		public override Color? GetAlpha(Color lightColor) {
			return Color.White * ((255 - projectile.alpha / 2) / 255f);
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
			for (int k = 0; k < projectile.oldPos.Length; k++) {
				Vector2 previous = projectile.position;
				if (k > 0) {
					previous = projectile.oldPos[k - 1];
				}
				Color alpha = new Color(0, 190, 0) * ((strikeTime - k) / (float)strikeTime);
				Vector2 drawPos = projectile.oldPos[k] + projectile.Size / 2f - Main.screenPosition;
				for (int j = 0; j < 4; j++) {
					spriteBatch.Draw(mod.GetTexture("Projectiles/ElementLaser"), drawPos, null, alpha, k, Vector2.Zero, 1f, SpriteEffects.None, 0f);
					drawPos += (previous - projectile.oldPos[k]) / 4;
				}
			}
			return true;
		}
	}
}