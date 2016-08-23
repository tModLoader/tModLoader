using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;

namespace ExampleMod.Tiles
{
	public class ExampleTree : ModTree
	{
		private Mod mod
		{
			get
			{
				return ModLoader.GetMod("ExampleMod");
			}
		}

		public override int CreateDust()
		{
			return mod.DustType("Sparkle");
		}

		public override int GrowthFXGore()
		{
			return mod.GetGoreSlot("Gores/ExampleTreeFX");
		}

		public override int DropWood()
		{
			return mod.ItemType("ExampleBlock");
		}

		public override Texture2D GetTexture()
		{
			return mod.GetTexture("Tiles/ExampleTree");
		}

		public override Texture2D GetTopTextures(int i, int j, ref int frame, ref int frameWidth, ref int frameHeight, ref int xOffsetLeft, ref int yOffset)
		{
			return mod.GetTexture("Tiles/ExampleTree_Tops");
		}

		public override Texture2D GetBranchTextures(int i, int j, int trunkOffset, ref int frame)
		{
			return mod.GetTexture("Tiles/ExampleTree_Branches");
		}
	}
}