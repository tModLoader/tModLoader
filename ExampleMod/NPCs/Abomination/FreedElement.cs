using ExampleMod.Dusts;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.NPCs.Abomination
{
	public class FreedElement : ModNPC
	{
		private int elementType {
			get => (int)npc.ai[0];
			set => npc.ai[0] = value;
		}

		private int chargeTimer {
			get => (int)npc.ai[1];
			set => npc.ai[1] = value;
		}

		private float chargeX {
			get => npc.ai[2];
			set => npc.ai[2] = value;
		}

		private float chargeY {
			get => npc.ai[3];
			set => npc.ai[3] = value;
		}

		public override string Texture => "ExampleMod/NPCs/Abomination/CaptiveElement2";

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Freed Element");
			Main.npcFrameCount[npc.type] = 5;
		}

		public override void SetDefaults() {
			npc.aiStyle = -1;
			npc.lifeMax = 15000;
			npc.damage = 100;
			npc.defense = 55;
			npc.knockBackResist = 0f;
			npc.dontTakeDamage = true;
			npc.alpha = 255;
			npc.width = 50;
			npc.height = 50;
			npc.value = Item.buyPrice(0, 20, 0, 0);
			npc.npcSlots = 5f;
			npc.boss = true;
			npc.lavaImmune = true;
			npc.noGravity = true;
			npc.noTileCollide = true;
			npc.HitSound = SoundID.NPCHit5;
			npc.DeathSound = SoundID.NPCDeath7;
			music = MusicID.Boss2;
		}

		public override void ScaleExpertStats(int numPlayers, float bossLifeScale) {
			npc.lifeMax = (int)(npc.lifeMax * 0.6f * bossLifeScale);
			npc.damage = (int)(npc.damage * 0.6f);
		}

		public override void AI() {
			if (npc.localAI[0] == 0f) {
				if (elementType == 0) {
					npc.coldDamage = true;
				}
				if (elementType == 2) {
					npc.damage += 20;
				}
				npc.localAI[0] = 1f;
			}
			if (NPC.AnyNPCs(NPCType<CaptiveElement2>())) {
				if (npc.timeLeft < 750) {
					npc.timeLeft = 750;
				}
			}
			else {
				npc.life = -1;
				npc.active = false;
				return;
			}
			chargeTimer--;
			if (chargeTimer <= 0) {
				npc.TargetClosest(false);
				Player player = Main.player[npc.target];
				Vector2 offset = player.Center - npc.Center;
				if (offset != Vector2.Zero) {
					offset.Normalize();
				}
				offset *= 12f;
				chargeX = offset.X;
				chargeY = offset.Y;
				chargeTimer = 150;
				npc.netUpdate = true;
			}
			else if (chargeTimer <= 30) {
				chargeX = 0;
				chargeY = 0;
			}
			npc.velocity = (99f * npc.velocity + new Vector2(chargeX, chargeY)) / 100f;
			CreateDust();
		}

		private void CreateDust() {
			Color? color = GetColor();
			if (color.HasValue) {
				for (int k = 0; k < 5; k++) {
					int dust = Dust.NewDust(npc.position, npc.width, npc.height, DustType<Pixel>(), 0f, 0f, 0, color.Value);
					double angle = Main.rand.NextDouble() * 2.0 * Math.PI;
					Main.dust[dust].velocity = 3f * new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
				}
			}
			else {
				for (int k = 0; k < 1; k++) {
					int dust = Dust.NewDust(npc.position, npc.width, npc.height, DustType<Bubble>(), 0f, 0f, 0);
					double angle = Main.rand.NextDouble() * 2.0 * Math.PI;
					Main.dust[dust].velocity = 2f * new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
				}
			}
		}

		public override bool PreNPCLoot() {
			return false;
		}

		public override bool CanHitPlayer(Player target, ref int cooldownSlot) {
			if (elementType == 2 && Main.expertMode) {
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
			switch (elementType) {
				case 0:
					return BuffID.Frostburn;
				case 1:
					return BuffID.Frostburn;
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
			switch (elementType) {
				case 0:
					time = 400;
					break;
				case 1:
					time = 400;//300;
					break;
				case 3:
					time = 400;
					break;
				case 4:
					time = 600;
					break;
				default:
					return -1;
			}
			return time;
		}

		public Color? GetColor() {
			switch (elementType) {
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
	}
}