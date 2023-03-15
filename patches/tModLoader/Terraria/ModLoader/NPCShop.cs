using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ID;

namespace Terraria.ModLoader;

public sealed partial class NPCShop {
	private List<Entry> entries;
	private readonly int npcId;
	private readonly string name;

	public IReadOnlyList<Entry> Entries => entries;
	public int NpcType => npcId;
	public string Name => name;
	public string FullName => NPCShopDatabase.GetNPCShopName(NpcType, Name);

	public bool FillLastSlot { get; private set; }

	public NPCShop(int npcId, string name = "Shop") {
		entries = new();
		this.name = name;
		this.npcId = npcId;
	}

	public Entry GetEntry(int item)
	{
		return entries[entries.FindIndex(x => x.item.type.Equals(item))];
	}

	public bool TryGetEntry(int item, out Entry entry)
	{
		int i = entries.FindIndex(x => x.item.type.Equals(item));
		if (i == -1) {
			entry = null;
			return false;
		}
		entry = entries[i];
		return true;
	}

	public void Register() {
		NPCShopDatabase.RegisterNpcShop(npcId, this, name);
	}

	public NPCShop AllowFillingLastSlot()
	{
		FillLastSlot = true;
		return this;
	}

	public NPCShop Add(params Entry[] entries) {
		this.entries.AddRange(entries);
		return this;
	}

	public NPCShop Add(int item, params ICondition[] condition) {
		return Add(ContentSamples.ItemsByType[item], condition);
	}

	public NPCShop Add(Item item, params ICondition[] condition) {
		return Add(new Entry(item, condition));
	}

	public NPCShop Add<T>(params ICondition[] condition) where T : ModItem {
		return Add(ModContent.ItemType<T>(), condition);
	}

	private NPCShop InsertAt(Entry targetEntry, Item item, bool after, params ICondition[] condition) {
		var orderEntry = new Entry(item, condition);
		orderEntry.SetOrdering(targetEntry, after);
		return Add(orderEntry);
	}

	private NPCShop InsertAt(int targetItem, Item item, bool after, params ICondition[] condition) {
		return InsertAt(GetEntry(targetItem), item, after, condition);
	}

	private NPCShop InsertAt(int targetItem, int item, bool after, params ICondition[] condition) {
		return InsertAt(targetItem, ContentSamples.ItemsByType[item], after, condition);
	}

	public NPCShop InsertBefore(int targetItem, int item, params ICondition[] condition) {
		return InsertAt(targetItem, item, false, condition);
	}

	public NPCShop InsertBefore(int targetItem, Item item, params ICondition[] condition) {
		return InsertAt(targetItem, item, false, condition);
	}

	public NPCShop InsertAfter(int targetItem, int item, params ICondition[] condition) {
		return InsertAt(targetItem, item, true, condition);
	}

	public NPCShop InsertAfter(int targetItem, Item item, params ICondition[] condition) {
		return InsertAt(targetItem, item, true, condition);
	}

	/// <summary>
	/// Fills a shop array with the contents of this shop, evaluating conditions and running callbacks. <br/>
	/// Does not fill the entire array if there are insufficient entries. <br/>
	/// The last slot will be kept empty (null) if <see cref="FillLastSlot"/> is false
	/// </summary>
	/// <param name="items">Array to be filled.</param>
	/// <param name="npc">The NPC the player is talking to, for <see cref="Entry.OnShopOpen(Item, NPC)"/> calls.</param>
	/// <param name="overflow">True if some items were unable to fit in the provided array.</param>
	public void Build(Item[] items, NPC npc, out bool overflow) {
		overflow = false;

		int limit = FillLastSlot ? items.Length : items.Length - 1;
		int i = 0;
		foreach (Entry entry in entries) {
			if (entry.Disabled) // Note, disabled entries can't reserve slots
				continue;

			bool conditionsMet = entry.ConditionsMet();
			if (!conditionsMet && !entry.SlotReserved)
				continue;

			if (i == limit) {
				overflow = true;
				return;
			}

			Item item;
			if (conditionsMet) {
				item = entry.Item.Clone();
				entry.OnShopOpen(item, npc);
			}
			else {
				item = new Item(0);
			}

			items[i++] = item;
		}
	}

	internal void Sort()
	{
		// process 'OrdersLast' first, so an entry which sorts after an 'OrdersLast' entry is still placed in the correct position
		var toBeLast = entries.Where(x => x.OrdersLast).ToList();
		entries.RemoveAll(x => x.OrdersLast);
		entries.AddRange(toBeLast);

		entries = SortBeforeAfter(entries, r => r.Ordering);
	}

	internal static List<T> SortBeforeAfter<T>(IEnumerable<T> values, Func<T, (T, bool after)> func) {
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
			return values.ToList();

		var sorted = new List<T>();
		void Sort(T r) {
			if (sortBefore.TryGetValue(r, out var before))
				foreach (var c in before)
					Sort(c);

			sorted.Add(r);

			if (sortAfter.TryGetValue(r, out var after))
				foreach (var c in after)
					Sort(c);
		}

		foreach (var r in baseOrder) {
			Sort(r);
		}

		return sorted;
	}
}