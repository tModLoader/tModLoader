﻿using System.Collections.Generic;
using System.Linq;
using System;

namespace Terraria.ModLoader;

public sealed partial class NPCShop {
	public sealed class Entry {
		internal readonly Item item;
		private readonly List<ICondition> conditions;
		private event Action<Item, Player> onOpened;

		internal (Entry target, bool after) Ordering { get; private set; } = (null, false);
		public bool Disabled { get; private set; }
		public bool OrdersLast { get; private set; }
		public bool SlotReserved { get; private set; }
		public Item Item => item;

		public Entry(int item, params ICondition[] condition) : this(new Item(item), condition) { }

		public Entry(Item item, params ICondition[] condition) {
			Disabled = false;
			this.item = item;
			conditions = condition.ToList();
		}

		internal Entry SetOrdering(Entry entry, bool after = false) {
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

		public Entry SortBefore(Entry target) => SetOrdering(target, after: false);
		public Entry SortAfter(Entry target) => SetOrdering(target, after: true);

		public Entry AddCondition(ICondition condition) {
			ArgumentNullException.ThrowIfNull(condition, nameof(condition));
			conditions.Add(condition);
			return this;
		}

		public Entry OrderLast() {
			OrdersLast = true;
			return this;
		}

		public Entry Disable() {
			Disabled = true;
			return this;
		}

		public Entry ReserveSlot() {
			SlotReserved = true;
			return this;
		}

		public Entry OnShopOpened(Action<Item, NPC> onShopOpened)
		{
			onOpened += onShopOpened;
			return this;
		}

		public void OnShopOpen(Item item, NPC npc)
		{
			onOpened(item, npc);
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
}
