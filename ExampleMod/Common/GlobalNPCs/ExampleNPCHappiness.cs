using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.GlobalNPCs
{
	public class ExampleNPCHappiness : GlobalNPC 
	{
		public override void NPCHappiness(NPC npc, int primaryPlayerBiome, ref ShopHelper shopHelperInstance, ref bool[] nearbyNPCsByType) 
		{
			int ExamplePersonType = ModContent.NPCType<Content.NPCs.ExamplePerson>(); //Get ExamplePerson's type
			switch (npc.type) 
			{
				case NPCID.Guide: // If the NPC is the Guide
					if (nearbyNPCsByType[ExamplePersonType]) { //If ExamplePerson is nearby
						shopHelperInstance.LikeNPC(ExamplePersonType); //Make the Guide like ExamplePerson!
					}
					break;
			}
		}
	}
}
