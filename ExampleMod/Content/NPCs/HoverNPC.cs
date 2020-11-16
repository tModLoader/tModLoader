using System;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Content.NPCs
{
	// TODO: Clean-up and documentation.
	public abstract class HoverNPC : ModNPC
	{
		protected float speedX = 2f;
		protected float speedY = 1.5f;
        protected float accelerationX = 0.1f;
		protected float accelerationY = 0.04f;

		public override void AI() {
			if (!ShouldMove(npc.ai[3])) { // Is ShouldMove returns false, run our custom behavior. These are overridable hooks.
				CustomBehavior(ref npc.ai[3]);

				return;
			}

			bool flag = false;

			if (npc.justHit) {
				npc.ai[2] = 0f;
			}

			if (npc.ai[2] >= 0f) {
				int num = 16;
				bool flag2 = false;
				bool flag3 = false;

				if (npc.position.X > npc.ai[0] - (float)num && npc.position.X < npc.ai[0] + (float)num) {
					flag2 = true;
				}
				else if (npc.velocity.X < 0f && npc.direction > 0 || npc.velocity.X > 0f && npc.direction < 0) {
					flag2 = true;
				}

				num += 24;

				if (npc.position.Y > npc.ai[1] - (float)num && npc.position.Y < npc.ai[1] + (float)num) {
					flag3 = true;
				}

				if (flag2 && flag3) {
					npc.ai[2] += 1f;

					if (npc.ai[2] >= 30f && num == 16) {
						flag = true;
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

			int num2 = (int)((npc.position.X + (float)(npc.width / 2)) / 16f) + npc.direction * 2;
			int num3 = (int)((npc.position.Y + (float)npc.height) / 16f);
			int num4 = 3;
			bool flag4 = true;
			bool flag5 = false;

			for (int i = num3; i < num3 + num4; i++) {
				if (Main.tile[num2, i] == null) {
					Main.tile[num2, i] = new Tile();
				}

				if (Main.tile[num2, i].nactive() && Main.tileSolid[(int)Main.tile[num2, i].type] || Main.tile[num2, i].liquid > 0) {
					if (i <= num3 + 1) {
						flag5 = true;
					}

					flag4 = false;

					break;
				}
			}

			if (flag) {
				flag5 = false;
				flag4 = true;
			}

			if (flag4) {
				npc.velocity.Y += Math.Max(0.2f, 2.5f * accelerationY);

				if (npc.velocity.Y > Math.Max(2f, speedY)) {
					npc.velocity.Y = Math.Max(2f, speedY);
				}
			}
			else {
				if (npc.directionY < 0 && npc.velocity.Y > 0f || flag5) {
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

			if (npc.direction == -1 && npc.velocity.X > -speedX) {
				npc.velocity.X -= accelerationX;

				if (npc.velocity.X > speedX) {
					npc.velocity.X -= accelerationX;
				}
				else if (npc.velocity.X > 0f) {
					npc.velocity.X += accelerationX / 2f;
				}

				if (npc.velocity.X < -speedX) {
					npc.velocity.X = -speedX;
				}
			}
			else if (npc.direction == 1 && npc.velocity.X < speedX) {
				npc.velocity.X += accelerationX;

				if (npc.velocity.X < -speedX) {
					npc.velocity.X += accelerationX;
				}
				else if (npc.velocity.X < 0f) {
					npc.velocity.X -= accelerationX / 2f;
				}

				if (npc.velocity.X > speedX) {
					npc.velocity.X = speedX;
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

		public virtual bool ShouldMove(float ai) => true;
	}
}