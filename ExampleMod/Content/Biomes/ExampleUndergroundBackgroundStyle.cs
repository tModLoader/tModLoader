using Terraria.ModLoader;

namespace ExampleMod.Backgrounds
{
	public class ExampleUndergroundBackgroundStyle : ModUndergroundBackgroundStyle
	{
		//TODO: This currently doesn't work
		public override void FillTextureArray(int[] textureSlots) {
			textureSlots[0] = BackgroundTextureLoader.GetBackgroundSlot("Assets/Textures/Backgrounds/ExampleBiomeUnderground0");
			textureSlots[1] = BackgroundTextureLoader.GetBackgroundSlot("Assets/Textures/Backgrounds/ExampleBiomeUnderground1");
			textureSlots[2] = BackgroundTextureLoader.GetBackgroundSlot("Assets/Textures/Backgrounds/ExampleBiomeUnderground2");
			textureSlots[3] = BackgroundTextureLoader.GetBackgroundSlot("Assets/Textures/Backgrounds/ExampleBiomeUnderground3");
		}
	}
}