#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Container
{
	public class ItemStorage : IReadOnlyList<Item>
	{
		public enum Operation
		{
			Input,
			Output
		}

		internal Item[] items;

		internal int Length => items.Length;

		public int Count => items.Length;

		public Item this[int index] => items[index];

		public ItemStorage(int size = 1) {
			items = new Item[size];
			for (int i = 0; i < size; i++)
				items[i] = new Item();
		}

		public ItemStorage(Item[] items) {
			this.items = items;
		}

		public ItemStorage Clone() {
			ItemStorage storage = (ItemStorage)MemberwiseClone();
			storage.items = items.Select(item => item.Clone()).ToArray();
			return storage;
		}

		protected void ForceReplaceItem(int slot, Item stack, object? user) {
			ValidateSlotIndex(slot);
			items[slot] = stack;
			OnContentsChanged?.Invoke(slot, user);
		}

		/// <summary>
		/// Puts an item into the storage.
		/// </summary>
		/// <param name="slot">The slot to put into.</param>
		/// <param name="item">The item.</param>
		/// <param name="user">The object doing this.</param>
		/// <returns>True if the item was successfully inserted.</returns>
		public bool InsertItem(int slot, ref Item item, object? user) {
			if (item == null || item.IsAir) return false;

			ValidateSlotIndex(slot);

			if (!CanInteract(slot, Operation.Input, user) || !IsItemValid(slot, item)) return false;

			Item existing = items[slot];
			if (!existing.IsAir && !CanItemsStack(item, existing)) return false;

			int slotSize = GetSlotSize(slot) ?? item.maxStack;
			int toInsert = Utils.Min(slotSize, slotSize - existing.stack, item.stack);
			if (toInsert <= 0) return false;

			bool reachedLimit = item.stack > toInsert;

			if (existing.IsAir) items[slot] = reachedLimit ? CloneItemWithSize(item, toInsert) : item;
			else existing.stack += toInsert;

			OnContentsChanged?.Invoke(slot, user);

			item = reachedLimit ? CloneItemWithSize(item, item.stack - toInsert) : new Item();
			return true;
		}

		/// <summary>
		/// Puts an item into storage, without caring about what slots to put it in.
		/// </summary>
		/// <param name="item">The item to put in.</param>
		/// <param name="user">The object doing this.</param>
		public void InsertItem(ref Item item, object? user) {
			for (int i = 0; i < Length; i++) {
				Item other = items[i];
				if (CanItemsStack(item, other) && other.stack < other.maxStack) {
					InsertItem(i, ref item, user);
					if (item.IsAir || !item.active) return;
				}
			}

			for (int i = 0; i < Length; i++) {
				InsertItem(i, ref item, user);
				if (item.IsAir) return;
			}
		}

		/// <summary>
		/// Removes an item from the storage and returns it.
		/// </summary>
		/// <param name="slot">The slot.</param>
		/// <param name="item">The item. Returns null if unsuccessful.</param>
		/// <param name="amount">The amount of items to take from a stack.</param>
		/// <param name="user">The object doing this.</param>
		/// <returns>Returns true if any items were actually removed.</returns>
		public bool RemoveItem(int slot, object? user, out Item? item, int amount = -1) {
			item = null;

			if (amount == 0) return false;

			ValidateSlotIndex(slot);

			if (!CanInteract(slot, Operation.Output, user)) return false;

			Item existing = items[slot];
			if (existing.IsAir) return false;

			int toExtract = Utils.Min(amount < 0 ? int.MaxValue : amount, existing.maxStack, existing.stack);

			if (existing.stack <= toExtract) {
				item = existing;
				items[slot] = new Item();

				OnContentsChanged?.Invoke(slot, user);

				return true;
			}

			item = CloneItemWithSize(existing, toExtract);
			items[slot] = CloneItemWithSize(existing, existing.stack - toExtract);

			OnContentsChanged?.Invoke(slot, user);

			return true;
		}

		public bool RemoveItem(int slot, object? user) => RemoveItem(slot, user, out _);

		private void ValidateSlotIndex(int slot) {
			if (slot < 0 || slot >= Length) throw new Exception($"Slot {slot} not in valid range - [0, {Length})");
		}

		public bool Grow(int slot, int quantity, bool user = false) {
			if (!CanInteract(slot, Operation.Input, user)) return false;

			ref Item item = ref items[slot];

			int limit = GetSlotSize(slot) ?? item.maxStack;
			if (item.IsAir || quantity <= 0 || item.stack + quantity > limit) return false;

			item.stack += quantity;
			OnContentsChanged?.Invoke(slot, user);

			return true;
		}

		public bool Shrink(int slot, int quantity, object? user) {
			if (!CanInteract(slot, Operation.Output, user)) return false;

			ref Item item = ref items[slot];

			if (item.IsAir || quantity <= 0 || item.stack - quantity < 0) return false;

			item.stack -= quantity;
			if (item.stack <= 0) item.TurnToAir();
			OnContentsChanged?.Invoke(slot, user);

			return true;
		}

		public delegate void ContentsChangedDelegate(int slot, object? user);
		public event ContentsChangedDelegate? OnContentsChanged;

		public virtual int? GetSlotSize(int slot) => null;

		public virtual bool IsItemValid(int slot, Item item) => true;

		public virtual bool CanInteract(int slot, Operation operation, object? user) => true;

		#region IO
		public virtual TagCompound Save() {
			return new TagCompound { ["Items"] = items.ToList() };
		}

		public virtual void Load(TagCompound tag) {
			items = tag.GetList<Item>("Items").ToArray();
		}

		public virtual void Write(BinaryWriter writer) {
			writer.Write(Length);

			for (int i = 0; i < Length; i++) {
				ItemIO.Send(items[i], writer, true, true);
			}
		}

		public virtual void Read(BinaryReader reader) {
			int size = reader.ReadInt32();

			items = new Item[size];

			for (int i = 0; i < Length; i++) {
				items[i] = ItemIO.Receive(reader, true, true);
			}
		}
		#endregion

		private static bool CanItemsStack(Item a, Item b) {
			// if (a.modItem != null && b.modItem != null) return a.modItem.CanStack(b.modItem);
			return a.IsTheSameAs(b);
		}

		private static Item CloneItemWithSize(Item itemStack, int size) {
			if (size == 0) return new Item();
			Item copy = itemStack.Clone();
			copy.stack = size;
			return copy;
		}

		public IEnumerator<Item> GetEnumerator() => items.AsEnumerable().GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}