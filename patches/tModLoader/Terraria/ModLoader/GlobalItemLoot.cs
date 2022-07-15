using System;
using System.Collections.Generic;
using Terraria.GameContent.ItemDropRules;

namespace Terraria.ModLoader
{
	/// <summary> This readonly struct is a simple shortcut for modifying global drop rules in an <see cref="ItemDropDatabase"/>. </summary>
	public readonly struct GlobalItemLoot
	{
		private readonly ItemDropDatabase itemDropDatabase;

		public GlobalItemLoot(ItemDropDatabase itemDropDatabase) {
			this.itemDropDatabase = itemDropDatabase;
		}

		//A whole new list is created here to avoid enumeration issues and direct edits. This is, of course, lame for the GC.
		//Should this return an IReadOnlyList wrapper?
		public List<IItemDropRule> Get() => new List<IItemDropRule>(itemDropDatabase._globalEntriesItem);

		public IItemDropRule Add(IItemDropRule entry) => itemDropDatabase.RegisterToGlobalItem(entry);

		public IItemDropRule Remove(IItemDropRule entry) {
			itemDropDatabase._globalEntriesItem.Remove(entry);

			return entry;
		}

		public void RemoveWhere(Predicate<IItemDropRule> predicate) {
			var list = itemDropDatabase._globalEntriesItem;

			for (int i = 0; i < list.Count; i++) {
				var entry = list[i];

				if (predicate(entry)) {
					list.RemoveAt(i--);
				}
			}
		}
	}
}
