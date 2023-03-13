using Terraria.ID;
using static Terraria.ModLoader.NPCShop;

namespace Terraria.ModLoader.Default;

public sealed class BartenderShopNPC : GlobalNPC
{
	public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
	{
		return entity.type == NPCID.DD2Bartender;
	}

	public override void ModifyShop(NPCShop shop)
	{
		static Item MakeItem(int id, int currencyPrice) =>
			new(id) { shopCustomPrice = currencyPrice, shopSpecialCurrency = CustomCurrencyID.DefenderMedals };

		// 1st row
		shop.Add(ItemID.Ale);
		shop.Add(ItemID.DD2ElderCrystal);																					// Eternia Crystal
		shop.Add(ItemID.DD2ElderCrystalStand);																				// Eternia Crystal Stand
		shop.Add(MakeItem(ItemID.DefendersForge, 50));

		shop.Add(new Entry(MakeItem(ItemID.SquireGreatHelm, 15),Condition.DownedMechBossAny).ReverseSlot());
		shop.Add(new Entry(MakeItem(ItemID.SquirePlating, 15),	Condition.DownedMechBossAny).ReverseSlot());
		shop.Add(new Entry(MakeItem(ItemID.SquireGreaves, 15),	Condition.DownedMechBossAny).ReverseSlot());
		shop.Add(new Entry(MakeItem(ItemID.SquireAltHead, 50),	Condition.DownedGolem).ReverseSlot());						// Valhalla Knight's Helm
		shop.Add(new Entry(MakeItem(ItemID.SquireAltShirt, 50), Condition.DownedGolem).ReverseSlot());						// Valhalla Knight's Breastplate
		shop.Add(new Entry(MakeItem(ItemID.SquireAltPants, 50), Condition.DownedGolem).ReverseSlot());						// Valhalla Knight's Greaves

		// 2nd row
		shop.Add(new Entry(MakeItem(ItemID.DD2FlameburstTowerT1Popper, 5)).ReverseSlot());									// Flameburst Rod
		shop.Add(new Entry(MakeItem(ItemID.DD2BallistraTowerT1Popper, 5)).ReverseSlot());									// Ballista Rod
		shop.Add(new Entry(MakeItem(ItemID.DD2ExplosiveTrapT1Popper, 5)).ReverseSlot());                                    // Explosive Trap Rod
		shop.Add(new Entry(MakeItem(ItemID.DD2LightningAuraT1Popper, 5)).ReverseSlot());                                    // Lightning Aura Rod
		shop.Add(new Entry(MakeItem(ItemID.ApprenticeHat, 15),	Condition.DownedMechBossAny).ReverseSlot());
		shop.Add(new Entry(MakeItem(ItemID.ApprenticeRobe, 15),	Condition.DownedMechBossAny).ReverseSlot());
		shop.Add(new Entry(MakeItem(ItemID.ApprenticeTrousers, 15), Condition.DownedMechBossAny).ReverseSlot());
		shop.Add(new Entry(MakeItem(ItemID.ApprenticeAltHead, 50), Condition.DownedGolem).ReverseSlot());					// Dark Atrist's Hat
		shop.Add(new Entry(MakeItem(ItemID.ApprenticeAltShirt, 50), Condition.DownedGolem).ReverseSlot());                  // Dark Atrist's Robes
		shop.Add(new Entry(MakeItem(ItemID.ApprenticeAltPants, 50), Condition.DownedGolem).ReverseSlot());                  // Dark Atrist's Leggings

		// 3rd row
		shop.Add(new Entry(MakeItem(ItemID.DD2FlameburstTowerT2Popper, 15),	Condition.DownedMechBossAny).ReverseSlot());	// Flameburst Cane
		shop.Add(new Entry(MakeItem(ItemID.DD2BallistraTowerT2Popper, 15),	Condition.DownedMechBossAny).ReverseSlot());    // Ballista Cane
		shop.Add(new Entry(MakeItem(ItemID.DD2ExplosiveTrapT2Popper, 15),	Condition.DownedMechBossAny).ReverseSlot());    // Explosive Trap Cane
		shop.Add(new Entry(MakeItem(ItemID.DD2LightningAuraT2Popper, 15),	Condition.DownedMechBossAny).ReverseSlot());    // Lightning Aura Cane
		shop.Add(new Entry(MakeItem(ItemID.HuntressWig, 15),	Condition.DownedMechBossAny).ReverseSlot());
		shop.Add(new Entry(MakeItem(ItemID.HuntressJerkin, 15), Condition.DownedMechBossAny).ReverseSlot());
		shop.Add(new Entry(MakeItem(ItemID.HuntressPants, 15),	Condition.DownedMechBossAny).ReverseSlot());
		shop.Add(new Entry(MakeItem(ItemID.HuntressAltHead, 50), Condition.DownedGolem).ReverseSlot());						// Red Riding Hood
		shop.Add(new Entry(MakeItem(ItemID.HuntressAltShirt, 50), Condition.DownedGolem).ReverseSlot());					// Red Riding Dress
		shop.Add(new Entry(MakeItem(ItemID.HuntressAltPants, 50), Condition.DownedGolem).ReverseSlot());					// Red Riding Leggings

		// 4th row
		shop.Add(new Entry(MakeItem(ItemID.DD2FlameburstTowerT3Popper, 60), Condition.DownedGolem).ReverseSlot());			// Flameburst Staff
		shop.Add(new Entry(MakeItem(ItemID.DD2BallistraTowerT3Popper, 60),	Condition.DownedGolem).ReverseSlot());			// Ballista Staff
		shop.Add(new Entry(MakeItem(ItemID.DD2ExplosiveTrapT3Popper, 60),	Condition.DownedGolem).ReverseSlot());          // Explosive Trap Staff
		shop.Add(new Entry(MakeItem(ItemID.DD2LightningAuraT3Popper, 60),	Condition.DownedGolem).ReverseSlot());          // Lightning Aura Staff
		shop.Add(new Entry(MakeItem(ItemID.MonkBrows, 15),		Condition.DownedMechBossAny).ReverseSlot());                // Monk's Bushy Brow Bald Cap
		shop.Add(new Entry(MakeItem(ItemID.MonkShirt, 15),		Condition.DownedMechBossAny).ReverseSlot());				// Monk's Shirt
		shop.Add(new Entry(MakeItem(ItemID.MonkPants, 15),		Condition.DownedMechBossAny).ReverseSlot());				// Monk's Pants
		shop.Add(new Entry(MakeItem(ItemID.MonkAltHead, 50), Condition.DownedGolem).ReverseSlot());                         // Shinobi Infiltrator's Helmet
		shop.Add(new Entry(MakeItem(ItemID.MonkAltShirt, 50), Condition.DownedGolem).ReverseSlot());                        // Shinobi Infiltrator's Torso
		shop.Add(new Entry(MakeItem(ItemID.MonkAltPants, 50), Condition.DownedGolem).ReverseSlot());                        // Shinobi Infiltrator's Pants
	}

	public override void ModifyActiveShop(NPC npc, string shopName, Item[] items)
	{
		foreach (var item in items) {
			if (item.type == ItemID.DD2ElderCrystal) {
				if (NPC.downedGolemBoss) {
					item.shopCustomPrice = Item.buyPrice(gold: 4);
				}
				else if (NPC.downedMechBossAny) {
					item.shopCustomPrice = Item.buyPrice(gold: 1);
				}
				else {
					item.shopCustomPrice = Item.buyPrice(silver: 25);
				}
			}
		}
	}
}
