using System.Collections.Generic;
using System.Linq;
using System;

namespace Terraria.ModLoader;

public sealed partial class NPCShop
{
	public new sealed class Entry : AbstractNPCShop.Entry
	{
		public Item Item { get; }

		private readonly List<Condition> conditions;
		public IEnumerable<Condition> Conditions => conditions;

		private Action<Item, NPC> shopOpenedHooks;

		internal (Entry target, bool after) Ordering { get; private set; } = (null, false);

		public bool Disabled { get; private set; }
		public bool OrdersLast { get; private set; }
		/// <inheritdoc cref="ReserveSlot"/>
		public bool SlotReserved { get; private set; }

		public Entry(int item, params Condition[] condition) : this(new Item(item), condition) { }

		public Entry(Item item, params Condition[] condition)
		{
			Disabled = false;
			Item = item;
			conditions = condition.ToList();
		}

		internal Entry SetOrdering(Entry entry, bool after)
		{
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

		public Entry AddCondition(Condition condition)
		{
			ArgumentNullException.ThrowIfNull(condition, nameof(condition));
			conditions.Add(condition);
			return this;
		}

		public Entry OrderLast()
		{
			OrdersLast = true;
			return this;
		}

		public Entry Disable()
		{
			Disabled = true;
			return this;
		}

		/// <summary>
		/// Reserves a slot for this entry even if its conditions are not met (<see cref="ConditionsMet"/>). This can be used to create a defined shop layout similar to the Tavernkeep shop.
		/// </summary>
		/// <returns></returns>
		public Entry ReserveSlot()
		{
			SlotReserved = true;
			return this;
		}

		public Entry AddShopOpenedCallback(Action<Item, NPC> callback)
		{
			shopOpenedHooks += callback;
			return this;
		}

		public void OnShopOpen(Item item, NPC npc)
		{
			shopOpenedHooks?.Invoke(item, npc);
		}

		public bool ConditionsMet()
		{
			for (int i = 0; i < conditions.Count; i++) {
				if (!conditions[i].IsMet()) {
					return false;
				}
			}
			return true;
		}
	}
}
