using Terraria;
using Terraria.ModLoader;

public abstract class ModTreeTest : ModTree
{
	public override int TreeLeaf() {
		return -1;
	}

	public override void SetTreeFoliageSettings(int i, int j, Tile tile, int xoffset, ref int treeFrame, int floorY, ref int topTextureFrameWidth, ref int topTextureFrameHeight) {
	}
}

public abstract class ModPalmTest : ModPalmTree
{
	public override int TreeLeaf() {
		return -1;
	}
}