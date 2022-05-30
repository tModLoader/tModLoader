using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class GlobalTileTest : GlobalTile {
	public override bool Dangersense(int i, int j, int type, Player player) {
		return false;
	}

	public override void SetDefaults() { /* Empty */ }

	public override void DrawEffects(int i, int j, int type, SpriteBatch spriteBatch, ref Color drawColor, ref int nextSpecialDrawIndex) { /* Empty */ }
}