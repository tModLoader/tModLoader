using System;
using System.Collections.Generic;
using Terraria.GameContent.ItemDropRules;

namespace Terraria.ModLoader;

/// <summary> This readonly struct is a simple shortcut for modifying global drop rules in an <see cref="ItemDropDatabase"/>. </summary>
public readonly struct GlobalLoot : ILoot
{
	private readonly ItemDropDatabase itemDropDatabase;

	public GlobalLoot(ItemDropDatabase itemDropDatabase)
	{
		this.itemDropDatabase = itemDropDatabase;
	}

	public List<IItemDropRule> Get(bool unusedParam = true) => new List<IItemDropRule>(itemDropDatabase._globalEntries);

	public IItemDropRule Add(IItemDropRule entry) => itemDropDatabase.RegisterToGlobal(entry);

	public IItemDropRule Remove(IItemDropRule entry)
	{
		itemDropDatabase._globalEntries.Remove(entry);

		return entry;
	}

	public void RemoveWhere(Predicate<IItemDropRule> predicate, bool unusedParam = true)
	{
		var list = itemDropDatabase._globalEntries;

		for (int i = 0; i < list.Count; i++) {
			var entry = list[i];

			if (predicate(entry)) {
				list.RemoveAt(i--);
			}
		}
	}
}
