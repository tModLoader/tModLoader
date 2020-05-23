using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace ExampleMod.NPCs
{
	//imported from my tAPI mod because I'm lazy
	public abstract class Fish : ModNPC
	{
		protected float speed = 7f;
		protected float speedY = 4f;
		protected float acceleration = 0.25f;
		protected float accelerationY = 0.2f;
		protected float correction = 0.95f;
		protected bool targetDryPlayer = true;
		protected float idleSpeed = 2f;
		protected bool bounces = true;

		public override void AI() {
			if (npc.direction == 0) {
				npc.TargetClosest(true);
			}
			if (npc.wet) {
				bool flag30 = false;
				npc.TargetClosest(false);
				if (Main.player[npc.target].wet && !Main.player[npc.target].dead) {
					flag30 = true;
				}
				if (!flag30) {
					if (npc.collideX) {
						npc.velocity.X *= -1f;
						npc.direction *= -1;
						npc.netUpdate = true;
					}
					if (npc.collideY) {
						npc.netUpdate = true;
						if (npc.velocity.Y > 0f) {
							npc.velocity.Y = -npc.velocity.Y;
							npc.directionY = -1;
							npc.ai[0] = -1f;
						}
						else if (npc.velocity.Y < 0f) {
							npc.velocity.Y = -npc.velocity.Y;
							npc.directionY = 1;
							npc.ai[0] = 1f;
						}
					}
				}
				if (flag30) {
					npc.TargetClosest(true);
					if (npc.velocity.X * npc.direction < 0f) {
						npc.velocity.X *= correction;
					}
					npc.velocity.X += npc.direction * acceleration;
					npc.velocity.Y += npc.directionY * accelerationY;
					if (npc.velocity.X > speed) {
						npc.velocity.X = speed;
					}
					if (npc.velocity.X < -speed) {
						npc.velocity.X = -speed;
					}
					if (npc.velocity.Y > speedY) {
						npc.velocity.Y = speedY;
					}
					if (npc.velocity.Y < -speedY) {
						npc.velocity.Y = -speedY;
					}
				}
				else {
					if (targetDryPlayer) {
						if (Main.player[npc.target].position.Y > npc.position.Y) {
							npc.directionY = 1;
						}
						else {
							npc.directionY = -1;
						}
						npc.velocity.X += (float)npc.direction * 0.1f * idleSpeed;
						if (npc.velocity.X < -idleSpeed || npc.velocity.X > idleSpeed) {
							npc.velocity.X *= 0.95f;
						}
						if (npc.ai[0] == -1f) {
							float num356 = -0.3f * idleSpeed;
							if (npc.directionY < 0) {
								num356 = -0.5f * idleSpeed;
							}
							if (npc.directionY > 0) {
								num356 = -0.1f * idleSpeed;
							}
							npc.velocity.Y -= 0.01f * idleSpeed;
							if (npc.velocity.Y < num356) {
								npc.ai[0] = 1f;
							}
						}
						else {
							float num357 = 0.3f * idleSpeed;
							if (npc.directionY < 0) {
								num357 = 0.1f * idleSpeed;
							}
							if (npc.directionY > 0) {
								num357 = 0.5f * idleSpeed;
							}
							npc.velocity.Y += 0.01f * idleSpeed;
							if (npc.velocity.Y > num357) {
								npc.ai[0] = -1f;
							}
						}
					}
					else {
						npc.velocity.X += (float)npc.direction * 0.1f * idleSpeed;
						if (npc.velocity.X < -idleSpeed || npc.velocity.X > idleSpeed) {
							npc.velocity.X *= 0.95f;
						}
						if (npc.ai[0] == -1f) {
							npc.velocity.Y -= 0.01f * idleSpeed;
							if ((double)npc.velocity.Y < -0.3) {
								npc.ai[0] = 1f;
							}
						}
						else {
							npc.velocity.Y += 0.01f * idleSpeed;
							if ((double)npc.velocity.Y > 0.3) {
								npc.ai[0] = -1f;
							}
						}
					}
					int num358 = (int)(npc.position.X + (float)(npc.width / 2)) / 16;
					int num359 = (int)(npc.position.Y + (float)(npc.height / 2)) / 16;
					if (Main.tile[num358, num359 - 1] == null) {
						Main.tile[num358, num359 - 1] = new Tile();
					}
					if (Main.tile[num358, num359 + 1] == null) {
						Main.tile[num358, num359 + 1] = new Tile();
					}
					if (Main.tile[num358, num359 + 2] == null) {
						Main.tile[num358, num359 + 2] = new Tile();
					}
					if (Main.tile[num358, num359 - 1].liquid > 128) {
						if (Main.tile[num358, num359 + 1].active()) {
							npc.ai[0] = -1f;
						}
						else if (Main.tile[num358, num359 + 2].active()) {
							npc.ai[0] = -1f;
						}
					}
					if (!targetDryPlayer && ((double)npc.velocity.Y > 0.4 || (double)npc.velocity.Y < -0.4)) {
						npc.velocity.Y *= 0.95f;
					}
				}
			}
			else {
				if (npc.velocity.Y == 0f) {
					if (!bounces) {
						npc.velocity.X *= 0.94f;
						if ((double)npc.velocity.X > -0.2 && (double)npc.velocity.X < 0.2) {
							npc.velocity.X = 0f;
						}
					}
					else if (Main.netMode != NetmodeID.MultiplayerClient) {
						npc.velocity.Y = (float)Main.rand.Next(-50, -20) * 0.1f;
						npc.velocity.X = (float)Main.rand.Next(-20, 20) * 0.1f;
						npc.netUpdate = true;
					}
				}
				npc.velocity.Y += 0.3f;
				if (npc.velocity.Y > 10f) {
					npc.velocity.Y = 10f;
				}
				npc.ai[0] = 1f;
			}
			npc.rotation = npc.velocity.Y * (float)npc.direction * 0.1f;
			if ((double)npc.rotation < -0.2) {
				npc.rotation = -0.2f;
			}
			if ((double)npc.rotation > 0.2) {
				npc.rotation = 0.2f;
			}
		}

		public override void FindFrame(int frameHeight) {
			npc.spriteDirection = npc.direction;
			npc.frameCounter += 1.0;
			if (npc.wet) {
				npc.frameCounter %= 24.0;
				npc.frame.Y = frameHeight * (int)(npc.frameCounter / 6.0);
			}
			else {
				npc.frameCounter %= 12.0;
				if (npc.frameCounter < 6.0) {
					npc.frame.Y = frameHeight * 4;
				}
				else {
					npc.frame.Y = frameHeight * 5;
				}
			}
		}
	}
}