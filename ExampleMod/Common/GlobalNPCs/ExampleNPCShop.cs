using ExampleMod.Content.Items;
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
				shop.Add(ModContent.ItemType<ExampleMountItem>());

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
					shopSpecialCurrency = ExampleMod.ExampleCustomCurrencyId
				});
			}
			else if (shop.NpcType == NPCID.Wizard) {
				// shopContents.Add(ModContent.ItemType<Infinity>(), ChestLoot.Condition.InExpertMode);
			}
			else if (shop.NpcType == NPCID.Stylist) {
				shop.Add(ModContent.ItemType<ExampleHairDye>());
			}
		}
	}
}
