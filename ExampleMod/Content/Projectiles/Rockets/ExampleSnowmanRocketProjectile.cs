using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Projectiles.Rockets
{
	public class ExampleSnowmanRocketProjectile : ModProjectile
	{
		public override void SetStaticDefaults() {
			ProjectileID.Sets.IsARocketThatDealsDoubleDamageToPrimaryEnemy[Type] = true; // Deals double damage on direct hits.
			ProjectileID.Sets.CultistIsResistantTo[Type] = true; // The Lunatic Cultist is resistant to homing weapons.
			ProjectileID.Sets.RocketsSkipDamageForPlayers[Type] = true; // This set makes it so the rocket doesn't deal damage to players.

			// This set handles some things for us already:
			// Sets the timeLeft to 3 and the projectile direction when colliding with an NPC or player in PVP (so the explosive can detonate).
			// Explosives also bounce off the top of Shimmer, detonate with no blast damage when touching the bottom or sides of Shimmer, and damage other players in For the Worthy worlds.
			ProjectileID.Sets.Explosive[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.width = 14;
			Projectile.height = 14;
			Projectile.friendly = true;
			Projectile.penetrate = -1; // Infinite penetration so that the blast can hit all enemies within its radius.
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.scale = 0.9f; // All snowmen rockets are 0.9f scale.

			// Rockets use explosive AI, ProjAIStyleID.Explosive (16). You could use that instead here with the correct AIType.
			// But, using our own AI allows us to customize things like the dusts that the rocket creates.
			// Projectile.aiStyle = ProjAIStyleID.Explosive;
			// AIType = ProjectileID.RocketSnowmanI;
		}
		public override void AI() {
			// If timeLeft is <= 3, then explode the rocket.
			if (Projectile.owner == Main.myPlayer && Projectile.timeLeft <= 3) {
				PrepareBombToBlow();
			}
			else {
				Projectile.localAI[1]++;
				// After 6 ticks, make the rocket completely opaque.
				if (Projectile.localAI[1] > 6f) {
					Projectile.alpha = 0; // 0 Alpha is completely opaque.
				}
				else {
					// Before then, fade in the rocket each tick.
					Projectile.alpha = (int)(255f - 42f * Projectile.localAI[1]) + 100;
					if (Projectile.alpha > 255) {
						Projectile.alpha = 255; // 255 Alpha is completely transparent.
					}
				}

				for (int i = 0; i < 2; i++) {
					// Don't start spawning dusts until after 9 ticks have passed.
					if (!(Projectile.localAI[1] > 9f)) {
						continue;
					}

					// These two variables are used to add some movement to the dusts.
					float velocityXAdder = 0f;
					float velocityYAdder = 0f;
					if (i == 1) {
						velocityXAdder = Projectile.velocity.X * 0.5f;
						velocityYAdder = Projectile.velocity.Y * 0.5f;
					}

					// Spawn some fire dusts.
					if (Main.rand.NextBool(2)) {
						Dust fireDust = Dust.NewDustDirect(new Vector2(Projectile.position.X + 3f + velocityXAdder, Projectile.position.Y + 3f + velocityYAdder) - Projectile.velocity * 0.5f, Projectile.width - 8, Projectile.height - 8, DustID.Torch, 0f, 0f, 100);
						fireDust.scale *= 1.4f + Main.rand.Next(10) * 0.1f;
						fireDust.velocity *= 0.2f;
						fireDust.noGravity = true;

						// Used by the liquid rockets which leave trails of their liquid instead of fire.
						// if (fireDust.type == Dust.dustWater()) {
						//	fireDust.scale *= 0.65f;
						//	fireDust.velocity += Projectile.velocity * 0.1f;
						// }
					}

					// Spawn some smoke dusts.
					if (Main.rand.NextBool(2)) {
						Dust smokeDust = Dust.NewDustDirect(new Vector2(Projectile.position.X + 3f + velocityXAdder, Projectile.position.Y + 3f + velocityYAdder) - Projectile.velocity * 0.5f, Projectile.width - 8, Projectile.height - 8, DustID.Smoke, 0f, 0f, 100, default, 0.5f);
						smokeDust.fadeIn = 0.5f + Main.rand.Next(5) * 0.1f;
						smokeDust.velocity *= 0.05f;
					}
				}

				// First, set the destination of the rocket to its current position. This will be updated in the following section.
				float projDestinationX = Projectile.position.X;
				float projDestinationY = Projectile.position.Y;
				float maxHomingDistance = 600f; // Max homing distance in pixels. 16 pixels per tile, so 600 pixels = 37.5 tiles.

				bool isHoming = false;
				Projectile.ai[0]++; // Timer for how long to wait before homing.

				// Wait a short amount of time before homing. 15 ticks in this case.
				if (Projectile.ai[0] > 15f) {
					Projectile.ai[0] = 15f;

					// Search through all of the NPCs to find a target.
					for (int i = 0; i < Main.maxNPCs; i++) {
						NPC searchNPC = Main.npc[i];
						// If the target can be homed on to.
						if (searchNPC.CanBeChasedBy(this)) {
							// Get the target's position.
							float targetPosX = searchNPC.position.X + (searchNPC.width / 2);
							float targetPosY = searchNPC.position.Y + (searchNPC.height / 2);
							// Find the distance from the projectile to the target.
							float distanceFromProjToTarget = Math.Abs(Projectile.position.X + (Projectile.width / 2) - targetPosX) + Math.Abs(Projectile.position.Y + (Projectile.height / 2) - targetPosY);
							// If the distance is within the max homing distance and the projectile has line of sight.
							if (distanceFromProjToTarget < maxHomingDistance && Collision.CanHit(Projectile.position, Projectile.width, Projectile.height, searchNPC.position, searchNPC.width, searchNPC.height)) {
								maxHomingDistance = distanceFromProjToTarget;
								projDestinationX = targetPosX;
								projDestinationY = targetPosY;
								isHoming = true;
							}
						}
					}
				}

				// If the rocket is not homing, set its destination to ahead of where it is currently traveling.
				if (!isHoming) {
					projDestinationX = Projectile.position.X + (Projectile.width / 2) + Projectile.velocity.X * 100f;
					projDestinationY = Projectile.position.Y + (Projectile.height / 2) + Projectile.velocity.Y * 100f;
				}

				// Values above 16f could cause the rocket to no clip through blocks.
				// To increase the speed even more, increase extraUpdates in SetDefaults().
				float speed = 16f;

				// Travel to the position set above. Either it will be to the target's position or just ahead of itself.
				Vector2 finalVelocity = (new Vector2(projDestinationX, projDestinationY) - Projectile.Center).SafeNormalize(-Vector2.UnitY) * speed;
				Projectile.velocity = Vector2.Lerp(Projectile.velocity, finalVelocity, 1f / 12f);

				// Rotate the rocket in the direction that it is moving and keep the sprite facing the correct direction.
				// This way the face on the sprite will always be right side up.
				if (Projectile.velocity.X < 0f) {
					Projectile.spriteDirection = -1;
					Projectile.rotation = (float)Math.Atan2(0f - Projectile.velocity.Y, 0f - Projectile.velocity.X) - MathHelper.PiOver2;
				}
				else {
					Projectile.spriteDirection = 1;
					Projectile.rotation = (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X) + MathHelper.PiOver2;
				}
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity) {
			Projectile.velocity *= 0f; // Stop moving so the explosion is where the rocket was.
			Projectile.timeLeft = 3; // Set the timeLeft to 3 so it can get ready to explode.
			return false; // Returning false is important here. Otherwise the projectile will die without being resized (no blast radius).
		}

		public override void PrepareBombToBlow() {
			Projectile.tileCollide = false; // This is important or the explosion will be in the wrong place if the rocket explodes on slopes.
			Projectile.alpha = 255; // Make the rocket invisible.

			// Resize the hitbox of the projectile for the blast "radius".
			// Rocket I: 128, Rocket III: 200, Mini Nuke Rocket: 250
			// Measurements are in pixels, so 128 / 16 = 8 tiles.
			Projectile.Resize(128, 128);
			// Set the knockback of the blast.
			// Rocket I: 8f, Rocket III: 10f, Mini Nuke Rocket: 12f
			Projectile.knockBack = 8f;
		}

		public override void OnKill(int timeLeft) {
			// Play an exploding sound.
			SoundEngine.PlaySound(SoundID.Item14, Projectile.position);

			// Resize the projectile again so the explosion dust and gore spawn from the middle.
			// Rocket I: 22, Rocket III: 80, Mini Nuke Rocket: 50
			Projectile.Resize(22, 22);

			// Spawn a bunch of smoke dusts.
			for (int i = 0; i < 30; i++) {
				Dust smoke = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default, 1.5f);
				smoke.velocity *= 1.4f;
			}

			// Spawn a bunch of fire dusts.
			for (int j = 0; j < 20; j++) {
				Dust fireDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 3.5f);
				fireDust.noGravity = true;
				fireDust.velocity *= 7f;
				fireDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 1.5f);
				fireDust.velocity *= 3f;
			}

			// Spawn a bunch of smoke gores.
			for (int k = 0; k < 2; k++) {
				float speedMulti = 0.4f;
				if (k == 1) {
					speedMulti = 0.8f;
				}

				Gore smokeGore = Gore.NewGoreDirect(Projectile.GetSource_Death(), Projectile.position, default, Main.rand.Next(GoreID.Smoke1, GoreID.Smoke3 + 1));
				smokeGore.velocity *= speedMulti;
				smokeGore.velocity += Vector2.One;
				smokeGore = Gore.NewGoreDirect(Projectile.GetSource_Death(), Projectile.position, default, Main.rand.Next(GoreID.Smoke1, GoreID.Smoke3 + 1));
				smokeGore.velocity *= speedMulti;
				smokeGore.velocity.X -= 1f;
				smokeGore.velocity.Y += 1f;
				smokeGore = Gore.NewGoreDirect(Projectile.GetSource_Death(), Projectile.position, default, Main.rand.Next(GoreID.Smoke1, GoreID.Smoke3 + 1));
				smokeGore.velocity *= speedMulti;
				smokeGore.velocity.X += 1f;
				smokeGore.velocity.Y -= 1f;
				smokeGore = Gore.NewGoreDirect(Projectile.GetSource_Death(), Projectile.position, default, Main.rand.Next(GoreID.Smoke1, GoreID.Smoke3 + 1));
				smokeGore.velocity *= speedMulti;
				smokeGore.velocity -= Vector2.One;
			}

			// To make the explosion destroy tiles, take a look at the commented out code in Example Rocket Projectile.
		}
	}
}