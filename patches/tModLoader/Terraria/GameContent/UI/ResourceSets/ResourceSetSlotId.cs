namespace Terraria.GameContent.UI.ResourceSets
{
	public struct ResourceSetSlotId<T> where T : IPlayerResourcesDisplaySet {
		public int Slot { get; private set; }

		public ResourceSetSlotId(int slot) {
			Slot = slot;
		}

		public static implicit operator int(ResourceSetSlotId<T> slot) => slot.Slot;

		public static implicit operator ResourceSetSlotId<T>(int slot) => new(slot);
	}
}
