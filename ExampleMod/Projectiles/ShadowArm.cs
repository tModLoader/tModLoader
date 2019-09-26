using ExampleMod.Dusts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Projectiles
{
	//ported from my tAPI mod because I'm lazy
	public class ShadowArm : ModProjectile
	{
		private const float increment = 16f;

		private Vector2 target => new Vector2(projectile.ai[0], projectile.ai[1]);

		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailCacheLength[projectile.type] = 60;
		}

		public override void SetDefaults() {
			projectile.width = 16;
			projectile.height = 16;
			projectile.magic = true;
			projectile.penetrate = -1;
			projectile.hostile = true;
			projectile.tileCollide = false;
			projectile.ignoreWater = true;
		}

		public override void AI() {
			if (projectile.localAI[1] == 0f) {
				Vector2 offset = target - projectile.Center;
				float angle = (float)Math.Atan2(offset.Y, offset.X);
				if (offset.X <= 0f) {
					angle += (float)Math.PI * 0.875f;
				}
				else {
					angle -= (float)Math.PI * 0.875f;
				}
				Vector2 targetOffset = increment * new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
				Add(projectile.position + targetOffset);
				projectile.localAI[1] = 1f;
			}
			if (Vector2.Distance(projectile.Center, target) <= increment) {
				projectile.localAI[0] = 1f;
			}
			if (projectile.localAI[0] > 0f) {
				if (projectile.localAI[0] < 25f) {
					Vector2 offset = projectile.position - projectile.oldPos[0];
					Add(projectile.position + offset);
					projectile.localAI[0] += 1f;
				}
				else {
					Remove();
				}
			}
			else {
				Vector2 offset = projectile.position - projectile.oldPos[0];
				Vector2 goal = target - projectile.Center;
				int triesLeft = (int)(goal.Length() / increment);
				double offsetAngle = Math.Atan2(offset.Y, offset.X);
				double goalAngle = Math.Atan2(goal.Y, goal.X);
				double angleDistance = Math.PI - Math.Abs(Math.PI - Math.Abs(offsetAngle - goalAngle));
				double angleIncrement = angleDistance / triesLeft;
				if (angleDistance >= Math.PI / 8 && angleIncrement < Math.PI / 16) {
					angleIncrement = Math.PI / 16;
				}
				double targetAngle1 = offsetAngle + angleIncrement;
				double targetAngle2 = offsetAngle - angleIncrement;
				double error1 = Math.PI - Math.Abs(Math.PI - Math.Abs(targetAngle1 - goalAngle));
				double error2 = Math.PI - Math.Abs(Math.PI - Math.Abs(targetAngle2 - goalAngle));
				Vector2 targetOffset = Vector2.Zero;
				if (error1 < error2) {
					targetOffset = increment * new Vector2((float)Math.Cos(targetAngle1), (float)Math.Sin(targetAngle1));
				}
				else {
					targetOffset = increment * new Vector2((float)Math.Cos(targetAngle2), (float)Math.Sin(targetAngle2));
				}
				Add(projectile.position + targetOffset);
			}
			CreateDust(projectile.position);
			for (int k = 0; k < projectile.oldPos.Length; k++) {
				if (projectile.oldPos[k] == Vector2.Zero) {
					break;
				}
				CreateDust(projectile.oldPos[k]);
			}
		}

		public void Add(Vector2 position) {
			for (int k = projectile.oldPos.Length - 1; k > 0; k--) {
				projectile.oldPos[k] = projectile.oldPos[k - 1];
			}
			projectile.oldPos[0] = projectile.position;
			projectile.position = position;
		}

		public void Remove() {
			int k;
			for (k = 0; k < projectile.oldPos.Length; k++) {
				if (projectile.oldPos[k] == Vector2.Zero) {
					if (k == 0) {
						projectile.active = false;
					}
					else {
						projectile.oldPos[k - 1] = Vector2.Zero;
					}
					return;
				}
			}
			projectile.oldPos[k - 1] = Vector2.Zero;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			for (int k = 0; k < projectile.oldPos.Length; k++) {
				if (projectile.oldPos[k] == Vector2.Zero) {
					return null;
				}
				projHitbox.X = (int)projectile.oldPos[k].X;
				projHitbox.Y = (int)projectile.oldPos[k].Y;
				if (projHitbox.Intersects(targetHitbox)) {
					return true;
				}
			}
			return null;
		}

		public void CreateDust(Vector2 pos) {
			if (Main.rand.NextBool(5)) {
				int dust = Dust.NewDust(pos, 16, 16, DustType<Smoke>(), 0f, 0f, 0, Color.Black);
				Main.dust[dust].scale = 2f;
				Main.dust[dust].velocity *= 0.5f;
			}
		}

		public override void PostDraw(SpriteBatch spriteBatch, Color lightColor) {
			for (int k = 0; k < projectile.oldPos.Length; k++) {
				if (projectile.oldPos[k] == Vector2.Zero) {
					return;
				}
				Vector2 drawPos = projectile.oldPos[k] - Main.screenPosition + projectile.Size / 2f;
				Color color = Lighting.GetColor((int)(projectile.oldPos[k].X / 16f), (int)(projectile.oldPos[k].Y / 16f));
				spriteBatch.Draw(Main.projectileTexture[projectile.type], drawPos, null, color, 0f, projectile.Size / 2f, 1f, SpriteEffects.None, 0f);
			}
		}
	}
}