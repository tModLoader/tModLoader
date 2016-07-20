using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Waters
{
	public class ExampleWaterStyle : ModWaterStyle
	{
		public override bool ChooseWaterStyle()
		{
			return Main.bgStyle == mod.GetSurfaceBgStyleSlot("ExampleSurfaceBgStyle");
		}

		public override int ChooseWaterfallStyle()
		{
			return mod.GetWaterfallStyleSlot("ExampleWaterfallStyle");
		}

		public override int GetSplashDust()
		{
			return mod.DustType("ExampleWaterSplash");
		}

		public override int GetDropletGore()
		{
			return mod.GetGoreSlot("Gores/ExampleDroplet");
		}

		public override void LightColorMultiplier(ref float r, ref float g, ref float b)
		{
			r = 1f;
			g = 1f;
			b = 1f;
		}

		public override Color BiomeHairColor()
		{
			return Color.White;
		}
	}
}