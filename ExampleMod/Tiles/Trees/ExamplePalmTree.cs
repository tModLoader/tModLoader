using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Tiles.Trees
{
	public class ExamplePalmTree : ModPalmTree
	{
		public override Texture2D GetTexture() => ModContent.GetTexture("ExampleMod/Tiles/Trees/ExamplePalmTree");
		
		public override Texture2D GetTopTextures() => ModContent.GetTexture("ExampleMod/Tiles/Trees/ExamplePalmTree_Tops");

		public override int DropWood() => ItemID.SandBlock; // TODO
	}
}