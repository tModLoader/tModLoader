using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class ModTileTest : ModTile
{
	void Method() {
		drop = 1;
	}

	public override bool Dangersense(int i, int j, Player player) {
		return false;
	}

	public override bool NewRightClick(int i, int j) { return false; /* comment */ }

	public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref Color drawColor, ref int nextSpecialDrawIndex) { /* comment */ }
}