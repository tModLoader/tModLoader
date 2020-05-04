using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;

namespace ExampleMod.Tiles.Trees
{
	public class ExampleCactus : ModCactus
	{
		public override Texture2D GetTexture() => ModContent.GetTexture("ExampleMod/Tiles/Trees/ExampleCactus");
	}
}