using ExampleMod.Content.Biomes;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;

namespace ExampleMod.Common.Players
{
	public class ExampleBiomePlayer : ModPlayer
	{
		public override Texture2D GetMapBackgroundImage() {
			if (Player.InModBiome(ModContent.GetInstance<ExampleSurfaceBiome>())) {
				return ModContent.Request<Texture2D>(ModContent.GetInstance<ExampleSurfaceBiome>().BackgroundPath).Value;
			}
			return null;
		}
	}
}
