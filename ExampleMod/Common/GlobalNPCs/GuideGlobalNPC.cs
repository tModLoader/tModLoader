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
			if (npc.velocity.Y == 0f) {
				Main.NewText($"NPC {npc.GetTypeNetName()}: I am a guide.");

				npc.velocity.Y = Math.Min(npc.velocity.Y, -10f);
			}
		}
	}
}
