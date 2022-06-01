using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;

public class ModTileTest : ModTile
{
	void Method() {
		ItemDrop = 1;
	}

	public override bool IsTileDangerous(int i, int j, Player player) {
		return false;
	}

	public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) { return true; /* comment */ }

	public override bool RightClick(int i, int j) { return false; /* comment */ }

	public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
		drawData.tileLight *= 0.5f;

		// Textbook usage of nextSpecialDrawIndex, reduced to one method in 1.4
		Main.instance.TilesRenderer.AddSpecialLegacyPoint(i, j);
	}
}