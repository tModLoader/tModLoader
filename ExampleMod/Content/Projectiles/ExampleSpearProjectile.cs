using ExampleMod.Content.Dusts;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.GameContent.Tile_Entities;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Projectiles
{
	public class ExampleSpearProjectile : ModProjectile
	{
		// Define the range of the Spear Projectile. These are overridable properties, in case you'll want to make a class inheriting from this one.
		protected virtual float HoldoutRangeMin => 24f;
		protected virtual float HoldoutRangeMax => 96f;

		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Spear); // Clone the default values for a vanilla spear. Spear specific values set for width, height, aiStyle, friendly, penetrate, tileCollide, scale, hide, ownerHitCheck, and melee.

			// Setting AIType along with a vanilla Projectile.aiStyle (which CloneDefaults sets) will make our modded projectile act like a vanilla projectile in its AI.
			// In the case for ExampleSpearProjectile, this lets the projectile match the vanilla spear for mannequins.
			AIType = ProjectileID.Spear;
		}

		public override bool PreAI() {
			Player player = Main.player[Projectile.owner]; // Since we access the owner player instance so much, it's useful to create a helper local variable for this
			int duration = player.itemAnimationMax; // Define the duration the projectile will exist in frames

			player.heldProj = Projectile.whoAmI; // Update the player's held projectile id

			// Reset projectile time left if necessary
			if (Projectile.timeLeft > duration) {
				Projectile.timeLeft = duration;
			}

			Projectile.velocity = Vector2.Normalize(Projectile.velocity); // Velocity isn't used in this spear implementation, but we use the field to store the spear's attack direction.

			float halfDuration = duration * 0.5f;
			float progress;

			// Here 'progress' is set to a value that goes from 0.0 to 1.0 and back during the item use animation.
			if (Projectile.timeLeft < halfDuration) {
				progress = Projectile.timeLeft / halfDuration;
			}
			else {
				progress = (duration - Projectile.timeLeft) / halfDuration;
			}

			// Move the projectile from the HoldoutRangeMin to the HoldoutRangeMax and back, using SmoothStep for easing the movement
			Projectile.Center = player.MountedCenter + Vector2.SmoothStep(Projectile.velocity * HoldoutRangeMin, Projectile.velocity * HoldoutRangeMax, progress);

			// Apply proper rotation to the sprite.
			if (Projectile.spriteDirection == -1) {
				// If sprite is facing left, rotate 45 degrees
				Projectile.rotation += MathHelper.ToRadians(45f);
			}
			else {
				// If sprite is facing right, rotate 135 degrees
				Projectile.rotation += MathHelper.ToRadians(135f);
			}

			// Avoid spawning dusts on dedicated servers
			if (!Main.dedServ) {
				// These dusts are added later, for the 'ExampleMod' effect
				if (Main.rand.NextBool(3)) {
					Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<Sparkle>(), Projectile.velocity.X * 2f, Projectile.velocity.Y * 2f, Alpha: 128, Scale: 1.2f);
				}

				if (Main.rand.NextBool(4)) {
					Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<Sparkle>(), Alpha: 128, Scale: 0.3f);
				}
			}

			return false; // Don't execute vanilla AI.
		}

		// This hook lets us change how the held projectile looks while a mannequin is holding it.
		// The following code is adapted from vanilla's Projectile.AI_DisplayDoll for aiStyle 19 (Spear)
		// If not setting an AIType, we need to customize the forward offset to make it look right and can't just use ProjAIStyleID.Spear for this one.
		// Since we are using AIType, this example is commented out and left as an example of what custom DisplayDollSettings code would look like.
		/*public override bool DisplayDollSettings(Player doll, TEDisplayDoll.DisplayDollPose pose, ref int aiStyle) {
			Projectile.direction = doll.direction;
			Projectile.spriteDirection = -Projectile.direction;
			Vector2 projectileDirection = Vector2.UnitX;
			float armRotation = 0f;
			if (pose.ItemAimRadians.HasValue)
				armRotation = pose.ItemAimRadians.Value;

			projectileDirection = projectileDirection.RotatedBy(armRotation);
			if (Projectile.direction == -1)
				projectileDirection.X *= -1f;

			Projectile.velocity = projectileDirection;

			int forwardOffset = 52; // This matches the vanilla Spear. Other spears may need a different value.
			Projectile.position += Projectile.velocity * forwardOffset;
			Projectile.rotation = projectileDirection.ToRotation() + (3f * MathHelper.PiOver4);
			if (Projectile.spriteDirection == -1)
				Projectile.rotation -= MathHelper.PiOver2;

			return false;
		}*/
	}
}
