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
		HitSound/* Suggestion: Use a SoundStyle here */ = 0;
		soundStyle/* Suggestion: Utilize HitSound */ = 0;
#endif

#if COMPILE_ERROR
		TileID.Sets.TreeSapling[Type]/* Suggestion: Also set TileID.Sets.CommonSapling */ = true;
		TileID.Sets.Torch[Type] = true;
		TileID.Sets.CanBeSleptIn[Type] = true;
		DresserDrop = 0;
		ContainerName.SetDefault("")/* Suggestion: Also set TileID.Sets.BasicDresser */;
		ChestDrop = 0;
		ContainerName.SetDefault("")/* Suggestion: Also set TileID.Sets.BasicChest */;
		CloseDoorID = 0;
		OpenDoorID = 0;
		TileID.Sets.DisableSmartInteract[Type] = true;
		TileID.Sets.DisableSmartCursor[Type] = true;
		MinPick = 0;
		MineResist = 0;
		AnimationFrameHeight = 0;
		AdjTiles = new int[0];
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