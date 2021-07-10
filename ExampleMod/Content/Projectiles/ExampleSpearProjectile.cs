using ExampleMod.Dusts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Projectiles
{
	public class ExampleSpearProjectile : ModProjectile
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Spear");
		}

		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Spear); // Clone the default values for a vanilla spear. Spear specific values set for width, height, aiStyle, friendly, penetrate, tileCollide, scale, hide, ownerHitCheck, and melee.  

		}

		// Define the range of the Spear Projectile
		protected abstract float HoldoutRangeMin = 0;
		protected abstract float HoldoutRangeMax = 6;

		// It appears that for this AI, only the ai[0] field is used!
		public override void AI() {
			
			// Since we access the owner player instance so much, it's useful to create a helper local variable for this
			Player player = Main.player[projectile.owner];
			
			player.heldProj = projectile.whoAmI; // Update the player's held projectile
			player.itemTime = player.itemAnimation; // Match item's use time to the same number of frames as the animation

			int duration = player.itemAnimationMax * player.meleeSpeed; // Define the duration the projectile will exist in frames

			// Reset projectile time left if necessary
			if(Projectile.timeLeft == int.MaxValue) {
				Projectile.timeLeft = duration;
			}

			Projectile.velocity = Vector2.Normalize(Projectile.velocity);

			float halfDuration = duration * 0.5;
			float progress = Projectile.timeLeft > halfDuration 
			    ? (duration - Projectile.timeLeft) / halfDuration
				: Projectile.timeLeft / halfDuration;

			// Move the projectile from the HoldoutRangeMin to the HoldoutRangeMax and back
			Projectile.Center = player.MountedCenter + Vector2.SmoothStep(Projectile.velocity * HoldoutRangeMin,
																		  Projectile.velocity * HoldoutRangeMax, progress);

			// When we reach the end of the animation, we can kill the spear projectile
			if (player.itemAnimation == 0) {
				projectile.Kill();
			}
			// Apply proper rotation to the sprite.
			if (projectile.spriteDirection == -1) {
				// If sprite is facing left, rotate 45 degrees
				projectile.rotation += MathHelper.ToRadians(45f);
			} else {
				// If sprite is facing right, rotate 135 degrees
				projectile.rotation += MathHelper.ToRadians(135f);
			}

			// Avoid spawning dusts on dedicated servers
			if(!Main.dedServ) {
				// These dusts are added later, for the 'ExampleMod' effect
				if (Main.rand.NextBool(3)) {
					Dust dust = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, DustType<Sparkle>(),
						projectile.velocity.X * 2f, projectile.velocity.Y * 2f, 200, Scale: 1.2f);
				}
				if (Main.rand.NextBool(4)) {
					Dust dust = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, DustType<Sparkle>(),
						0, 0, 254, Scale: 0.3f);
				}
			}
		}
	}
}
