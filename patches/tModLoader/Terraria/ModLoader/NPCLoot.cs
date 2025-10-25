using System;
using System.Collections.Generic;
using Terraria.GameContent.ItemDropRules;

namespace Terraria.ModLoader;

/// <summary> Provides access to <see cref="ItemDropDatabase"/>'s methods specific to this NPC type. </summary>
public readonly struct NPCLoot : ILoot
{
	private readonly int npcNetId;
	private readonly ItemDropDatabase itemDropDatabase;

	public NPCLoot(int npcNetId, ItemDropDatabase itemDropDatabase)
	{
		this.npcNetId = npcNetId;
		this.itemDropDatabase = itemDropDatabase;
	}

	/// <inheritdoc cref="ItemDropDatabase.GetRulesForNPCID(int, bool)"/>
	public List<IItemDropRule> Get(bool includeGlobalDrops = true)
		=> itemDropDatabase.GetRulesForNPCID(npcNetId, includeGlobalDrops);

	public IItemDropRule Add(IItemDropRule entry)
	{
		itemDropDatabase.RegisterToNPCNetId(npcNetId, entry);

		return entry;
	}

	/// <summary>
	/// Removes a specific <see cref="IItemDropRule"/> from this NPC type.
	/// <para/> Note that <paramref name="entry"/> must be an existing <see cref="IItemDropRule"/> instance retrieved from <see cref="Get(bool)"/>, not a newly created instance.
	/// </summary>
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
