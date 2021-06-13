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

		// This is a reference property that allows us to write to Projectile.ai[0] without always needing to reference an array index
		public float VelocityMultiplier => ref Projectile.ai[0];

		// It appears that for this AI, only the ai[0] field is used!
		public override void AI() {
			// Since we access the owner player instance so much, it's useful to create a helper local variable for this
			Player player = Main.player[projectile.owner];
			// Here we set some of the projectile's owner properties, such as held item and itemtime, along with projectile direction and position based on the player
			Vector2 ownerMountedCenter = player.RotatedRelativePoint(player.MountedCenter, true);
			projectile.direction = player.direction; // Match the projectile direction with the player direction
			player.heldProj = projectile.whoAmI; // Update the player's held projectile
			player.itemTime = player.itemAnimation; // Match item's use time to the same number of frames as the animation
			projectile.position.X = ownerMountedCenter.X - (float)(projectile.width / 2);
			projectile.position.Y = ownerMountedCenter.Y - (float)(projectile.height / 2);
			// As long as the player isn't frozen, the spear can move
			if (!player.frozen) {
				if (VelocityMultiplier == 0f) { // When initially thrown out, the velocity multiplier will be 0
					VelocityMultiplier = 3f; // Make sure the spear accelerates when initially thrown out
					projectile.netUpdate = true; // Make sure to netUpdate this spear
				}
				if (player.itemAnimation < player.itemAnimationMax / 3) { // Somewhere along the item animation, make sure the spear decelerates
					VelocityMultiplier -= 2.4f;
				}
				else { // Otherwise, increase the velocity multiplier
					VelocityMultiplier += 2.1f;
				}
			}
			// Change the spear position based off of the velocity and the velocity multiplier
			projectile.position += projectile.velocity * VelocityMultiplier;
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
