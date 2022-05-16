using ExampleMod.Content.Dusts;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace ExampleMod.Content.Tiles
{
	public class ExampleTree : ModTree
	{
		public override void SetStaticDefaults() {
			// Makes Example Tree grow on ExampleBlock
			GrowsOnTileId = new int[1] { ModContent.TileType<ExampleBlock>() };
		}

		// This is the primary texture for the trunk. Branches and foliage use different settings.
		public override Asset<Texture2D> GetTexture() => ModContent.Request<Texture2D>("ExampleMod/Content/Tiles/Plants/ExampleTree");

		// This is a blind copy-paste from Vanilla's PurityPalmTree settings.
		//TODO: This needs some explanations
		public override TreePaintingSettings TreeShaderSettings => new TreePaintingSettings {
			UseSpecialGroups = true,
			SpecialGroupMinimalHueValue = 11f / 72f,
			SpecialGroupMaximumHueValue = 0.25f,
			SpecialGroupMinimumSaturationValue = 0.88f,
			SpecialGroupMaximumSaturationValue = 1f
		};

		public override void SetTreeFoliageSettings(Tile tile, int xoffset, ref int treeFrame, ref int floorY, ref int topTextureFrameWidth, ref int topTextureFrameHeight) {
			// This is where fancy code could go, but let's save that for an advanced example
		}

		// Top Textures
		public override Asset<Texture2D> GetBranchTextures() => ModContent.Request<Texture2D>("ExampleMod/Content/Tiles/Plants/ExampleTree_Branches");

		// Top Textures
		public override Asset<Texture2D> GetTopTextures() => ModContent.Request<Texture2D>("ExampleMod/Content/Tiles/Plants/ExampleTree_Tops");

		//TODO: Is this the right item drop?
		public override int DropWood() => ModContent.Find<ModItem>("ExampleMod/ExampleDye").Type;
	}
}