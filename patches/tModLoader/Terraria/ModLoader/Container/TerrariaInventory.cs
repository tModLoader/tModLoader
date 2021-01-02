#nullable enable

namespace Terraria.ModLoader.Container
{
	/// <summary>
	/// Represents vanilla Terraria inventories. Chests, players, etc. Do not rely on vanilla to call hooks.
	/// </summary>
	public sealed class TerrariaInventory : HookedItemStorage
	{
		public TerrariaInventory(int size) : base(size) { }

		public new Item this[int index] {
			get => base[index];
			set {
				if (Items[index].Equals(value) || Items[index].IsAir && value.IsAir) {
					return;
				}
				OnUpdateItem?.Invoke(index, Items[index], value);
				Items[index] = value;
			}
		}

		public delegate void UpdateItem(int slot, Item oldItem, Item newItem);

		/// <summary>
		/// Fired just before updating an item through the indexer.
		/// </summary>
		public event UpdateItem? OnUpdateItem;

		/// <summary>
		/// Synonymous with Count; kept for backwards compatibility.
		/// </summary>
		public int Length => Count;

		/// <summary>
		/// Returns the underlying item array. Useful for resizing the player inventory or similar.
		/// </summary>
		/// <returns></returns>
		public ref Item[] GetItemArray() => ref Items;

		public static implicit operator Item[](TerrariaInventory inv) => inv.Items;
	}
}
