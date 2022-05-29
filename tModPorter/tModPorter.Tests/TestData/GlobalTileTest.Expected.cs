using Terraria;
using Terraria.ModLoader; 

public class GlobalTileTest : GlobalTile {
	public override bool? IsTileDangerous(int i, int j, int type, Player player) {
		return false;
	}

	public override void SetStaticDefaults() { /* Empty */ }
}