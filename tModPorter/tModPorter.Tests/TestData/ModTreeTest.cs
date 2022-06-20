using Terraria.ModLoader;

public class ModTreeTest : ModTree
{
	public override int GrowthFXGore() {
		return -1;
	}
}

public class ModTreeTest : ModPalmTree
{
	public override int GrowthFXGore() {
		return -1;
	}
}
