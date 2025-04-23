using Terraria.ModLoader;

namespace ExampleMod.Common.CustomVisualEquipType
{
	// Note: To fully understand this example, please start by reading https://github.com/tModLoader/tModLoader/tree/stable/ExampleMod/Common/CustomVisualEquipType/README.md

	/// <summary>
	/// EarringsPlayer stores which earring will be drawn, and with which dye/shader.
	/// </summary>
	public class EarringsPlayer : ModPlayer
	{
		// The earring item type.
		public int earring;
		// The dye/shader used to draw the earring.
		public int earringShader;

		public override void ResetEffects() {
			earring = 0;
			earringShader = 0;
		}
	}
}
