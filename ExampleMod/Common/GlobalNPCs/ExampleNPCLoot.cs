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
			if (!NPCID.Sets.CountsAsCritter[npc.type]) { //If npc is not a critter
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ExampleItem>(), 1)); //Make it drop ExampleItem.
				// ItemDropRule.Common is what you would use in most cases, it simply drops the item with a chance specified.
				// The dropsOutOfY int is used for the numerator of the fractional chance of dropping this item.
				// Likewise, the dropsXOutOfY int is used for the denominator.
				// For example, if you had a dropsOutOfY as 7 and a dropsXOutOfY as 2, then the chance the item would drop is 2/7 or about 28%.
			}

			// We will now use the Guide to explain many of the other types of drop rules.
			if (npc.type == NPCID.Guide) {
				// RemoveWhere will remove any drop rule that matches the provided expression.
				// To make your own expressions to remove vanilla drop rules, you'll usually have to study the original source code that adds those rules.
				npcLoot.RemoveWhere(
					// The following expression returns true if the following conditions are met:
					rule => rule is ItemDropWithConditionRule drop // If the rule is an ItemDropWithConditionRule instance
						&& drop.itemId == ItemID.GreenCap // And that instance drops a green cap
						&& drop.condition is Conditions.NamedNPC npcNameCondition // ..And if its condition is that an npc name must match some string
						&& npcNameCondition.neededName == "Andrew" // And the condition's string is "Andrew". 
				);
				
				npcLoot.Add(ItemDropRule.Common(ItemID.GreenCap, 1)); //In conjunction with the above removal, this makes it so a guide with any name will drop the Green Cap.
			}

			if (npc.type == NPCID.Crimera || npc.type == NPCID.Corruptor) {
				//Here we make use of our own special rule we created: drop during daytime
				ExampleDropCondition exampleDropCondition = new ExampleDropCondition();
				IItemDropRule conditionalRule = new LeadingConditionRule(exampleDropCondition);

				int itemType = ItemID.Vertebrae;
				if (npc.type == NPCID.Crimera) {
					itemType = ItemID.RottenChunk;
				}
				//33% chance to drop other corresponding item in addition
				IItemDropRule rule = ItemDropRule.Common(itemType, chanceDenominator: 3);

				//Apply our item drop rule to the conditional rule
				conditionalRule.OnSuccess(rule);
				//Add the rule
				npcLoot.Add(conditionalRule);
				//It will result in the drop being shown in the bestiary, but only drop if the condition is true.
			}
      
			//TODO: Add the rest of the vanilla drop rules!!
		}

		//ModifyGlobalLoot allows you to modify loot that every NPC should be able to drop, preferably with a condition.
		//Vanilla uses this for the biome keys, souls of night/light, as well as the holiday drops.
		//Any drop rules in ModifyGlobalLoot should only run once. Everything else should go in ModifyNPCLoot.
		public override void ModifyGlobalLoot(GlobalLoot globalLoot) {
			globalLoot.Add(ItemDropRule.ByCondition(new Conditions.IsMasterMode(), ModContent.ItemType<ExampleSoul>(), 5, 1, 1)); //If the world is in master mode, drop ExampleSouls 20% of the time from every npc.
			//TODO: Make it so it only drops from enemies in ExampleBiome when that gets made.
		}
	}
}
