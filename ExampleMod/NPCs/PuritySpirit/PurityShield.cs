using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.NPCs.PuritySpirit
{
	public class PurityShield : ModNPC
	{
		public override void SetDefaults()
		{
			npc.name = "PurityShield";
			npc.displayName = "Shield of Purity";
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
			npc.soundHit = 1;
			npc.soundKilled = 6;
			for (int k = 0; k < npc.buffImmune.Length; k++)
			{
				npc.buffImmune[k] = true;
			}
			Main.npcFrameCount[npc.type] = 4;
			NPCID.Sets.MustAlwaysDraw[npc.type] = true;
		}

		public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
		{
			npc.lifeMax = (int)(npc.lifeMax / Main.expertLife * bossLifeScale);
			npc.defense = 72;
		}

		public override void AI()
		{
			NPC owner = Main.npc[(int)npc.ai[0]];
			if (!owner.active || owner.type != mod.NPCType("PuritySpirit"))
			{
				npc.active = false;
				return;
			}
			PuritySpirit modOwner = (PuritySpirit)owner.modNPC;
			if (npc.localAI[0] == 0f)
			{
				if (modOwner.targets.Contains(Main.myPlayer))
				{
					Main.PlaySound(2, -1, -1, 2);
				}
				else
				{
					Main.PlaySound(2, (int)npc.position.X, (int)npc.position.Y, 2);
				}
				npc.localAI[0] = 1f;
			}
			Vector2 targetPos = new Vector2(npc.ai[1], npc.ai[2]);
			Vector2 direction = targetPos - npc.Center;
			if (direction != Vector2.Zero)
			{
				float speed = direction.Length();
				if (speed > 2f)
				{
					speed = 2f;
				}
				direction.Normalize();
				direction *= speed;
				npc.position += direction;
			}
		}

		public override void FindFrame(int frameHeight)
		{
			npc.frameCounter += 1.0;
			npc.frameCounter %= 60.0;
			npc.frame.Y = frameHeight * (((int)npc.frameCounter % 20) / 5);
		}

		public override void HitEffect(int hitDirection, double damage)
		{
			if (npc.life <= 0)
			{
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

		public override bool? CanBeHitByItem(Player player, Item item)
		{
			return CanBeHitByPlayer(player);
		}

		public override bool? CanBeHitByProjectile(Projectile projectile)
		{
			return CanBeHitByPlayer(Main.player[projectile.owner]);
		}

		private bool? CanBeHitByPlayer(Player player)
		{
			NPC owner = Main.npc[(int)npc.ai[0]];
			PuritySpirit modOwner = owner.modNPC == null ? null : owner.modNPC as PuritySpirit;
			if (modOwner != null && !modOwner.targets.Contains(player.whoAmI))
			{
				return false;
			}
			return null;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
		{
			Vector2 end1 = npc.Center;
			Vector2 end2 = Main.npc[(int)npc.ai[0]].Center;
			if (end1 == end2)
			{
				return true;
			}
			float length = Vector2.Distance(end1, end2);
			Texture2D texture = mod.GetTexture("NPCs/PuritySpirit/PurityShieldChain");
			Vector2 scale = new Vector2(length / texture.Width, 1f);
			Vector2 drawPos = (end1 + end2) / 2f - Main.screenPosition;
			Vector2 drawOrigin = new Vector2(texture.Width / 2, texture.Height / 2);
			float rotation = (float)Math.Atan2(end2.Y - end1.Y, end2.X - end1.X);
			spriteBatch.Draw(texture, drawPos, null, Color.White * 0.9f, rotation, drawOrigin, scale, SpriteEffects.None, 0f);
			texture = Main.npcTexture[npc.type];
			drawPos = npc.Center - Main.screenPosition;
			drawOrigin = new Vector2(texture.Width / 2, texture.Height / 2 / Main.npcFrameCount[npc.type]);
			spriteBatch.Draw(texture, drawPos, npc.frame, Color.White, 0f, drawOrigin, 1f, SpriteEffects.None, 0f);
			return false;
		}

		public override void PostDraw(SpriteBatch spriteBatch, Color drawColor)
		{
			Texture2D texture = mod.GetTexture("NPCs/PuritySpirit/PurityShieldGlow");
			Vector2 drawPos = npc.position - Main.screenPosition;
			Rectangle frame = new Rectangle(0, 0, texture.Width, texture.Height / 25);
			frame.Y = (int)npc.frameCounter;
			if (frame.Y > 24)
			{
				frame.Y = 24;
			}
			frame.Y *= npc.height;
			spriteBatch.Draw(texture, drawPos, frame, Color.White * 0.7f, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
		}
	}
}