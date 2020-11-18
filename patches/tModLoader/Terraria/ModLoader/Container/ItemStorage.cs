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
	public class ItemStorage : IReadOnlyList<Item> {
		public enum Operation {
			Input,
			Output
		}

		protected Item[] items;

		internal int Length => items.Length;

		public int Count => items.Length;

		public Item this[int index] {
			get => items[index];
			protected set => items[index] = value;
		}

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

		private void ValidateSlotIndex(int slot) {
			if (slot < 0 || slot >= Length)
				throw new Exception($"Slot {slot} not in valid range - [0, {Length})");
		}

		/// <summary>
		/// Puts an item into the storage.
		/// </summary>
		/// <param name="slot">The slot.</param>
		/// <param name="item">The item. If the item cannot be fully inserted, then part of it will be, and this instance will have its stack reduced accordingly.</param>
		/// <param name="user">The object doing this.</param>
		/// <returns>True if the item was successfully inserted. False if the item is air, if the slot is already fully occupied, if the slot rejects the item, or if the slot rejects the user.</returns>
		public bool InsertItem(int slot, ref Item item, object? user) {
			if (item == null || item.IsAir) return false;

			ValidateSlotIndex(slot);

			if (!CanInteract(slot, Operation.Input, user) || !IsItemValid(slot, item)) return false;

			Item existing = items[slot];
			if (!existing.IsAir && !CanItemsStack(item, existing)) return false;

			int slotSize = GetSlotSize(slot);
			if (slotSize < 0)
				slotSize = item.maxStack;
			int toInsert = Utils.Min(slotSize, slotSize - existing.stack, item.stack);
			if (toInsert <= 0) 
				return false;

			bool reachedLimit = item.stack > toInsert;

			OnItemInsert?.Invoke(slot, item, user);

			if (existing.IsAir) items[slot] = reachedLimit ? CloneItemWithSize(item, toInsert) : item;
			else existing.stack += toInsert;

			item = reachedLimit ? CloneItemWithSize(item, item.stack - toInsert) : new Item();
			return true;
		}

		/// <summary>
		/// Puts an item into storage, disregarding what slots to put it in.
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
		/// Removes an item from storage and returns the item that was grabbed.
		/// </summary>
		/// <param name="slot">The slot.</param>
		/// <param name="item">The item that is . Returns null if unsuccessful.</param>
		/// <param name="amount">The amount of items to take from a stack.</param>
		/// <param name="user">The object doing this.</param>
		/// <returns>Returns true if any items were actually removed. False if the slot is air or if the slot rejects the user.</returns>
		public bool RemoveItem(int slot, object? user, out Item item, int amount = -1) {
			item = items[slot];

			if (amount == 0) return false;

			ValidateSlotIndex(slot);

			if (!CanInteract(slot, Operation.Output, user)) return false;

			if (item.IsAir) return false;

			OnItemRemove?.Invoke(slot, user);

			int toExtract = Utils.Min(amount < 0 ? int.MaxValue : amount, item.maxStack, item.stack);

			if (item.stack <= toExtract) {
				items[slot] = new Item();

				return true;
			}

			item = CloneItemWithSize(item, toExtract);
			items[slot] = CloneItemWithSize(item, item.stack - toExtract);

			return true;
		}

		/// <summary>
		/// Removes an item from storage.
		/// </summary>
		/// <param name="slot">The slot.</param>
		/// <param name="user">The object doing this.</param>
		/// <returns>Returns true if any items were actually removed.</returns>
		public bool RemoveItem(int slot, object? user) => RemoveItem(slot, user, out _);

		/// <summary>
		/// Swaps two items in a slot.
		/// </summary>
		/// <param name="slot">The slot.</param>
		/// <param name="user">The object doing this.</param>
		/// <param name="newStack">The item to insert.</param>
		/// <returns>True if the items were successfully swapped. False if the slot did not have enough stack size to be fully swapped, refused the new item, or refused the user.</returns>
		public bool SwapStacks(int slot, object? user, Item newStack) {
			ValidateSlotIndex(slot);

			if (!IsItemValid(slot, newStack))
				return false;

			int size = GetSlotSize(slot);
			if (size < 0)
				size = newStack.maxStack;
			if (newStack.stack > size) {
				return false;
			}

			OnItemRemove?.Invoke(slot, user);
			OnItemInsert?.Invoke(slot, newStack, user);
			items[slot] = newStack;

			return true;
		}

		/// <summary>
		/// Adds or subtracts to the item in the slot specified's stack.
		/// </summary>
		/// <param name="quantity">The amount to increase/decrease the item's stack.</param>
		/// <param name="user">The object doing this.</param>
		/// <returns>True if the item was successfully affected. False if the slot denies the user, if the item is air, or if the quantity is zero.</returns>
		public bool ModifyStackSize(int slot, int quantity, object? user) {
			Item item = items[slot];

			if (quantity > 0 && !CanInteract(slot, Operation.Input, user) || 
				quantity < 0 && !CanInteract(slot, Operation.Output, user) ||
				quantity == 0 || item.IsAir)
				return false;

			OnStackModify?.Invoke(slot, ref quantity, user);

			item.stack += Math.Min(quantity, MaxStackFor(slot, item));

			if (item.stack <= 0)
				RemoveItem(slot, user);

			return true;
		}

		/// <param name="quantity">This parameter is not clamped upon firing. After firing, it will be clamped between 0 and <see cref="MaxStackFor(int, Item)"/>.</param>
		public delegate void StackChangedDelegate(int slot, ref int quantity, object? user);
		public delegate void InsertItemDelegate(int slot, Item inserting, object? user);
		public delegate void RemoveItemDelegate(int slot, object? user);

		/// <summary>
		/// Fired just before the slot's item's stack is modified through <see cref="ModifyStackSize(int, int, object?)"/>.
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
		/// Gets the size of a given slot. Negatives indicate no size limit.
		/// </summary>
		public virtual int GetSlotSize(int slot) => -1;

		/// <summary>
		/// Gets if a given item is valid to be inserted into in a given slot.
		/// </summary>
		public virtual bool IsItemValid(int slot, Item item) => true;

		/// <summary>
		/// Gets if a given user can interact with a slot in the storage.
		/// </summary>
		/// <param name="operation">Whether the user is putting an item in or taking an item out.</param>
		public virtual bool CanInteract(int slot, Operation operation, object? user) => true;

		public int MaxStackFor(int slot, Item item) {
			int limit = GetSlotSize(slot);
			if (limit < 0)
				limit = item.maxStack;
			return limit;
		}

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