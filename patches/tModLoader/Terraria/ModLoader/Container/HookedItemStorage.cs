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

		public delegate void CanInteractDelegate(object? user, int slot, Operation operation, in Flag cancel);
		public delegate void IsInsertValidDelegate(int slot, Item item, in Flag cancel);
		public delegate void IsRemoveValidDelegate(int slot, in Flag cancel);
		public delegate void GetSlotSizeDelegate(int slot, Item item, ref int result);

		public event CanInteractDelegate? OnCanInteract;
		public event IsInsertValidDelegate? OnIsInsertValid;
		public event IsRemoveValidDelegate? OnIsRemoveValid;
		public event GetSlotSizeDelegate? OnGetSlotSize;

		public override bool CanInteract(int slot, Operation operation, object? user) {
			Flag ret = new Flag();
			OnCanInteract?.Invoke(user, slot, operation, in ret);
			return !ret.Get();
		}

		public override bool IsInsertValid(int slot, Item item) {
			Flag ret = new Flag();
			OnIsInsertValid?.Invoke(slot, item, in ret);
			return !ret.Get();
		}

		protected override bool IsRemoveValid(int slot) {
			Flag ret = new Flag();
			OnIsRemoveValid?.Invoke(slot, in ret);
			return !ret.Get();
		}

		public override int GetSlotSize(int slot, Item item) {
			int ret = base.GetSlotSize(slot, item);
			OnGetSlotSize?.Invoke(slot, item, ref ret);
			return ret;
		}
	}
}
