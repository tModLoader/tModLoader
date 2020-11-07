using System;
using System.IO;
using System.Linq;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Container
{
	public class ItemHandler
	{
		public enum Operation
		{
			Input,
			Output
		}

		public Item[] Items { get; private set; }

		public int Slots => Items.Length;

		public ItemHandler(int size = 1) {
			SetSize(size);
		}

		private ItemHandler(Item[] items) {
			Items = items;
		}

		private void SetSize(int size) {
			Items = new Item[size];

			for (int i = 0; i < size; i++) Items[i] = new Item();
		}

		public ItemHandler Clone() {
			return new ItemHandler(Items.Select(x => x.Clone()).ToArray());
		}

		public void SetItemInSlot(int slot, Item stack, bool user = false) {
			ValidateSlotIndex(slot);
			Items[slot] = stack;
			OnContentsChanged(slot, user);
		}

		public ref Item GetItemInSlot(int slot) {
			ValidateSlotIndex(slot);
			return ref Items[slot];
		}

		/// <summary>
		/// </summary>
		/// <param name="slot"></param>
		/// <param name="item"></param>
		/// <param name="user"></param>
		/// <returns></returns>
		public bool InsertItem(int slot, ref Item item, bool user = false) {
			if (item == null || item.IsAir) return false;

			ValidateSlotIndex(slot);

			if (!CanInteract(slot, Operation.Input, user) || !IsItemValid(slot, item)) return false;

			Item existing = Items[slot];
			if (!existing.IsAir && !CanItemsStack(item, existing)) return false;

			int slotSize = GetSlotSize(slot) ?? item.maxStack;
			int toInsert = Utils.Min(slotSize, slotSize - existing.stack, item.stack);
			if (toInsert <= 0) return false;

			bool reachedLimit = item.stack > toInsert;

			if (existing.IsAir) Items[slot] = reachedLimit ? CloneItemWithSize(item, toInsert) : item;
			else existing.stack += toInsert;

			OnContentsChanged(slot, user);

			item = reachedLimit ? CloneItemWithSize(item, item.stack - toInsert) : new Item();
			return true;
		}

		public void InsertItem(ref Item item, bool user = false) {
			for (int i = 0; i < Slots; i++) {
				Item other = Items[i];
				if (CanItemsStack(item, other) && other.stack < other.maxStack) {
					InsertItem(i, ref item, user);
					if (item.IsAir || !item.active) return;
				}
			}

			for (int i = 0; i < Slots; i++) {
				InsertItem(i, ref item, user);
				if (item.IsAir) return;
			}
		}

		/// <summary>
		///     Extracts an item from the handler
		/// </summary>
		/// <param name="slot">The slot.</param>
		/// <param name="item">The item. Returns null if unsuccessful</param>
		/// <param name="amount">The amount.</param>
		/// <param name="user">Pass true if interaction was caused by user</param>
		/// <returns>Returns true if successful, otherwise returns false</returns>
		public bool ExtractItem(int slot, out Item item, int? amount = null, bool user = false) {
			item = null;

			if (amount == null || amount <= 0) return false;

			ValidateSlotIndex(slot);

			if (!CanInteract(slot, Operation.Output, user)) return false;

			Item existing = Items[slot];
			if (existing.IsAir) return false;

			int toExtract = Utils.Min(amount.Value, existing.maxStack, existing.stack);

			if (existing.stack <= toExtract) {
				item = existing;
				Items[slot] = new Item();

				OnContentsChanged(slot, user);

				return true;
			}

			item = CloneItemWithSize(existing, toExtract);
			Items[slot] = CloneItemWithSize(existing, existing.stack - toExtract);

			OnContentsChanged(slot, user);

			return true;
		}

		private void ValidateSlotIndex(int slot) {
			if (slot < 0 || slot >= Slots) throw new Exception($"Slot {slot} not in valid range - [0, {Slots})");
		}

		public bool Grow(int slot, int quantity, bool user = false) {
			// if (CanInteract?.Invoke(slot, Operation.Input, user) == false) return false;

			ref Item item = ref GetItemInSlot(slot);

			int limit = GetSlotSize(slot) ?? item.maxStack;
			if (item.IsAir || quantity <= 0 || item.stack + quantity > limit) return false;

			item.stack += quantity;
			OnContentsChanged(slot, user);

			return true;
		}

		public bool Shrink(int slot, int quantity, bool user = false) {
			// if (CanInteract?.Invoke(slot, Operation.Output, user) == false) return false;

			ref Item item = ref GetItemInSlot(slot);

			if (item.IsAir || quantity <= 0 || item.stack - quantity < 0) return false;

			item.stack -= quantity;
			if (item.stack <= 0) item.TurnToAir();
			OnContentsChanged(slot, user);

			return true;
		}

		public virtual void OnContentsChanged(int slot, bool user) {
		}

		public virtual int? GetSlotSize(int slot) {
			return null;
		}

		public virtual bool IsItemValid(int slot, Item item) {
			return true;
		}

		public virtual bool CanInteract(int slot, Operation operation, bool user) {
			return true;
		}

		#region IO
		public TagCompound Save() {
			return new TagCompound { ["Items"] = Items.ToList() };
		}

		public void Load(TagCompound tag) {
			Items = tag.GetList<Item>("Items").ToArray();
		}

		public void Write(BinaryWriter writer) {
			writer.Write(Slots);

			for (int i = 0; i < Slots; i++) {
				ItemIO.Send(Items[i], writer, true, true);
			}
		}

		public void Read(BinaryReader reader) {
			int size = reader.ReadInt32();
			SetSize(size);

			for (int i = 0; i < Slots; i++) {
				Items[i] = ItemIO.Receive(reader, true, true);
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
	}
}