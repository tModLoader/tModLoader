using Terraria;
using Terraria.ModLoader; 

public class ModTileTest : ModTile {
	public override bool Dangersense(int i, int j, Player player) {
		return false;
	}

	public override bool NewRightClick(int i, int j) { return false; /* comment */ }
}