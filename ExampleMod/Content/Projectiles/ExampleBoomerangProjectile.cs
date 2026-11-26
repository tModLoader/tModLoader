using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Projectiles
{
	// This projectile recreates Wooden Boomerang behavior, demonstrating timed flight, returning to the owner, and bouncing off tiles.
	// A commented alternative shows how to reuse vanilla boomerang AI instead.
	public class ExampleBoomerangProjectile : ModProjectile
	{
		private ref float ReturnState => ref Projectile.ai[0];
		private ref float FlightTimer => ref Projectile.ai[1];

		public override void SetDefaults() {
			Projectile.width = 22;
			Projectile.height = 22;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.penetrate = -1;

			// To use vanilla behavior instead, uncomment these two lines. The custom hooks below will then defer to vanilla.
			// Projectile.aiStyle = ProjAIStyleID.Boomerang;
			// AIType = ProjectileID.WoodenBoomerang;
		}

		public override void AI() {
			if (Projectile.aiStyle == ProjAIStyleID.Boomerang) {
				return;
			}

			if (Projectile.soundDelay == 0) {
				Projectile.soundDelay = 8;
				SoundEngine.PlaySound(SoundID.Item7, Projectile.position);
			}

			if (ReturnState == 0f) {
				// Fly straight for 30 updates unless a collision causes an earlier return.
				FlightTimer++;
				if (FlightTimer >= 30f) {
					ReturnState = 1f;
					FlightTimer = 0f;
					Projectile.netUpdate = true;
				}
			}
			else {
				Projectile.tileCollide = false; // Returning boomerangs pass through tiles to reach their owner.
				Player owner = Main.player[Projectile.owner];
				Vector2 toOwner = owner.Center - Projectile.Center;
				if (toOwner.Length() > 3000f) {
					Projectile.Kill();
					return;
				}

				// Match vanilla's return speed adjustment for melee attack speed bonuses and penalties.
				float speedMultiplier = Math.Clamp(Utils.Remap(1.5f, 0f, 1f, 1f, 1f / owner.GetAttackSpeed(DamageClass.Melee), clamped: false), 1f, 10f);
				float returnSpeed = 9f * speedMultiplier;
				float acceleration = 0.4f * speedMultiplier;
				Vector2 desiredVelocity = toOwner.SafeNormalize(Vector2.Zero) * returnSpeed;

				// Adjust each axis separately, accelerating faster when reversing direction.
				Projectile.velocity.X = ApproachVelocity(Projectile.velocity.X, desiredVelocity.X, acceleration);
				Projectile.velocity.Y = ApproachVelocity(Projectile.velocity.Y, desiredVelocity.Y, acceleration);

				// Once the boomerang is close enough to the owner, despawn it.
				if (Main.myPlayer == Projectile.owner && Projectile.Hitbox.Intersects(owner.Hitbox)) {
					Projectile.Kill();
				}
			}

			// Vanilla's AutomaticallyChangesDirection excludes returning boomerangs. Preserve that update behavior with custom AI too.
			Projectile.manualDirectionChange = ReturnState == 1f;
			Projectile.rotation += 0.4f * Projectile.direction;
		}

		private static float ApproachVelocity(float velocity, float target, float acceleration) {
			if (velocity < target) {
				velocity += acceleration;
				if (velocity < 0f && target > 0f) {
					velocity += acceleration;
				}
			}
			else if (velocity > target) {
				velocity -= acceleration;
				if (velocity > 0f && target < 0f) {
					velocity -= acceleration;
				}
			}
			return velocity;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			ReturnAfterHit();
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			ReturnAfterHit();
		}

		private void ReturnAfterHit() {
			if (Projectile.aiStyle == ProjAIStyleID.Boomerang) {
				return;
			}

			if (ReturnState == 0f) {
				Projectile.velocity = -Projectile.velocity;
				Projectile.netUpdate = true;
			}
			ReturnState = 1f;
			Projectile.manualDirectionChange = true;
		}

		public override bool OnTileCollide(Vector2 oldVelocity) {
			if (Projectile.aiStyle == ProjAIStyleID.Boomerang) {
				return true;
			}

			Projectile.manualDirectionChange = true;
			Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
			SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
			ReturnState = 1f;
			Projectile.velocity = -oldVelocity;
			Projectile.netUpdate = true;
			return false; // A tile collision starts the return flight instead of destroying the projectile.
		}
	}
}
