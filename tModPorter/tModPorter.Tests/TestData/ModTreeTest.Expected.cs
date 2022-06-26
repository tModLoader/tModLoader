using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.GameContent;
using Terraria.ModLoader;

public class ModTreeTest : ModTree
{
	public override int TreeLeaf() {
		return -1;
	}

	// Just so it compiles fine

	public override TreePaintingSettings TreeShaderSettings => new();
	public override void SetStaticDefaults() { }
	public override Asset<Texture2D> GetTexture() => Asset<Texture2D>.Empty;
	public override int DropWood() => ItemID.Wood;
	public override void SetTreeFoliageSettings(Tile tile, ref int xoffset, ref int treeFrame, ref int floorY, ref int topTextureFrameWidth, ref int topTextureFrameHeight) { }
	public override Asset<Texture2D> GetTopTextures() => Asset<Texture2D>.Empty;
	public override Asset<Texture2D> GetBranchTextures() => Asset<Texture2D>.Empty;
}

public class ModPalmTreeTest : ModPalmTree
{
	public override int TreeLeaf() {
		return -1;
	}

	// Just so it compiles fine

	public override TreePaintingSettings TreeShaderSettings => new();
	public override void SetStaticDefaults() { }
	public override Asset<Texture2D> GetTexture() => Asset<Texture2D>.Empty;
	public override int DropWood() => ItemID.Wood;
	public override Asset<Texture2D> GetTopTextures() => Asset<Texture2D>.Empty;
	public override Asset<Texture2D> GetOasisTopTextures() => Asset<Texture2D>.Empty;
}
