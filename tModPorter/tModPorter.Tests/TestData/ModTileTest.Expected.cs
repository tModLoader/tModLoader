using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class ModTileTest : ModTile
{
	void Method() {
		ItemDrop = 1;
		DustType = 0;
#if COMPILE_ERROR
		HitSound = /* Suggestion: Use a SoundStyle here */ 0;
		soundStyle = /* Suggestion: Utilize HitSound */ 0;
#endif

#if COMPILE_ERROR //TODO
		sapling = true;
		torch = true;
		bed = true;
		dresserDrop = 0;
		dresser = "";
		chestDrop = 0;
		chest = "";
		closeDoorID = 0;
		openDoorID = 0;
		disableSmartInteract = true;
		disableSmartCursor = true;
		minPick = 0;
		mineResist = 0;
		animationFrameHeight = 0;
		adjTiles = new int[0];
#endif
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