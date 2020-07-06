using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Projectiles.PuritySpirit
{
	public class PurityBeam : ModProjectile
	{
		internal const float charge = 60f;

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("PurifyingColumn");
			Main.projFrames[projectile.type] = 3;
		}

		public override void SetDefaults() {
			projectile.width = 80;
			projectile.height = 14;
			projectile.penetrate = -1;
			projectile.magic = true;
			projectile.tileCollide = false;
			projectile.ignoreWater = true;
			cooldownSlot = 1;
		}

		public override void AI() {
			if (projectile.height != (int)projectile.ai[0]) {
				Vector2 center = projectile.Center;
				projectile.height = (int)projectile.ai[0];
				projectile.Center = center;
			}
			projectile.ai[1] += 1f;
			if (projectile.ai[1] == charge) {
				ExamplePlayer modPlayer = Main.LocalPlayer.GetModPlayer<ExamplePlayer>();
				if (modPlayer.heroLives > 0) {
					SoundEngine.PlaySound(SoundID.Zombie, -1, -1, 104);
				}
				else {
					SoundEngine.PlaySound(SoundID.Zombie, projectile.Center, 104);
				}
				projectile.hostile = true;
			}
			if (projectile.ai[1] >= charge + 60f) {
				projectile.Kill();
			}
		}

		public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit) {
			if (target.hurtCooldowns[1] <= 0) {
				ExamplePlayer modPlayer = target.GetModPlayer<ExamplePlayer>();
				modPlayer.constantDamage = projectile.damage;
				modPlayer.percentDamage = Main.expertMode ? 0.6f : 0.5f;
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
			Color color = Color.White * 0.8f;
			Vector2 drawPos = projectile.Top - Main.screenPosition;
			Rectangle frame = new Rectangle(0, 0, 100, 14);
			Vector2 drawCenter = new Vector2(50f, 0f);
			spriteBatch.Draw(Main.projectileTexture[projectile.type], drawPos, frame, color, 0f, drawCenter, 1f, SpriteEffects.None, 0f);
			drawPos.Y += projectile.height / 2;
			frame.Y += 14;
			drawCenter.Y += 7f;
			spriteBatch.Draw(Main.projectileTexture[projectile.type], drawPos, frame, color, 0f, drawCenter, new Vector2(Math.Min(projectile.ai[1], charge) / charge, (projectile.height - 28) / 14f), SpriteEffects.None, 0f);
			drawPos.Y += projectile.height / 2;
			frame.Y += 14;
			drawCenter.Y += 7f;
			spriteBatch.Draw(Main.projectileTexture[projectile.type], drawPos, frame, color, 0f, drawCenter, 1f, SpriteEffects.None, 0f);
			return false;
		}
	}
}