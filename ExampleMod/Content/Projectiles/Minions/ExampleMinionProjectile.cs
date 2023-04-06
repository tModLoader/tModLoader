using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Projectiles.Minions
{
	public class ExampleMinionProjectile : ModProjectile
	{
		public override void SetStaticDefaults() {
			ProjectileID.Sets.MinionShot[Projectile.type] = true;
		}

		public override void SetDefaults() {
			Projectile.width = 16; // Sets the width of the projectile
			Projectile.height = 16; // Sets the height of the projectile
			Projectile.alpha = 255;
			Projectile.penetrate = 1; // The projectile will be destroyed on contact with enemies and tiles
			Projectile.friendly = true;
			Projectile.ignoreWater = true; // The projectile's movement ignores water
		}

		public override void AI() {
			if (Projectile.localAI[0] == 0f) {
				SoundEngine.PlaySound(SoundID.Item20, Projectile.position); // Plays the projectile's sound
				Projectile.localAI[0] = 1f;
			}

			// Creates a dust effect for the projectile
			var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 66, 0f, 0f, 100, new Color(0, 255, 0), 1.5f);

			dust.velocity *= 0.1f;

			if (Projectile.velocity == Vector2.Zero) {
				dust.velocity.Y -= 1f;
				dust.scale = 1.2f;
			}
			else {
				dust.velocity += Projectile.velocity * 0.2f;
			}

			dust.position.X = Projectile.Center.X + 4f + (float)Main.rand.Next(-2, 3);
			dust.position.Y = Projectile.Center.Y + (float)Main.rand.Next(-2, 3);
			dust.noGravity = true;
		}

		public override bool OnTileCollide(Vector2 oldVelocity) {
			Projectile.penetrate = -1;
			Projectile.maxPenetrate = -1;
			Projectile.tileCollide = false;
			Projectile.position += Projectile.velocity;
			Projectile.velocity = Vector2.Zero;
			Projectile.timeLeft = 180;

			return false;
		}
	}
}