using Terraria;
using Terraria.ModLoader;

public abstract class ModTreeTest : ModTree
{
	public override int GrowthFXGore() {
		return -1;
	}

	public override void SetTreeFoliageSettings(Tile tile, ref int xoffset, ref int treeFrame, ref int floorY, ref int topTextureFrameWidth, ref int topTextureFrameHeight) {
	}
}

public abstract class ModPalmTest : ModPalmTree
{
	public override int GrowthFXGore() {
		return -1;
	}
}