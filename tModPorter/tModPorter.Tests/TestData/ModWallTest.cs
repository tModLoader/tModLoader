using Terraria;
using Terraria.ModLoader; 

public class ModWallTest : ModWall
{
	void Method() {
		drop = 1;
		ItemDrop = 12;
		dustType = 0;
		soundType = 1;
		soundStyle = 0;
	}

	public override void RandomUpdate(int i, int j) { /* Empty */ }
}