using ExampleMod.Content.Items;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.GlobalNPCs
{
	class ExampleNPCShop : GlobalNPC
	{
		public override void SetupShop(string shopId, ChestLoot shopContents) {
			if (shopId == TMLLootDatabase.GetNPCShopName(NPCID.Dryad)) {
				// Adding an item to a vanilla NPC is easy:
				// This item sells for the normal price.
				shopContents.Add(ModContent.ItemType<ExampleMountItem>());

				// We can use shopCustomPrice and shopSpecialCurrency to support custom prices and currency. Usually a shop sells an item for item.value.
				// Editing item.value in SetupShop is an incorrect approach.

				// This shop entry sells for 2 Defenders Medals.
				shopContents.Add(ModContent.ItemType<ExampleMountItem>());
				shopContents.LastEntry.Item.shopCustomPrice = 2;
				shopContents.LastEntry.Item.shopSpecialCurrency = CustomCurrencyID.DefenderMedals; // omit this line if shopCustomPrice should be in regular coins.

				// This shop entry sells for 3 of a custom currency added in our mod.
				shopContents.Add(ModContent.ItemType<ExampleMountItem>());
				shopContents.LastEntry.Item.shopCustomPrice = 2;
				shopContents.LastEntry.Item.shopSpecialCurrency = ExampleMod.ExampleCustomCurrencyId;
			}
			else if (shopId == TMLLootDatabase.GetNPCShopName(NPCID.Wizard)) {
				// shopContents.Add(ModContent.ItemType<Infinity>(), ChestLoot.Condition.InExpertMode);
			}
			else if (shopId == TMLLootDatabase.GetNPCShopName(NPCID.Stylist)) {
				shopContents.Add(ModContent.ItemType<ExampleHairDye>());
			}
		}
	}
}
