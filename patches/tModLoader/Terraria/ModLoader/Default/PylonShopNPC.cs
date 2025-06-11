using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ID;
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

		if (NPCShopDatabase.NoPylons.Contains(shop.FullName))
			return;
		
		foreach (var entry in _pylonEntries) {
			shop.Add(entry);
		}
	}

	public override void Unload()
	{
		_pylonEntries = null;
	}

	public override void ModifyActiveShop(NPC npc, string shopName, Item[] items)
	{
		if (shopName == NPCShopDatabase.GetShopName(NPCID.DD2Bartender))
			AddPylonsToBartenderShop(npc, items);
	}

	private void AddPylonsToBartenderShop(NPC npc, Item[] items)
	{
		// pylons can spawn in slots 4 and 30
		int slot;
		if (items[4].IsAir)
			slot = 4;
		else if (items[30].IsAir)
			slot = 30;
		else
			return;

		foreach (var entry in _pylonEntries) {
			if (entry.Disabled || !entry.ConditionsMet())
				continue;
			
			items[slot] = entry.Item.Clone();
			entry.OnShopOpen(items[slot], npc);

			do {
				if (++slot >= items.Length)
					return;
			}
			while (!items[slot].IsAir);
		}
	}
}
