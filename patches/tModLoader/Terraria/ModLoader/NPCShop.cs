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
	/// 
	/// </summary>
	/// <param name="items">Array to be filled.</param>
	/// <param name="overflow">Equals to true if amount of added items is greater than 39.</param>
	public void Build(Item[] items, out bool overflow) {
		overflow = false;

		int i = 0;
		foreach (Entry entry in entries) {
			if (entry.Disabled) // Note, disabled entries can't reserve slots
				continue;

			var item = entry.Item;
			if (!entry.ConditionsMet()) {
				if (!entry.SlotReserved)
					continue;

				item = new Item(0);
			}

			if (i == items.Length) {
				overflow = true;
				return;
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
}