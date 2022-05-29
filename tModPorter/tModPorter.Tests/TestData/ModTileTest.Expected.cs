using Terraria;
using Terraria.ModLoader; 

public class ModTileTest : ModTile {
	public override bool IsTileDangerous(int i, int j, Player player) {
		return false;
	}

	public override bool RightClick(int i, int j) { return false; /* comment */ }
}