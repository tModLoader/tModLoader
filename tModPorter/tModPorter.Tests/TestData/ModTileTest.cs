using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class ModTileTest : ModTile
{
	void Method() {
		drop = 1;
		dustType = 0;
#if COMPILE_ERROR
		soundType = 1;
		soundStyle = 0;
#endif

		dresserDrop = 0;
		chestDrop = 0;
		closeDoorID = 0;
		openDoorID = 0;
		minPick = 0;
		mineResist = 0;
		animationFrameHeight = 0;
		adjTiles = new int[0];

#if COMPILE_ERROR
		sapling = true;
		torch = true;
		bed = true;
		dresser = "";
		chest = "";
		disableSmartInteract = true;
		disableSmartCursor = true;

		SetModTree(new ExampleTree());
		SetModCactus(new ExampleCactus());
		SetModPalmTree(new ExamplePalmTree());
#endif
	}

#if COMPILE_ERROR
	public override int SaplingGrowthType(ref int style) { return -1; }
#endif

	public override bool Dangersense(int i, int j, Player player) {
		return false;
	}

	public override bool HasSmartInteract() { return true; /* comment */ }

	public override bool NewRightClick(int i, int j) { return false; /* comment */ }

	public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref Color drawColor, ref int nextSpecialDrawIndex) {
		drawColor *= 0.5f;

		// Textbook usage of nextSpecialDrawIndex, reduced to one method in 1.4
		Main.specX[nextSpecialDrawIndex] = i;
		Main.specY[nextSpecialDrawIndex] = j;
		nextSpecialDrawIndex++;
	}

	public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height) { /* comment */ }
}