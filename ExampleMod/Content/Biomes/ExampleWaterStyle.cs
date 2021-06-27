using ExampleMod.Content.Dusts;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace ExampleMod.Content.Biomes
{
	public class ExampleWaterStyle : ModWaterStyle
	{
		public override int ChooseWaterfallStyle() => ModContent.Find<ModWaterfallStyle>("ExampleMod/ExampleWaterfallStyle").Slot;

		public override int GetSplashDust() => ModContent.GetId<ExampleSolution>();

		public override int GetDropletGore() => ModContent.Find<ModGore>("ExampleMod/MinionBossBody_Back").Type;

		public override void LightColorMultiplier(ref float r, ref float g, ref float b) {
			r = 1f;
			g = 1f;
			b = 1f;
		}

		public override Color BiomeHairColor()
			=> Color.White;
	}
}