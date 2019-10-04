using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.GameContent.Achievements;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Projectiles
{
	// to investigate: Projectile.Damage, (8843)
	internal class ExampleExplosive : ModProjectile
	{
		public override void SetDefaults() {
			// while the sprite is actually bigger than 15x15, we use 15x15 since it lets the projectile clip into tiles as it bounces. It looks better.
			projectile.width = 15;
			projectile.height = 15;
			projectile.friendly = true;
			projectile.penetrate = -1;

			// 5 second fuse.
			projectile.timeLeft = 300;

			// These 2 help the projectile hitbox be centered on the projectile sprite.
			drawOffsetX = 5;
			drawOriginOffsetY = 5;
		}

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
			// Vanilla explosions do less damage to Eater of Worlds in expert mode, so we will too.
			if (Main.expertMode) {
				if (target.type >= NPCID.EaterofWorldsHead && target.type <= NPCID.EaterofWorldsTail) {
					damage /= 5;
				}
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity) {
			// Die immediately if ai[1] isn't 0 (We set this to 1 for the 5 extra explosives we spawn in Kill)
			if (projectile.ai[1] != 0) {
				return true;
			}
			// OnTileCollide can trigger quite quickly, so using soundDelay helps prevent the sound from overlapping too much.
			if (projectile.soundDelay == 0) {
				// We use WithVolume since the sound is a bit too loud, and WithPitchVariance to give the sound some random pitch variance.
				Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/BananaImpact").WithVolume(.7f).WithPitchVariance(.5f));
			}
			projectile.soundDelay = 10;

			// This code makes the projectile very bouncy.
			if (projectile.velocity.X != oldVelocity.X && Math.Abs(oldVelocity.X) > 1f) {
				projectile.velocity.X = oldVelocity.X * -0.9f;
			}
			if (projectile.velocity.Y != oldVelocity.Y && Math.Abs(oldVelocity.Y) > 1f) {
				projectile.velocity.Y = oldVelocity.Y * -0.9f;
			}
			return false;
		}

		public override void AI() {
			if (projectile.owner == Main.myPlayer && projectile.timeLeft <= 3) {
				projectile.tileCollide = false;
				// Set to transparent. This projectile technically lives as  transparent for about 3 frames
				projectile.alpha = 255;
				// change the hitbox size, centered about the original projectile center. This makes the projectile damage enemies during the explosion.
				projectile.position = projectile.Center;
				//projectile.position.X = projectile.position.X + (float)(projectile.width / 2);
				//projectile.position.Y = projectile.position.Y + (float)(projectile.height / 2);
				projectile.width = 250;
				projectile.height = 250;
				projectile.Center = projectile.position;
				//projectile.position.X = projectile.position.X - (float)(projectile.width / 2);
				//projectile.position.Y = projectile.position.Y - (float)(projectile.height / 2);
				projectile.damage = 250;
				projectile.knockBack = 10f;
			}
			else {
				// Smoke and fuse dust spawn.
				if (Main.rand.NextBool()) {
					int dustIndex = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 31, 0f, 0f, 100, default(Color), 1f);
					Main.dust[dustIndex].scale = 0.1f + (float)Main.rand.Next(5) * 0.1f;
					Main.dust[dustIndex].fadeIn = 1.5f + (float)Main.rand.Next(5) * 0.1f;
					Main.dust[dustIndex].noGravity = true;
					Main.dust[dustIndex].position = projectile.Center + new Vector2(0f, (float)(-(float)projectile.height / 2)).RotatedBy((double)projectile.rotation, default(Vector2)) * 1.1f;
					dustIndex = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 6, 0f, 0f, 100, default(Color), 1f);
					Main.dust[dustIndex].scale = 1f + (float)Main.rand.Next(5) * 0.1f;
					Main.dust[dustIndex].noGravity = true;
					Main.dust[dustIndex].position = projectile.Center + new Vector2(0f, (float)(-(float)projectile.height / 2 - 6)).RotatedBy((double)projectile.rotation, default(Vector2)) * 1.1f;
				}
			}
			projectile.ai[0] += 1f;
			if (projectile.ai[0] > 5f) {
				projectile.ai[0] = 10f;
				// Roll speed dampening.
				if (projectile.velocity.Y == 0f && projectile.velocity.X != 0f) {
					projectile.velocity.X = projectile.velocity.X * 0.97f;
					//if (projectile.type == 29 || projectile.type == 470 || projectile.type == 637)
					{
						projectile.velocity.X = projectile.velocity.X * 0.99f;
					}
					if ((double)projectile.velocity.X > -0.01 && (double)projectile.velocity.X < 0.01) {
						projectile.velocity.X = 0f;
						projectile.netUpdate = true;
					}
				}
				projectile.velocity.Y = projectile.velocity.Y + 0.2f;
			}
			// Rotation increased by velocity.X 
			projectile.rotation += projectile.velocity.X * 0.1f;
			return;
		}

		public override void Kill(int timeLeft) {
			// If we are the original projectile, spawn the 5 child projectiles
			if (projectile.ai[1] == 0) {
				for (int i = 0; i < 5; i++) {
					// Random upward vector.
					Vector2 vel = new Vector2(Main.rand.NextFloat(-3, 3), Main.rand.NextFloat(-10, -8));
					Projectile.NewProjectile(projectile.Center, vel, projectile.type, projectile.damage, projectile.knockBack, projectile.owner, 0, 1);
				}
			}
			// Play explosion sound
			Main.PlaySound(SoundID.Item15, projectile.position);
			// Smoke Dust spawn
			for (int i = 0; i < 50; i++) {
				int dustIndex = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 31, 0f, 0f, 100, default(Color), 2f);
				Main.dust[dustIndex].velocity *= 1.4f;
			}
			// Fire Dust spawn
			for (int i = 0; i < 80; i++) {
				int dustIndex = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 6, 0f, 0f, 100, default(Color), 3f);
				Main.dust[dustIndex].noGravity = true;
				Main.dust[dustIndex].velocity *= 5f;
				dustIndex = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 6, 0f, 0f, 100, default(Color), 2f);
				Main.dust[dustIndex].velocity *= 3f;
			}
			// Large Smoke Gore spawn
			for (int g = 0; g < 2; g++) {
				int goreIndex = Gore.NewGore(new Vector2(projectile.position.X + (float)(projectile.width / 2) - 24f, projectile.position.Y + (float)(projectile.height / 2) - 24f), default(Vector2), Main.rand.Next(61, 64), 1f);
				Main.gore[goreIndex].scale = 1.5f;
				Main.gore[goreIndex].velocity.X = Main.gore[goreIndex].velocity.X + 1.5f;
				Main.gore[goreIndex].velocity.Y = Main.gore[goreIndex].velocity.Y + 1.5f;
				goreIndex = Gore.NewGore(new Vector2(projectile.position.X + (float)(projectile.width / 2) - 24f, projectile.position.Y + (float)(projectile.height / 2) - 24f), default(Vector2), Main.rand.Next(61, 64), 1f);
				Main.gore[goreIndex].scale = 1.5f;
				Main.gore[goreIndex].velocity.X = Main.gore[goreIndex].velocity.X - 1.5f;
				Main.gore[goreIndex].velocity.Y = Main.gore[goreIndex].velocity.Y + 1.5f;
				goreIndex = Gore.NewGore(new Vector2(projectile.position.X + (float)(projectile.width / 2) - 24f, projectile.position.Y + (float)(projectile.height / 2) - 24f), default(Vector2), Main.rand.Next(61, 64), 1f);
				Main.gore[goreIndex].scale = 1.5f;
				Main.gore[goreIndex].velocity.X = Main.gore[goreIndex].velocity.X + 1.5f;
				Main.gore[goreIndex].velocity.Y = Main.gore[goreIndex].velocity.Y - 1.5f;
				goreIndex = Gore.NewGore(new Vector2(projectile.position.X + (float)(projectile.width / 2) - 24f, projectile.position.Y + (float)(projectile.height / 2) - 24f), default(Vector2), Main.rand.Next(61, 64), 1f);
				Main.gore[goreIndex].scale = 1.5f;
				Main.gore[goreIndex].velocity.X = Main.gore[goreIndex].velocity.X - 1.5f;
				Main.gore[goreIndex].velocity.Y = Main.gore[goreIndex].velocity.Y - 1.5f;
			}
			// reset size to normal width and height.
			projectile.position.X = projectile.position.X + (float)(projectile.width / 2);
			projectile.position.Y = projectile.position.Y + (float)(projectile.height / 2);
			projectile.width = 10;
			projectile.height = 10;
			projectile.position.X = projectile.position.X - (float)(projectile.width / 2);
			projectile.position.Y = projectile.position.Y - (float)(projectile.height / 2);

			// TODO, tmodloader helper method
			{
				int explosionRadius = 3;
				//if (projectile.type == 29 || projectile.type == 470 || projectile.type == 637)
				{
					explosionRadius = 7;
				}
				int minTileX = (int)(projectile.position.X / 16f - (float)explosionRadius);
				int maxTileX = (int)(projectile.position.X / 16f + (float)explosionRadius);
				int minTileY = (int)(projectile.position.Y / 16f - (float)explosionRadius);
				int maxTileY = (int)(projectile.position.Y / 16f + (float)explosionRadius);
				if (minTileX < 0) {
					minTileX = 0;
				}
				if (maxTileX > Main.maxTilesX) {
					maxTileX = Main.maxTilesX;
				}
				if (minTileY < 0) {
					minTileY = 0;
				}
				if (maxTileY > Main.maxTilesY) {
					maxTileY = Main.maxTilesY;
				}
				bool canKillWalls = false;
				for (int x = minTileX; x <= maxTileX; x++) {
					for (int y = minTileY; y <= maxTileY; y++) {
						float diffX = Math.Abs((float)x - projectile.position.X / 16f);
						float diffY = Math.Abs((float)y - projectile.position.Y / 16f);
						double distance = Math.Sqrt((double)(diffX * diffX + diffY * diffY));
						if (distance < (double)explosionRadius && Main.tile[x, y] != null && Main.tile[x, y].wall == 0) {
							canKillWalls = true;
							break;
						}
					}
				}
				AchievementsHelper.CurrentlyMining = true;
				for (int i = minTileX; i <= maxTileX; i++) {
					for (int j = minTileY; j <= maxTileY; j++) {
						float diffX = Math.Abs((float)i - projectile.position.X / 16f);
						float diffY = Math.Abs((float)j - projectile.position.Y / 16f);
						double distanceToTile = Math.Sqrt((double)(diffX * diffX + diffY * diffY));
						if (distanceToTile < (double)explosionRadius) {
							bool canKillTile = true;
							if (Main.tile[i, j] != null && Main.tile[i, j].active()) {
								canKillTile = true;
								if (Main.tileDungeon[(int)Main.tile[i, j].type] || Main.tile[i, j].type == 88 || Main.tile[i, j].type == 21 || Main.tile[i, j].type == 26 || Main.tile[i, j].type == 107 || Main.tile[i, j].type == 108 || Main.tile[i, j].type == 111 || Main.tile[i, j].type == 226 || Main.tile[i, j].type == 237 || Main.tile[i, j].type == 221 || Main.tile[i, j].type == 222 || Main.tile[i, j].type == 223 || Main.tile[i, j].type == 211 || Main.tile[i, j].type == 404) {
									canKillTile = false;
								}
								if (!Main.hardMode && Main.tile[i, j].type == 58) {
									canKillTile = false;
								}
								if (!TileLoader.CanExplode(i, j)) {
									canKillTile = false;
								}
								if (canKillTile) {
									WorldGen.KillTile(i, j, false, false, false);
									if (!Main.tile[i, j].active() && Main.netMode != 0) {
										NetMessage.SendData(17, -1, -1, null, 0, (float)i, (float)j, 0f, 0, 0, 0);
									}
								}
							}
							if (canKillTile) {
								for (int x = i - 1; x <= i + 1; x++) {
									for (int y = j - 1; y <= j + 1; y++) {
										if (Main.tile[x, y] != null && Main.tile[x, y].wall > 0 && canKillWalls && WallLoader.CanExplode(x, y, Main.tile[x, y].wall)) {
											WorldGen.KillWall(x, y, false);
											if (Main.tile[x, y].wall == 0 && Main.netMode != 0) {
												NetMessage.SendData(17, -1, -1, null, 2, (float)x, (float)y, 0f, 0, 0, 0);
											}
										}
									}
								}
							}
						}
					}
				}
				AchievementsHelper.CurrentlyMining = false;
			}
		}
	}
}
