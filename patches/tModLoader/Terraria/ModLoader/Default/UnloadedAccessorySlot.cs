namespace Terraria.ModLoader.Default
{
	public class UnloadedAccessorySlot : ModAccessorySlot
	{
		internal override bool suppressUnloadedSlot => true;

		public override string Name { get; }

		internal UnloadedAccessorySlot(int slot) {
			this.slot = slot;
			this.Name = "UnloadedAccessorySlot" + slot.ToString();
		}

		public override bool IsSlotValid() { 
			return false;
		}
	}
}