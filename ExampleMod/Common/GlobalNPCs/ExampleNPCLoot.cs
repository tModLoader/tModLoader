using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using ExampleMod.Content.Items;
using ExampleMod.Common.ItemDropRules.Conditions;

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
			// We will now use the Guide to explain many of the other types of drop rules.
			if (npc.type == NPCID.Guide) {
				// RemoveWhere will remove any drop rule that matches the provided expression.
				// To make your own expressions to remove vanilla drop rules, you'll usually have to study the original source code that adds those rules.
				npcLoot.RemoveWhere(
					// The following expression returns true if the following conditions are met:
					rule => rule is ItemDropWithConditionRule drop // If the rule is an ItemDropWithConditionRule instance
						&& drop._itemId == ItemID.GreenCap // And that instance drops a green cap
						&& drop._condition is Conditions.NamedNPC npcNameCondition // ..And if its condition is that an npc name must match some string
						&& npcNameCondition._neededName == "Andrew" // And the condition's string is "Andrew". 
				);
				
				npcLoot.Add(ItemDropRule.Common(ItemID.GreenCap, 1)); //In conjunction with the above removal, this makes it so a guide with any name will drop the Green Cap.
			}

			if (npc.type == NPCID.Crimera || npc.type == NPCID.Corruptor) {
				//Here we make use of our own special rule we created: drop during daytime
				ExampleDropCondition exampleDropCondition = new ExampleDropCondition();
				IItemDropRule conditionalRule = new LeadingConditionRule(exampleDropCondition);

				IItemDropRule exampleItemRule = ItemDropRule.Common(ModContent.ItemType<ExampleItem>(), 1);
				// ItemDropRule.Common is what you would use in most cases, it simply drops the item with a chance specified.
				// The dropsOutOfY int is used for the numerator of the fractional chance of dropping this item.
				// Likewise, the dropsXOutOfY int is used for the denominator.
				// For example, if you had a dropsOutOfY as 7 and a dropsXOutOfY as 2, then the chance the item would drop is 2/7 or about 28%.

				//Apply our ExampleItem drop rule to the conditional rule
				conditionalRule.OnSuccess(exampleItemRule);
				//Add the rule
				npcLoot.Add(conditionalRule);
				//It will result in the drop being shown in the bestiary, but only drop if the condition is true.
			}
		}
	}
}
