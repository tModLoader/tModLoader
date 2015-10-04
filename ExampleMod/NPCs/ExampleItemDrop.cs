using System;
using Terraria;
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
		}
	}
}