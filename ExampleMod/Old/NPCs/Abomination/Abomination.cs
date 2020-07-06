using ExampleMod.Dusts;
using ExampleMod.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.NPCs.Abomination
{
	//ported from my tAPI mod because I'm lazy
	// Abomination is a multi-stage boss.
	[AutoloadBossHead]
	public class Abomination : ModNPC
	{
		private static int hellLayer => Main.maxTilesY - 200;

		private const int sphereRadius = 300;

		private float attackCool {
			get => npc.ai[0];
			set => npc.ai[0] = value;
		}

		private float moveCool {
			get => npc.ai[1];
			set => npc.ai[1] = value;
		}

		private float rotationSpeed {
			get => npc.ai[2];
			set => npc.ai[2] = value;
		}

		private float captiveRotation {
			get => npc.ai[3];
			set => npc.ai[3] = value;
		}

		private int moveTime = 300;
		private int moveTimer = 60;
		internal int laserTimer;
		internal int laser1 = -1;
		internal int laser2 = -1;
		private bool dontDamage;

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("The Abomination");
			Main.npcFrameCount[npc.type] = 2;
		}

		public override void SetDefaults() {
			npc.aiStyle = -1;
			npc.lifeMax = 40000;
			npc.damage = 100;
			npc.defense = 55;
			npc.knockBackResist = 0f;
			npc.width = 100;
			npc.height = 100;
			npc.value = Item.buyPrice(0, 20, 0, 0);
			npc.npcSlots = 15f;
			npc.boss = true;
			npc.lavaImmune = true;
			npc.noGravity = true;
			npc.noTileCollide = true;
			npc.HitSound = SoundID.NPCHit1;
			npc.DeathSound = SoundID.NPCDeath1;
			npc.buffImmune[24] = true;
			music = MusicID.Boss2;
		}

		public override void ScaleExpertStats(int numPlayers, float bossLifeScale) {
			npc.lifeMax = (int)(npc.lifeMax * 0.625f * bossLifeScale);
			npc.damage = (int)(npc.damage * 0.6f);
		}

		public override void AI() {
			if (Main.netMode != NetmodeID.MultiplayerClient && npc.localAI[0] == 0f) {
				for (int k = 0; k < 5; k++) {
					int captive = NPC.NewNPC((int)npc.position.X, (int)npc.position.Y, NPCType<CaptiveElement>());
					Main.npc[captive].ai[0] = npc.whoAmI;
					Main.npc[captive].ai[1] = k;
					Main.npc[captive].ai[2] = 50 * (k + 1);
					if (k == 2) {
						Main.npc[captive].damage += 20;
					}
					CaptiveElement.SetPosition(Main.npc[captive]);
					Main.npc[captive].netUpdate = true;
				}
				npc.netUpdate = true;
				npc.localAI[0] = 1f;
			}
			Player player = Main.player[npc.target];
			if (!player.active || player.dead || player.position.Y < hellLayer * 16) {
				npc.TargetClosest(false);
				player = Main.player[npc.target];
				if (!player.active || player.dead || player.position.Y < hellLayer * 16) {
					npc.velocity = new Vector2(0f, 10f);
					if (npc.timeLeft > 10) {
						npc.timeLeft = 10;
					}
					return;
				}
			}
			moveCool -= 1f;
			if (Main.netMode != NetmodeID.MultiplayerClient && moveCool <= 0f) {
				npc.TargetClosest(false);
				player = Main.player[npc.target];
				double angle = Main.rand.NextDouble() * 2.0 * Math.PI;
				int distance = sphereRadius + Main.rand.Next(200);
				Vector2 moveTo = player.Center + (float)distance * new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
				moveCool = (float)moveTime + (float)Main.rand.Next(100);
				npc.velocity = (moveTo - npc.Center) / moveCool;
				rotationSpeed = (float)(Main.rand.NextDouble() + Main.rand.NextDouble());
				if (rotationSpeed > 1f) {
					rotationSpeed = 1f + (rotationSpeed - 1f) / 2f;
				}
				if (Main.rand.NextBool()) {
					rotationSpeed *= -1;
				}
				rotationSpeed *= 0.01f;
				npc.netUpdate = true;
			}
			if (Vector2.Distance(Main.player[npc.target].position, npc.position) > sphereRadius) {
				moveTimer--;
			}
			else {
				moveTimer += 3;
				if (moveTime >= 300 && moveTimer > 60) {
					moveTimer = 60;
				}
			}
			if (moveTimer <= 0) {
				moveTimer += 60;
				moveTime -= 3;
				if (moveTime < 99) {
					moveTime = 99;
					moveTimer = 0;
				}
				npc.netUpdate = true;
			}
			else if (moveTimer > 60) {
				moveTimer -= 60;
				moveTime += 3;
				npc.netUpdate = true;
			}
			captiveRotation += rotationSpeed;
			if (captiveRotation < 0f) {
				captiveRotation += 2f * (float)Math.PI;
			}
			if (captiveRotation >= 2f * (float)Math.PI) {
				captiveRotation -= 2f * (float)Math.PI;
			}
			attackCool -= 1f;
			if (Main.netMode != NetmodeID.MultiplayerClient && attackCool <= 0f) {
				attackCool = 200f + 200f * (float)npc.life / (float)npc.lifeMax + (float)Main.rand.Next(200);
				Vector2 delta = player.Center - npc.Center;
				float magnitude = (float)Math.Sqrt(delta.X * delta.X + delta.Y * delta.Y);
				if (magnitude > 0) {
					delta *= 5f / magnitude;
				}
				else {
					delta = new Vector2(0f, 5f);
				}
				int damage = (npc.damage - 30) / 2;
				if (Main.expertMode) {
					damage = (int)(damage / Main.expertDamage);
				}
				Projectile.NewProjectile(npc.Center.X, npc.Center.Y, delta.X, delta.Y, ProjectileType<ElementBall>(), damage, 3f, Main.myPlayer, BuffID.OnFire, 600f);
				npc.netUpdate = true;
			}
			if (Main.expertMode) {
				ExpertLaser();
			}
			if (Main.rand.NextBool()) {
				float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius * sphereRadius));
				double angle = Main.rand.NextDouble() * 2.0 * Math.PI;
				Dust.NewDust(new Vector2(npc.Center.X + radius * (float)Math.Cos(angle), npc.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, DustType<Sparkle>(), 0f, 0f, 0, default(Color), 1.5f);
			}
		}

		private void ExpertLaser() {
			laserTimer--;
			if (laserTimer <= 0 && Main.netMode != NetmodeID.MultiplayerClient) {
				if (npc.localAI[0] == 2f) {
					int laser1Index;
					int laser2Index;
					if (laser1 < 0) {
						laser1Index = npc.whoAmI;
					}
					else {
						for (laser1Index = 0; laser1Index < 200; laser1Index++) {
							if (Main.npc[laser1Index].type == NPCType<CaptiveElement>() && laser1 == Main.npc[laser1Index].ai[1]) {
								break;
							}
						}
					}
					if (laser2 < 0) {
						laser2Index = npc.whoAmI;
					}
					else {
						for (laser2Index = 0; laser2Index < 200; laser2Index++) {
							if (Main.npc[laser2Index].type == NPCType<CaptiveElement>() && laser2 == Main.npc[laser2Index].ai[1]) {
								break;
							}
						}
					}
					Vector2 pos = Main.npc[laser1Index].Center;
					int damage = Main.npc[laser1Index].damage / 2;
					if (Main.expertMode) {
						damage = (int)(damage / Main.expertDamage);
					}
					Projectile.NewProjectile(pos.X, pos.Y, 0f, 0f, ProjectileType<ElementLaser>(), damage, 0f, Main.myPlayer, laser1Index, laser2Index);
				}
				else {
					npc.localAI[0] = 2f;
				}
				laserTimer = 500 + Main.rand.Next(100);
				laserTimer = 60 + laserTimer * npc.life / npc.lifeMax;
				laser1 = Main.rand.Next(6) - 1;
				laser2 = Main.rand.Next(5) - 1;
				if (laser2 >= laser1) {
					laser2++;
				}
			}
		}

		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write((short)moveTime);
			writer.Write((short)moveTimer);
			if (Main.expertMode) {
				writer.Write((short)laserTimer);
				writer.Write((byte)(laser1 + 1));
				writer.Write((byte)(laser2 + 1));
			}
		}

		public override void ReceiveExtraAI(BinaryReader reader) {
			moveTime = reader.ReadInt16();
			moveTimer = reader.ReadInt16();
			if (Main.expertMode) {
				laserTimer = reader.ReadInt16();
				laser1 = reader.ReadByte() - 1;
				laser2 = reader.ReadByte() - 1;
			}
		}

		public override void FindFrame(int frameHeight) {
			if (attackCool < 50f) {
				npc.frame.Y = frameHeight;
			}
			else {
				npc.frame.Y = 0;
			}
		}

		public override void HitEffect(int hitDirection, double damage) {
			for (int k = 0; k < damage / npc.lifeMax * 100.0; k++) {
				Dust.NewDust(npc.position, npc.width, npc.height, 5, hitDirection, -1f, 0, default(Color), 1f);
			}
			if (Main.netMode != NetmodeID.MultiplayerClient && npc.life <= 0) {
				Vector2 spawnAt = npc.Center + new Vector2(0f, (float)npc.height / 2f);
				NPC.NewNPC((int)spawnAt.X, (int)spawnAt.Y, NPCType<AbominationRun>());
			}
		}

		// We use this hook to prevent any loot from dropping. We do this because this is a multistage npc and it shouldn't drop anything until the final form is dead.
		public override bool PreNPCLoot() {
			return false;
		}

		// We use this method to inflict a debuff on a player on contact. OnFire is inflicted 100% of the time in expert, and 50% of the time on non-expert mode.
		public override void OnHitPlayer(Player player, int damage, bool crit) {
			if (Main.expertMode || Main.rand.NextBool()) {
				player.AddBuff(BuffID.OnFire, 600, true);
			}
		}

		public override void ModifyHitByItem(Player player, Item item, ref int damage, ref float knockback, ref bool crit) {
			dontDamage = (player.Center - npc.Center).Length() > sphereRadius;
		}

		public override void ModifyHitByProjectile(Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
			Player player = Main.player[projectile.owner];
			dontDamage = player.active && (player.Center - npc.Center).Length() > sphereRadius;
		}

		public override bool StrikeNPC(ref double damage, int defense, ref float knockback, int hitDirection, ref bool crit) {
			if (dontDamage) {
				damage = 0;
				crit = true;
				dontDamage = false;
				SoundEngine.PlaySound(npc.HitSound, npc.position);
				return false;
			}
			return true;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor) {
			spriteBatch.Draw(mod.GetTexture("NPCs/Abomination/HolySphere"), npc.Center - Main.screenPosition, null, Color.White * (70f / 255f), 0f, new Vector2(sphereRadius, sphereRadius), 1f, SpriteEffects.None, 0f);
			spriteBatch.Draw(mod.GetTexture("NPCs/Abomination/HolySphereBorder"), npc.Center - Main.screenPosition, null, Color.White * 0.5f, 0f, new Vector2(sphereRadius, sphereRadius), 1f, SpriteEffects.None, 0f);
			if (Main.expertMode && laserTimer <= 60 && (laser1 == -1 || laser2 == -1)) {
				float rotation = laserTimer / 30f;
				if (laser1 == -1) {
					rotation *= -1f;
				}
				spriteBatch.Draw(mod.GetTexture("NPCs/Abomination/Rune"), npc.Center - Main.screenPosition, null, new Color(255, 10, 0), rotation, new Vector2(64, 64), 1f, SpriteEffects.None, 0f);
			}
			return true;
		}

		public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position) {
			scale = 1.5f;
			return null;
		}
	}
}