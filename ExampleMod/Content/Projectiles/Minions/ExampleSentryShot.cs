using ExampleMod.Content.Dusts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Projectiles.Minions
{
	// The projectile shot by ExampleSentry.
	// The most important things needed for a projectile spawned by a sentry are:
	//		ProjectileID.Sets.SentryShot and Projectile.DamageType = DamageClass.Summon
	public class ExampleSentryShot : ModProjectile
	{
		public override void SetStaticDefaults() {
			// It is important that projectiles shot by sentries are in this set to properly work with effects that are triggered by sentry attacks.
			ProjectileID.Sets.SentryShot[Type] = true;
		}

		public override void SetDefaults() {
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.timeLeft = 600;
		}

		public override void AI() {
			if (Main.rand.NextBool(3)) {
				Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, ModContent.DustType<Sparkle>(), Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f, newColor: Color.SkyBlue);
			}
		}

		public override void OnKill(int timeLeft) {
			for (int k = 0; k < 5; k++) {
				Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, ModContent.DustType<Sparkle>(), Projectile.oldVelocity.X * 0.5f, Projectile.oldVelocity.Y * 0.5f, newColor: Color.SkyBlue);
			}
			SoundEngine.PlaySound(SoundID.Item25, Projectile.position);
		}
	}
}
