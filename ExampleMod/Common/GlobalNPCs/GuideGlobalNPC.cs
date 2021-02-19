using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.GlobalNPCs
{
	public class GuideGlobalNPC : GlobalNPC
	{
		public override bool InstanceForEntity(NPC npc) {
			return npc.type == NPCID.Guide;
		}

		public override void AI(NPC npc) {
			//Make the guide giant and green.
			npc.scale = 1.5f;
			npc.color = Color.ForestGreen;
		}
	}
}
