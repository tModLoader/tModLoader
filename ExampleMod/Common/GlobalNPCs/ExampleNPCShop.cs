using ExampleMod.Content.Items;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.GlobalNPCs
{
	class ExampleNPCShop : GlobalNPC
	{
		public override void SetupShop(string shopId, ChestLoot shopContents) {
			// This example does not use the AppliesToEntity hook, as such, we can handle multiple npcs here by using if statements.
			if (shopId == TMLLootDatabase.CalculateShopName(NPCID.Dryad, "Shop")) {
				// Adding an item to a vanilla NPC is easy:
				// This item sells for the normal price.
				shopContents.Add(ModContent.ItemType<ExampleMountItem>());

				// We can use shopCustomPrice and shopSpecialCurrency to support custom prices and currency. Usually a shop sells an item for item.value.
				// Editing item.value in SetupShop is an incorrect approach.

				// This shop entry sells for 2 Defenders Medals.
				shopContents.Add(ModContent.ItemType<ExampleMountItem>());
				shopContents[^1].Item.shopCustomPrice = 2;
				shopContents[^1].Item.shopSpecialCurrency = CustomCurrencyID.DefenderMedals; // omit this line if shopCustomPrice should be in regular coins.

				// This shop entry sells for 3 of a custom currency added in our mod.
				shopContents.Add(ModContent.ItemType<ExampleMountItem>());
				shopContents[^1].Item.shopCustomPrice = 2;
				shopContents[^1].Item.shopSpecialCurrency = ExampleMod.ExampleCustomCurrencyId;
			}
			else if (shopId == TMLLootDatabase.CalculateShopName(NPCID.Wizard, "Shop")) {
				// shopContents.Add(ModContent.ItemType<Infinity>(), ChestLoot.Condition.InExpertMode);
			}
			else if (shopId == TMLLootDatabase.CalculateShopName(NPCID.Wizard, "Shop")) {
				shopContents.Add(ModContent.ItemType<ExampleHairDye>());
			}
		}
	}
}
