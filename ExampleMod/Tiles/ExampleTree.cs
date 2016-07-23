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

		public override Texture2D GetTopTextures()
		{
			return mod.GetTexture("Tiles/ExampleTree_Tops");
		}

		public override Texture2D GetBranchTextures()
		{
			return mod.GetTexture("Tiles/ExampleTree_Branches");
		}
	}
}