﻿using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Projectiles
{
	// This projectile demonstrates exploding tiles (like a bomb or dynamite), spawning child projectiles, and explosive visual effects.
	// TODO: This projectile does not currently damage the owner, or damage other players on the For the worthy secret seed.
	public class ExampleExplosive : ModProjectile
	{
		private const int DefaultWidthHeight = 15;
		private const int ExplosionWidthHeight = 250;

		public override void SetDefaults() {
			// While the sprite is actually bigger than 15x15, we use 15x15 since it lets the projectile clip into tiles as it bounces. It looks better.
			Projectile.width = DefaultWidthHeight;
			Projectile.height = DefaultWidthHeight;
			Projectile.friendly = true;
			Projectile.penetrate = -1;

			// 5 second fuse.
			Projectile.timeLeft = 300;

			// These help the projectile hitbox be centered on the projectile sprite.
			DrawOffsetX = -2;
			DrawOriginOffsetY = -5;
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			// Vanilla explosions do less damage to Eater of Worlds in expert mode, so we will too.
			if (Main.expertMode) {
				if (target.type >= NPCID.EaterofWorldsHead && target.type <= NPCID.EaterofWorldsTail) {
					modifiers.FinalDamage /= 5;
				}
			}
		}

		// The projectile is very bouncy, but the spawned children projectiles shouldn't bounce at all.
		public override bool OnTileCollide(Vector2 oldVelocity) {
			// Die immediately if ai[1] isn't 0 (We set this to 1 for the 5 extra explosives we spawn in Kill)
			if (Projectile.ai[1] != 0) {
				return true;
			}
			// OnTileCollide can trigger quite frequently, so using soundDelay helps prevent the sound from overlapping too much.
			if (Projectile.soundDelay == 0) {
				// We adjust Volume since the sound is a bit too loud. PitchVariance gives the sound some random pitch variance.
				SoundStyle impactSound = new SoundStyle($"{nameof(ExampleMod)}/Assets/Sounds/Items/BananaImpact") {
					Volume = 0.7f,
					PitchVariance = 0.5f,
				};
				SoundEngine.PlaySound(impactSound);
			}
			Projectile.soundDelay = 10;

			// This code makes the projectile very bouncy.
			if (Projectile.velocity.X != oldVelocity.X && Math.Abs(oldVelocity.X) > 1f) {
				Projectile.velocity.X = oldVelocity.X * -0.9f;
			}
			if (Projectile.velocity.Y != oldVelocity.Y && Math.Abs(oldVelocity.Y) > 1f) {
				Projectile.velocity.Y = oldVelocity.Y * -0.9f;
			}
			return false;
		}

		public override void AI() {
			// The projectile is in the midst of exploding during the last 3 updates.
			if (Projectile.owner == Main.myPlayer && Projectile.timeLeft <= 3) {
				Projectile.tileCollide = false;
				// Set to transparent. This projectile technically lives as transparent for about 3 frames
				Projectile.alpha = 255;

				// change the hitbox size, centered about the original projectile center. This makes the projectile damage enemies during the explosion.
				Projectile.Resize(ExplosionWidthHeight, ExplosionWidthHeight);

				Projectile.damage = 250;
				Projectile.knockBack = 10f;
			}
			else {
				// Smoke and fuse dust spawn. The position is calculated to spawn the dust directly on the fuse.
				if (Main.rand.NextBool()) {
					Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default, 1f);
					dust.scale = 0.1f + Main.rand.Next(5) * 0.1f;
					dust.fadeIn = 1.5f + Main.rand.Next(5) * 0.1f;
					dust.noGravity = true;
					dust.position = Projectile.Center + new Vector2(1, 0).RotatedBy(Projectile.rotation - 2.1f, default) * 10f;

					dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 1f);
					dust.scale = 1f + Main.rand.Next(5) * 0.1f;
					dust.noGravity = true;
					dust.position = Projectile.Center + new Vector2(1, 0).RotatedBy(Projectile.rotation - 2.1f, default) * 10f;
				}
			}
			Projectile.ai[0] += 1f;
			if (Projectile.ai[0] > 10f) {
				Projectile.ai[0] = 10f;
				// Roll speed dampening. 
				if (Projectile.velocity.Y == 0f && Projectile.velocity.X != 0f) {
					Projectile.velocity.X = Projectile.velocity.X * 0.96f;

					if (Projectile.velocity.X > -0.01 && Projectile.velocity.X < 0.01) {
						Projectile.velocity.X = 0f;
						Projectile.netUpdate = true;
					}
				}
				// Delayed gravity
				Projectile.velocity.Y = Projectile.velocity.Y + 0.2f;
			}
			// Rotation increased by velocity.X 
			Projectile.rotation += Projectile.velocity.X * 0.1f;
		}

		public override void OnKill(int timeLeft) {
			// If we are the original projectile running on the owner, spawn the 5 child projectiles.
			if (Projectile.owner == Main.myPlayer && Projectile.ai[1] == 0) {
				for (int i = 0; i < 5; i++) {
					// Random upward vector.
					Vector2 launchVelocity = new Vector2(Main.rand.NextFloat(-3, 3), Main.rand.NextFloat(-10, -8));
					// Importantly, ai1 is set to 1 here. This is checked in OnTileCollide to prevent bouncing and here in Kill to prevent an infinite chain of splitting projectiles.
					Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, launchVelocity, Projectile.type, Projectile.damage, Projectile.knockBack, Main.myPlayer, 0, 1);
				}
			}

			// Play explosion sound
			SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
			// Smoke Dust spawn
			for (int i = 0; i < 50; i++) {
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default, 2f);
				dust.velocity *= 1.4f;
			}

			// Fire Dust spawn
			for (int i = 0; i < 80; i++) {
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 3f);
				dust.noGravity = true;
				dust.velocity *= 5f;
				dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 2f);
				dust.velocity *= 3f;
			}

			// Large Smoke Gore spawn
			for (int g = 0; g < 2; g++) {
				var goreSpawnPosition = new Vector2(Projectile.position.X + Projectile.width / 2 - 24f, Projectile.position.Y + Projectile.height / 2 - 24f);
				Gore gore = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), goreSpawnPosition, default, Main.rand.Next(61, 64), 1f);
				gore.scale = 1.5f;
				gore.velocity.X += 1.5f;
				gore.velocity.Y += 1.5f;
				gore = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), goreSpawnPosition, default, Main.rand.Next(61, 64), 1f);
				gore.scale = 1.5f;
				gore.velocity.X -= 1.5f;
				gore.velocity.Y += 1.5f;
				gore = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), goreSpawnPosition, default, Main.rand.Next(61, 64), 1f);
				gore.scale = 1.5f;
				gore.velocity.X += 1.5f;
				gore.velocity.Y -= 1.5f;
				gore = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), goreSpawnPosition, default, Main.rand.Next(61, 64), 1f);
				gore.scale = 1.5f;
				gore.velocity.X -= 1.5f;
				gore.velocity.Y -= 1.5f;
			}
			// reset size to normal width and height.
			Projectile.Resize(DefaultWidthHeight, DefaultWidthHeight);

			// Finally, actually explode the tiles and walls. Run this code only for the owner
			if (Projectile.owner == Main.myPlayer) {
				int explosionRadius = 7;
				int minTileX = (int)(Projectile.Center.X / 16f - explosionRadius);
				int maxTileX = (int)(Projectile.Center.X / 16f + explosionRadius);
				int minTileY = (int)(Projectile.Center.Y / 16f - explosionRadius);
				int maxTileY = (int)(Projectile.Center.Y / 16f + explosionRadius);

				// Ensure that all tile coordinates are within the world bounds
				Utils.ClampWithinWorld(ref minTileX, ref minTileY, ref maxTileX, ref maxTileY);

				// These 2 methods handle actually mining the tiles and walls while honoring tile explosion conditions
				bool explodeWalls = Projectile.ShouldWallExplode(Projectile.Center, explosionRadius, minTileX, maxTileX, minTileY, maxTileY);
				Projectile.ExplodeTiles(Projectile.Center, explosionRadius, minTileX, maxTileX, minTileY, maxTileY, explodeWalls);
			}
		}
	}
}
