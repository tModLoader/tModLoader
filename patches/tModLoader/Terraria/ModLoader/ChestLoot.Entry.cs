using System.Collections.Generic;
using System.Linq;
using System;
using Terraria.ID;

namespace Terraria.ModLoader;

public partial class ChestLoot {
	private static Item EmptyInstance => new();

	public class Entry {
		internal readonly Item item;
		private readonly List<ICondition> conditions;
		private readonly bool askingNicelyToNotAdd = false;
		private Ref<bool> hide;

		public Item Item => item;

		public Dictionary<bool, List<Entry>> ChainedEntries;

		public Entry(params ICondition[] condition) : this(EmptyInstance, condition) {
			askingNicelyToNotAdd = true;
		}

		public Entry(int item, params ICondition[] condition) : this(ContentSamples.ItemsByType[item], condition) { }

		public Entry(Item item, params ICondition[] condition) {
			hide = new(false);
			this.item = item;
			conditions = condition.ToList();

			ChainedEntries = new()
			{
				{ false, new() },
				{ true, new() }
			};
		}

		public Entry OnSuccess(int itemId, params ICondition[] condition) {
			return OnSuccess(new Entry(ContentSamples.ItemsByType[itemId], condition));
		}

		public Entry OnSuccess(Entry entry) {
			ChainedEntries[true].Add(entry);
			return this;
		}

		public Entry OnFail(int itemId, params ICondition[] condition) {
			return OnFail(new Entry(ContentSamples.ItemsByType[itemId], condition));
		}

		public Entry OnFail(Entry entry) {
			ChainedEntries[false].Add(entry);
			return this;
		}

		public Entry AddCondition(ICondition condition) {
			ArgumentNullException.ThrowIfNull(condition, nameof(condition));
			conditions.Add(condition);
			return this;
		}

		public Entry Hide() {
			hide.Value = true;
			return this;
		}

		public bool IsAvailable() {
			foreach (ICondition condition in conditions) {
				if (!condition.IsAvailable()) {
					return false;
				}
			}
			return true;
		}

		public void TryAdd(List<Item> items) {
			if (hide.Value)
				return;

			if (IsAvailable()) {
				if (!askingNicelyToNotAdd) {
					items.Add(item);
				}
				foreach (var entry in ChainedEntries[true]) {
					entry.TryAdd(items);
				}
			}
			else {
				foreach (var entry in ChainedEntries[false]) {
					entry.TryAdd(items);
				}
			}
		}
	}
}
