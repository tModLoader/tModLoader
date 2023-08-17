using Terraria.ModLoader;

public abstract class ModTreeTest : ModTree
{
	public override int TreeLeaf() {
		return -1;
	}
}

public abstract class ModPalmTest : ModPalmTree
{
	public override int TreeLeaf() {
		return -1;
	}
}