using System;
using System.Collections.Generic;
using Terraria.GameContent.ItemDropRules;

namespace Terraria.ModLoader;

/// <summary> Provides access to <see cref="ItemDropDatabase"/>'s methods specific to this item type. </summary>
public readonly struct ItemLoot : ILoot
{
	private readonly int itemType;
	private readonly ItemDropDatabase itemDropDatabase;

	public ItemLoot(int itemType, ItemDropDatabase itemDropDatabase)
	{
		this.itemType = itemType;
		this.itemDropDatabase = itemDropDatabase;
	}

	/// <summary>
	/// <inheritdoc cref="ItemDropDatabase.GetRulesForItemID(int)"/>
	/// </summary>
	/// <param name="includeGlobalDrops">Unused</param>
	public List<IItemDropRule> Get(bool includeGlobalDrops = true) => itemDropDatabase.GetRulesForItemID(itemType);

	public IItemDropRule Add(IItemDropRule entry) => itemDropDatabase.RegisterToItem(itemType, entry);

	/// <summary>
	/// Removes a specific <see cref="IItemDropRule"/> from this item type.
	/// <para/> Note that <paramref name="entry"/> must be an existing <see cref="IItemDropRule"/> instance retrieved from <see cref="Get(bool)"/>, not a newly created instance.
	/// </summary>
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
