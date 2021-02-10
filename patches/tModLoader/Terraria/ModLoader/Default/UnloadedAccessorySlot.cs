namespace Terraria.ModLoader.Default
{
	public class UnloadedAccessorySlot : ModAccessorySlot
	{
		internal override bool skipRegister => true;

		public override bool CanUseSlot() { 
			return false; // Don't display slot
		}
	}
}