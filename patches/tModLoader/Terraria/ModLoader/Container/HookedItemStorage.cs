#nullable enable
using System.Collections.Generic;
using System.Windows.Forms;

namespace Terraria.ModLoader.Container
{
	public abstract class HookedItemStorage : ItemStorage
	{
		protected HookedItemStorage(int size) : base(size) {
		}

		protected HookedItemStorage(IEnumerable<Item> items) : base(items) {
		}

		public struct Flag
		{
			private bool set;

			public bool Get() => set;
			public void Set() => set = true;
		}

		public delegate void IsInsertValidDelegate(object? user, int slot, Item item, in Flag cancelOperation);
		public delegate void IsRemoveValidDelegate(object? user, int slot, int amount, in Flag cancelOperation);
		public delegate void GetSlotSizeDelegate(int slot, Item item, ref int result);

		public event IsInsertValidDelegate? OnIsInsertValid;
		public event IsRemoveValidDelegate? OnIsRemoveValid;
		public event GetSlotSizeDelegate? OnGetSlotSize;

		public override bool IsInsertValid(object? user, int slot, Item inserting) {
			Flag ret = new Flag();
			OnIsInsertValid?.Invoke(user, slot, inserting, in ret);
			return !ret.Get();
		}

		public override bool IsRemoveValid(object? user, int slot, int amount) {
			Flag ret = new Flag();
			OnIsRemoveValid?.Invoke(user, slot, amount, in ret);
			return !ret.Get();
		}

		public override int GetSlotSize(int slot, Item item) {
			int ret = base.GetSlotSize(slot, item);
			OnGetSlotSize?.Invoke(slot, item, ref ret);
			return ret;
		}
	}
}
