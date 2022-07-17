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

		/// <summary>
		/// Not recomended for use. Use GetX instead.
		/// </summary>
		/// <returns></returns>
		public List<IItemDropRule> Get() => new List<IItemDropRule>(itemDropDatabase._globalEntriesItem);

		public List<IItemDropRule> GetBossBag() => new List<IItemDropRule>(itemDropDatabase._globalEntriesItem_BossBag);

		public List<IItemDropRule> GetPresent() => new List<IItemDropRule>(itemDropDatabase._globalEntriesItem_Present);

		public List<IItemDropRule> GetGoodieBag() => new List<IItemDropRule>(itemDropDatabase._globalEntriesItem_GoodieBag);

		public List<IItemDropRule> GetHerbBag() => new List<IItemDropRule>(itemDropDatabase._globalEntriesItem_HerbBag);

		public List<IItemDropRule> GetCrate() => new List<IItemDropRule>(itemDropDatabase._globalEntriesItem_Crate);

		/// <summary>
		/// Not recomended for use. Use AddX instead, unless you want every bag-type items to have this drop rule.
		/// </summary>
		/// <param name="entry"></param>
		/// <returns></returns>
		public IItemDropRule Add(IItemDropRule entry) => itemDropDatabase.RegisterToGlobalItem(entry);

		public IItemDropRule AddBossBag(IItemDropRule entry) => itemDropDatabase.RegisterToGlobalItem_BossBag(entry);

		public IItemDropRule AddPresent(IItemDropRule entry) => itemDropDatabase.RegisterToGlobalItem_Present(entry);

		public IItemDropRule AddGoodieBag(IItemDropRule entry) => itemDropDatabase.RegisterToGlobalItem_GoodieBag(entry);

		public IItemDropRule AddHerbBag(IItemDropRule entry) => itemDropDatabase.RegisterToGlobalItem_HerbBag(entry);

		public IItemDropRule AddCrate(IItemDropRule entry) => itemDropDatabase.RegisterToGlobalItem_Crate(entry);

		/// <summary>
		/// Not recomended for use. Use RemoveX instead, unless you want every bag-type items to remove this drop rule.
		/// </summary>
		/// <param name="entry"></param>
		/// <returns></returns>
		public IItemDropRule Remove(IItemDropRule entry) {
			itemDropDatabase._globalEntriesItem.Remove(entry);
			return entry;
		}

		public IItemDropRule RemoveBossBag(IItemDropRule entry) {
			itemDropDatabase._globalEntriesItem_BossBag.Remove(entry);
			return entry;
		}

		public IItemDropRule RemovePresent(IItemDropRule entry) {
			itemDropDatabase._globalEntriesItem_Present.Remove(entry);
			return entry;
		}

		public IItemDropRule RemoveGoodieBag(IItemDropRule entry) {
			itemDropDatabase._globalEntriesItem_GoodieBag.Remove(entry);
			return entry;
		}

		public IItemDropRule RemoveHerbBag(IItemDropRule entry) {
			itemDropDatabase._globalEntriesItem_HerbBag.Remove(entry);
			return entry;
		}

		public IItemDropRule RemoveCrate(IItemDropRule entry) {
			itemDropDatabase._globalEntriesItem_Crate.Remove(entry);
			return entry;
		}

		/// <summary>
		/// Not recomended for use. Use RemoveWhereX instead, unless you want every bag-type items to remove this drop rule.
		/// </summary>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public void RemoveWhere(Predicate<IItemDropRule> predicate) {
			var list = itemDropDatabase._globalEntriesItem;

			for (int i = 0; i < list.Count; i++) {
				var entry = list[i];

				if (predicate(entry)) {
					list.RemoveAt(i--);
				}
			}
		}

		public void RemoveWhereBossBag(Predicate<IItemDropRule> predicate) {
			var list = itemDropDatabase._globalEntriesItem_BossBag;

			for (int i = 0; i < list.Count; i++) {
				var entry = list[i];

				if (predicate(entry)) {
					list.RemoveAt(i--);
				}
			}
		}

		public void RemoveWherePresent(Predicate<IItemDropRule> predicate) {
			var list = itemDropDatabase._globalEntriesItem_Present;

			for (int i = 0; i < list.Count; i++) {
				var entry = list[i];

				if (predicate(entry)) {
					list.RemoveAt(i--);
				}
			}
		}

		public void RemoveWhereGoodieBag(Predicate<IItemDropRule> predicate) {
			var list = itemDropDatabase._globalEntriesItem_GoodieBag;

			for (int i = 0; i < list.Count; i++) {
				var entry = list[i];

				if (predicate(entry)) {
					list.RemoveAt(i--);
				}
			}
		}

		public void RemoveWhereHerbBag(Predicate<IItemDropRule> predicate) {
			var list = itemDropDatabase._globalEntriesItem_HerbBag;

			for (int i = 0; i < list.Count; i++) {
				var entry = list[i];

				if (predicate(entry)) {
					list.RemoveAt(i--);
				}
			}
		}

		public void RemoveWhereCrate(Predicate<IItemDropRule> predicate) {
			var list = itemDropDatabase._globalEntriesItem_Crate;

			for (int i = 0; i < list.Count; i++) {
				var entry = list[i];

				if (predicate(entry)) {
					list.RemoveAt(i--);
				}
			}
		}
	}
}
