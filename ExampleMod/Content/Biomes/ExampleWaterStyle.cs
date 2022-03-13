using ExampleMod.Content.Dusts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Content.Biomes
{
	public class ExampleWaterStyle : ModWaterStyle
	{
		public override int ChooseWaterfallStyle() {
			return ModContent.Find<ModWaterfallStyle>("ExampleMod/ExampleWaterfallStyle").Slot;
		}

		public override int GetSplashDust() {
			return ModContent.DustType<ExampleSolution>();
		}

		public override int GetDropletGore() {
			return ModContent.Find<ModGore>("ExampleMod/MinionBossBody_Back").Type;
		}

		public override void LightColorMultiplier(ref float r, ref float g, ref float b) {
			r = 1f;
			g = 1f;
			b = 1f;
		}

		public override Color BiomeHairColor() {
			return Color.White;
		}

		public override byte GetRainVariant() {
			return (byte)Main.rand.Next(3);
		}

		public override Asset<Texture2D> GetRainTexture() {
			return ModContent.Request<Texture2D>("ExampleMod/Content/Biomes/ExampleRain");
		}
	}
}