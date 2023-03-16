using System.Collections.Generic;
using System.Linq;
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
		_pylonEntries ??= NPCShopDatabase.GetPylonEntries().ToList();
		foreach (var entry in _pylonEntries) {
			shop.Add(entry);
		}
	}
}
