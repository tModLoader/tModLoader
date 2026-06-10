using Microsoft.Xna.Framework.Graphics.PackedVector;
using System;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Content.Liquids
{
	//Not a complex example of a waterfall, just the waterfall for the complex liquid
	public class ExampleComplexLiquidfall : ModWaterfallStyle
	{
		public override bool PlayWaterfallSounds() {
			return false;
		}

		//Since the liquid is rainbow, we also make the waterfall rainbow by using ColorMultiplier
		public override void ColorMultiplier(ref float r, ref float g, ref float b, float a) {
			r = Main.DiscoR * a;
			g = Main.DiscoG * a;
			b = Main.DiscoB * a;
		}
	}
}
