using ExampleMod.Dusts;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Projectiles
{
	//ported from my tAPI mod because I'm lazy
	public class Wisp : ModProjectile
	{
		public override void SetStaticDefaults() {
			ProjectileID.Sets.Homing[projectile.type] = true;
		}

		public override void SetDefaults() {
			projectile.width = 8;
			projectile.height = 8;
			projectile.alpha = 255;
			projectile.friendly = true;
			projectile.tileCollide = false;
			projectile.ignoreWater = true;
			projectile.ranged = true;
		}

		public override void AI() {
			if (projectile.alpha > 70) {
				projectile.alpha -= 15;
				if (projectile.alpha < 70) {
					projectile.alpha = 70;
				}
			}
			if (projectile.localAI[0] == 0f) {
				AdjustMagnitude(ref projectile.velocity);
				projectile.localAI[0] = 1f;
			}
			Vector2 move = Vector2.Zero;
			float distance = 400f;
			bool target = false;
			for (int k = 0; k < 200; k++) {
				if (Main.npc[k].active && !Main.npc[k].dontTakeDamage && !Main.npc[k].friendly && Main.npc[k].lifeMax > 5) {
					Vector2 newMove = Main.npc[k].Center - projectile.Center;
					float distanceTo = (float)Math.Sqrt(newMove.X * newMove.X + newMove.Y * newMove.Y);
					if (distanceTo < distance) {
						move = newMove;
						distance = distanceTo;
						target = true;
					}
				}
			}
			if (target) {
				AdjustMagnitude(ref move);
				projectile.velocity = (10 * projectile.velocity + move) / 11f;
				AdjustMagnitude(ref projectile.velocity);
			}
			if (projectile.alpha <= 100) {
				int dust = Dust.NewDust(projectile.position, projectile.width, projectile.height, DustType<SpectreDust>());
				Main.dust[dust].velocity /= 2f;
			}
		}

		private void AdjustMagnitude(ref Vector2 vector) {
			float magnitude = (float)Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
			if (magnitude > 6f) {
				vector *= 6f / magnitude;
			}
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
			if (Main.rand.NextBool()) {
				target.AddBuff(BuffType<Buffs.EtherealFlames>(), 300);
			}
		}
	}
}