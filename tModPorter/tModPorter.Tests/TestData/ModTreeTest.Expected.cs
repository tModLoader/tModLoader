using Terraria.ModLoader;

public class ModTreeTest : ModTree
{
	public override int TreeLeaf() {
		return -1;
	}
}

public class ModTreeTest : ModPalmTree
{
	public override int TreeLeaf() {
		return -1;
	}
}
