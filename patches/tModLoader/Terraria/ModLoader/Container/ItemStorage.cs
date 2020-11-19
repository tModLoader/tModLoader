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
		/// Puts an item into the storage.
		/// </summary>
		/// <param name="user">The object doing this.</param>
		/// <param name="slot">The slot.</param>
		/// <param name="item">The item.</param>
		/// <returns>True if the item was successfully inserted, even partially. False if the item is air, if the slot is already fully occupied, if the slot rejects the item, or if the slot rejects the user.</returns>
		public bool InsertItem(object? user, int slot, ref Item item) {
			if (item == null || item.IsAir)
				return false;

			ValidateSlotIndex(slot);

			if (!CanInteract(slot, Operation.Input, user) || !IsItemValid(slot, item))
				return false;

			Item existing = Items[slot];
			if (!existing.IsAir && !CanItemsStack(item, existing))
				return false;

			int slotSize = GetSlotSize(slot, item);
			if (slotSize < 0)
				slotSize = item.maxStack;
			int toInsert = Utils.Min(slotSize, slotSize - existing.stack, item.stack);
			if (toInsert <= 0)
				return false;

			bool reachedLimit = item.stack > toInsert;

			OnItemInsert?.Invoke(user, slot, item);

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
		public bool InsertItem(object? user, ref Item item) {
			if (item is null || item.IsAir) {
				return false;
			}

			bool ret = false;
			for (int i = 0; i < Count; i++) {
				Item other = Items[i];
				if (CanItemsStack(item, other) && other.stack < other.maxStack) {
					ret |= InsertItem(user, i, ref item);
				}
			}
			for (int i = 0; i < Count; i++) {
				Item other = Items[i];
				if (other.IsAir) {
					ret |= InsertItem(user, i, ref item);
				}
			}
			return ret;
		}

		/// <summary>
		/// Removes an item from storage and returns the item that was grabbed. 
		/// <para/> Compare the stack of the <paramref name="item"/> parameter with the <paramref name="amount"/> parameter to see if the item was completely taken.
		/// </summary>
		/// <param name="slot">The slot.</param>
		/// <param name="item">The item that is . Returns null if unsuccessful.</param>
		/// <param name="amount">The amount of items to take from a stack.</param>
		/// <param name="user">The object doing this.</param>
		/// <returns>Returns true if any items were actually removed. False if the slot is air or if the slot rejects the user.</returns>
		public bool RemoveItem(object? user, int slot, out Item item, int amount = -1) {
			item = Items[slot];

			if (amount == 0)
				return false;

			ValidateSlotIndex(slot);

			if (!CanInteract(slot, Operation.Output, user))
				return false;

			if (item.IsAir)
				return false;

			OnItemRemove?.Invoke(user, slot);

			int toExtract = Utils.Min(amount < 0 ? int.MaxValue : amount, item.maxStack, item.stack);

			if (item.stack <= toExtract) {
				Items[slot] = new Item();

				return true;
			}

			item = CloneItemWithSize(item, toExtract);
			Items[slot] = CloneItemWithSize(item, item.stack - toExtract);

			return true;
		}

		/// <summary>
		/// Removes an item from storage.
		/// </summary>
		/// <param name="user">The object doing this.</param>
		/// <param name="slot">The slot.</param>
		/// <returns>Returns true if any items were actually removed.</returns>
		public bool RemoveItem(object? user, int slot) => RemoveItem(user, slot, out _);

		/// <summary>
		/// Swaps two items in a slot.
		/// </summary>
		/// <param name="user">The object doing this.</param>
		/// <param name="slot">The slot.</param>
		/// <param name="newStack">The item to insert.</param>
		/// <returns>True if the items were successfully swapped. False if the slot did not have enough stack size to be fully swapped, refused the new item, or refused the user.</returns>
		public bool SwapStacks(object? user, int slot, Item newStack) {
			ValidateSlotIndex(slot);

			if (!IsItemValid(slot, newStack))
				return false;

			int size = GetSlotSize(slot, newStack);
			if (size < 0)
				size = newStack.maxStack;
			if (newStack.stack > size) {
				return false;
			}

			OnItemRemove?.Invoke(user, slot);
			OnItemInsert?.Invoke(user, slot, newStack);
			Items[slot] = newStack;

			return true;
		}

		/// <summary>
		/// Adds or subtracts to the item in the slot specified's stack.
		/// </summary>
		/// <param name="quantity">The amount to increase/decrease the item's stack.</param>
		/// <param name="user">The object doing this.</param>
		/// <returns>True if the item was successfully affected. False if the slot denies the user, if the item is air, or if the quantity is zero.</returns>
		public bool ModifyStackSize(object? user, int slot, int quantity) {
			Item item = Items[slot];

			if (quantity > 0 && !CanInteract(slot, Operation.Input, user) ||
				quantity < 0 && !CanInteract(slot, Operation.Output, user) ||
				quantity == 0 || item.IsAir)
				return false;

			OnStackModify?.Invoke(user, slot, ref quantity);

			item.stack += Math.Min(quantity, MaxStackFor(slot, item));

			if (item.stack <= 0)
				RemoveItem(user, slot);

			return true;
		}

		/// <param name="quantity">This parameter is not clamped upon firing. After firing, it will be clamped between 0 and <see cref="MaxStackFor(int, Item)"/>.</param>
		public delegate void StackChangedDelegate(object? user, int slot, ref int quantity);
		public delegate void InsertItemDelegate(object? user, int slot, Item inserting);
		public delegate void RemoveItemDelegate(object? user, int slot);

		/// <summary>
		/// Fired just before the slot's item's stack is modified through <see cref="ModifyStackSize(object?, int, int)"/>.
		/// <para/> The quantity parameter is not clamped upon firing. After firing, it will be clamped between 0 and <see cref="MaxStackFor(int, Item)"/>
		/// </summary>
		public event StackChangedDelegate? OnStackModify;
		/// <summary>
		/// Fired just before an item is inserted into storage.
		/// </summary>
		public event InsertItemDelegate? OnItemInsert;
		/// <summary>
		/// Fired just before an item is removed from storage.
		/// </summary>
		public event RemoveItemDelegate? OnItemRemove;

		/// <summary>
		/// Gets the size of a given slot and item. Non-negatives indicate a value to override items' max stack. Negatives indicate to use to max stack size.
		/// </summary>
		/// <param name="item">An item to be tried against the slot.</param>
		public virtual int GetSlotSize(int slot, Item item) => -1;

		/// <summary>
		/// Gets if a given item is valid to be inserted into in a given slot.
		/// </summary>
		/// <param name="item">An item to be tried against the slot.</param>
		public virtual bool IsItemValid(int slot, Item item) => true;

		/// <summary>
		/// Gets if a given user can interact with a slot in the storage.
		/// </summary>
		/// <param name="operation">Whether the user is putting an item in or taking an item out.</param>
		public virtual bool CanInteract(int slot, Operation operation, object? user) => true;

		public int MaxStackFor(int slot, Item item) {
			int limit = GetSlotSize(slot, item);
			if (limit < 0)
				limit = item.maxStack;
			return limit;
		}

		#region IO
		public virtual TagCompound Save() {
			return new TagCompound { ["Items"] = Items.ToList() };
		}

		public virtual void Load(TagCompound tag) {
			Items = tag.GetList<Item>("Items").ToArray();
		}

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

		private static bool CanItemsStack(Item a, Item b) {
			// if (a.modItem != null && b.modItem != null) return a.modItem.CanStack(b.modItem);
			return a.IsTheSameAs(b);
		}

		private static Item CloneItemWithSize(Item itemStack, int size) {
			if (size == 0)
				return new Item();
			Item copy = itemStack.Clone();
			copy.stack = size;
			return copy;
		}

		public IEnumerator<Item> GetEnumerator() => Items.AsEnumerable().GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public override string ToString() {
			return $"{GetType()} with {Count} slots";
		}
	}
}