using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.Enums;
using static Terraria.Player;

namespace ExampleMod.Content.Projectiles
{
	public class ExampleCustomSwingProjectile : ModProjectile
	{
		public override string Texture => "ExampleMod/Content/Items/Weapons/ExampleCustomSwingSword"; // Use texture of item as projectile texture

		private enum AttackType // Which attack is being performed
		{
			// Swings are normal sword swings that can be slightly aimed
			// Swings goes through the full cycle of animations
			Swing,
			// Spins are swings that go full circle
			// They are slower and deal more knockback
			Spin,
		}

		private enum AttackStage // What stage of the attack is being executed
		{
			Prepare,
			Release,
			Unwind
		}

		// These properties wrap the usual ai and localAI arrays for cleaner and easier to understand code.
		private AttackType CurrentAttack {
			get => (AttackType)Projectile.ai[0];
			set => Projectile.ai[0] = (float)value;
		}

		private AttackStage CurrentStage {
			get => (AttackStage)Projectile.localAI[0];
			set {
				Projectile.localAI[0] = (float)value;
				timer = 0; // reset the timer when the projectile switches states
			}
		}

		private ref float timer => ref Projectile.ai[1]; // Timer to keep track of progression of each stage
		private ref float targetAngle => ref Projectile.ai[2]; // Angle aimed in (with constraints)
		Player owner => Main.player[Projectile.owner];

		public override void SetDefaults() {
			Projectile.width = 40; // Hitbox width of projectile
			Projectile.height = 40; // Hitbox height of projectile
			Projectile.friendly = true; // Projectile hits enemies
			Projectile.timeLeft = 10000; // Time it takes for projectile to expire
			Projectile.penetrate = -1; // Projectile pierces infinitely
			Projectile.tileCollide = false; // Projectile does not collide with tiles
			Projectile.usesLocalNPCImmunity = true; // Uses local immunity frames
			Projectile.localNPCHitCooldown = -1; // We set this to -1 to make sure the projectile doesn't hit twice
			Projectile.ownerHitCheck = true; // Make sure the owner of the projectile has line of sight to the target (aka can't hit things through tile).
			Projectile.DamageType = DamageClass.Melee; // Projectile is a melee projectile
		}

		public override void OnSpawn(IEntitySource source) {
			Projectile.spriteDirection = Main.MouseWorld.X > owner.MountedCenter.X ? 1 : -1;
			targetAngle = (float)Math.Atan2(Main.MouseWorld.Y - owner.MountedCenter.Y, Main.MouseWorld.X - owner.MountedCenter.X);

			if (CurrentAttack == AttackType.Swing) {
				if (Projectile.spriteDirection == 1) {
					// However, we limit the rangle of possible directions so it does not look too ridiculous
					targetAngle = MathHelper.Clamp(targetAngle, (float)-Math.PI * 1 / 3, (float)Math.PI * 1 / 6);
				}
				else {
					if (targetAngle < 0) {
						targetAngle += 2 * (float)Math.PI; // This makes the range continuous for easier operations
					}
					targetAngle = MathHelper.Clamp(targetAngle, (float)Math.PI * 5 / 6, (float)Math.PI * 4 / 3);
				}
			}

			// Centers sword at hilt
			DrawOriginOffsetY = -Projectile.width / 2;
			DrawOriginOffsetX = -Projectile.height / 2 * Projectile.spriteDirection;
		}

		public override void AI() {
			// We define some constants that we can use later on to prevent confusion and unify location of values
			// Not that we use multipliers a here since that simplifies the amount of tweaks for these interactions
			// You could change the values or even replace them entirely, but they are tweaked with looks in mind
			float swingRange = 1.67f * (float)Math.PI; // The angle a swing attack covers (300 deg)
			float spinRange = 3.5f * (float)Math.PI; // The angle a spin attack covers (540 degrees)
			float firsthalfSwing = 0.45f; // How much of the swing happens before it reaches the target angle (in relation to swingRange)
			float preStrikeWind = 0.15f; // How far back the player's hand goes when winding their attack (in relation to swingRange)
			float postStrikeUnwind = 0.4f; // When should the sword start disappearing
			float spinTime = 2.5f; // How much longer a spin is than a swing
			float initialAngle = 0; // Initial rotation of projectile

			if (CurrentAttack == AttackType.Spin) {
				initialAngle = (float)(-Math.PI / 2 - Math.PI * 1 / 3 * Projectile.spriteDirection); // For the spin, starting angle is designated based on direction of hit
			}
			else {
				initialAngle = targetAngle - firsthalfSwing * swingRange * Projectile.spriteDirection; // Otherwise, we calculate the angle
			}

			// We set the use time of each stage based on some number, taking into account melee attack speed 
			float prepTime = 12f / owner.GetTotalAttackSpeed<MeleeDamageClass>();
			float execTime = 12f / owner.GetTotalAttackSpeed<MeleeDamageClass>();
			float hideTime = 12f / owner.GetTotalAttackSpeed<MeleeDamageClass>();

			// Extend use animation until projectile is killed
			owner.itemAnimation = 2;
			owner.itemTime = 2;

			// Kill the projectile if the player dies or gets crowd controlled
			if (!owner.active || owner.dead || owner.noItems || owner.CCed) {
				Projectile.Kill();
				return;
			}

			float progress;
			float size;

			// AI depends on stage and attack
			switch (CurrentStage) {
				case AttackStage.Prepare:
					progress = preStrikeWind * swingRange * (1f - timer / prepTime); // Calculates rotation from initial angle
					size = MathHelper.SmoothStep(0, 1, timer / prepTime); // Make sword slowly increase in size as we prepare to strike until it reaches max

					setSwordPosition(initialAngle + Projectile.spriteDirection * progress, size);

					if (timer >= prepTime) {
						SoundEngine.PlaySound(SoundID.Item1); // Play sword sound here since playing it on spawn is too early
						CurrentStage = AttackStage.Release; // If attack is over prep time, we go to next stage
					}
					break;
				case AttackStage.Release:
					if (CurrentAttack == AttackType.Swing) {
						progress = MathHelper.SmoothStep(0, swingRange, (1f - postStrikeUnwind) * timer / execTime);
						setSwordPosition(initialAngle + Projectile.spriteDirection * progress);

						if (timer >= execTime) {
							CurrentStage = AttackStage.Unwind;
						}
					}
					else {
						progress = MathHelper.SmoothStep(0, spinRange, (1f - postStrikeUnwind / 2) * timer / (execTime * spinTime));
						setSwordPosition(initialAngle + Projectile.spriteDirection * progress);

						if (timer == (int)(execTime * spinTime * 3 / 4)) {
							SoundEngine.PlaySound(SoundID.Item1); // Play sword sound again
							Projectile.ResetLocalNPCHitImmunity(); // Reset the local npc hit immunity for second half of spin
						}

						if (timer >= execTime * spinTime) {
							CurrentStage = AttackStage.Unwind;
						}
					}
					break;
				default:
					if (CurrentAttack == AttackType.Swing) {
						progress = MathHelper.SmoothStep(0, swingRange, (1f - postStrikeUnwind) + postStrikeUnwind * timer / hideTime);
						size = 1f - MathHelper.SmoothStep(0, 1, timer / hideTime); // Make sword slowly decrease in size as we end the swing to make a smooth hiding animation
						setSwordPosition(initialAngle + Projectile.spriteDirection * progress, size);

						if (timer >= hideTime) {
							Projectile.Kill();
						}
					}
					else {
						progress = MathHelper.SmoothStep(0, spinRange, (1f - postStrikeUnwind / 2) + postStrikeUnwind / 2 * timer / (hideTime * spinTime / 2));
						size = 1f - MathHelper.SmoothStep(0, 1, timer / (hideTime * spinTime / 2));
						setSwordPosition(initialAngle + Projectile.spriteDirection * progress, size);

						if (timer >= hideTime * spinTime / 2) {
							Projectile.Kill();
						}
					}
					break;
			}
			timer++;
		}

		// Function to easily set projectile and arm position
		public void setSwordPosition(float rotation, float size = 1) {
			owner.heldProj = Projectile.whoAmI; // set held projectile to this projectile

			// Set composite arm allows you to set the state of the front and back arms independently
			// This also allows for setting the rotation of the arm and the stretch amount independently
			owner.SetCompositeArmFront(true, CompositeArmStretchAmount.Full, rotation - MathHelper.ToRadians(90f)); // set arm position (90 degree offset since arm starts lowered)
			Vector2 armPosition = owner.GetFrontHandPosition(CompositeArmStretchAmount.Full, rotation - (float)Math.PI / 2); // get position of hand

			// This fixes a vanilla GetPlayerArmPosition bug causing the chain to draw incorrectly when stepping up slopes. This should be removed once the vanilla bug is fixed.
			armPosition.Y -= owner.gfxOffY;
			Projectile.position = armPosition; // Set projectile to arm position
			Projectile.position.Y -= (float)(Projectile.height / 2);

			Projectile.scale = size * 1.2f * owner.GetAdjustedItemScale(owner.HeldItem); // Slightly scale up the projectile and also take into account melee size modifiers
			Projectile.rotation = rotation + (float)Math.PI / 4; // Set projectile rotation (45 degrees offset since sword is already rotated -45 deg)

			// Projectile is offset in rotation and position when flipped, this is the fix
			if (Projectile.spriteDirection == -1) {
				Projectile.position.X -= Projectile.width;
				Projectile.rotation += (float)Math.PI / 2;
			}
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			// Find the start and end of the sword and use a line collider to check for collision with enemies
			Vector2 start = owner.MountedCenter;

			// Since we rotated the projectile when the direction is reverse, it messes up the angles so we offset it back 
			float angle = Projectile.spriteDirection == 1 ? Projectile.rotation : Projectile.rotation - (float)Math.PI / 2;
			Vector2 end = start + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * ((Projectile.Size.Length()) * Projectile.scale);
			float collisionPoint = 0f;
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 20f * Projectile.scale, ref collisionPoint);
		}

		public override void CutTiles() {
			// Do a similar collision check for tiles
			Vector2 start = owner.MountedCenter;
			float angle = Projectile.spriteDirection == 1 ? Projectile.rotation : Projectile.rotation - (float)Math.PI / 2;
			Vector2 end = start + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * ((Projectile.Size.Length()) * Projectile.scale);
			Utils.PlotTileLine(start, end, (Projectile.width + 16) * Projectile.scale, DelegateMethods.CutTiles);
		}

		// We make it so that the projectile can only do damage in its release and unwind phases
		public override bool? CanDamage() {
			if (CurrentStage == AttackStage.Prepare)
				return false;
			return base.CanDamage();
		}

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
			// Make knockback go away from player
			hitDirection = target.position.X > owner.position.X ? 1 : -1;

			// If the NPC is hit by the swing, increas knockback slightly
			if (CurrentAttack == AttackType.Spin)
				knockback++;
		}
	}
}