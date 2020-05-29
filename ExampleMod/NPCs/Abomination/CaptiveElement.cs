using ExampleMod.Dusts;
using ExampleMod.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.NPCs.Abomination
{
	//ported from my tAPI mod because I'm lazy
	[AutoloadBossHead]
	public class CaptiveElement : ModNPC
	{
		public const string CaptiveElementHead = "ExampleMod/NPCs/Abomination/CaptiveElement_Head_Boss_";

		public override bool Autoload(ref string name) {
			// Adds boss head textures for the Abomination boss
			for (int k = 1; k <= 4; k++) {
				mod.AddBossHeadTexture(CaptiveElementHead + k);
			}
			return base.Autoload(ref name);
		}

		private int center {
			get => (int)npc.ai[0];
			set => npc.ai[0] = value;
		}

		private int captiveType {
			get => (int)npc.ai[1];
			set => npc.ai[1] = value;
		}

		private float attackCool {
			get => npc.ai[2];
			set => npc.ai[2] = value;
		}

		private int change {
			get => (int)npc.ai[3];
			set => npc.ai[3] = value;
		}

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Captive Element");
			Main.npcFrameCount[npc.type] = 10;
		}

		public override void SetDefaults() {
			npc.aiStyle = -1;
			npc.lifeMax = 15000;
			npc.damage = 100;
			npc.defense = 55;
			npc.knockBackResist = 0f;
			npc.dontTakeDamage = true;
			npc.width = 100;
			npc.height = 100;
			npc.value = Item.buyPrice(0, 20, 0, 0);
			npc.npcSlots = 10f;
			npc.boss = true;
			npc.lavaImmune = true;
			npc.noGravity = true;
			npc.noTileCollide = true;
			npc.HitSound = SoundID.NPCHit1;
			npc.DeathSound = SoundID.NPCDeath1;
			music = MusicID.Boss2;
		}

		public override void ScaleExpertStats(int numPlayers, float bossLifeScale) {
			npc.lifeMax = (int)(npc.lifeMax * 0.6f * bossLifeScale);
			npc.damage = (int)(npc.damage * 0.6f);
		}

		public override void AI() {
			NPC abomination = Main.npc[center];
			if (!abomination.active || abomination.type != NPCType<Abomination>()) {
				if (change > 0 || NPC.AnyNPCs(NPCType<AbominationRun>())) {
					if (change == 0) {
						npc.netUpdate = true;
					}
					change++;
				}
				else {
					npc.life = -1;
					npc.active = false;
					return;
				}
			}
			if (change > 0) {
				Color? color = GetColor();
				if (color.HasValue) {
					for (int x = 0; x < 5; x++) {
						int dust = Dust.NewDust(npc.position, npc.width, npc.height, DustType<Pixel>(), 0f, 0f, 0, color.Value);
						double angle = Main.rand.NextDouble() * 2.0 * Math.PI;
						Main.dust[dust].velocity = 3f * new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
					}
				}
				if (Main.netMode != NetmodeID.MultiplayerClient && change >= 100f) {
					int next = NPC.NewNPC((int)npc.Center.X, (int)npc.position.Y + npc.height, NPCType<CaptiveElement2>());
					Main.npc[next].ai[0] = captiveType;
					if (captiveType != 4) {
						Main.npc[next].ai[1] = 300f + (float)Main.rand.Next(100);
					}
					npc.life = -1;
					npc.active = false;
				}
				return;
			}
			else if (npc.timeLeft < 750) {
				npc.timeLeft = 750;
			}
			if (npc.localAI[0] == 0f) {
				if (GetDebuff() >= 0f) {
					npc.buffImmune[GetDebuff()] = true;
				}
				if (captiveType == 3f) {
					npc.buffImmune[20] = true;
				}
				if (captiveType == 0f) {
					npc.coldDamage = true;
				}
				npc.localAI[0] = 1f;
			}
			SetPosition(npc);
			attackCool -= 1f;
			if (Main.netMode != NetmodeID.MultiplayerClient && attackCool <= 0f) {
				attackCool = 200f + 200f * (float)abomination.life / (float)abomination.lifeMax + (float)Main.rand.Next(200);
				Vector2 delta = Main.player[abomination.target].Center - npc.Center;
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
				Projectile.NewProjectile(npc.Center.X, npc.Center.Y, delta.X, delta.Y, ProjectileType<ElementBall>(), damage, 3f, Main.myPlayer, GetDebuff(), GetDebuffTime());
				npc.netUpdate = true;
			}
		}

		public static void SetPosition(NPC npc) {
			CaptiveElement modNPC = npc.modNPC as CaptiveElement;
			if (modNPC != null) {
				Vector2 center = Main.npc[modNPC.center].Center;
				double angle = Main.npc[modNPC.center].ai[3] + 2.0 * Math.PI * modNPC.captiveType / 5.0;
				npc.position = center + 300f * new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) - npc.Size / 2f;
			}
		}

		public override void FindFrame(int frameHeight) {
			npc.frame.Y = captiveType * frameHeight;
			if (captiveType == 1) {
				npc.alpha = 100;
			}
			if (attackCool < 50f) {
				npc.frame.Y += 5 * frameHeight;
			}
		}

		public override bool CanHitPlayer(Player target, ref int cooldownSlot) {
			if (captiveType == 2 && Main.expertMode) {
				cooldownSlot = 1;
			}
			return true;
		}

		public override void OnHitPlayer(Player player, int dmgDealt, bool crit) {
			if (Main.expertMode || Main.rand.NextBool()) {
				int debuff = GetDebuff();
				if (debuff >= 0) {
					player.AddBuff(debuff, GetDebuffTime(), true);
				}
			}
		}

		public int GetDebuff() {
			switch (captiveType) {
				case 0:
					return BuffID.Frostburn;
				case 1:
					return BuffType<Buffs.EtherealFlames>();
				case 3:
					return BuffID.Venom;
				case 4:
					return BuffID.Ichor;
				default:
					return -1;
			}
		}

		public int GetDebuffTime() {
			int time;
			switch (captiveType) {
				case 0:
					time = 400;
					break;
				case 1:
					time = 300;
					break;
				case 3:
					time = 400;
					break;
				case 4:
					time = 900;
					break;
				default:
					return -1;
			}
			return time;
		}

		public Color? GetColor() {
			switch (captiveType) {
				case 0:
					return new Color(0, 230, 230);
				case 1:
					return new Color(0, 153, 230);
				case 3:
					return new Color(0, 178, 0);
				case 4:
					return new Color(230, 192, 0);
				default:
					return null;
			}
		}

		public override void BossHeadSlot(ref int index) {
			if (captiveType > 0) {
				index = ModContent.GetModBossHeadSlot(CaptiveElementHead + captiveType);
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor) {
			Abomination abomination = Main.npc[center].modNPC as Abomination;
			if (Main.expertMode && abomination != null && abomination.npc.active && abomination.laserTimer <= 60 && (abomination.laser1 == captiveType || abomination.laser2 == captiveType)) {
				Color? color = GetColor();
				if (!color.HasValue) {
					color = Color.White;
				}
				float rotation = abomination.laserTimer / 30f;
				if (abomination.laser1 == captiveType) {
					rotation *= -1f;
				}
				spriteBatch.Draw(ModContent.GetTexture("ExampleMod/NPCs/Abomination/Rune"), npc.Center - Main.screenPosition, null, color.Value, rotation, new Vector2(64, 64), 1f, SpriteEffects.None, 0f);
			}
			return true;
		}
	}
}