using ExampleMod.Content.Dusts;
using ExampleMod.Content.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Tiles
{
	public class ExampleTree : ModTree
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
			// Makes Example Tree grow on ExampleBlock
			GrowsOnTileId = new int[1] { ModContent.TileType<ExampleBlock>() };
		}

		// This is the primary texture for the trunk. Branches and foliage use different settings.
		public override Asset<Texture2D> GetTexture() {
			return ModContent.Request<Texture2D>("ExampleMod/Content/Tiles/Plants/ExampleTree");
		}

		public override int SaplingGrowthType(ref int style) {
			style = 0;
			return ModContent.TileType<Plants.ExampleSapling>();
		}

		public override void SetTreeFoliageSettings(Tile tile, ref int xoffset, ref int treeFrame, ref int floorY, ref int topTextureFrameWidth, ref int topTextureFrameHeight) {
			// This is where fancy code could go, but let's save that for an advanced example
		}

		// Branch Textures
		public override Asset<Texture2D> GetBranchTextures() {
			return ModContent.Request<Texture2D>("ExampleMod/Content/Tiles/Plants/ExampleTree_Branches");
		}

		// Top Textures
		public override Asset<Texture2D> GetTopTextures() {
			return ModContent.Request<Texture2D>("ExampleMod/Content/Tiles/Plants/ExampleTree_Tops");
		}

		public override int DropWood() {
			return ModContent.ItemType<ExampleDye>();
		}

		public override bool Shake(int x, int y, ref bool createLeaves) {
			Item.NewItem(WorldGen.GetItemSource_FromTreeShake(x, y), new Vector2(x, y) * 16, ModContent.ItemType<Items.Placeable.ExampleBlock>());
			return false;
		}

		public override int TreeLeaf() {
			return ModContent.GoreType<ExampleTreeLeaf>();
		}
	}
}