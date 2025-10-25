using ExampleMod.Content.Currencies;
using ExampleMod.Content.Items;
using ExampleMod.Content.Items.Ammo;
using ExampleMod.Content.Items.Consumables;
using ExampleMod.Content.Items.Mounts;
using ExampleMod.Content.NPCs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.GlobalNPCs
{
	class ExampleNPCShop : GlobalNPC
	{
		public override void ModifyShop(NPCShop shop) {
			if (shop.NpcType == NPCID.Dryad) {
				// Adding an item to a vanilla NPC is easy:
				// This item sells for the normal price.
				shop.Add<ExampleMountItem>();

				// We can use shopCustomPrice and shopSpecialCurrency to support custom prices and currency. Usually a shop sells an item for item.value.
				// Editing item.value in SetupShop is an incorrect approach.

				// This shop entry sells for 2 Defenders Medals.
				shop.Add(new Item(ModContent.ItemType<ExampleMountItem>()) {
					shopCustomPrice = 2,
					shopSpecialCurrency = CustomCurrencyID.DefenderMedals // omit this line if shopCustomPrice should be in regular coins.
				});

				// This shop entry sells for 3 of a custom currency added in our mod.
				shop.Add(new Item(ModContent.ItemType<ExampleMountItem>()) {
					shopCustomPrice = 2,
					shopSpecialCurrency = ExampleCustomCurrencies.ExampleItemCurrency,
				});
			}
			else if (shop.NpcType == NPCID.Wizard) {
				// shopContents.Add(ModContent.ItemType<Infinity>(), ChestLoot.Condition.InExpertMode);
			}
			else if (shop.NpcType == NPCID.Stylist) {
				shop.Add<ExampleHairDye>();
			}
			else if (shop.NpcType == NPCID.BestiaryGirl) {
				shop.Add<ExampleTownPetLicense>(Condition.BestiaryFilledPercent(50));
			}
			else if (shop.NpcType == NPCID.Cyborg) {
				shop.Add<ExampleRocket>(Condition.NpcIsPresent(ModContent.NPCType<ExamplePerson>()));
			}

			// Example of adding new items with complex conditions in the Merchant shop.
			// Style 1 check for application
			if (shop.FullName != NPCShopDatabase.GetShopName(NPCID.Merchant, "Shop"))
				return;

			// Style 2 check for application
			if (shop.NpcType != NPCID.Merchant || shop.Name != "Shop")
				return;

			// Style 3 check for application (works just if NPC has one shop)
			if (shop.NpcType != NPCID.Merchant)
				return;

			// Adding ExampleTorch to Merchant, with condition being sold only during daytime. Have it appear just after Torch
			shop.InsertAfter(ItemID.Torch, ModContent.ItemType<Content.Items.Placeable.ExampleTorch>(), Condition.TimeDay);

			// Hiding Copper Pickaxe and Copper Axe. They will never appear in Merchant shop anymore
			// However, this approach may fail if item doesn't exists in shop.
			shop.GetEntry(ItemID.CopperAxe).Disable();

			// Safer approach for disabling item
			if (shop.TryGetEntry(ItemID.CopperPickaxe, out NPCShop.Entry entry)) {
				entry.Disable();
			}

			// Adding new Condition to Blue Flare. Now it will appear just if player carries a Flare Gun in their inventory AND is in Snow biome
			shop.GetEntry(ItemID.BlueFlare).AddCondition(Condition.InSnow);

			// Let's add an item that appears just during Windy day and when NPC is happy enough (can sell pylons)
			// If condition is fulfilled, add an item to the shop.
			shop.Add<ExampleItem>(Condition.HappyWindyDay, Condition.HappyEnough);

			// Custom condition, opposite of conditions for ExampleItem above.
			var redPotCondition = new Condition("Mods.ExampleMod.Conditions.NotSellingExampleItem", () => !Condition.HappyWindyDay.IsMet() || !Condition.HappyEnough.IsMet());
			// Otherwise, if condition is not fulfilled, then let's check if its For The Worthy world and then sell Red Potion.
			shop.Add(ItemID.RedPotion, redPotCondition, Condition.ForTheWorthyWorld);
		}
	}
}
