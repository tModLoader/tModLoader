using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Tiles
{
	public class ExamplePalmTree : ModPalmTree
	{
		// This is a blind copy-paste from Vanilla's PurityPalmTree settings.
		//TODO: This needs some explanations
		public override TreePaintingSettings TreeShaderSettings => new TreePaintingSettings {
			UseSpecialGroups = true,
			SpecialGroupMinimalHueValue = 11f / 72f,
			SpecialGroupMaximumHueValue = 0.25f,
			SpecialGroupMinimumSaturationValue = 0.88f,
			SpecialGroupMaximumSaturationValue = 1f
		};

		public override void SetStaticDefaults() {
			// Makes Example Palm Tree grow on ExampleBar
			GrowsOnTileId = new int[1] { ModContent.TileType<ExampleBar>() };
		}

		// This is the primary texture for the trunk. Branches and foliage use different settings.
		public override Asset<Texture2D> GetTexture() => ModContent.Request<Texture2D>("ExampleMod/Content/Tiles/Plants/ExamplePalmTree");

		public override int SaplingGrowthType(ref int style) {
			style = 1;
			return ModContent.TileType<Plants.ExampleSapling>();
		}

		// Palm Trees come in an Oasis variant. The Branch Textures for it, if we had them:
		public override Asset<Texture2D> GetOasisBranchTextures() => null;

		// Regular Top Textures for the Ocean (we reused the same texture to save time, you don't have to reuse them): 
		public override Asset<Texture2D> GetBranchTextures() => null;

		// Palm Trees come in an Oasis variant. The Top Textures for it:
		public override Asset<Texture2D> GetOasisTopTextures() => ModContent.Request<Texture2D>("ExampleMod/Content/Tiles/Plants/ExamplePalmTree_Tops");

		// Regular Top Textures for the Ocean (we reused the same texture to save time, you don't have to reuse them): 
		public override Asset<Texture2D> GetTopTextures() => ModContent.Request<Texture2D>("ExampleMod/Content/Tiles/Plants/ExamplePalmTree_Tops");

		//TODO: Is this the right item drop?
		public override int DropWood() => ModContent.Find<ModItem>("ExampleMod/ExampleOre").Type; 
	}
}