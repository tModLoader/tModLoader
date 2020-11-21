#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Container
{
	public class ItemStorage : IReadOnlyList<Item> {
		[Flags]
		public enum Operation {
			/// <summary>
			/// Used for insertion.
			/// </summary>
			Input = 1,
			/// <summary>
			/// Used for removal.
			/// </summary>
			Output = 2,
			/// <summary>
			/// Used for insertion and removal, or swapping.
			/// </summary>
			Both = 1 | 2
		}

		protected Item[] Items;

		public int Count => Items.Length;

		public Item this[int index] => Items[index];

		public ItemStorage(int size) {
			Items = new Item[size];
			for (int i = 0; i < size; i++)
				Items[i] = new Item();
		}

		public ItemStorage(IEnumerable<Item> items) {
			Items = items.ToArray();
		}

		public ItemStorage Clone() {
			ItemStorage storage = (ItemStorage)MemberwiseClone();
			storage.Items = Items.Select(item => item.Clone()).ToArray();
			return storage;
		}

		protected void ValidateSlotIndex(int slot) {
			if (slot < 0 || slot >= Count)
				throw new Exception($"Slot {slot} not in valid range - [0, {Count - 1}]");
		}

		/// <summary>
		/// Gets if this item can be inserted completely into the storage.
		/// </summary>
		public bool CanInsertItem(object? user, Item item) {
			if (item is null || item.IsAir) {
				return false;
			}
			item = item.Clone();
			for (int i = 0; i < Count; i++) {
				if (CanItemStackPartially(i, item, out int leftOver) && IsInsertValid(user, i, item)) {
					item.stack = leftOver;
					if (item.stack <= 0) {
						return true;
					}
				}
			}
			return false;
		}

		/// <summary>
		/// Gets if this item can be inserted, even partially, into the storage.
		/// </summary>
		public bool CanInsertItemPartially(object? user, Item item) {
			if (item is null || item.IsAir) {
				return false;
			}
			for (int i = 0; i < Count; i++) {
				if (CanItemStackPartially(i, item, out _) && IsInsertValid(user, i, item)) {
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Simulates putting an item into storage and returns if it is feasible.
		/// </summary>
		/// <param name="leftOver">		
		/// Positive values represent how many items could not fit.
		/// Zero represents a perfect fit.
		/// Negative values represent how many extra items could fit.</param>
		/// <returns>
		/// True if the item was successfully inserted, even partially. False if the item is air, if the slot is already
		/// fully occupied, if the slot rejects the item, or if the slot rejects the user.</returns>
		public bool CanInsertItem(object? user, int slot, Item item, out int leftOver) {
			leftOver = 0;
			if (item == null || item.IsAir)
				return false;

			ValidateSlotIndex(slot);

			return CanItemStackPartially(slot, item, out leftOver) && IsInsertValid(user, slot, item);
		}

		/// <summary>
		/// Puts an item into the storage.
		/// </summary>
		/// <param name="user">The object doing this.</param>
		/// <param name="slot">The slot.</param>
		/// <param name="item">The item.</param>
		/// <returns>
		/// <see cref="CanInsertItem(object?, int, Item, out int)"/>
		/// </returns>
		public bool InsertItem(object? user, int slot, ref Item item) {
			if (!CanInsertItem(user, slot, item, out int leftOver))
				return false;

			var existing = Items[slot];

			bool reachedLimit = leftOver > 0;
			int toInsert = reachedLimit ? item.stack - leftOver : item.stack;

			// OnItemInsert?.Invoke(user, slot, item);

			if (existing.IsAir)
				Items[slot] = reachedLimit ? CloneItemWithSize(item, toInsert) : item;
			else
				existing.stack += toInsert;

			item = reachedLimit ? CloneItemWithSize(item, item.stack - toInsert) : new Item();
			return true;
		}

		/// <summary>
		/// Puts an item into storage, disregarding what slots to put it in.
		/// </summary>
		/// <param name="user">The object doing this.</param>
		/// <param name="item">The item to insert.</param>
		/// <returns>True if the item was successfully inserted, even partially. False if the item is air, if the slot is already fully occupied, if the slot rejects the item, or if the slot rejects the user.</returns>
		public bool InsertItem(object? user, ref Item item) => InsertItemStartingFrom(user, 0, Count, ref item);

		/// <summary>
		/// Puts an item into storage, starting from a slot and inserting iteratively from there.
		/// </summary>
		/// <param name="user">The object doing this.</param>
		/// <param name="item">The item to insert.</param>
		/// <param name="slot">The slot to start from.</param>
		/// <param name="length">How many slots to traverse before stopping. Starts at 1.</param>
		/// <returns>True if the item was successfully inserted, even partially. False if the item is air, if the slot is already fully occupied, if the slot rejects the item, or if the slot rejects the user.</returns>
		public bool InsertItemStartingFrom(object? user, int slot, int length, ref Item item) {
			if (item is null || item.IsAir) {
				return false;
			}

			bool ret = false;
			int end = slot + length;
			for (int i = slot; i < end; i++) {
				if (!Items[i].IsAir) {
					ret |= InsertItem(user, i, ref item);
				}
				if (item.IsAir) {
					return true;
				}
			}
			for (int i = slot; i < end; i++) {
				if (Items[i].IsAir) {
					ret |= InsertItem(user, i, ref item);
				}
				if (item.IsAir) {
					return true;
				}
			}

			return ret;
		}

		/// <summary>
		/// Simulates removing an item from storage and returns if it is feasible.
		/// </summary>
		/// <returns>True if any items can be removed; false if the slot item is air, if the slot rejects the interaction, or if the removal is invalid.</returns>
		public bool CanRemoveItem(object? user, int slot, int amount) {
			ValidateSlotIndex(slot);

			return !Items[slot].IsAir && IsRemoveValid(user, slot, amount);
		}

		/// <summary>
		/// Removes an item from storage.
		/// </summary>
		/// <param name="user">The object doing this.</param>
		/// <param name="slot">The slot.</param>
		/// <returns><see cref="CanRemoveItem(object?, int, int)"/></returns>
		public bool RemoveItem(object? user, int slot) => RemoveItem(user, slot, out _);

		/// <summary>
		/// Removes an item from storage and returns the item that was grabbed.
		/// <para />
		/// Compare the stack of the <paramref name="item" /> parameter with the <paramref name="amount" /> parameter to see if
		/// the item was completely taken.
		/// </summary>
		/// <param name="slot">The slot.</param>
		/// <param name="item">The item that is . Returns null if unsuccessful.</param>
		/// <param name="amount">The amount of items to take from a stack.</param>
		/// <param name="user">The object doing this.</param>
		/// <returns><see cref="CanRemoveItem(object?, int)"/></returns>
		public bool RemoveItem(object? user, int slot, out Item item, int amount = -1) {
			item = Items[slot];

			if (amount == 0 || !CanRemoveItem(user, slot, amount))
				return false;

			// OnItemRemove?.Invoke(user, slot);

			int toExtract = Utils.Min(amount < 0 ? int.MaxValue : amount, item.maxStack, item.stack);

			if (item.stack <= toExtract) {
				Items[slot] = new Item();
			}
			else {
				item = CloneItemWithSize(item, toExtract);
				Items[slot] = CloneItemWithSize(item, item.stack - toExtract);
			}
			return true;
		}

		/// <summary>
		/// Simulates two items swapping, and returns if it is feasible.
		/// </summary>
		/// <returns>True if the items were successfully swapped. False if the slot did not have enough stack size to be fully
		/// swapped, refused the new item, or refused the user.</returns>
		public bool CanSwap(object? user, int slot, Item newStack) {
			ValidateSlotIndex(slot);

			if (!IsInsertValid(user, slot, newStack) || !IsRemoveValid(user, slot, Items[slot].stack))
				return false;

			if (newStack.stack > MaxStackFor(slot, newStack))
				return false;

			return true;
		}

		/// <summary>
		/// Swaps two items in a slot.
		/// </summary>
		/// <param name="user">The object doing this.</param>
		/// <param name="slot">The slot.</param>
		/// <param name="newStack">The item to insert.</param>
		/// <returns><see cref="CanRemoveItem(object?, int, int)"/></returns>
		public bool SwapStacks(object? user, int slot, ref Item newStack) {
			if (!CanSwap(user, slot, newStack))
				return false;

			// OnItemRemove?.Invoke(user, slot);
			// OnItemInsert?.Invoke(user, slot, newStack);

			Utils.Swap(ref Items[slot], ref newStack);

			return true;
		}

		// public delegate void InsertItemDelegate(object? user, int slot, Item inserting);
		// public delegate void RemoveItemDelegate(object? user, int slot);

		/// <summary>
		/// Fired just before the slot's item's stack is modified through <see cref="ModifyStackSize(object?, int, int)"/>.
		/// <para/> The quantity parameter is not clamped upon firing. After firing, it will be clamped between 0 and <see cref="MaxStackFor(int, Item)"/>
		/// </summary>
		// public event StackChangedDelegate? OnStackModify;
		/// <summary>
		/// Fired just before an item is inserted into storage.
		/// </summary>
		// public event InsertItemDelegate? OnItemInsert;
		/// <summary>
		/// Fired just before an item is removed from storage.
		/// </summary>
		// public event RemoveItemDelegate? OnItemRemove;

		/// <summary>
		/// Gets the size of a given slot and item. Negative values indicate no stack limit. The default is to use
		/// <see cref="Item.maxStack" />.
		/// </summary>
		/// <param name="item">An item to be tried against the slot.</param>
		public virtual int GetSlotSize(int slot, Item item) => item.maxStack;

		/// <summary>
		/// Gets if a given user can insert an item into a slot in the storage.
		/// </summary>
		/// <param name="inserting">The item that's being inserted.</param>
		public virtual bool IsInsertValid(object? user, int slot, Item inserting) => true;

		/// <summary>
		/// Gets if a given user can remove an item from a slot in the storage.
		/// </summary>
		public virtual bool IsRemoveValid(object? user, int slot, int amount) => true;

		public int MaxStackFor(int slot, Item item) {
			int limit = GetSlotSize(slot, item);
			if (limit < 0)
				limit = int.MaxValue;
			return limit;
		}

		#region IO
		public virtual TagCompound Save() => new TagCompound { ["Items"] = Items.ToList() };

		public virtual void Load(TagCompound tag) => Items = tag.GetList<Item>("Items").ToArray();

		public virtual void Write(BinaryWriter writer) {
			writer.Write(Count);

			for (int i = 0; i < Count; i++) {
				ItemIO.Send(Items[i], writer, true, true);
			}
		}

		public virtual void Read(BinaryReader reader) {
			int size = reader.ReadInt32();

			Items = new Item[size];

			for (int i = 0; i < Count; i++) {
				Items[i] = ItemIO.Receive(reader, true, true);
			}
		}
		#endregion

		/// <summary>
		/// Gets if the <paramref name="operand"/> can fit in the slot completely.
		/// </summary>
		public bool CanItemStack(int slot, Item operand) => CanItemStackPartially(slot, operand, out int leftOver) && leftOver <= 0;

		/// <summary>
		/// Gets if the <paramref name="operand"/> can fit in the slot partially.
		/// </summary>
		/// <param name="leftOver">The amount of items left over after simulated stacking. <para/>
		/// Positive values represent how many items could not fit.
		/// Zero represents a perfect fit.
		/// Negative values represent how many extra items could fit.
		/// </param>
		public bool CanItemStackPartially(int slot, Item operand, out int leftOver) {
			leftOver = 0;
			if (operand is null || operand.IsAir) {
				return false;
			}
			leftOver = operand.stack;
			int size = MaxStackFor(slot, operand);
			if (size == 0) {
				return false;
			}
			if (Items[slot].IsAir) {
				leftOver = operand.stack - size;
			}
			else {
				if (!CanItemsStack(Items[slot], operand)) {
					return false;
				}
				leftOver = Items[slot].stack + operand.stack - size;
			}
			return true;
		}

		private static bool CanItemsStack(Item a, Item b) => a.IsTheSameAs(b);


		private static Item CloneItemWithSize(Item itemStack, int size) {
			if (size == 0)
				return new Item();
			Item copy = itemStack.Clone();
			copy.stack = size;
			return copy;
		}

		public IEnumerator<Item> GetEnumerator() => Items.AsEnumerable().GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public override string ToString() => $"{GetType()} with {Count} slots";
	}
}