using ExampleMod.Content.Items;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod
{
	public class ExampleInventoryPlayer : ModPlayer
	{
		// AddStartingItems is a method you can use to add items to the player's starting inventory.
		// It is also called when the player dies a mediumcore death
		// Simply create an enumerable (think a collection) of the items you want to add.
		// This method adds an ExampleItem and 256 gold ore to the player's inventory.
		public override IEnumerable<Item> AddStartingItems(bool mediumCoreDeath) {
			// TODO: someone make this use the new item constructor
			Item testItem = new Item();
			testItem.SetDefaults(ItemType<ExampleItem>());

			Item otherItem = new Item();
			otherItem.SetDefaults(ItemID.GoldOre);
			otherItem.stack = 256;

			if (mediumCoreDeath) {
				Item potionItem = new Item();
				potionItem.SetDefaults(ItemID.HealingPotion);

				return new[] { potionItem };
			}

			return new[] { testItem, otherItem };
		}

		// ModifyStartingItems is a more elaborate version of AddStartingItems, which lets you remove items
		// that either vanilla or other mods add. You can technically use it to add items as well, but it's recommended
		// to only do that in AddStartingItems.
		// In this example, we stop Terraria from adding an Iron Axe to the player's inventory if it's journey mode.
		// (If you want to stop another mod from adding an item, its entry is the mod's internal name, e.g additions["SomeMod"]
		// Terraria's entry is always named just "Terraria"
		public override void ModifyStartingInventory(IReadOnlyDictionary<string, List<Item>> additions, bool mediumCoreDeath) {
			additions["Terraria"].RemoveAll(item => item.type == ItemID.IronAxe);
		}
	}
}
