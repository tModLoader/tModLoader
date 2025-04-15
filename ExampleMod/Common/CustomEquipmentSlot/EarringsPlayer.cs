using Terraria.ModLoader;

namespace ExampleMod.Common.CustomEquipmentSlot
{
	// Note: To fully understand this example, please start by reading https://github.com/tModLoader/tModLoader/tree/stable/ExampleMod/Common/CustomEquipmentSlot/README.md

	/// <summary>
	/// EarringsPlayer stores which earring slot will be drawn, and with which dye/shader.
	/// </summary>
	public class EarringsPlayer : ModPlayer
	{
		// The earring equipment slot ID.
		public int earring = -1; // Actually, should this be ear?
		public int earringShader;

		public override void ResetEffects() {
			// TODO: Do we need a ResetVisibleAccessories? Player.Update and UpdateDead don't have the usual ResetEffects->ResetVisibleAccessories call order so there might be some edge case.

			earring = -1;
			earringShader = 0;
		}
	}
}
