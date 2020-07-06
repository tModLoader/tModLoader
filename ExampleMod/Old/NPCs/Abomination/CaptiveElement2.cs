using ExampleMod.Dusts;
using ExampleMod.Items;
using ExampleMod.Items.Abomination;
using ExampleMod.Items.Armor;
using ExampleMod.Items.Placeable;
using ExampleMod.Items.Weapons;
using ExampleMod.Projectiles;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.NPCs.Abomination
{
	//ported from my tAPI mod because I'm lazy
	[AutoloadBossHead]
	public class CaptiveElement2 : ModNPC
	{
		public const string CaptiveElement2Head = "ExampleMod/NPCs/Abomination/CaptiveElement2_Head_Boss_";

		public override bool Autoload(ref string name) {
			// Adds boss head textures for the Abomination boss
			for (int k = 1; k <= 4; k++) {
				mod.AddBossHeadTexture(CaptiveElement2Head + k);
			}
			return base.Autoload(ref name);
		}

		private static int hellLayer => Main.maxTilesY - 200;

		private int captiveType {
			get => (int)npc.ai[0];
			set => npc.ai[0] = value;
		}

		private float attackCool {
			get => npc.ai[1];
			set => npc.ai[1] = value;
		}

		private int run {
			get => (int)npc.ai[2];
			set => npc.ai[2] = value;
		}

		private int jungleAI {
			get => (int)npc.ai[3];
			set => npc.ai[3] = value;
		}

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Captive Element");
			Main.npcFrameCount[npc.type] = 5;
		}

		public override void SetDefaults() {
			npc.aiStyle = -1;
			npc.lifeMax = 15000;
			npc.damage = 100;
			npc.defense = 55;
			npc.knockBackResist = 0f;
			npc.width = 100;
			npc.height = 100;
			npc.value = Item.buyPrice(0, 20, 0, 0);
			npc.npcSlots = 10f;
			npc.boss = true;
			npc.lavaImmune = true;
			npc.noGravity = true;
			npc.noTileCollide = true;
			npc.HitSound = SoundID.NPCHit5;
			npc.DeathSound = SoundID.NPCDeath7;
			music = MusicID.Boss2;
			bossBag = ItemType<AbominationBag>();
		}

		public override void ScaleExpertStats(int numPlayers, float bossLifeScale) {
			npc.lifeMax = (int)(npc.lifeMax * 0.6f * bossLifeScale);
			npc.damage = (int)(npc.damage * 0.6f);
		}

		public override void AI() {
			Player player = Main.player[npc.target];
			if (npc.localAI[0] == 0f) {
				if (GetDebuff() >= 0f) {
					npc.buffImmune[GetDebuff()] = true;
				}
				if (captiveType == 3) {
					npc.buffImmune[20] = true;
					npc.ai[3] = 1f;
				}
				if (captiveType == 0) {
					npc.coldDamage = true;
				}
				if (captiveType == 1) {
					npc.alpha = 100;
				}
				if (Main.expertMode) {
					npc.damage = 60;
				}
				if (captiveType == 2) {
					npc.damage += 20;
				}
				npc.localAI[0] = 1f;
				SoundEngine.PlaySound(SoundID.NPCDeath7, npc.position);
			}
			//run away
			if ((!player.active || player.dead || player.position.Y + player.height < hellLayer * 16) && run < 2) {
				npc.TargetClosest(false);
				player = Main.player[npc.target];
				if (!player.active || player.dead || player.position.Y + player.height < hellLayer * 16) {
					run = 1;
				}
				else {
					run = 0;
				}
			}
			if (run > 0) {
				bool flag = true;
				if (run == 1) {
					for (int k = 0; k < 200; k++) {
						if (Main.npc[k].active && Main.npc[k].type == NPCType<CaptiveElement2>() && Main.npc[k].ai[2] == 0f) {
							flag = false;
							break;
						}
					}
				}
				if (flag) {
					run = 2;
					npc.velocity = new Vector2(0f, 10f);
					npc.rotation = 0.5f * (float)Math.PI;
					CreateDust();
					if (npc.timeLeft > 10) {
						npc.timeLeft = 10;
					}
					npc.netUpdate = true;
					return;
				}
			}
			if (run < 2 && npc.timeLeft < 750) {
				npc.timeLeft = 750;
			}
			//move
			int count = 0;
			for (int k = 0; k < 200; k++) {
				if (Main.npc[k].active && Main.npc[k].type == NPCType<CaptiveElement2>()) {
					count++;
				}
			}
			if (captiveType != 1 && captiveType != 4) {
				Vector2 moveTo = player.Center;
				if (captiveType == 0) {
					moveTo.Y -= 320f;
				}
				if (captiveType == 2) {
					moveTo.Y += 320f;
				}
				if (captiveType == 3) {
					if (jungleAI < 0) {
						moveTo.X -= 320f;
					}
					else {
						moveTo.X += 320f;
					}
				}
				float minX = moveTo.X - 50f;
				float maxX = moveTo.X + 50f;
				float minY = moveTo.Y;
				float maxY = moveTo.Y;
				if (captiveType == 0) {
					minY -= 50f;
				}
				if (captiveType == 2) {
					maxY += 50f;
				}
				if (captiveType == 3) {
					minY -= 240f;
					maxY += 240f;
				}
				if (npc.Center.X >= minX && npc.Center.X <= maxX && npc.Center.Y >= minY && npc.Center.Y <= maxY) {
					npc.velocity *= 0.98f;
				}
				else {
					Vector2 move = moveTo - npc.Center;
					float magnitude = (float)Math.Sqrt(move.X * move.X + move.Y * move.Y);
					float speed = 10f;
					if (captiveType == 3 && (jungleAI == -5 || jungleAI == 1)) {
						speed = 8f;
					}
					if (magnitude > speed) {
						move *= speed / magnitude;
					}
					float inertia = 10f;
					if (speed == 8f) {
						inertia = 20f;
					}
					npc.velocity = (inertia * npc.velocity + move) / (inertia + 1);
					magnitude = (float)Math.Sqrt(npc.velocity.X * npc.velocity.X + npc.velocity.Y + npc.velocity.Y);
					if (magnitude > speed) {
						npc.velocity *= speed / magnitude;
					}
				}
			}
			if (captiveType == 1) {
				Vector2 move = player.Center - npc.Center;
				float magnitude = (float)Math.Sqrt(move.X * move.X + move.Y * move.Y);
				if (magnitude > 3.5f) {
					move *= 3.5f / magnitude;
				}
				npc.velocity = move;
			}
			//look and shoot
			if (captiveType != 4) {
				LookToPlayer();
				attackCool -= 1f;
				if (attackCool <= 0f && Main.netMode != NetmodeID.MultiplayerClient) {
					if (captiveType == 3) {
						jungleAI++;
						if (jungleAI == 0) {
							jungleAI = 1;
						}
						if (jungleAI == 6) {
							jungleAI = -5;
						}
					}
					attackCool = 150f + 100f * (float)npc.life / (float)npc.lifeMax + (float)Main.rand.Next(50);
					attackCool *= (float)count / 5f;
					if (captiveType != 3 || jungleAI != -5 && jungleAI != 1) {
						int damage = npc.damage / 2;
						if (Main.expertMode) {
							damage = (int)(damage / Main.expertDamage);
						}
						float speed = 5f;
						if (captiveType != 1) {
							speed = Main.expertMode ? 9f : 7f;
						}
						Projectile.NewProjectile(npc.Center.X, npc.Center.Y, 5f * (float)Math.Cos(npc.rotation), speed * (float)Math.Sin(npc.rotation), ProjectileType<PixelBall>(), damage, 3f, Main.myPlayer, GetDebuff(), GetDebuffTime());
					}
					npc.TargetClosest(false);
					npc.netUpdate = true;
				}
			}
			else {
				attackCool -= 1f;
				if (attackCool <= 0f) {
					attackCool = 80f + 40f * (float)npc.life / (float)npc.lifeMax;
					attackCool *= (float)count / 5f;
					npc.TargetClosest(false);
					LookToPlayer();
					float speed = 12.5f - 2.5f * (float)npc.life / (float)npc.lifeMax;
					npc.velocity = speed * new Vector2((float)Math.Cos(npc.rotation), (float)Math.Sin(npc.rotation));
					npc.netUpdate = true;
				}
				else {
					LookInDirection(npc.velocity);
					npc.velocity *= 0.995f;
				}
			}
			CreateDust();
		}

		private void CreateDust() {
			Color? color = GetColor();
			if (color.HasValue) {
				Vector2 unit = new Vector2((float)Math.Cos(npc.rotation), (float)Math.Sin(npc.rotation));
				Vector2 center = npc.Center;
				for (int k = 0; k < 4; k++) {
					int dust = Dust.NewDust(npc.position, npc.width, npc.height, DustType<Pixel>(), 0f, 0f, 0, color.Value);
					Vector2 offset = Main.dust[dust].position - center;
					offset.X = (offset.X - (float)npc.width / 2f) / 2f;
					Main.dust[dust].position = center + new Vector2(unit.X * offset.X - unit.Y * offset.Y, unit.Y * offset.X + unit.X * offset.Y);
					Main.dust[dust].velocity += -3f * unit;
					Main.dust[dust].rotation = npc.rotation;
					Main.dust[dust].velocity += npc.velocity;
				}
			}
		}

		private void LookToPlayer() {
			Vector2 look = Main.player[npc.target].Center - npc.Center;
			LookInDirection(look);
		}

		private void LookInDirection(Vector2 look) {
			float angle = 0.5f * (float)Math.PI;
			if (look.X != 0f) {
				angle = (float)Math.Atan(look.Y / look.X);
			}
			else if (look.Y < 0f) {
				angle += (float)Math.PI;
			}
			if (look.X < 0f) {
				angle += (float)Math.PI;
			}
			npc.rotation = angle;
		}

		public override void FindFrame(int frameHeight) {
			npc.frame.Y = captiveType * frameHeight;
		}

		public override void HitEffect(int hitDirection, double damage) {
			if (npc.life <= 0) {
				if (Main.expertMode) {
					int next = NPC.NewNPC((int)npc.Center.X, (int)npc.position.Y + npc.height * 3 / 4, NPCType<FreedElement>());
					Main.npc[next].ai[0] = captiveType;
					Main.npc[next].netUpdate = true;
				}
				else {
					Color? color = GetColor();
					if (color.HasValue) {
						for (int k = 0; k < 75; k++) {
							Dust.NewDust(npc.position, npc.width, npc.height, DustType<PixelHurt>(), 0f, 0f, 0, color.Value);
						}
					}
				}
			}
		}

		public override bool PreNPCLoot() {
			for (int k = 0; k < 200; k++) {
				if (Main.npc[k].active && k != npc.whoAmI && Main.npc[k].type == npc.type) {
					return false;
				}
			}
			NPCLoader.blockLoot.Add(ItemType<ExampleItem>());
			return true;
		}

		public override void NPCLoot() {
			if (Main.netMode != NetmodeID.MultiplayerClient) {
				int centerX = (int)(npc.position.X + (float)(npc.width / 2)) / 16;
				int centerY = (int)(npc.position.Y + (float)(npc.height / 2)) / 16;
				int halfLength = npc.width / 2 / 16 + 1;
				for (int x = centerX - halfLength; x <= centerX + halfLength; x++) {
					for (int y = centerY - halfLength; y <= centerY + halfLength; y++) {
						if ((x == centerX - halfLength || x == centerX + halfLength || y == centerY - halfLength || y == centerY + halfLength) && !Main.tile[x, y].active()) {
							Main.tile[x, y].type = TileID.HellstoneBrick;
							Main.tile[x, y].active(true);
						}
						Main.tile[x, y].lava(false);
						Main.tile[x, y].liquid = 0;
						if (Main.netMode == NetmodeID.Server) {
							NetMessage.SendTileSquare(-1, x, y, 1);
						}
						else {
							WorldGen.SquareTileFrame(x, y, true);
						}
					}
				}
			}
			if (Main.rand.NextBool(10)) {
				Item.NewItem(npc.getRect(), ItemType<AbominationTrophy>());
			}
			if (Main.expertMode) {
				npc.DropBossBags();
			}
			else {
				if (Main.rand.NextBool(7)) {
					Item.NewItem(npc.getRect(), ItemType<AbominationMask>());
				}
				Item.NewItem(npc.getRect(), ItemType<Items.Abomination.MoltenDrill>());
				Item.NewItem(npc.getRect(), ItemType<ElementResidue>());
				Item.NewItem(npc.getRect(), ItemType<PurityTotem>());
			}
			if (!ExampleWorld.downedAbomination) {
				ExampleWorld.downedAbomination = true;
				if (Main.netMode == NetmodeID.Server) {
					NetMessage.SendData(MessageID.WorldData); // Immediately inform clients of new world state.
				}
			}
		}

		public override void BossLoot(ref string name, ref int potionType) {
			name = "The Abomination";
			potionType = ItemID.GreaterHealingPotion;
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
					time = 600;
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
				index = ModContent.GetModBossHeadSlot(CaptiveElement2Head + captiveType);
			}
		}

		public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position) {
			scale = 1.5f;
			return null;
		}
	}
}