using System;
using System.Collections.Generic;
using Terraria.GameContent.ItemDropRules;

namespace Terraria.ModLoader;

/// <summary> This readonly struct is a simple shortcut to <see cref="ItemDropDatabase"/>'s methods. </summary>
public readonly struct ItemLoot : ILoot
{
	private readonly int itemType;
	private readonly ItemDropDatabase itemDropDatabase;

	public ItemLoot(int itemType, ItemDropDatabase itemDropDatabase)
	{
		this.itemType = itemType;
		this.itemDropDatabase = itemDropDatabase;
	}

	public List<IItemDropRule> Get(bool includeGlobalDrops = true) => itemDropDatabase.GetRulesForItemID(itemType);

	public IItemDropRule Add(IItemDropRule entry) => itemDropDatabase.RegisterToItem(itemType, entry);

	public IItemDropRule Remove(IItemDropRule entry) => itemDropDatabase.RemoveFromItem(itemType, entry);

	public void RemoveWhere(Predicate<IItemDropRule> predicate, bool includeGlobalDrops = true)
	{
		foreach (var entry in Get()) {
			if (predicate(entry)) {
				Remove(entry);
			}
		}
	}
}
