using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.GlobalNPCs
{
	//TODO: documentation
	public class ExampleNPCHappiness : GlobalNPC 
	{
		public override void NPCHappiness(NPC npc, ref ShopHelper shopHelperInstance, ref int primaryPlayerBiome, ref bool[] nearbyNPCsByType) 
		{
			int ExamplePersonType = ModContent.NPCType<Content.NPCs.ExamplePerson>();
			switch (npc.type) 
			{
				case NPCID.Guide:
					if (nearbyNPCsByType[ExamplePersonType]) {
						shopHelperInstance.LikeNPC(ExamplePersonType);
					}
					break;
			}
		}
	}
}
