using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class GlobalTileTest : GlobalTile {
	public override bool Dangersense(int i, int j, int type, Player player) {
		return false;
	}

	public override void SetDefaults() { /* Empty */ }

	public override void DrawEffects(int i, int j, int type, SpriteBatch spriteBatch, ref Color drawColor, ref int nextSpecialDrawIndex) {
		drawColor = drawColor * 0.5f;

		// Textbook usage of nextSpecialDrawIndex, reduced to one method in 1.4
		Main.specX[nextSpecialDrawIndex] = i;
		Main.specY[nextSpecialDrawIndex] = j;
		nextSpecialDrawIndex++;
	}

	public override void PlaceInWorld(int i, int j, Item item) { /* Empty */ }

	public override bool Drop(int i, int j, int type) { /* Empty */ }
}