using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;

namespace ExampleMod.Tiles.Cacti
{
	public class ExampleCactus : ModCactus
	{
		public override Texture2D GetTexture() => ModContent.GetTexture("ExampleMod/Tiles/Cacti/ExampleCactus");
	}
}