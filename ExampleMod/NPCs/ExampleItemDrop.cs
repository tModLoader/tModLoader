using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.NPCs
{
	public class ExampleItemDrop : GlobalNPC
	{
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
		}
	}
}