using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Enums;
using Terraria.GameContent.Events;
using Terraria.ID;
using Terraria.Localization;

namespace Terraria.ModLoader;

public partial class ChestLoot {
	private static readonly Item defaultInstance = default;

	private readonly List<Entry> items;
	private string name;

	private readonly List<(int nextTo, bool after)> putCandidates;
	private readonly List<Entry> putCandidates2; // list that contains all entries those going to get from putCandidates

	public IReadOnlyList<Entry> Items {
		get {
			List<Entry> entries = items;
			return entries;
		}
	}

	public Entry this[int item] {
		get {
			int index = items.FindIndex(x => x.item.type.Equals(item));
			bool hasInNormal = index != -1;
			if (hasInNormal)
				return items[index];

			index = putCandidates2.FindIndex(x => x.item.type.Equals(item));
			return putCandidates2[index];
		}
	}

	public Entry this[Index index] {
		get {
			var ind2 = items.ElementAtOrDefault(index);
			bool hasInNormal = ind2.item != defaultInstance;
			if (hasInNormal)
				return ind2;

			return putCandidates2.ElementAtOrDefault(index);
		}
	}

	public ChestLoot() {
		items = new();
		putCandidates = new();
		putCandidates2 = new();
	}

	public void RegisterShop(int npcId, string name = "Shop") {
		this.name = name;
		Main.TMLLootDB.RegisterNpcShop(npcId, this, name);
	}

	private void AddCandidates(List<Entry> entries) {
		List<(int nextTo, bool after)> candidates = putCandidates;
		List<Entry> candidates2 = putCandidates2;
		candidates.Reverse();
		candidates2.Reverse();

		var a = candidates;
		for (int i = 0; i < a.Count; i++) {
			(int nextTo, bool after) = a[i];
			int index = entries.FindIndex(x => x.item.type.Equals(nextTo));
			if (index != -1) {
				entries.Insert(index + after.ToInt(), candidates2[i]);
			}
		}
	}

	public ChestLoot AddRange(params Entry[] entries) {
		items.AddRange(entries);
		return this;
	}

	public ChestLoot Add(Entry entry) {
		items.Add(entry);
		return this;
	}

	public ChestLoot Add(int item, params ICondition[] condition) {
		return Add(ContentSamples.ItemsByType[item], condition);
	}

	public ChestLoot Add(Item item, params ICondition[] condition) {
		return Add(new Entry(item, condition));
	}

	private ChestLoot PutAt(int destination, Item item, bool after, params ICondition[] condition) {
		putCandidates.Add(new(destination, after));
		putCandidates2.Add(new(item, condition));
		return this;
	}

	private ChestLoot PutAt(int targetItem, int item, bool after, params ICondition[] condition) {
		return PutAt(targetItem, ContentSamples.ItemsByType[item], after, condition);
	}

	public ChestLoot InsertBefore(int targetItem, int item, params ICondition[] condition) {
		return PutAt(targetItem, item, false, condition);
	}

	public ChestLoot InsertBefore(int targetItem, Item item, params ICondition[] condition) {
		return PutAt(targetItem, item, false, condition);
	}

	public ChestLoot InsertAfter(int targetItem, int item, params ICondition[] condition) {
		return PutAt(targetItem, item, true, condition);
	}

	public ChestLoot InsertAfter(int targetItem, Item item, params ICondition[] condition) {
		return PutAt(targetItem, item, true, condition);
	}

	public ChestLoot Hide(int item) {
		this[item].Hide();
		return this;
	}

	public Item[] Build(bool lastSlotEmpty = true) {
		return Build(out _, lastSlotEmpty);
	}

	public Item[] Build(out int slots, bool lastSlotEmpty = true) {
		List<Item> array = new();
		List<Entry> oldEntries = new List<Entry>();
		oldEntries.AddRange(items); // incase current instance still gets used after building for some reason.

		AddCandidates(oldEntries);
		foreach (Entry group in oldEntries) {
			group.TryAdd(array);
		}

		slots = array.Count;
		if (array.Count > 39) {
			Main.NewText("Not all items could fit in the shop!", 255, 0, 0);
			Logging.tML.Warn($"Unable to fit all item in the shop {name}");
			slots = 39;
		}
		if (array.Count < 40) {
			for (int i = array.Count; i < 40; i++) {
				array.Add(EmptyInstance);
			}
		}
		array = array.Take(40).ToList();
		if (lastSlotEmpty)
			array[^1] = EmptyInstance;

		return array.ToArray();
	}
}