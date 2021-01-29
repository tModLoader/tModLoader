using System;
using System.Collections.Generic;
using Terraria.GameContent.ItemDropRules;

namespace Terraria.ModLoader
{
	/// <summary> This readonly struct is a simple shortcut for modifying global drop rules in an <see cref="ItemDropDatabase"/>. </summary>
	public readonly struct GlobalLoot
	{
		private readonly ItemDropDatabase itemDropDatabase;

		public GlobalLoot(ItemDropDatabase itemDropDatabase) {
			this.itemDropDatabase = itemDropDatabase;
		}

		//A whole new list is created here to avoid enumeration issues and direct edits. This is, of course, lame for the GC.
		//Should this return an IReadOnlyList wrapper?
		public List<IItemDropRule> Get() => new List<IItemDropRule>(itemDropDatabase._globalEntries);

		public IItemDropRule Add(IItemDropRule entry) => itemDropDatabase.RegisterToGlobal(entry);

		public IItemDropRule Remove(IItemDropRule entry) {
			itemDropDatabase._globalEntries.Remove(entry);

			return entry;
		}

		public void RemoveWhere(Predicate<IItemDropRule> predicate) {
			var list = itemDropDatabase._globalEntries;

			for (int i = 0; i < list.Count; i++) {
				var entry = list[i];

				if (predicate(entry)) {
					list.RemoveAt(i--);
				}
			}
		}
	}
}
