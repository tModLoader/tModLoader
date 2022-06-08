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
		drawData.tileLight = drawData.tileLight * 0.5f;

		// Textbook usage of nextSpecialDrawIndex, reduced to one method in 1.4
		Main.instance.TilesRenderer.AddSpecialLegacyPoint(i, j);
	}

	public override void PlaceInWorld(int i, int j, int type, Item item) { /* Empty */ }
}