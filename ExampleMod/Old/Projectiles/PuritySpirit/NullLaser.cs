using ExampleMod.Buffs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Projectiles.PuritySpirit
{
	public class NullLaser : ModProjectile
	{
		public float warningTime;

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Nullification Laser");
			Main.projFrames[projectile.type] = 3;
		}

		public override void SetDefaults() {
			projectile.width = 40;
			projectile.height = 40;
			projectile.hide = true;
			projectile.penetrate = -1;
			projectile.magic = true;
			projectile.tileCollide = false;
			projectile.ignoreWater = true;
			cooldownSlot = 1;
		}

		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(projectile.localAI[0]);
			writer.Write(warningTime);
		}

		public override void ReceiveExtraAI(BinaryReader reader) {
			projectile.localAI[0] = reader.ReadSingle();
			warningTime = reader.ReadSingle();
		}

		public override void AI() {
			NPC npc = Main.npc[(int)projectile.ai[0]];
			if (!npc.active || npc.type != NPCType<NPCs.PuritySpirit.PuritySpirit>() || projectile.localAI[0] <= 0f) {
				projectile.Kill();
				return;
			}
			projectile.ai[1] -= 1f;
			projectile.localAI[0] -= 1f;
			if (projectile.localAI[0] < 0f) {
				projectile.Kill();
				return;
			}
			if (projectile.ai[1] <= warningTime) {
				projectile.hostile = true;
			}
			if (projectile.ai[1] == 0f) {
				SetDirection(npc);
				projectile.hide = false;
			}
			if (projectile.ai[1] <= 0) {
				CreateDust();
			}
		}

		private void SetDirection(NPC npc) {
			IList<int> targets = ((NPCs.PuritySpirit.PuritySpirit)npc.modNPC).targets;
			bool needsRotation = true;
			if (targets.Count > 0) {
				int player = targets[0];
				Vector2 offset = Main.player[player].Center - projectile.Center;
				if (offset != Vector2.Zero) {
					projectile.rotation = (float)Math.Atan2(offset.Y, offset.X);
					needsRotation = false;
				}
			}
			if (needsRotation) {
				projectile.rotation = -(float)Math.PI / 2f;
			}
			int numChecks = 3;
			projectile.localAI[1] = 0f;
			Vector2 direction = new Vector2((float)Math.Cos(projectile.rotation), (float)Math.Sin(projectile.rotation));
			for (int k = 0; k < numChecks; k++) {
				float side = (float)k / (numChecks - 1f);
				Vector2 sidePos = projectile.Center + direction.RotatedBy(Math.PI / 2) * (side - 0.5f) * projectile.width;
				int startX = (int)sidePos.X / 16;
				int startY = (int)sidePos.Y / 16;
				Vector2 endCheck = sidePos + direction * 16f * 150f;
				int endX = (int)endCheck.X / 16;
				int endY = (int)endCheck.Y / 16;
				Tuple<int, int> collide;
				if (!Collision.TupleHitLine(startX, startY, endX, endY, 0, 0, new List<Tuple<int, int>>(), out collide)) {
					projectile.localAI[1] += new Vector2((float)(startX - collide.Item1), (float)(startY - collide.Item2)).Length() * 16f;
				}
				else if (collide.Item1 == endX && collide.Item2 == endY) {
					projectile.localAI[1] += 1800f;
				}
				else {
					projectile.localAI[1] += new Vector2((float)(startX - collide.Item1), (float)(startY - collide.Item2)).Length() * 16f;
				}
			}
			projectile.localAI[1] /= numChecks;
		}

		private void CreateDust() {
			Color color = new Color(64, 255, 64);
			Vector2 direction = new Vector2((float)Math.Cos(projectile.rotation), (float)Math.Sin(projectile.rotation));
			Vector2 center = projectile.Center + direction * projectile.localAI[1];
			for (int k = 0; k < 4; k++) {
				float angle = projectile.rotation + (Main.rand.Next(2) * 2 - 1) * (float)Math.PI / 2f;
				float speed = (float)Main.rand.NextDouble() * 2.6f + 1f;
				Vector2 velocity = speed * new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
				int dust = Dust.NewDust(center, 0, 0, 267, velocity.X, velocity.Y, 0, color, 1.2f);
				Main.dust[dust].noGravity = true;
			}
		}

		public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit) {
			if (Main.rand.NextBool(3) || Main.expertMode && Main.rand.NextBool(3)) {
				target.AddBuff(BuffType<Nullified>(), Main.rand.Next(240, 300));
			}
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			if (projectile.ai[1] > -warningTime) {
				return false;
			}
			float num = 0f;
			Vector2 end = projectile.Center + projectile.localAI[1] * new Vector2((float)Math.Cos(projectile.rotation), (float)Math.Sin(projectile.rotation));
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), projectile.Center, end, projectile.width, ref num);
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
			Color color = Color.White * 0.9f;
			Vector2 center = projectile.Center + 0.5f * projectile.localAI[1] * new Vector2((float)Math.Cos(projectile.rotation), (float)Math.Sin(projectile.rotation)) - Main.screenPosition;
			Vector2 drawCenter = new Vector2(1f, 20f);
			Vector2 scale = new Vector2(projectile.localAI[1] / 2f, Math.Min(-projectile.ai[1], warningTime) / warningTime);
			spriteBatch.Draw(Main.projectileTexture[projectile.type], center, null, color, projectile.rotation, drawCenter, scale, SpriteEffects.None, 0f);
			return false;
		}
	}
}