using System.Collections.Generic;
using System.Linq;
using Terraria.ID;

namespace Terraria.ModLoader;

public sealed partial class NPCShop {
	internal readonly List<Entry> items;
	private readonly int npcId;
	private readonly string name;

	public IReadOnlyList<Entry> Items => items;
	public int NpcType => npcId;
	public string Name => name;
	public string FullName => NPCShopDatabase.GetNPCShopName(NpcType, Name);

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
		NPCShopDatabase.RegisterNpcShop(npcId, this, name);
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

	/// <summary>
	/// 
	/// </summary>
	/// <param name="itemArray">Array to be filled.</param>
	/// <param name="overflow">Equals to true if amount of added items is greater than 39.</param>
	public void Build(Item[] itemArray, out bool overflow) {
		List<Item> newItems = new();
		List<Entry> oldEntries = new(items);

		overflow = false;
		foreach (Entry group in oldEntries) {
			if (group.Disabled || !group.ConditionsMet()) {
				continue;
			}

			newItems.Add(group.Item);
			if (newItems.Count < 40) {
				continue;
			}
			newItems[^1] = new();
			overflow = true;
			break;
		}

		newItems.CopyTo(itemArray);
	}
}