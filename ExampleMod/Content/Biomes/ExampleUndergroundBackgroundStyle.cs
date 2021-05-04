using Terraria.ModLoader;

namespace ExampleMod.Backgrounds
{
	public class ExampleUndergroundBackgroundStyle : ModUndergroundBackgroundStyle
	{
		//TODO: This currently doesn't work
		public override void FillTextureArray(int[] textureSlots) {
			textureSlots[0] = Mod.GetBackgroundSlot("Assets/Textures/Backgrounds/ExampleBiomeUnderground0.rawimg");
			textureSlots[1] = Mod.GetBackgroundSlot("Assets/Textures/Backgrounds/ExampleBiomeUnderground1.rawimg");
			textureSlots[2] = Mod.GetBackgroundSlot("Assets/Textures/Backgrounds/ExampleBiomeUnderground2.rawimg");
			textureSlots[3] = Mod.GetBackgroundSlot("Assets/Textures/Backgrounds/ExampleBiomeUnderground3.rawimg");
		}
	}
}