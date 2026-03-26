using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Projectiles
{
	// ExampleCustomSwingSword is an example of a sword with a custom swing using a held projectile
	// This is great if you want to make melee weapons with complex swing behavior
	// Note that this projectile only covers 2 relatively simple swings, everything else is up to you
	// Aside from the custom animation, the custom collision code in Colliding is very important to this weapon
	public class ExampleCustomSwingProjectile : ModProjectile
	{
		// We define some constants that determine the swing range of the sword
		// Not that we use multipliers here since that simplifies the amount of tweaks for these interactions
		// You could change the values or even replace them entirely, but they are tweaked with looks in mind
		private const float SWINGRANGE = 1.67f * (float)Math.PI; // The angle a swing attack covers (300 deg)
		private const float FIRSTHALFSWING = 0.45f; // How much of the swing happens before it reaches the target angle (in relation to swingRange)
		private const float SPINRANGE = 3.5f * (float)Math.PI; // The angle a spin attack covers (630 degrees)
		private const float WINDUP = 0.15f; // How far back the player's hand goes when winding their attack (in relation to swingRange)
		private const float UNWIND = 0.4f; // When should the sword start disappearing
		private const float SPINTIME = 2.5f; // How much longer a spin is than a swing

		private enum AttackType // Which attack is being performed
		{
			// Swings are normal sword swings that can be slightly aimed
			// Swings goes through the full cycle of animations
			Swing,
			// Spins are swings that go full circle
			// They are slower and deal more knockback
			Spin,
		}

		private enum AttackStage // What stage of the attack is being executed, see functions found in AI for description
		{
			Prepare,
			Execute,
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
				Timer = 0; // reset the timer when the projectile switches states
			}
		}

		// Variables to keep track of during runtime
		private ref float InitialAngle => ref Projectile.ai[1]; // Angle aimed in (with constraints)
		private ref float Timer => ref Projectile.ai[2]; // Timer to keep track of progression of each stage
		private ref float Progress => ref Projectile.localAI[1]; // Position of sword relative to initial angle
		private ref float Size => ref Projectile.localAI[2]; // Size of sword

		// We define timing functions for each stage, taking into account melee attack speed
		// Note that you can change this to suit the need of your projectile
		private float prepTime => 12f / Owner.GetTotalAttackSpeed(Projectile.DamageType);
		private float execTime => 12f / Owner.GetTotalAttackSpeed(Projectile.DamageType);
		private float hideTime => 12f / Owner.GetTotalAttackSpeed(Projectile.DamageType);

		public override string Texture => "ExampleMod/Content/Items/Weapons/ExampleCustomSwingSword"; // Use texture of item as projectile texture
		private Player Owner => Main.player[Projectile.owner];

		public override void SetStaticDefaults() {
			ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
			ProjectileID.Sets.AllowsContactDamageFromJellyfish[Type] = true;
		}

		public override void SetDefaults() {
			Projectile.width = 46; // Hitbox width of projectile
			Projectile.height = 48; // Hitbox height of projectile
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
			Projectile.spriteDirection = Main.MouseWorld.X > Owner.MountedCenter.X ? 1 : -1;
			float targetAngle = (Main.MouseWorld - Owner.MountedCenter).ToRotation();

			if (CurrentAttack == AttackType.Spin) {
				InitialAngle = (float)(-Math.PI / 2 - Math.PI * 1 / 3 * Projectile.spriteDirection); // For the spin, starting angle is designated based on direction of hit
			}
			else {
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

				InitialAngle = targetAngle - FIRSTHALFSWING * SWINGRANGE * Projectile.spriteDirection; // Otherwise, we calculate the angle
			}
		}

		public override void SendExtraAI(BinaryWriter writer) {
			// Projectile.spriteDirection for this projectile is derived from the mouse position of the owner in OnSpawn, as such it needs to be synced. spriteDirection is not one of the fields automatically synced over the network. All Projectile.ai slots are used already, so we will sync it manually.
			writer.Write((sbyte)Projectile.spriteDirection);
		}

		public override void ReceiveExtraAI(BinaryReader reader) {
			Projectile.spriteDirection = reader.ReadSByte();
		}

		public override void AI() {
			// Extend use animation until projectile is killed
			Owner.itemAnimation = 2;
			Owner.itemTime = 2;

			// Kill the projectile if the player dies or gets crowd controlled
			if (!Owner.active || Owner.dead || Owner.noItems || Owner.CCed) {
				Projectile.Kill();
				return;
			}

			// AI depends on stage and attack
			// Note that these stages are to facilitate the scaling effect at the beginning and end
			// If this is not desirable for you, feel free to simplify
			switch (CurrentStage) {
				case AttackStage.Prepare:
					PrepareStrike();
					break;
				case AttackStage.Execute:
					ExecuteStrike();
					break;
				default:
					UnwindStrike();
					break;
			}

			SetSwordPosition();
			Timer++;
		}

		public override bool PreDraw(ref Color lightColor) {
			// Calculate origin of sword (hilt) based on orientation and offset sword rotation (as sword is angled in its sprite)
			Vector2 origin;
			float rotationOffset;
			SpriteEffects effects;

			if (Projectile.spriteDirection > 0) {
				origin = new Vector2(0, Projectile.height);
				rotationOffset = MathHelper.ToRadians(45f);
				effects = SpriteEffects.None;
			}
			else {
				origin = new Vector2(Projectile.width, Projectile.height);
				rotationOffset = MathHelper.ToRadians(135f);
				effects = SpriteEffects.FlipHorizontally;
			}

			Texture2D texture = TextureAssets.Projectile[Type].Value;

			Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, default, lightColor * Projectile.Opacity, Projectile.rotation + rotationOffset, origin, Projectile.scale, effects, 0);

			// Since we are doing a custom draw, prevent it from normally drawing
			return false;
		}

		// Find the start and end of the sword and use a line collider to check for collision with enemies
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			Vector2 start = Owner.MountedCenter;
			Vector2 end = start + Projectile.rotation.ToRotationVector2() * ((Projectile.Size.Length()) * Projectile.scale);
			float collisionPoint = 0f;
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 15f * Projectile.scale, ref collisionPoint);
		}

		// Do a similar collision check for tiles
		public override void CutTiles() {
			Vector2 start = Owner.MountedCenter;
			Vector2 end = start + Projectile.rotation.ToRotationVector2() * (Projectile.Size.Length() * Projectile.scale);
			Utils.PlotTileLine(start, end, 15 * Projectile.scale, DelegateMethods.CutTiles);
		}

		// We make it so that the projectile can only do damage in its release and unwind phases
		public override bool? CanDamage() {
			if (CurrentStage == AttackStage.Prepare)
				return false;
			return base.CanDamage();
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			// Make knockback go away from player
			modifiers.HitDirectionOverride = target.position.X > Owner.MountedCenter.X ? 1 : -1;

			// If the NPC is hit by the spin attack, increase knockback slightly
			if (CurrentAttack == AttackType.Spin)
				modifiers.Knockback += 1;
		}

		// Function to easily set projectile and arm position
		public void SetSwordPosition() {
			Projectile.rotation = InitialAngle + Projectile.spriteDirection * Progress; // Set projectile rotation

			// Set composite arm allows you to set the rotation of the arm and stretch of the front and back arms independently
			Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.ToRadians(90f)); // set arm position (90 degree offset since arm starts lowered)
			Vector2 armPosition = Owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - (float)Math.PI / 2); // get position of hand

			// Adjust the position for reversed gravity.
			if (Owner.gravDir == -1f) {
				Projectile.rotation = 0f - Projectile.rotation;
				armPosition.Y = Owner.Bottom.Y + (Owner.position.Y - armPosition.Y);
			}

			armPosition.Y += Owner.gfxOffY;
			Projectile.Center = armPosition; // Set projectile to arm position
			Projectile.scale = Size * 1.2f * Owner.GetAdjustedItemScale(Owner.HeldItem); // Slightly scale up the projectile and also take into account melee size modifiers

			Owner.heldProj = Projectile.whoAmI; // set held projectile to this projectile
		}

		// Function facilitating the taking out of the sword
		private void PrepareStrike() {
			Progress = WINDUP * SWINGRANGE * (1f - Timer / prepTime); // Calculates rotation from initial angle
			Size = MathHelper.SmoothStep(0, 1, Timer / prepTime); // Make sword slowly increase in size as we prepare to strike until it reaches max

			if (Timer >= prepTime) {
				SoundEngine.PlaySound(SoundID.Item1); // Play sword sound here since playing it on spawn is too early
				CurrentStage = AttackStage.Execute; // If attack is over prep time, we go to next stage
			}
		}

		// Function facilitating the first half of the swing
		private void ExecuteStrike() {
			if (CurrentAttack == AttackType.Swing) {
				Progress = MathHelper.SmoothStep(0, SWINGRANGE, (1f - UNWIND) * Timer / execTime);

				if (Timer >= execTime) {
					CurrentStage = AttackStage.Unwind;
				}
			}
			else {
				Progress = MathHelper.SmoothStep(0, SPINRANGE, (1f - UNWIND / 2) * Timer / (execTime * SPINTIME));

				if (Timer == (int)(execTime * SPINTIME * 3 / 4)) {
					SoundEngine.PlaySound(SoundID.Item1); // Play sword sound again
					Projectile.ResetLocalNPCHitImmunity(); // Reset the local npc hit immunity for second half of spin
				}

				if (Timer >= execTime * SPINTIME) {
					CurrentStage = AttackStage.Unwind;
				}
			}
		}

		// Function facilitating the latter half of the swing where the sword disappears
		private void UnwindStrike() {
			if (CurrentAttack == AttackType.Swing) {
				Progress = MathHelper.SmoothStep(0, SWINGRANGE, (1f - UNWIND) + UNWIND * Timer / hideTime);
				Size = 1f - MathHelper.SmoothStep(0, 1, Timer / hideTime); // Make sword slowly decrease in size as we end the swing to make a smooth hiding animation

				if (Timer >= hideTime) {
					Projectile.Kill();
				}
			}
			else {
				Progress = MathHelper.SmoothStep(0, SPINRANGE, (1f - UNWIND / 2) + UNWIND / 2 * Timer / (hideTime * SPINTIME / 2));
				Size = 1f - MathHelper.SmoothStep(0, 1, Timer / (hideTime * SPINTIME / 2));

				if (Timer >= hideTime * SPINTIME / 2) {
					Projectile.Kill();
				}
			}
		}
	}
}