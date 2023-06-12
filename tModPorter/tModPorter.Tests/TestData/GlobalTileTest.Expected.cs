using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class GlobalTileTest : GlobalTile {
	public override bool? IsTileDangerous(int i, int j, int type, Player player)/* tModPorter Suggestion: Return null instead of false */ {
		return false;
	}

	public override void SetStaticDefaults() { /* Empty */ }

	public override void DrawEffects(int i, int j, int type, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
		// not-yet-implemented
		drawData.tileLight = drawData.tileLight * 0.5f;

		// Textbook usage of nextSpecialDrawIndex, reduced to one method in 1.4
		Main.instance.TilesRenderer.AddSpecialLegacyPoint(i, j);
		// instead-expect
#if COMPILE_ERROR
		drawColor = drawColor * 0.5f;

		// Textbook usage of nextSpecialDrawIndex, reduced to one method in 1.4
		Main.specX[nextSpecialDrawIndex] = i;
		Main.specY[nextSpecialDrawIndex] = j;
		nextSpecialDrawIndex++;
#endif
	}

	public override void PlaceInWorld(int i, int j, int type, Item item) { /* Empty */ }

	public override void Drop(int i, int j, int type)/* tModPorter Suggestion: Use CanDrop to decide if items can drop, use this method to drop additional items. See documentation. */ { /* Empty */ }
}