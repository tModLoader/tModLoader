using System.Collections.Generic;
using System.Linq;
using System;
using Terraria.ID;

namespace Terraria.ModLoader;

public sealed partial class NPCShop {
	private static Item EmptyInstance => new();

	public sealed class Entry {
		internal readonly Item item;
		private readonly List<ICondition> conditions;

		internal (Entry target, bool after) Ordering { get; private set; } = (null, false);
		public bool Disabled { get; private set; }
		public Item Item => item;

		public Entry(int item, params ICondition[] condition) : this(new Item(item), condition) { }

		public Entry(Item item, params ICondition[] condition) {
			Disabled = false;
			this.item = item;
			conditions = condition.ToList();
		}

		public Entry Target(Entry entry, bool after = false) {
			ArgumentNullException.ThrowIfNull(entry, nameof(entry));
			Ordering = (entry, after);

			var target = entry;
			do {
				if (target == this)
					throw new Exception("Entry ordering loop!");

				target = target.Ordering.target;
			} while (target != null);
			return this;
		}

		public Entry AddCondition(ICondition condition) {
			ArgumentNullException.ThrowIfNull(condition, nameof(condition));
			conditions.Add(condition);
			return this;
		}

		public Entry Disable() {
			Disabled = true;
			return this;
		}

		public bool ConditionsMet() {
			for (int i = 0; i < conditions.Count; i++) {
				if (!conditions[i].IsAvailable()) {
					return false;
				}
			}
			return true;
		}
	}

	private static void SortBeforeAfter<T>(IEnumerable<T> values, Func<T, (T, bool after)> func) {
		var baseOrder = new List<T>();
		var sortBefore = new Dictionary<T, List<T>>();
		var sortAfter = new Dictionary<T, List<T>>();

		foreach (var r in values) {
			switch (func(r)) {
				case (null, _):
					baseOrder.Add(r);
					break;
				case (var target, false):
					if (!sortBefore.TryGetValue(target, out var before))
						before = sortBefore[target] = new();

					before.Add(r);
					break;
				case (var target, true):
					if (!sortAfter.TryGetValue(target, out var after))
						after = sortAfter[target] = new();

					after.Add(r);
					break;
			}
		}

		if ((sortBefore.Count + sortAfter.Count) == 0)
			return;

		void Sort(T r) {
			if (sortBefore.TryGetValue(r, out var before))
				foreach (var c in before)
					Sort(c);

			if (sortAfter.TryGetValue(r, out var after))
				foreach (var c in after)
					Sort(c);
		}

		foreach (var r in baseOrder) {
			Sort(r);
		}
	}
}
