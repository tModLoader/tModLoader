using System.Collections.Generic;
using System.Linq;
using Terraria.ID;

namespace Terraria.ModLoader;

public sealed partial class NPCShop {
	private readonly List<Entry> items;
	private readonly int npcId;
	private readonly string name;

	public IReadOnlyList<Entry> Items => items;
	public int NpcID => npcId;
	public string Name => name;
	public string FullName => TMLLootDatabase.GetNPCShopName(NpcID, Name);

	public Entry this[int item] {
		get {
			int index = items.FindIndex(x => x.item.type.Equals(item));
			bool hasInNormal = index != -1;
			return items[index];
		}
	}

	public NPCShop(int npcId, string name = "Shop") {
		items = new();
		this.name = name;
		this.npcId = npcId;
	}

	public Entry GetEntry(int item)
	{
		return items[items.FindIndex(x => x.item.type.Equals(item))];
	}

	public bool TryGetEntry(int item, out Entry entry)
	{
		int i = items.FindIndex(x => x.item.type.Equals(item));
		if (i == -1) {
			entry = null;
			return false;
		}
		entry = items[i];
		return true;
	}

	public void Register() {
		TMLLootDatabase.RegisterNpcShop(npcId, this, name);
	}

	public NPCShop Add(params Entry[] entries) {
		items.AddRange(entries);
		return this;
	}

	public NPCShop Add(int item, params ICondition[] condition) {
		return Add(ContentSamples.ItemsByType[item], condition);
	}

	public NPCShop Add(Item item, params ICondition[] condition) {
		return Add(new Entry(item, condition));
	}

	private NPCShop InsertAt(Entry targetEntry, Item item, bool after, params ICondition[] condition) {
		var orderEntry = new Entry(item, condition);
		orderEntry.Target(targetEntry, after);
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

	public Item[] Build() {
		return Build(out _);
	}

	public Item[] Build(out int slots) {
		List<Item> array = new();
		List<Entry> oldEntries = new(items);

		SortBeforeAfter(oldEntries, r => r.Ordering);
		foreach (Entry group in oldEntries) {
			if (group.Disabled || !group.ConditionsMet()) {
				continue;
			}
			array.Add(group.Item);
		}

		slots = array.Count;
		int limit = 39;
		if (array.Count > limit) {
			Main.NewText("Not all items could fit in the shop!", 255, 0, 0);
			Logging.tML.Warn($"Unable to fit all item in the shop {name}");
			slots = limit;
		}
		for (int i = array.Count; i < 40; i++) {
			array.Add(EmptyInstance);
		}
		array = array.Take(40).ToList();
		array[^1] = EmptyInstance;

		return array.ToArray();
	}
}