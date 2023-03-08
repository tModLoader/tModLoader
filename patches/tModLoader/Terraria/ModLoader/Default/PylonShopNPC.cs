using Terraria.Localization;

namespace Terraria.ModLoader.Default;

/// <summary>
/// This is a GlobalNPC native to tML that handles adding Pylon items to NPC's shops, to save on patch size within vanilla.
/// </summary>
public sealed class PylonShopNPC : GlobalNPC
{
	public override void ModifyShop(NPCShop shop)
	{
		foreach (ModPylon pylon in PylonLoader.modPylons) {
			if (pylon.ItemDrop == 0) {
				continue;
			}

			shop.Add(new NPCShop.Entry(pylon.ItemDrop, new NPCShop.Condition(NetworkText.Empty, () =>
				Main.LocalPlayer.talkNPC != -1 &&
				pylon.IsPylonForSale(
					Main.npc[Main.LocalPlayer.talkNPC].type,
					Main.LocalPlayer,
					Main.LocalPlayer.currentShoppingSettings.PriceAdjustment <= 0.8999999761581421
				).HasValue
				)).OrderLast());
		}
	}
}
