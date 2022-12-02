using System;
using System.Collections.Generic;
using Terraria.GameContent.ItemDropRules;

namespace Terraria.ModLoader;

public interface ILoot
{
	//A whole new list is created here to avoid enumeration issues and direct edits. This is, of course, lame for the GC.
	//Should this return an IReadOnlyList wrapper?
	List<IItemDropRule> Get(bool includeGlobalDrops = true);

	IItemDropRule Add(IItemDropRule entry);

	IItemDropRule Remove(IItemDropRule entry);

	void RemoveWhere(Predicate<IItemDropRule> predicate, bool includeGlobalDrops = true);
}
