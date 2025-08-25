using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Projectiles.Minions
{
	// ExampleSentry is an example sentry.
	// ExampleSentry demonstrates both floating and grounded sentry behaviors. Use ExampleSentryItem to the left of the player spawn a grounded sentry and use it to the right to spawn a floating sentry.
	// Sentries are similar to Minions, but typically don't move, are limited by the sentry limit instead of the minion limit, don't have a corresponding buff, and last for 10 minutes instead of despawning when the player dies.
	// The most critical fields necessary for a projectile to count as a sentry will be noted in this file and in ExampleSentryItem.cs. See also ExampleSentryShot.cs.
	public class ExampleSentry : ModProjectile
	{
		public ref float ShootTimer => ref Projectile.ai[0];

		public ref float TargetNPCIndex => ref Projectile.ai[1];

		public bool Floating => Projectile.ai[2] == 0;

		public bool JustSpawned {
			get => Projectile.localAI[0] == 0;
			set => Projectile.localAI[0] = value ? 0 : 1;
		}

		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 4;
			ProjectileID.Sets.MinionTargettingFeature[Type] = true;
		}

		public override void SetDefaults() {
			Projectile.width = 42;
			Projectile.height = 30;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.sentry = true; // Sets the weapon as a sentry for sentry accessories to properly work.
			Projectile.timeLeft = Projectile.SentryLifeTime; // Sentries last 10 minutes
			Projectile.ignoreWater = true;
			Projectile.netImportant = true; // Sentries need this so they are synced to newly joining players

			// The texture is 54 pixels wide, but we set width to 42 and DrawOffsetX to -6 so it doesn't look weird hanging off the edge of tiles (because it is oval shaped).
			DrawOffsetX = -6;
		}

		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			fallThrough = false; // Allow this projectile to collide with platforms
			return true;
		}

		public override bool OnTileCollide(Vector2 oldVelocity) {
			return false; // Prevent tile collision from killing the projectile
		}

		public override Color? GetAlpha(Color lightColor) {
			// Always draw fully bright. This is important because sentries can usually be placed inside tiles where it would be dark.
			return Color.White;
		}

		// This AI will function as a static sentry, and will not move. If you would like to know how to do more advanced minion AI, check out ExampleSimpleMinion.cs.
		public override void AI() {
			const int ShootFrequency = 60; // How long the sentry waits between shots.
			const int TargetingRange = 50 * 16; // The sentry's targeting range, 50 tiles.
			const float FireVelocity = 10f; // The velocity the sentry's shot projectile will travel.

			// Code to run when spawned
			if (JustSpawned) {
				JustSpawned = false;
				ShootTimer = ShootFrequency * 1.5f; // Delay the first shot slightly

				// The sound that Frost Hydra, Spider Turret, and Houndius Shootius play when spawned. Optional.
				SoundEngine.PlaySound(SoundID.Item46, Projectile.position);

				// Dust indicating the sentry spawned. Optional.
				for (int i = 0; i < 50; i++) {
					Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
					Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.BlueCrystalShard, speed * 4, Scale: 1.5f);
					d.noGravity = true;
				}
			}

			// Spawn dust randomly
			if (Main.rand.NextBool(10)) {
				Dust dust = Dust.NewDustDirect(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, DustID.Firework_Blue, Projectile.oldVelocity.X * 0.5f, Projectile.oldVelocity.Y * 0.5f);
				dust.noGravity = true;
				dust.velocity *= 0.8f;
			}

			// Gravity
			Projectile.velocity.X = 0f;
			if (!Floating) { // Only apply gravity if canPlaceInAir from ExampleSentryItem.Shoot was false
				Projectile.velocity.Y += 0.2f;
				if (Projectile.velocity.Y > 16f) {
					Projectile.velocity.Y = 16f;
				}
			}

			// Find an enemy to target.
			bool hasValidTarget = false;
			float closestTargetDistance = TargetingRange;
			TargetNPCIndex = -1;
			// Prioritize the owner's minion attack target. (Right click or whip feature)
			NPC ownerMinionAttackTargetNPC = Projectile.OwnerMinionAttackTargetNPC;
			if (ownerMinionAttackTargetNPC != null && ownerMinionAttackTargetNPC.CanBeChasedBy(this)) {
				float distanceToTargetNPC = Vector2.Distance(Projectile.Center, ownerMinionAttackTargetNPC.Center);
				if (distanceToTargetNPC < closestTargetDistance && Collision.CanHit(Projectile.position, Projectile.width, Projectile.height, ownerMinionAttackTargetNPC.position, ownerMinionAttackTargetNPC.width, ownerMinionAttackTargetNPC.height)) {
					closestTargetDistance = distanceToTargetNPC;
					hasValidTarget = true;
					TargetNPCIndex = ownerMinionAttackTargetNPC.whoAmI;
				}
			}

			// Otherwise check for the closest enemy.
			if (!hasValidTarget) {
				foreach (var npc in Main.ActiveNPCs) {
					if (npc.CanBeChasedBy(this)) {
						float distanceToTargetNPC = Vector2.Distance(Projectile.Center, npc.Center);
						// Is this enemy closer than others? Is it in line of sight?
						if (distanceToTargetNPC < closestTargetDistance && Collision.CanHit(Projectile.position, Projectile.width, Projectile.height, npc.position, npc.width, npc.height)) {
							closestTargetDistance = distanceToTargetNPC; // Set a new closest distance value
							hasValidTarget = true;
							TargetNPCIndex = npc.whoAmI;
						}
					}
				}
			}

			if (hasValidTarget) {
				NPC target = Main.npc[(int)TargetNPCIndex];
				if (ShootTimer <= 0) {
					ShootTimer = ShootFrequency;

					// Play a shoot sound
					SoundEngine.PlaySound(SoundID.Item102 with { Volume = 0.4f }, Projectile.Center);

					// Actually spawning the projectile only runs if the local player is the owner
					if (Main.myPlayer == Projectile.owner) {
						// The direction the projectile will fire.
						Vector2 shootDirection = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
						// The final velocity vector
						Vector2 shootVelocity = shootDirection * FireVelocity;

						// The type of projectile the sentry will shoot. It is important that sentry shots are included in ProjectileID.Sets.SentryShot, so reusing unrelated vanilla projectiles as-is won't work 100%.
						int type = ModContent.ProjectileType<ExampleSentryShot>();

						Projectile.NewProjectile(Projectile.GetSource_FromThis(), new Vector2(Projectile.Center.X - 4f, Projectile.Center.Y), shootVelocity, type, Projectile.damage, 3, Projectile.owner);
						// Note that Projectile.damage will take into account current equipment damage bonuses automatically for sentries and minions, so there is no need to calculate that here to take advantage of current equipment bonuses.
						// See Projectile.ContinuouslyUpdateDamageStats docs for more information.
					}
				}
			}

			// Count down the shoot timer
			ShootTimer--;

			// Animate the projectile. Cycle through 4 frames of animation spending 10 ticks on each.
			if (++Projectile.frameCounter >= 10) {
				Projectile.frameCounter = 0;
				Projectile.frame = ++Projectile.frame % Main.projFrames[Type];
			}
		}

		public override void OnKill(int timeLeft) {
			// Some sentries play a sound when despawned:
			//SoundEngine.PlaySound(SoundID.Item27, Projectile.position);

			// Dust indicating the sentry despawned
			for (int i = 0; i < 50; i++) {
				Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
				Dust d = Dust.NewDustPerfect(Projectile.Center + speed * 50, DustID.BlueCrystalShard, speed * -5, Scale: 1.5f);
				d.noGravity = true;
			}
		}
	}
}
