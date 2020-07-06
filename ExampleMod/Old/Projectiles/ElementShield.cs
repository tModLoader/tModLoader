using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Projectiles
{
	public class ElementShield : ModProjectile
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Six-Color Shield");
			Main.projFrames[projectile.type] = 6;
		}

		public override void SetDefaults() {
			projectile.width = 48;
			projectile.height = 48;
			projectile.alpha = 75;
			projectile.penetrate = -1;
			projectile.friendly = true;
			projectile.magic = true;
			projectile.tileCollide = false;
			projectile.ignoreWater = true;
		}

		public override void AI() {
			if (projectile.localAI[0] == 0f) {
				if (projectile.ai[0] == 1) {
					projectile.coldDamage = true;
				}
				if (projectile.ai[0] == 3) {
					projectile.damage = (int)(1.2f * projectile.damage);
				}
				projectile.Name = GetName();
				projectile.localAI[0] = 1f;
			}
			Player player = Main.player[projectile.owner];
			if (!player.active || player.dead) {
				projectile.Kill();
				return;
			}
			ExamplePlayer modPlayer = player.GetModPlayer<ExamplePlayer>();
			if (modPlayer.elementShields <= projectile.ai[0]) {
				projectile.Kill();
				return;
			}
			projectile.timeLeft = 2;
			projectile.Center = player.Center;
			if (projectile.ai[0] > 0f) {
				float offset = (projectile.ai[0] - 1f) / (modPlayer.elementShields - 1);
				float rotation = modPlayer.elementShieldPos / 300f + offset;
				rotation = rotation % 1f * 2f * (float)Math.PI;
				projectile.position += 160f * new Vector2((float)Math.Cos(rotation), (float)Math.Sin(rotation));
				projectile.rotation = -rotation;
			}
			LightColor();
			projectile.frame = (int)projectile.ai[0];
			projectile.ai[1] += 1f;
			projectile.ai[1] %= 300f;
			projectile.alpha = 75 + (int)(50 * Math.Sin(projectile.ai[1] * 2f * (float)Math.PI / 300f));
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
			if (Main.rand.NextBool()) {
				int debuff = GetDebuff();
				if (debuff > 0) {
					target.AddBuff(debuff, GetDebuffTime());
				}
			}
		}

		public override void OnHitPvp(Player target, int damage, bool crit) {
			if (Main.rand.NextBool()) {
				int debuff = GetDebuff();
				if (debuff > 0) {
					target.AddBuff(debuff, GetDebuffTime() / 2);
				}
			}
		}

		public override Color? GetAlpha(Color lightColor) {
			return Color.White;
		}

		public string GetName() {
			switch ((int)projectile.ai[0]) {
				case 0:
					return "Fire Shield";
				case 1:
					return "Frost Shield";
				case 2:
					return "Ethereal Shield";
				case 3:
					return "Foam Shield";
				case 4:
					return "Venom Shield";
				case 5:
					return "Ichor Shield";
				default:
					return projectile.Name;
			}
		}

		public void LightColor() {
			float r = 0f;
			float g = 0f;
			float b = 0f;
			switch ((int)projectile.ai[0]) {
				case 0:
					r = 1f;
					g = 0.25f;
					b = 0.25f;
					break;
				case 1:
					r = 0.25f;
					g = 0.75f;
					b = 1f;
					break;
				case 2:
					r = 0.25f;
					g = 0.25f;
					b = 1f;
					break;
				case 3:
					r = 0.5f;
					g = 0.7f;
					b = 0.75f;
					break;
				case 4:
					r = 0.25f;
					g = 0.75f;
					b = 0.25f;
					break;
				case 5:
					r = 1f;
					g = 1f;
					b = 0.25f;
					break;
			}
			Lighting.AddLight(projectile.position, r, g, b);
		}

		public int GetDebuff() {
			switch ((int)projectile.ai[0]) {
				case 0:
					return BuffID.OnFire;
				case 1:
					return BuffID.Frostburn;
				case 2:
					return BuffType<Buffs.EtherealFlames>();
				case 3:
					return 0;
				case 4:
					return BuffID.Venom;
				case 5:
					return BuffID.Ichor;
				default:
					return 0;
			}
		}

		public int GetDebuffTime() {
			switch ((int)projectile.ai[0]) {
				case 0:
					return 600;
				case 1:
					return 400;
				case 2:
					return 300;
				case 3:
					return 0;
				case 4:
					return 400;
				case 5:
					return 900;
				default:
					return 0;
			}
		}
	}
}