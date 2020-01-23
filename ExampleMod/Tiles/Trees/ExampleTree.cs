using ExampleMod.Dusts;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Tiles.Trees
{
	public class ExampleTree : ModTree
	{
		private Mod Mod => ModLoader.GetMod("ExampleMod");

		public override int CreateDust() => DustType<Sparkle>();

		public override int GrowthFXGore() => Mod.GetGoreSlot("Gores/ExampleTreeFX");

		public override int DropWood() => ItemType<Items.Placeable.ExampleBlock>();

		public override Texture2D GetTexture() => Mod.GetTexture("Tiles/ExampleTree");

		public override Texture2D GetTopTextures(int i, int j, ref int frame, ref int frameWidth, ref int frameHeight, ref int xOffsetLeft, ref int yOffset) => Mod.GetTexture("Tiles/ExampleTree_Tops");

		public override Texture2D GetBranchTextures(int i, int j, int trunkOffset, ref int frame) => Mod.GetTexture("Tiles/ExampleTree_Branches");
	}
}