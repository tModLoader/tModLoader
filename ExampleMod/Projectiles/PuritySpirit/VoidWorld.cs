using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Projectiles.PuritySpirit
{
	public class VoidWorld : ModProjectile
	{
		private Random rand;

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Void World");
			Main.projFrames[projectile.type] = 8;
			ProjectileID.Sets.TrailingMode[projectile.type] = 0;
			ProjectileID.Sets.TrailCacheLength[projectile.type] = 200;
		}

		public override void SetDefaults() {
			projectile.width = 80;
			projectile.height = 80;
			projectile.penetrate = -1;
			projectile.magic = true;
			projectile.hostile = true;
			projectile.tileCollide = false;
			projectile.ignoreWater = true;
			cooldownSlot = 1;
		}

		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(projectile.localAI[0]);
		}

		public override void ReceiveExtraAI(BinaryReader reader) {
			projectile.localAI[0] = reader.ReadSingle();
		}

		public override void AI() {
			projectile.localAI[0] += 1f;
			if (!Main.dedServ && projectile.localAI[0] >= 180f && projectile.localAI[0] < 480f && Main.rand.NextBool(10)) {
				ExamplePlayer modPlayer = Main.LocalPlayer.GetModPlayer<ExamplePlayer>();
				if (modPlayer.heroLives > 0) {
					SoundEngine.PlaySound(SoundID.Item14);
				}
				else {
					SoundEngine.PlaySound(SoundID.Item14, projectile.position);
				}
			}
			projectile.position = NextPosition();
			if (projectile.localAI[0] >= 500f) {
				projectile.Kill();
			}
		}

		private Vector2 NextPosition() {
			if (rand == null) {
				rand = new Random((int)projectile.ai[1]);
			}
			const int interval = 60;
			int arenaWidth = NPCs.PuritySpirit.PuritySpirit.arenaWidth;
			int arenaHeight = NPCs.PuritySpirit.PuritySpirit.arenaHeight;
			NPC npc = Main.npc[(int)projectile.ai[0]];
			NPCs.PuritySpirit.PuritySpirit modNPC = (NPCs.PuritySpirit.PuritySpirit)npc.modNPC;
			Vector2 nextPos;
			if (projectile.localAI[0] > 300f) {
				nextPos = npc.Center;
			}
			else if ((int)projectile.localAI[0] % 100 == 0 || Main.expertMode && (int)projectile.localAI[0] % 50 == 0) {
				int k = modNPC.targets[rand.Next(modNPC.targets.Count)];
				nextPos = Main.player[k].Center;
			}
			else if (rand.Next(5) == 0) {
				int k = modNPC.targets[rand.Next(modNPC.targets.Count)];
				nextPos = Main.player[k].Center + interval * new Vector2(Main.rand.Next(-5, 6), Main.rand.Next(-5, 6));
				if (nextPos.X < npc.Center.X - arenaWidth / 2) {
					nextPos.X += arenaWidth;
				}
				else if (nextPos.X > npc.Center.X + arenaWidth / 2) {
					nextPos.X -= arenaWidth;
				}
				if (nextPos.Y < npc.Center.Y - arenaHeight / 2) {
					nextPos.Y += arenaHeight;
				}
				else if (nextPos.Y > npc.Center.Y + arenaHeight / 2) {
					nextPos.Y -= arenaHeight;
				}
			}
			else {
				int leftBound = (-arenaWidth / 2 + 40) / interval;
				int rightBound = (arenaWidth / 2 - 40) / interval + 1;
				int upperBound = (-arenaHeight / 2 + 40) / interval;
				int lowerBound = (arenaHeight / 2 - 40) / interval + 1;
				nextPos = npc.Center + interval * new Vector2(rand.Next(leftBound, rightBound), rand.Next(upperBound, lowerBound));
			}
			nextPos.X -= projectile.width / 2;
			nextPos.Y -= projectile.height / 2;
			return nextPos;
		}

		public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit) {
			if (target.hurtCooldowns[1] <= 0) {
				ExamplePlayer modPlayer = target.GetModPlayer<ExamplePlayer>();
				modPlayer.constantDamage = projectile.damage;
				modPlayer.percentDamage = Main.expertMode ? 1.2f : 1f;
			}
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			projHitbox.Width -= 16;
			projHitbox.Height -= 16;
			for (int k = Math.Max(180, (int)projectile.localAI[0] - 301); k < projectile.oldPos.Length; k++) {
				if (projectile.oldPos[k] != Vector2.Zero) {
					projHitbox.X = (int)projectile.oldPos[k].X + 8;
					projHitbox.Y = (int)projectile.oldPos[k].Y + 8;
					if (projHitbox.Intersects(targetHitbox)) {
						return true;
					}
				}

			}
			return false;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
			const int prime1 = 101;
			const int prime2 = 107;
			for (int k = Math.Max(0, (int)projectile.localAI[0] - 300); k < projectile.oldPos.Length; k++) {
				if (projectile.oldPos[k] != Vector2.Zero) {
					Vector2 drawPos = projectile.oldPos[k] - Main.screenPosition;
					drawPos.X += k / 5 * prime1 % 13 - 6;
					drawPos.Y += k / 5 * prime2 % 13 - 6;
					Rectangle frame = new Rectangle(0, 0, 80, 80);
					frame.Y += 164 * (k / 60);
					if (k / 10 % 2 == 1) {
						frame.Y += 82;
					}
					spriteBatch.Draw(Main.projectileTexture[projectile.type], drawPos, frame, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
				}
			}
			return false;
		}
	}
}