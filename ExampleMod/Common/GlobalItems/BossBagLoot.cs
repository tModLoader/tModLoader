using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.GlobalItems
{
	public class BossBagLoot : GlobalItem
	{
		public override void ModifyItemLoot(Item item, ItemLoot itemLoot) {
			// In addition to this code, we also do similar code in Common/GlobalNPCs/ExampleNPCLoot.cs to edit the boss loot for non-expert drops. Remember to do both if your edits should affect non-expert drops as well.
			if(item.type == ItemID.QueenBeeBossBag) {
				foreach (var rule in itemLoot.Get()) {
					if (rule is OneFromOptionsNotScaledWithLuckDropRule oneFromOptionsDrop && oneFromOptionsDrop.dropIds.Contains(ItemID.BeeGun)) {
						var original = oneFromOptionsDrop.dropIds.ToList();
						original.Add(ModContent.ItemType<Content.Items.Accessories.WaspNest>());
						oneFromOptionsDrop.dropIds = original.ToArray();
					}
				}
			}
		}
	}
}
