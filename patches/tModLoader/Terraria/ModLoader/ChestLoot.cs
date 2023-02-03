using System.Collections.Generic;
using System.Linq;
using Terraria.ID;

namespace Terraria.ModLoader;

public partial class ChestLoot {
	private static readonly Item defaultInstance = default;

	private readonly List<Entry> items;
	private string name;

	public IReadOnlyList<Entry> Items => items;

	public Entry LastEntry => items[^1];

	public Entry this[int item] {
		get {
			int index = items.FindIndex(x => x.item.type.Equals(item));
			bool hasInNormal = index != -1;
			return items[index];
		}
	}

	public ChestLoot() {
		items = new();
	}

	public void RegisterShop(int npcId, string name = "Shop") {
		this.name = name;
		Main.TMLLootDB.RegisterNpcShop(npcId, this, name);
	}

	public ChestLoot Add(params Entry[] entries) {
		items.AddRange(entries);
		return this;
	}

	public ChestLoot Add(int item, params ICondition[] condition) {
		return Add(ContentSamples.ItemsByType[item], condition);
	}

	public ChestLoot Add(Item item, params ICondition[] condition) {
		return Add(new Entry(item, condition));
	}

	private ChestLoot InsertAt(Entry targetEntry, Item item, bool after, params ICondition[] condition) {
		var orderEntry = new Entry(item, condition);
		orderEntry.Target(targetEntry, after);
		return Add(orderEntry);
	}

	private ChestLoot InsertAt(int targetItem, Item item, bool after, params ICondition[] condition) {
		return InsertAt(this[targetItem], item, after, condition);
	}

	private ChestLoot InsertAt(int targetItem, int item, bool after, params ICondition[] condition) {
		return InsertAt(targetItem, ContentSamples.ItemsByType[item], after, condition);
	}

	public ChestLoot InsertBefore(int targetItem, int item, params ICondition[] condition) {
		return InsertAt(targetItem, item, false, condition);
	}

	public ChestLoot InsertBefore(int targetItem, Item item, params ICondition[] condition) {
		return InsertAt(targetItem, item, false, condition);
	}

	public ChestLoot InsertAfter(int targetItem, int item, params ICondition[] condition) {
		return InsertAt(targetItem, item, true, condition);
	}

	public ChestLoot InsertAfter(int targetItem, Item item, params ICondition[] condition) {
		return InsertAt(targetItem, item, true, condition);
	}

	public ChestLoot Hide(Entry entry) {
		entry.Hide();
		return this;
	}

	public ChestLoot Hide(int item) {
		return Hide(this[item]);
	}

	public Item[] Build(bool lastSlotEmpty = true) {
		return Build(out _, lastSlotEmpty);
	}

	public Item[] Build(out int slots, bool lastSlotEmpty = true) {
		List<Item> array = new();
		List<Entry> oldEntries = new(items);

		SortBeforeAfter(oldEntries, r => r.Ordering);
		foreach (Entry group in oldEntries) {
			group.Add(array);
		}

		slots = array.Count;
		int sOff = lastSlotEmpty.ToInt();
		int limit = 40 - sOff;
		if (array.Count > limit) {
			Main.NewText("Not all items could fit in the shop!", 255, 0, 0);
			Logging.tML.Warn($"Unable to fit all item in the shop {name}");
			slots = limit;
		}
		for (int i = array.Count; i < 40; i++) {
			array.Add(EmptyInstance);
		}
		array = array.Take(40).ToList();
		if (lastSlotEmpty)
			array[^1] = EmptyInstance;

		return array.ToArray();
	}
}