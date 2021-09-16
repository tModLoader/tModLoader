namespace Terraria.ModLoader.Default
{
	[Autoload(false)]
	public class UnloadedAccessorySlot : ModAccessorySlot
	{
		public override string Name { get; }

		internal UnloadedAccessorySlot(int slot, string oldName) {
			type = slot;
			Name = oldName;
		}

		public override bool IsSlotValid() { 
			return false;
		}
	}
}