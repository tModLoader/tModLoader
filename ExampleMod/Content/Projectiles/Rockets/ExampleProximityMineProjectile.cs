using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Projectiles.Rockets
{
	public class ExampleProximityMineProjectile : ModProjectile
	{
		public override void SetStaticDefaults() {
			ProjectileID.Sets.IsAMineThatDealsTripleDamageWhenStationary[Type] = true; // Deal triple damage when not moving and "armed".
			ProjectileID.Sets.PlayerHurtDamageIgnoresDifficultyScaling[Type] = true; // Damage dealt to players does not scale with difficulty in vanilla.

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

			// Proximity Mines use explosive AI, ProjAIStyleID.Explosive (16). You could use that instead here with the correct AIType.
			// But, using our own AI allows us to customize things like the dusts that the mine creates.
			// Projectile.aiStyle = ProjAIStyleID.Explosive;
			// AIType = ProjectileID.ProximityMineI;
		}
		public override void AI() {
			// If timeLeft is <= 3, then explode the mine.
			if (Projectile.owner == Main.myPlayer && Projectile.timeLeft <= 3) {
				Projectile.PrepareBombToBlow();
			}
			else {
				// If the mine is not moving or barely moving, make it turn almost invisible.
				if (Projectile.velocity.X > -0.2f && Projectile.velocity.X < 0.2f && Projectile.velocity.Y > -0.2f && Projectile.velocity.Y < 0.2f) {
					Projectile.alpha += 2;
					if (Projectile.alpha > 200) {
						Projectile.alpha = 200; // 255 Alpha is completely transparent. So, 200 is almost completely invisible.
					}
				}
				// Otherwise make it opaque and spawn a bunch of smoke dusts.
				else {
					Projectile.alpha = 0; // 0 Alpha is completely opaque.
					var smokeDust = Dust.NewDustDirect(new Vector2(Projectile.position.X + 3f, Projectile.position.Y + 3f) - Projectile.velocity * 0.5f, Projectile.width - 8, Projectile.height - 8, DustID.Smoke, 0f, 0f, 100);
					smokeDust.scale *= 1.6f + Main.rand.Next(5) * 0.1f;
					smokeDust.velocity *= 0.05f;
					smokeDust.noGravity = true;
				}
			}

			Projectile.velocity.Y += 0.2f; // Make it fall down. Remember, positive Y is down.
			Projectile.velocity *= 0.97f; // Make it slow down.

			// If the mine is moving very slowly, just make it stop entirely.
			if (Projectile.velocity.X > -0.1f && Projectile.velocity.X < 0.1f) {
				Projectile.velocity.X = 0f;
			}

			if (Projectile.velocity.Y > -0.1f && Projectile.velocity.Y < 0.1f) {
				Projectile.velocity.Y = 0f;
			}

			Projectile.rotation += Projectile.velocity.X * 0.1f; // Rotate the mine based on the direction it is moving.
		}

		public override bool OnTileCollide(Vector2 oldVelocity) {
			// Bounce off of tiles.
			if (Projectile.velocity.X != oldVelocity.X) {
				Projectile.velocity.X = oldVelocity.X * -0.4f;
			}

			if (Projectile.velocity.Y != oldVelocity.Y && oldVelocity.Y > 0.7f) {
				Projectile.velocity.Y = oldVelocity.Y * -0.4f;
			}
			// Return false so the projectile doesn't get killed. If you do want your projectile to explode on contact with tiles, do not return true here.
			// If you return true, the projectile will die without being resized (no blast radius).
			// Instead, set `Projectile.timeLeft = 3;` like the Example Rocket Projectile.
			return false;
		}

		public override void PrepareBombToBlow() {
			Projectile.tileCollide = false; // This is important or the explosion will be in the wrong place if the mine explodes on slopes.
			Projectile.alpha = 255; // Make the mine invisible.

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
				var smokeDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default, 1.5f);
				smokeDust.velocity *= 1.4f;
			}

			// Spawn a bunch of fire dusts.
			for (int j = 0; j < 20; j++) {
				var fireDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 3.5f);
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

				var smokeGore = Gore.NewGoreDirect(Projectile.GetSource_Death(), Projectile.position, default, Main.rand.Next(GoreID.Smoke1, GoreID.Smoke3 + 1));
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