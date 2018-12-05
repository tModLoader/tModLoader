using System;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.NPCs
{
	//ported from my tAPI mod because I'm lazy
	public abstract class Hover : ModNPC
	{
		protected float speed = 2f;
		protected float acceleration = 0.1f;
		protected float speedY = 1.5f;
		protected float accelerationY = 0.04f;

		public override void AI() {
			if (!ShouldMove(npc.ai[3])) {
				CustomBehavior(ref npc.ai[3]);
				return;
			}
			bool flag33 = false;
			if (npc.justHit) {
				npc.ai[2] = 0f;
			}
			if (npc.ai[2] >= 0f) {
				int num379 = 16;
				bool flag34 = false;
				bool flag35 = false;
				if (npc.position.X > npc.ai[0] - (float)num379 && npc.position.X < npc.ai[0] + (float)num379) {
					flag34 = true;
				}
				else if (npc.velocity.X < 0f && npc.direction > 0 || npc.velocity.X > 0f && npc.direction < 0) {
					flag34 = true;
				}
				num379 += 24;
				if (npc.position.Y > npc.ai[1] - (float)num379 && npc.position.Y < npc.ai[1] + (float)num379) {
					flag35 = true;
				}
				if (flag34 && flag35) {
					npc.ai[2] += 1f;
					if (npc.ai[2] >= 30f && num379 == 16) {
						flag33 = true;
					}
					if (npc.ai[2] >= 60f) {
						npc.ai[2] = -200f;
						npc.direction *= -1;
						npc.velocity.X *= -1f;
						npc.collideX = false;
					}
				}
				else {
					npc.ai[0] = npc.position.X;
					npc.ai[1] = npc.position.Y;
					npc.ai[2] = 0f;
				}
				npc.TargetClosest(true);
			}
			else {
				npc.ai[2] += 1f;
				if (Main.player[npc.target].position.X + (float)(Main.player[npc.target].width / 2) > npc.position.X + (float)(npc.width / 2)) {
					npc.direction = -1;
				}
				else {
					npc.direction = 1;
				}
			}
			int num380 = (int)((npc.position.X + (float)(npc.width / 2)) / 16f) + npc.direction * 2;
			int num381 = (int)((npc.position.Y + (float)npc.height) / 16f);
			bool flag36 = true;
			bool flag37 = false;
			int num382 = 3;
			for (int num404 = num381; num404 < num381 + num382; num404++) {
				if (Main.tile[num380, num404] == null) {
					Main.tile[num380, num404] = new Tile();
				}
				if (Main.tile[num380, num404].nactive() && Main.tileSolid[(int)Main.tile[num380, num404].type] || Main.tile[num380, num404].liquid > 0) {
					if (num404 <= num381 + 1) {
						flag37 = true;
					}
					flag36 = false;
					break;
				}
			}
			if (flag33) {
				flag37 = false;
				flag36 = true;
			}
			if (flag36) {
				npc.velocity.Y += Math.Max(0.2f, 2.5f * accelerationY);
				if (npc.velocity.Y > Math.Max(2f, speedY)) {
					npc.velocity.Y = Math.Max(2f, speedY);
				}
			}
			else {
				if (npc.directionY < 0 && npc.velocity.Y > 0f || flag37) {
					npc.velocity.Y -= 0.2f;
				}
				if (npc.velocity.Y < -4f) {
					npc.velocity.Y = -4f;
				}
			}
			if (npc.collideX) {
				npc.velocity.X = npc.oldVelocity.X * -0.4f;
				if (npc.direction == -1 && npc.velocity.X > 0f && npc.velocity.X < 1f) {
					npc.velocity.X = 1f;
				}
				if (npc.direction == 1 && npc.velocity.X < 0f && npc.velocity.X > -1f) {
					npc.velocity.X = -1f;
				}
			}
			if (npc.collideY) {
				npc.velocity.Y = npc.oldVelocity.Y * -0.25f;
				if (npc.velocity.Y > 0f && npc.velocity.Y < 1f) {
					npc.velocity.Y = 1f;
				}
				if (npc.velocity.Y < 0f && npc.velocity.Y > -1f) {
					npc.velocity.Y = -1f;
				}
			}
			if (npc.direction == -1 && npc.velocity.X > -speed) {
				npc.velocity.X -= acceleration;
				if (npc.velocity.X > speed) {
					npc.velocity.X -= acceleration;
				}
				else if (npc.velocity.X > 0f) {
					npc.velocity.X += acceleration / 2f;
				}
				if (npc.velocity.X < -speed) {
					npc.velocity.X = -speed;
				}
			}
			else if (npc.direction == 1 && npc.velocity.X < speed) {
				npc.velocity.X += acceleration;
				if (npc.velocity.X < -speed) {
					npc.velocity.X += acceleration;
				}
				else if (npc.velocity.X < 0f) {
					npc.velocity.X -= acceleration / 2f;
				}
				if (npc.velocity.X > speed) {
					npc.velocity.X = speed;
				}
			}
			if (npc.directionY == -1 && (double)npc.velocity.Y > -speedY) {
				npc.velocity.Y -= accelerationY;
				if ((double)npc.velocity.Y > speedY) {
					npc.velocity.Y -= accelerationY * 1.25f;
				}
				else if (npc.velocity.Y > 0f) {
					npc.velocity.Y += accelerationY * 0.75f;
				}
				if ((double)npc.velocity.Y < -speedY) {
					npc.velocity.Y = -speedY;
				}
			}
			else if (npc.directionY == 1 && (double)npc.velocity.Y < speedY) {
				npc.velocity.Y += accelerationY;
				if ((double)npc.velocity.Y < -speedY) {
					npc.velocity.Y += accelerationY * 1.25f;
				}
				else if (npc.velocity.Y < 0f) {
					npc.velocity.Y -= accelerationY * 0.75f;
				}
				if ((double)npc.velocity.Y > speedY) {
					npc.velocity.Y = speedY;
				}
			}
			CustomBehavior(ref npc.ai[3]);
		}

		public virtual void CustomBehavior(ref float ai) {
		}

		public virtual bool ShouldMove(float ai) {
			return true;
		}
	}
}