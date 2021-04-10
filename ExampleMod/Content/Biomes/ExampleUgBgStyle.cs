using Terraria.ModLoader;

namespace ExampleMod.Backgrounds
{
	public class ExampleUgBgStyle : ModUgBgStyle
	{
		public override void FillTextureArray(int[] textureSlots) {
			textureSlots[0] = Mod.GetBackgroundSlot("Backgrounds/ExampleBiomeUG0");
			textureSlots[1] = Mod.GetBackgroundSlot("Backgrounds/ExampleBiomeUG1");
			textureSlots[2] = Mod.GetBackgroundSlot("Backgrounds/ExampleBiomeUG2");
			textureSlots[3] = Mod.GetBackgroundSlot("Backgrounds/ExampleBiomeUG3");
		}
	}
}