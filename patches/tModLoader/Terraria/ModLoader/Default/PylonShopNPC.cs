using System.Collections.Generic;
using Terraria.Localization;

namespace Terraria.ModLoader.Default;

/// <summary>
/// This is a GlobalNPC native to tML that handles adding Pylon items to NPC's shops, to save on patch size within vanilla.
/// </summary>
public sealed class PylonShopNPC : GlobalNPC
{
	private static List<NPCShop.Entry> _pylonEntries;

	public override void ModifyShop(NPCShop shop)
	{
		if (_pylonEntries == null)
			GetAndCacheAllPylonEntries();

		foreach (var entry in _pylonEntries) {
			shop.Add(entry);
		}
	}

	private void GetAndCacheAllPylonEntries()
	{
		_pylonEntries = new(NPCShopDatabase.GetVanillaPylonEntries());

		foreach (ModPylon pylon in PylonLoader.modPylons) {
			if (pylon.ItemDrop == 0)
				continue;

			_pylonEntries.Add(new NPCShop.Entry(pylon.ItemDrop, new NPCShop.Condition(NetworkText.Empty, () =>
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
