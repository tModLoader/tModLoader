#nullable enable

namespace Terraria.ModLoader.Container
{
	public sealed class PlayerInventory : HookedItemStorage
	{
		public PlayerInventory() : base(59) { }

		/// <summary>
		/// Synonymous with Count; kept for backwards compatibility.
		/// </summary>
		public int Length => Count;

		/// <summary>
		/// Returns the underlying item array. Useful for resizing the player inventory or similar.
		/// </summary>
		/// <returns></returns>
		public ref Item[] GetItemArray() => ref Items;

		public static implicit operator Item[](PlayerInventory inv) => inv.Items;
	}
}
