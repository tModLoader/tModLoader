namespace Terraria.ModLoader.Default
{
	public class UnloadedAccessorySlot : ModAccessorySlot
	{
		internal override bool skipRegister => true;

		public override string Name { get; }

		internal UnloadedAccessorySlot(int slot) {
			this.xColumn = (int)((slot) / ModAccessorySlot.accessoryPerColumn) + 1;
			this.yRow = (slot) % ModAccessorySlot.accessoryPerColumn;
			this.slot = slot;
			this.Name = "UnloadedAccessorySlot" + slot.ToString();
		}

		public override bool CanUseSlot() { 
			return false; //TODO: Don't display slot unless has items in it
		}
	}
}