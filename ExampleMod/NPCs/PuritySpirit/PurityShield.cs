using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.NPCs.PuritySpirit
{
	public class PurityShield : ModNPC
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Shield of Purity");
			Main.npcFrameCount[npc.type] = 4;
			NPCID.Sets.MustAlwaysDraw[npc.type] = true;
		}

		public override void SetDefaults() {
			npc.aiStyle = -1;
			npc.lifeMax = PuritySpirit.dpsCap;
			npc.damage = 0;
			npc.defense = 70;
			npc.knockBackResist = 0f;
			npc.width = 80;
			npc.height = 80;
			npc.lavaImmune = true;
			npc.noGravity = true;
			npc.noTileCollide = true;
			npc.HitSound = SoundID.NPCHit1;
			npc.DeathSound = SoundID.NPCDeath6;
			for (int k = 0; k < npc.buffImmune.Length; k++) {
				npc.buffImmune[k] = true;
			}
		}

		public override void ScaleExpertStats(int numPlayers, float bossLifeScale) {
			npc.lifeMax = (int)(npc.lifeMax / Main.expertLife * bossLifeScale);
			npc.defense = 72;
		}

		public override void AI() {
			NPC owner = Main.npc[(int)npc.ai[0]];
			if (!owner.active || owner.type != NPCType<PuritySpirit>()) {
				npc.active = false;
				return;
			}
			PuritySpirit modOwner = (PuritySpirit)owner.modNPC;
			if (npc.localAI[0] == 0f) {
				if (modOwner.targets.Contains(Main.myPlayer)) {
					SoundEngine.PlaySound(SoundID.Item2);
				}
				else {
					SoundEngine.PlaySound(SoundID.Item2, npc.position);
				}
				npc.localAI[0] = 1f;
			}
			Vector2 targetPos = new Vector2(npc.ai[1], npc.ai[2]);
			Vector2 direction = targetPos - npc.Center;
			if (direction != Vector2.Zero) {
				float speed = direction.Length();
				if (speed > 2f) {
					speed = 2f;
				}
				direction.Normalize();
				direction *= speed;
				npc.position += direction;
			}
			else {
				npc.localAI[1] = 1f;
			}
		}

		public override void FindFrame(int frameHeight) {
			npc.frameCounter += 1.0;
			npc.frameCounter %= 120.0;
			npc.frame.Y = frameHeight * ((int)npc.frameCounter % 20 / 5);
		}

		public override void HitEffect(int hitDirection, double damage) {
			if (npc.life <= 0) {
				int gore = Gore.NewGore(npc.position + new Vector2(10f, 0f), Vector2.Zero, Main.rand.Next(435, 438), 2f);
				Main.gore[gore].velocity *= 0.3f;
				gore = Gore.NewGore(npc.position + new Vector2(50f, 10f), Vector2.Zero, Main.rand.Next(435, 438), 2f);
				Main.gore[gore].velocity *= 0.3f;
				gore = Gore.NewGore(npc.position + new Vector2(0f, 60f), Vector2.Zero, Main.rand.Next(435, 438), 2f);
				Main.gore[gore].velocity *= 0.3f;
				gore = Gore.NewGore(npc.position + new Vector2(40f, 50f), Vector2.Zero, Main.rand.Next(435, 438), 2f);
				Main.gore[gore].velocity *= 0.3f;
				gore = Gore.NewGore(npc.position + new Vector2(30f, 30f), Vector2.Zero, Main.rand.Next(435, 438), 2f);
				Main.gore[gore].velocity *= 0.3f;
			}
		}

		public override bool? CanBeHitByItem(Player player, Item item) {
			return CanBeHitByPlayer(player);
		}

		public override bool? CanBeHitByProjectile(Projectile projectile) {
			return CanBeHitByPlayer(Main.player[projectile.owner]);
		}

		private bool? CanBeHitByPlayer(Player player) {
			NPC owner = Main.npc[(int)npc.ai[0]];
			PuritySpirit modOwner = owner.modNPC == null ? null : owner.modNPC as PuritySpirit;
			if (modOwner != null && !modOwner.targets.Contains(player.whoAmI)) {
				return false;
			}
			return null;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor) {
			Vector2 end1 = npc.Center;
			Vector2 end2 = Main.npc[(int)npc.ai[0]].Center;
			Texture2D texture;
			if (end1 != end2) {
				float length = Vector2.Distance(end1, end2);
				Vector2 direction = end2 - end1;
				direction.Normalize();
				float start = (float)npc.frameCounter % 8f;
				start *= 2f;
				if (npc.localAI[1] == 0f) {
					start *= 2f;
					start %= 16f;
				}
				texture = mod.GetTexture("NPCs/PuritySpirit/PurityShieldChain");
				for (float k = start; k <= length; k += 16f) {
					spriteBatch.Draw(texture, end1 + k * direction - Main.screenPosition, null, Color.White, 0f, new Vector2(16f, 16f), 1f, SpriteEffects.None, 0f);
				}
			}
			texture = Main.npcTexture[npc.type];
			Vector2 drawPos = npc.Center - Main.screenPosition;
			Vector2 drawOrigin = new Vector2(texture.Width / 2, texture.Height / 2 / Main.npcFrameCount[npc.type]);
			spriteBatch.Draw(texture, drawPos, npc.frame, Color.White, 0f, drawOrigin, 1f, SpriteEffects.None, 0f);
			return false;
		}

		public override void PostDraw(SpriteBatch spriteBatch, Color drawColor) {
			Texture2D texture = mod.GetTexture("NPCs/PuritySpirit/PurityShieldGlow");
			Vector2 drawPos = npc.position - Main.screenPosition;
			Rectangle frame = new Rectangle(0, 0, texture.Width, texture.Height / 25) {
				Y = (int)npc.frameCounter % 60
			};
			if (frame.Y > 24) {
				frame.Y = 24;
			}
			frame.Y *= npc.height;
			spriteBatch.Draw(texture, drawPos, frame, Color.White * 0.7f, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
		}
	}
}