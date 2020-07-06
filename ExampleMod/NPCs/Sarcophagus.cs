using ExampleMod.Dusts;
using ExampleMod.Items.Banners;
using ExampleMod.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.NPCs
{
	//ported from my tAPI mod because I'm lazy
	public class Sarcophagus : Hover
	{
		public Sarcophagus() {
			speedY = 1f;
			accelerationY = 0.1f;
		}

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Sarcophagus");
		}

		public override void SetDefaults() {
			npc.lifeMax = 1100;
			npc.damage = 140;
			npc.defense = 100;
			npc.knockBackResist = 0.3f;
			npc.width = 26;
			npc.height = 56;
			npc.aiStyle = -1;
			npc.noGravity = true;
			npc.HitSound = SoundID.NPCHit1;
			npc.DeathSound = SoundID.NPCDeath6;
			npc.value = Item.buyPrice(0, 0, 15, 0);
			npc.buffImmune[BuffID.Poisoned] = true;
			npc.buffImmune[BuffID.Venom] = true;
			banner = npc.type;
			bannerItem = ItemType<SarcophagusBanner>();
		}

		public override void CustomBehavior(ref float ai) {
			Player player = Main.player[npc.target];
			ai += 1f;
			if (Math.Abs(npc.Center.X - player.Center.X) < 16f * 30f && Math.Abs(npc.Center.Y - player.Center.Y) < 16f * 20f) {
				if (!player.buffImmune[BuffID.Cursed] && ai >= 120f) {
					ai = -60f;
					npc.netUpdate = true;
				}
				else if (ai >= 180f) {
					ai = -120f;
					if (Main.netMode != NetmodeID.MultiplayerClient) {
						int proj = Projectile.NewProjectile(npc.Center.X, npc.Center.Y, 0f, 0f, ProjectileType<ShadowArm>(), npc.damage / 2, 0f, Main.myPlayer, player.Center.X, player.Center.Y);
					}
					npc.netUpdate = true;
				}
			}
			else if (ai > 300f) {
				ai = 300f;
			}
			if (ai < 0f) {
				if (Math.Abs(npc.velocity.X) >= 0.01f) {
					npc.velocity *= 0.95f;
				}
				else {
					npc.velocity.X = 0.01f * npc.direction;
				}
				if (ai == -60f || ai == -120f) {
					SoundEngine.PlaySound(SoundID.NPCDeath6, npc.position);
				}
				if (ai == -1f) {
					for (int k = 0; k < 255; k++) {
						Player target = Main.player[k];
						if (Math.Abs(npc.Center.X - target.Center.X) < 16f * 30f && Math.Abs(npc.Center.Y - target.Center.Y) < 16f * 20f) {
							target.AddBuff(BuffID.Cursed, 240, true);
							target.AddBuff(BuffID.Slow, 240, true);
							target.AddBuff(BuffID.Darkness, 240, true);
							if (target.FindBuffIndex(BuffID.Cursed) >= 0 || target.FindBuffIndex(BuffID.Slow) >= 0 || target.FindBuffIndex(BuffID.Darkness) >= 0) {
								target.GetModPlayer<ExamplePlayer>().lockTime = 60;
							}
						}
					}
				}
				if (ai == -61f) {
					ai = -1f;
				}
			}
			for (int k = 0; k < 2; k++) {
				int dust = Dust.NewDust(npc.position - new Vector2(8f, 8f), npc.width + 16, npc.height + 16, DustType<Smoke>(), 0f, 0f, 0, Color.Black);
				Main.dust[dust].velocity += npc.velocity * 0.25f;
			}
		}

		public override bool ShouldMove(float ai) {
			return ai >= 0;
		}

		public override void FindFrame(int frameHeight) {
			npc.frameCounter += 1;
			if (npc.frameCounter >= 30) {
				npc.rotation = Main.rand.Next(-2, 3) * (float)Math.PI / 32f;
				npc.frameCounter = 0;
			}
			npc.spriteDirection = npc.direction;
		}

		public override void NPCLoot() {
			if (Main.rand.NextBool(50)) {
				Item.NewItem(npc.getRect(), ItemID.Nazar);
			}
		}

		public override void OnHitPlayer(Player player, int damage, bool crit) {
			if (Main.rand.NextBool(3)) {
				player.AddBuff(BuffID.Cursed, 240, true);
			}
		}

		public override void PostDraw(SpriteBatch spriteBatch, Color drawColor) {
			if (npc.ai[3] < 0f && npc.ai[3] >= -60f) {
				float angle = npc.ai[3] / 30f * (float)Math.PI;
				spriteBatch.Draw(mod.GetTexture("NPCs/Seal"), npc.Center - Main.screenPosition + new Vector2(0f, 10f), null, Lighting.GetColor((int)(npc.Center.X / 16f), (int)(npc.Center.Y / 16f)) * 0.9f, angle, new Vector2(16f, 16f), 1f, SpriteEffects.None, 0f);
			}
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (spawnInfo.playerSafe || !ExampleWorld.downedAbomination) {
				return 0f;
			}
			if (SpawnCondition.DesertCave.Chance > 0f) {
				return SpawnCondition.DesertCave.Chance / 3f;
			}
			return SpawnCondition.Mummy.Chance + SpawnCondition.LightMummy.Chance + SpawnCondition.DarkMummy.Chance;
		}
	}
}