using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Content.Liquids
{
	public class ExampleLiquidfall : ModWaterfallStyle
	{
		//Usually waterfalls draw at a partical opacity
		//Lava, Honey and shimmer all draw at a slight higher opacity than water
		//We can modify how strong the alpha is.
		//0 (un-see-able), 1 (fully opaque)
		public override float? Alpha(int x, int y, float Alpha, int maxSteps, int s, Tile tileCache) {
			float num = 1f; //the strength we usually want
			if (s > maxSteps - 10) {
				num *= (float)(maxSteps - s) / 10f; //modifies the strength based on how the length of the waterfall
			}
			return num;
		}

		//We add light to our waterfall as the liquid tied to this fall also shines a bright white light
		public override void AddLight(int i, int j) {
			Lighting.AddLight(i, j, 1f, 1f, 1f);
		}

		//Here we make our waterfall twice as slow as lava, honey and shimmer waterfalls
		//A basic example of manually animating a normal waterfall
		public override void AnimateWaterfall(ref int frame, ref int frameBackground, ref int frameCounter) {
			frameCounter++;
			if (frameCounter > 12) {
				frameCounter = 0;
				frame++;
				if (frame > 15) {
					frame = 0;
				}
			}
		}

		//Used to prevent waterfall sounds from playing from this waterfall
		//Mainly used for waterfalls not made from water
		public override bool PlayWaterfallSounds() {
			return false;
		}
	}
}
