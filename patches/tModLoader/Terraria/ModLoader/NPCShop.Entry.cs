using System;

namespace Terraria.ModLoader;

public partial class NPCShop
{
	public new class Entry : AbstractNPCShop.Entry
	{
		private Action<Item, NPC> shopOpenedHooks;

		internal (Entry target, bool after) Ordering { get; private set; } = (null, false);

		public bool OrdersLast { get; private set; }
		/// <inheritdoc cref="ReserveSlot"/>
		public bool SlotReserved { get; private set; }

		public Entry(int item, params Condition[] condition) : this(new Item(item), condition) { }
		public Entry(Item item, params Condition[] condition) : base(item, condition) { }

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

		public new Entry AddCondition(Condition condition)
		{
			base.AddCondition(condition);
			return this;
		}

		public Entry OrderLast()
		{
			OrdersLast = true;
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
	}
}
