namespace Terraria.ModLoader.Default
{
	public class UnloadedAccessorySlot : ModAccessorySlot
	{
		internal override bool skipRegister => true;

		public override string Name { get; }

		internal UnloadedAccessorySlot(int slot) {
			this.slot = slot;
			this.Name = "UnloadedAccessorySlot" + slot.ToString();
		}

		public override bool CanUseSlot() { 
			return false;
		}
	}
}