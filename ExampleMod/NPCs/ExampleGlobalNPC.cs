using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.NPCs
{
	public class ExampleGlobalNPC : GlobalNPC
	{
		public override void ResetEffects(NPC npc)
		{
			ExampleNPCInfo info = (ExampleNPCInfo)npc.GetModInfo(mod, "ExampleNPCInfo");
			info.eFlames = false;
		}

		public override void UpdateLifeRegen(NPC npc, ref int damage)
		{
			ExampleNPCInfo info = (ExampleNPCInfo)npc.GetModInfo(mod, "ExampleNPCInfo");
			if (info.eFlames)
			{
				if (npc.lifeRegen > 0)
				{
					npc.lifeRegen = 0;
				}
				npc.lifeRegen -= 16;
				if (damage < 2)
				{
					damage = 2;
				}
			}
		}

		public override void NPCLoot(NPC npc)
		{
			if (npc.lifeMax > 5 && npc.value > 0f)
			{
				Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, mod.ItemType("ExampleItem"));
			}
			if (((npc.type == NPCID.Pumpking && Main.pumpkinMoon) || (npc.type == NPCID.IceQueen && Main.snowMoon)) && NPC.waveCount > 10)
			{
				int chance = NPC.waveCount - 10;
				if (Main.expertMode)
				{
					chance++;
				}
				if (Main.rand.Next(5) < chance)
				{
					int stack = 1;
					if (NPC.waveCount >= 15)
					{
						stack = Main.rand.Next(4, 7);
						if (Main.expertMode)
						{
							stack++;
						}
					}
					else if (Main.rand.Next(2) == 0)
					{
						stack++;
					}
					string type = npc.type == NPCID.Pumpking ? "ScytheBlade" : "Icicle";
					Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, mod.ItemType(type), stack);
				}
			}
			if (npc.type == NPCID.DukeFishron && !Main.expertMode)
			{
				Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, mod.ItemType("Bubble"), Main.rand.Next(5, 8));
			}
			if (npc.type == NPCID.Bunny && npc.AnyInteractions())
			{
				Vector2 pos = npc.Center;
				int i = (int)pos.X / 16;
				int j = (int)pos.Y / 16;
				Tile tile = Main.tile[i, j];
				if (tile.active() && tile.type == mod.TileType("ElementalPurge") && !NPC.AnyNPCs(mod.NPCType("PuritySpirit")))
				{
					i -= Main.tile[i, j].frameX / 18;
					j -= Main.tile[i, j].frameY / 18;
					i = (i * 16) + 16;
					j = (j * 16) + 24 + 60;
					bool flag = false;
					for (int k = 0; k < 255; k++)
					{
						Player player = Main.player[k];
						if (player.active && player.position.X > i - NPC.sWidth / 2 && player.position.X + player.width < i + NPC.sWidth / 2 && player.position.Y > j - NPC.sHeight / 2 && player.position.Y < j + NPC.sHeight / 2)
						{
							flag = true;
							break;
						}
					}
					if (flag)
					{
						NPC.NewNPC(i, j, mod.NPCType("PuritySpirit"));
					}
				}
			}
		}

		public override void DrawEffects(NPC npc, ref Color drawColor)
		{
			ExampleNPCInfo info = (ExampleNPCInfo)npc.GetModInfo(mod, "ExampleNPCInfo");
			if (info.eFlames)
			{
				if (Main.rand.Next(4) < 3)
				{
					int dust = Dust.NewDust(npc.position - new Vector2(2f, 2f), npc.width + 4, npc.height + 4, mod.DustType("EtherealFlame"), npc.velocity.X * 0.4f, npc.velocity.Y * 0.4f, 100, default(Color), 3.5f);
					Main.dust[dust].noGravity = true;
					Main.dust[dust].velocity *= 1.8f;
					Main.dust[dust].velocity.Y -= 0.5f;
					if (Main.rand.Next(4) == 0)
					{
						Main.dust[dust].noGravity = false;
						Main.dust[dust].scale *= 0.5f;
					}
				}
				Lighting.AddLight(npc.position, 0.1f, 0.2f, 0.7f);
			}
		}
	}
}