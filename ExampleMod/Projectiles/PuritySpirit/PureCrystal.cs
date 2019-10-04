using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Projectiles.PuritySpirit
{
	public class PureCrystal : ModProjectile
	{
		private int timer;

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Crystal of Cleansing");
		}

		public override void SetDefaults() {
			projectile.width = 48;
			projectile.height = 48;
			projectile.penetrate = -1;
			projectile.magic = true;
			projectile.tileCollide = false;
			projectile.ignoreWater = true;
			projectile.netImportant = true;
		}

		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(projectile.localAI[0]);
			writer.Write(projectile.localAI[1]);
		}

		public override void ReceiveExtraAI(BinaryReader reader) {
			projectile.localAI[0] = reader.ReadSingle();
			projectile.localAI[1] = reader.ReadSingle();
		}

		public override void AI() {
			NPC center = Main.npc[(int)projectile.ai[0]];
			if (!center.active || center.type != NPCType<NPCs.PuritySpirit.PuritySpirit>()) {
				projectile.Kill();
			}
			if (timer < 120) {
				projectile.alpha = (120 - timer) * 255 / 120;
				timer++;
			}
			else {
				projectile.alpha = 0;
				projectile.hostile = true;
			}
			projectile.timeLeft = 2;
			projectile.ai[1] += 2f * (float)Math.PI / 600f * projectile.localAI[1];
			projectile.ai[1] %= 2f * (float)Math.PI;
			projectile.rotation -= 2f * (float)Math.PI / 120f * projectile.localAI[1];
			projectile.Center = center.Center + projectile.localAI[0] * new Vector2((float)Math.Cos(projectile.ai[1]), (float)Math.Sin(projectile.ai[1]));
		}

		public override void OnHitPlayer(Player target, int damage, bool crit) {
			for (int k = 0; k < Player.MaxBuffs; k++) {
				if (target.buffType[k] > 0 && target.buffTime[k] > 0 && BuffLoader.CanBeCleared(target.buffType[k]) && Main.rand.NextBool()) {
					target.DelBuff(k);
					k--;
				}
			}
		}

		public override Color? GetAlpha(Color lightColor) {
			return Color.White * ((255 - projectile.alpha) / 255f);
		}

		public override void PostDraw(SpriteBatch spriteBatch, Color lightColor) {
			//Vector2 drawPos = projectile.position - Main.screenPosition;
			//spriteBatch.Draw(mod.GetTexture("Projectiles/PuritySpirit/PureCrystalShield"), drawPos, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
			if (!projectile.hostile) {
				return;
			}
			Vector2 drawPos = projectile.Center - Main.screenPosition;
			Vector2 drawCenter = new Vector2(24f, 24f);
			for (int k = 2; k <= 24; k += 2) {
				float scale = 2f * k / 48f;
				spriteBatch.Draw(mod.GetTexture("Projectiles/PuritySpirit/PureCrystalRing"), drawPos, null, Color.White * ShieldTransparency(k), 0f, drawCenter, scale, SpriteEffects.None, 0f);
			}
		}

		private float ShieldTransparency(int radius) {
			switch (radius) {
				case 24:
					return 0.5f;
				case 22:
					return 0.35f;
				case 20:
					return 0.25f;
				case 18:
					return 0.2f;
				case 16:
					return 0.15f;
				case 14:
					return 0.1f;
				case 12:
					return 0.06f;
				case 10:
					return 0.03f;
				default:
					return 0.01f;
			}
		}
	}
}