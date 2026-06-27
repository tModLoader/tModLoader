using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace ExampleMod.Content.Tiles
{
	public class ExampleCactus : ModCactus
	{
		private Asset<Texture2D> texture;
		private Asset<Texture2D> fruitTexture;

		public override void SetStaticDefaults() {
			// Makes Example Cactus grow on ExampleSand. You will need to use ExampleSolution to convert regular sand since ExampleCactus will not grow naturally yet.
			GrowsOnTileId = [ModContent.TileType<ExampleSand>()];
			texture = ModContent.Request<Texture2D>("ExampleMod/Content/Tiles/Plants/ExampleCactus");
			fruitTexture = ModContent.Request<Texture2D>("ExampleMod/Content/Tiles/Plants/ExampleCactus_Fruit");
		}

		public override Asset<Texture2D> GetTexture() => texture;

		// This would be where the Cactus Fruit Texture would go, if we had one.
		public override Asset<Texture2D> GetFruitTexture() => fruitTexture;
	}
}