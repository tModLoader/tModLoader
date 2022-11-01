using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ID;

namespace Terraria.ModLoader
{
	public readonly struct ChestLoot {
		public struct Group {
			public readonly Item item;
			private Func<bool> condition;

			public Group(Item item, Func<bool> condition = null) {
				this.item = item;
				this.condition = condition;
			}

			public Func<bool> AddCondition(Func<bool> condition) {
				if (this.condition == null)
					return this.condition = condition;
				return this.condition = (Func<bool>)Delegate.Combine(this.condition, condition);
			}

			public bool GetCondition() {
				return condition?.Invoke() ?? true;
			}
		}

		private static readonly Item emptyInstance = new();

		private static object cache;

		private readonly List<Group> items;
		private readonly List<int> candidatesByType;
		private readonly List<Predicate<Item>> candidatesByMatch;

		public Group this[int item] { // TODO: ensure that you can modify item instance and add new conditions to it.
			get {
				cache = item;
				return items.Find(x => x.item.type.Equals(item));
			}
		}

		public Group this[Index index] {
			get {
				return items[index];
			}
		}

		public ChestLoot() {
			cache = null;
			items = new();
			candidatesByType = new();
			candidatesByMatch = new();
		}

		private static bool CheckInvalidness(int item) {
			return item < ItemID.None || item >= ItemLoader.ItemCount;
		}

		private static bool CheckInvalidness(Item item) {
			ArgumentNullException.ThrowIfNull(item, nameof(item));
			return CheckInvalidness(item.type);
		}

		public bool Has(int item) {
			if (item == 0)
				return false;

			cache = item;
			if (CheckInvalidness(item))
				return false;
			return items.FindIndex(x => x.item.Equals(cache)) != -1;
		}

		public bool Has(Item item) {
			return Has(item.type);
		}

		public bool Has(Predicate<Item> match) {
			cache = match;
			return items.FindIndex(x => ((Predicate<Item>)cache)(x.item)) != -1;
		}

		public bool Put(int item, Func<bool> condition = null) {
			return Put(ContentSamples.ItemsByType[item], condition);
		}

		public bool PutAt(int index, int item, Func<bool> condition = null) {
			return PutAt(index, ContentSamples.ItemsByType[item], condition);
		}

		public bool PutAt(Predicate<Item> match, int item, Func<bool> condition = null) {
			return PutAt(match, ContentSamples.ItemsByType[item], condition);
		}

		public bool Put(Item item, Func<bool> condition = null) {
			if (Has(item)) {
				return false;
			}

			items.Add(new(item, condition));
			return true;
		}

		public bool PutAt(int index, Item item, Func<bool> condition = null) {
			if (Has(item)) {
				return false;
			}

			items.Insert(index, new(item, condition));
			return true;
		}

		public bool PutAt(Predicate<Item> match, Item item, Func<bool> condition = null) {
			cache = match;
			int index = items.FindIndex(x => ((Predicate<Item>)cache)(x.item));
			if (CheckInvalidness(item) || index != -1) {
				return false;
			}

			items.Insert(index, new(item, condition));
			return true;
		}

		public bool Put(Predicate<Item> match, int item, Func<bool> condition = null) {
			return PutAt(match, item, condition);
		}

		public bool Put(Predicate<Item> match, Item item, Func<bool> condition = null) {
			return PutAt(match, item, condition);
		}

		public bool PutBefore(int destination, int item, Func<bool> condition = null) {
			return PutAt(destination, item, condition);
		}

		public bool PutBefore(int destination, Item item, Func<bool> condition = null) {
			return PutAt(destination, item, condition);
		}

		public bool PutAfter(int destination, int item, Func<bool> condition = null) {
			return PutAt(destination + 1, item, condition);
		}

		public bool PutAfter(int destination, Item item, Func<bool> condition = null) {
			return PutAt(destination + 1, item, condition);
		}

		public bool Remove(int item) {
			cache = item;
			if (CheckInvalidness(item) || items.FindIndex(group => group.item.type.Equals(cache)) == -1) {
				return false;
			}

			candidatesByType.Add(item);
			return true;
		}

		public bool Remove(Predicate<Item> match) {
			cache = match;
			if (items.FindIndex(group => ((Predicate<Item>)cache)(group.item)) == -1) {
				return false;
			}

			candidatesByMatch.Add(match);
			return true;
		}

		public Item[] Build(bool lastSlotEmpty = true) {
			List<Item> array = new();
			cache = candidatesByType;

			foreach (Group group in items) {
				if (group.GetCondition()) {
					array.Add(group.item);
				}
			}
			array.RemoveAll(x => ((List<int>)cache).Contains(x.type));
			foreach (Predicate<Item> match in candidatesByMatch) {
				array.RemoveAll(match);
			}
			if (array.Count < 40) {
				array.AddRange(Enumerable.Repeat(emptyInstance, 40 - array.Count));
			}
			array = array.Take(40).ToList();
			if (lastSlotEmpty)
				array[^1] = emptyInstance;

			return array.ToArray();
		}

		public Item[] Build(out int slots, bool lastSlotEmpty = true) {
			List<Item> array = new();
			cache = candidatesByType;

			foreach (Group group in items) {
				if (group.GetCondition()) {
					array.Add(group.item);
				}
			}
			array.RemoveAll(x => ((List<int>)cache).Contains(x.type));
			foreach (Predicate<Item> match in candidatesByMatch) {
				array.RemoveAll(match);
			}
			slots = array.Count;
			if (array.Count < 40) {
				array.AddRange(Enumerable.Repeat(emptyInstance, 40 - array.Count));
			}
			array = array.Take(40).ToList();
			if (lastSlotEmpty)
				array[^1] = emptyInstance;

			return array.ToArray();
		}
	}
}
