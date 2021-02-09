namespace Terraria.ModLoader.Default
{
	public class UnloadedAccessorySlot : ModAccessorySlot
	{
		public override void Initialize() {	}

		public override bool CanUseSlot() { 
			return false; // Don't display slot
		}
	}
}