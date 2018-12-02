using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Gores
{
	public class ExampleDroplet : ModGore
	{
		public override void OnSpawn(Gore gore) {
			gore.numFrames = 15;
			gore.behindTiles = true;
			gore.timeLeft = Gore.goreTime * 3;
		}

		public override bool Update(Gore gore) {
			gore.alpha = gore.position.Y < Main.worldSurface * 16.0 + 8.0 
				? 0 
				: 100;

			int frameDuration = 4;
			gore.frameCounter += 1;
			if (gore.frame <= 4) {
				int tileX = (int)(gore.position.X / 16f);
				int tileY = (int)(gore.position.Y / 16f) - 1;
				if (WorldGen.InWorld(tileX, tileY, 0) && !Main.tile[tileX, tileY].active()) {
					gore.active = false;
				}
				if (gore.frame == 0 || gore.frame == 1 || gore.frame == 2) {
					frameDuration = 24 + Main.rand.Next(256);
				}
				if (gore.frame == 3) {
					frameDuration = 24 + Main.rand.Next(96);
				}
				if ((int)gore.frameCounter >= frameDuration) {
					gore.frameCounter = 0;
					gore.frame += 1;
					if (gore.frame == 5) {
						int droplet = Gore.NewGore(gore.position, gore.velocity, gore.type, 1f);
						Main.gore[droplet].frame = 9;
						Main.gore[droplet].velocity *= 0f;
					}
				}
			}
			else if (gore.frame <= 6) {
				frameDuration = 8;
				if ((int)gore.frameCounter >= frameDuration) {
					gore.frameCounter = 0;
					gore.frame += 1;
					if (gore.frame == 7) {
						gore.active = false;
					}
				}
			}
			else if (gore.frame <= 9) {
				frameDuration = 6;
				gore.velocity.Y += 0.2f;
				if (gore.velocity.Y < 0.5f) {
					gore.velocity.Y = 0.5f;
				}
				if (gore.velocity.Y > 12f) {
					gore.velocity.Y = 12f;
				}
				if ((int)gore.frameCounter >= frameDuration) {
					gore.frameCounter = 0;
					gore.frame += 1;
				}
				if (gore.frame > 9) {
					gore.frame = 7;
				}
			}
			else {
				gore.velocity.Y += 0.1f;
				if ((int)gore.frameCounter >= frameDuration) {
					gore.frameCounter = 0;
					gore.frame += 1;
				}
				gore.velocity *= 0f;
				if (gore.frame > 14) {
					gore.active = false;
				}
			}

			Vector2 oldVelocity = gore.velocity;
			gore.velocity = Collision.TileCollision(gore.position, gore.velocity, 16, 14, false, false, 1);
			if (gore.velocity != oldVelocity) {
				if (gore.frame < 10) {
					gore.frame = 10;
					gore.frameCounter = 0;
					Main.PlaySound(SoundID.Drip, (int)gore.position.X + 8, (int)gore.position.Y + 8, Main.rand.Next(2));
				}
			}
			else if (Collision.WetCollision(gore.position + gore.velocity, 16, 14)) {
				if (gore.frame < 10) {
					gore.frame = 10;
					gore.frameCounter = 0;
					Main.PlaySound(SoundID.Drip, (int)gore.position.X + 8, (int)gore.position.Y + 8, 2);
				}
				int tileX = (int)(gore.position.X + 8f) / 16;
				int tileY = (int)(gore.position.Y + 14f) / 16;
				if (Main.tile[tileX, tileY] != null && Main.tile[tileX, tileY].liquid > 0) {
					gore.velocity *= 0f;
					gore.position.Y = tileY * 16 - (int)(Main.tile[tileX, tileY].liquid / 16);
				}
			}

			gore.position += gore.velocity;
			return false;
		}
	}
}