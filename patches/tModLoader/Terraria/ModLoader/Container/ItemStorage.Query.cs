#nullable enable

using System.Linq;

namespace Terraria.ModLoader.Container
{
	public partial class ItemStorage
	{
		public bool Contains(int type) => Items.Any(item => !item.IsAir && item.type == type);

		public bool Contains(Item item) => Items.Any(item.IsTheSameAs);

		/// <summary>
		/// Gets if the <paramref name="item" /> can fit in the slot partially.
		/// </summary>
		/// <param name="leftOver">
		/// The amount of items left over after simulated stacking.
		/// <para />
		/// Positive values represent how many items could not fit.
		/// Zero represents a perfect fit.
		/// Negative values represent how many extra items could fit.
		/// </param>
		public bool CanInsertItemPartially(object? user, int slot, Item item, out int leftOver) {
			ValidateSlotIndex(slot);

			leftOver = 0;
			if (item is null || item.IsAir) {
				return false;
			}

			if (!CanInteract(slot, Operation.Insert, user) || !IsItemValid(slot, item)) {
				return false;
			}

			leftOver = item.stack;
			int size = MaxStackFor(slot, item);
			if (size <= 0) {
				return false;
			}

			Item storageItem = Items[slot];
			if (storageItem.IsAir) {
				leftOver = item.stack - size;
			}
			else {
				if (!storageItem.IsTheSameAs(item)) {
					return false;
				}

				leftOver = (storageItem.stack + item.stack) - size;
			}

			return true;
		}

		/// <summary>
		/// Simulates putting an item into storage and returns if it is feasible.
		/// </summary>
		/// <param name="leftOver">
		/// Positive values represent how many items could not fit.
		/// Zero represents a perfect fit.
		/// Negative values represent how many extra items could fit.
		/// </param>
		/// <returns>
		/// True if the item was successfully inserted, even partially. False if the item is air, if the slot is already
		/// fully occupied, if the slot rejects the item, or if the slot rejects the user.
		/// </returns>
		public bool CanInsertItem(object? user, int slot, Item item) {
			ValidateSlotIndex(slot);

			if (item == null || item.IsAir)
				return false;

			return CanInsertItemPartially(user, slot, item, out int leftOver) && leftOver <= 0;
		}

		/// <summary>
		/// Gets if this item can be inserted, even partially, into the storage.
		/// </summary>
		public bool CanInsertItemPartially(object? user, Item item, out int leftOver) {
			leftOver = 0;
			if (item is null || item.IsAir) {
				return false;
			}

			Item temp = item.Clone();
			for (int i = 0; i < Count; i++) {
				if (CanInsertItemPartially(user, i, temp, out int left)) {
					temp.stack = left;

					// Early return if can insert fully
					if (temp.stack <= 0) {
						return true;
					}
				}
			}

			leftOver = item.stack - temp.stack;
			return temp.stack < item.stack;
		}

		/// <summary>
		/// Gets if this item can be inserted completely into the storage.
		/// </summary>
		public bool CanInsertItem(object? user, Item item) {
			if (item is null || item.IsAir) {
				return false;
			}

			return CanInsertItemPartially(user, item, out int leftOver) && leftOver <= 0;
		}

		// todo: canremove
	}
}