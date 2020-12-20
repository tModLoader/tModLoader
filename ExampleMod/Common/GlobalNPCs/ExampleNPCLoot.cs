using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using ExampleMod.Content.Items;

namespace ExampleMod.Common.GlobalNPCs
{
	// This file shows numerous examples of what you can do with the extensive NPC Loot lootable system. 
	// Despite this file being GlobalNPC, everything here can be used with a ModNPC as well! See examples of this in the Content/NPCs folder.
	public class ExampleNPCLoot : GlobalNPC
	{
		//ModifyNPCLoot uses a unique system called the ItemDropDatabase, which has many different rules for many different drop use cases.
		//Here we go through all of them, and how they can be used.
		//There are tons of other examples in vanilla! In a decompiled vanilla build, GameContent/ItemDropRules/ItemDropDatabase adds item drops to every single vanilla NPC, which can be a good resource.
		public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot) {
			if (npc.lifeMax > 5 && npc.value > 0f) { //If npc has health higher than 5 and drops money (aka is not a critter)
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ExampleItem>(), 1)); //Make it drop ExampleItem.
				// ItemDropRule.Common is what you would use in most cases, it simply drops the item with a chance specified.
				// The dropsOutOfY int is used for the numerator of the fractional chance of dropping this item.
				// Likewise, the dropsXOutOfY int is used for the denominator.
				// For example, if you had a dropsOutOfY as 7 and a dropsXOutOfY as 2, then the chance the item would drop is 2/7 or about 28%.
			}

			if (npc.type == NPCID.Guide) { //We will now use the Guide to explain many of the other types of drop rules.
				npcLoot.Remove(new ItemDropWithConditionRule(ItemID.GreenCap, 1, 1, 1, new Conditions.NamedNPC("Andrew"))); //RemoveFromNPC will uniquely remove any drop with the specific drop rule from any NPC specified.
				npcLoot.Add(ItemDropRule.Common(ItemID.GreenCap, 1)); //In conjunction with the above removal, this makes it so a guide with any name will drop the Green Cap.
			}

			//add more stuff here

		}
	}
}
