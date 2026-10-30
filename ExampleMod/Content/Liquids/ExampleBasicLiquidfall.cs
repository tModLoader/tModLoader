using Terraria.ModLoader;

namespace ExampleMod.Content.Liquids
{
	public class ExampleBasicLiquidfall : ModWaterfallStyle
	{
		public override bool PlayWaterfallSounds() {
			return false;
		}
	}
}
