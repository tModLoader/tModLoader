using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using ExampleMod.Content.Items;
using ExampleMod.Common.ItemDropRules.DropConditions;
using System.Linq;

namespace ExampleMod.Common.GlobalNPCs
{
	// This file shows numerous examples of what you can do with the extensive NPC Loot lootable system.
	// You can find more info on the wiki: https://github.com/tModLoader/tModLoader/wiki/Basic-NPC-Drops-and-Loot-1.4
	// Despite this file being GlobalNPC, everything here can be used with a ModNPC as well! See examples of this in the Content/NPCs folder.
	public class ExampleNPCLoot : GlobalNPC
	{
		// ModifyNPCLoot uses a unique system called the ItemDropDatabase, which has many different rules for many different drop use cases.
		// Here we go through all of them, and how they can be used.
		// There are tons of other examples in vanilla! In a decompiled vanilla build, GameContent/ItemDropRules/ItemDropDatabase adds item drops to every single vanilla NPC, which can be a good resource.

		public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot) {
			if (!NPCID.Sets.CountsAsCritter[npc.type]) { // If npc is not a critter
				// Make it drop ExampleItem.
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ExampleItem>(), 1));

				// ItemDropRule.Common is what you would use in most cases, it simply drops the item with a fractional chance specified.
				// The chanceDenominator int is used for the denominator part of the fractional chance of dropping this item.

				// Drop an ExampleResearchPresent in journey mode with 2/7ths base chance.
				IItemDropRule presentDropRule = new LeadingConditionRule(new ExampleJourneyModeDropCondition());

				// ItemDropRule.Common(...) does not let you specify the numerator, so you can use new CommonDrop(...) instead.
				// (1 by default if using ItemDropRule.Common)
				// For example, if you had a chanceDenominator as 7 and a chanceNumerator as 2, then the chance the item would drop is 2/7 or about 28%.
				presentDropRule.OnSuccess(new CommonDrop(ModContent.ItemType<ExampleResearchPresent>(), chanceDenominator: 7, chanceNumerator: 2));
				npcLoot.Add(presentDropRule);
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

				npcLoot.Add(ItemDropRule.Common(ItemID.GreenCap, 1)); // In conjunction with the above removal, this makes it so a guide with any name will drop the Green Cap.
			}

			// Editing an existing drop rule
			if (npc.type == NPCID.BloodNautilus) {
				// Dreadnautilus, known as BloodNautilus in the code, drops SanguineStaff. The drop rate is 100% in Expert mode and 50% in Normal mode. This example will change that rate.
				// The vanilla code responsible for this drop is: ItemDropRule.NormalvsExpert(4269, 2, 1)
				// The NormalvsExpert method creates a DropBasedOnExpertMode rule, and that rule is made up of 2 CommonDrop rules. We'll need to use this information in our casting to properly identify the recipe to edit.

				// There are 2 options. One option is remove the original rule and then add back a similar rule. The other option is to modify the existing rule.
				// It is preferred to modify the existing rule to preserve compatibility with other mods.

				// Adjust the existing rule: Change the Normal mode drop rate from 50% to 33.3%
				foreach (var rule in npcLoot.Get()) {
					// You must study the vanilla code to know what to objects to cast to.
					if (rule is DropBasedOnExpertMode drop && drop.ruleForNormalMode is CommonDrop normalDropRule && normalDropRule.itemId == ItemID.SanguineStaff)
						normalDropRule.chanceDenominator = 3;
				}

				// Remove the rule, then add another rule: Change the Normal mode drop rate from 50% to 16.6%
				/*
				npcLoot.RemoveWhere(
					rule => rule is DropBasedOnExpertMode drop && drop.ruleForNormalMode is CommonDrop normalDropRule && normalDropRule.itemId == ItemID.SanguineStaff
				);
				npcLoot.Add(ItemDropRule.NormalvsExpert(4269, 6, 1));
				*/
			}
			// Editing an existing drop rule, but for a boss
			// In addition to this code, we also do similar code in Common/GlobalItems/BossBagLoot.cs to edit the boss bag loot. Remember to do both if your edits should affect boss bags as well.
			if (npc.type == NPCID.QueenBee) {
				foreach (var rule in npcLoot.Get()) {
					if (rule is DropBasedOnExpertMode dropBasedOnExpertMode && dropBasedOnExpertMode.ruleForNormalMode is OneFromOptionsNotScaledWithLuckDropRule oneFromOptionsDrop && oneFromOptionsDrop.dropIds.Contains(ItemID.BeeGun)) {
						var original = oneFromOptionsDrop.dropIds.ToList();
						original.Add(ModContent.ItemType<Content.Items.Accessories.WaspNest>());
						oneFromOptionsDrop.dropIds = original.ToArray();
					}
				}
			}

			if (npc.type == NPCID.Crimera || npc.type == NPCID.Corruptor) {
				// Here we make use of our own special rule we created: drop during daytime
				ExampleDropCondition exampleDropCondition = new ExampleDropCondition();
				IItemDropRule conditionalRule = new LeadingConditionRule(exampleDropCondition);

				int itemType = ItemID.Vertebrae;
				if (npc.type == NPCID.Crimera) {
					itemType = ItemID.RottenChunk;
				}
				// 33% chance to drop other corresponding item in addition
				IItemDropRule rule = ItemDropRule.Common(itemType, chanceDenominator: 3);

				// Apply our item drop rule to the conditional rule
				conditionalRule.OnSuccess(rule);
				// Add the rule
				npcLoot.Add(conditionalRule);
				// It will result in the drop being shown in the bestiary, but only drop if the condition is true.
			}

			//TODO: Add the rest of the vanilla drop rules!!
		}

		// ModifyGlobalLoot allows you to modify loot that every NPC should be able to drop, preferably with a condition.
		// Vanilla uses this for the biome keys, souls of night/light, as well as the holiday drops.
		// Any drop rules in ModifyGlobalLoot should only run once. Everything else should go in ModifyNPCLoot.
		public override void ModifyGlobalLoot(GlobalLoot globalLoot) {
			// If the ExampleSoulCondition is true, drop ExampleSoul 20% of the time. See Common/ItemDropRules/DropConditions/ExampleSoulCondition.cs for how it's determined
			globalLoot.Add(ItemDropRule.ByCondition(new ExampleSoulCondition(), ModContent.ItemType<ExampleSoul>(), 5, 1, 1));
		}
	}
}
