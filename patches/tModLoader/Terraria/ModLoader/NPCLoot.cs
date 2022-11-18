using System;
using System.Collections.Generic;
using Terraria.GameContent.ItemDropRules;

namespace Terraria.ModLoader;

/// <summary> This readonly struct is a simple shortcut to <see cref="ItemDropDatabase"/>'s methods. </summary>
public readonly struct NPCLoot : ILoot
{
	private readonly int npcNetId;
	private readonly ItemDropDatabase itemDropDatabase;

	public NPCLoot(int npcNetId, ItemDropDatabase itemDropDatabase)
	{
		this.npcNetId = npcNetId;
		this.itemDropDatabase = itemDropDatabase;
	}

	public List<IItemDropRule> Get(bool includeGlobalDrops = true)
		=> itemDropDatabase.GetRulesForNPCID(npcNetId, includeGlobalDrops);

	public IItemDropRule Add(IItemDropRule entry)
	{
		itemDropDatabase.RegisterToNPCNetId(npcNetId, entry);

		return entry;
	}

	public IItemDropRule Remove(IItemDropRule entry)
	{
		itemDropDatabase.RemoveFromNPCNetId(npcNetId, entry);

		return entry;
	}

	public void RemoveWhere(Predicate<IItemDropRule> predicate, bool includeGlobalDrops = true)
	{
		foreach (var entry in Get(includeGlobalDrops)) {
			if (predicate(entry)) {
				Remove(entry);
			}
		}
	}
}
