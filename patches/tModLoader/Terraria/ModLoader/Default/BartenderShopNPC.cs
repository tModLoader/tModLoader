using Terraria.ID;

namespace Terraria.ModLoader.Default;

public sealed class BartenderShopNPC : GlobalNPC
{
	public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
	{
		return entity.type == NPCID.DD2Bartender;
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
