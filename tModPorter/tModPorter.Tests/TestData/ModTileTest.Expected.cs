using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class ModTileTest : ModTile
{
	void Method() {
		ItemDrop = 1;
		DustType = 0;
#if COMPILE_ERROR
		// not-yet-implemented
		HitSound/* tModPorter Suggestion: Use a SoundStyle here */ = 0;
		// instead-expect
		HitSound = 1;
		soundStyle/* tModPorter Note: Removed. Integrate into HitSound */ = 0;
#endif

		DresserDrop = 0;
		ChestDrop = 0;
		CloseDoorID = 0;
		OpenDoorID = 0;
		MinPick = 0;
		MineResist = 0;
		AnimationFrameHeight = 0;
		AdjTiles = new int[0];

#if COMPILE_ERROR
		sapling/* tModPorter Note: Removed. Use TileID.Sets.TreeSapling and TileID.Sets.CommonSapling instead */ = true;
		torch/* tModPorter Note: Removed. Use TileID.Sets.Torch instead */ = true;
		bed/* tModPorter Note: Removed. Use TileID.Sets.CanBeSleptIn instead */ = true;
		dresser/* tModPorter Note: Removed. Use ContainerName.SetDefault() and TileID.Sets.BasicDresser instead */ = "";
		chest/* tModPorter Note: Removed. Use ContainerName.SetDefault() and TileID.Sets.BasicChest instead */ = "";
		disableSmartInteract/* tModPorter Note: Removed. Use TileID.Sets.DisableSmartInteract instead */ = true;
		disableSmartCursor/* tModPorter Note: Removed. Use TileID.Sets.DisableSmartCursor instead */ = true;

		SetModTree(new ExampleTree())/* tModPorter Note: Removed. Assign GrowsOnTileId to this tile type in ModTree.SetStaticDefaults instead */;
		SetModCactus(new ExampleCactus())/* tModPorter Note: Removed. Assign GrowsOnTileId to this tile type in ModCactus.SetStaticDefaults instead */;
		SetModPalmTree(new ExamplePalmTree())/* tModPorter Note: Removed. Assign GrowsOnTileId to this tile type in ModPalmTree.SetStaticDefaults instead */;
#endif
	}

	public override void SetStaticDefaults() {
#if COMPILE_ERROR
		TileID.Sets.TouchDamageSands/* tModPorter Suggestion: Suffocate */[Type] = 15;
		TileID.Sets.TouchDamageOther/* tModPorter Suggestion: TouchDamageImmediate and possibly TouchDamageBleeding */[Type] = 99;
		TileID.Sets.TouchDamageVines/* tModPorter Suggestion: TouchDamageImmediate and TouchDamageDestroyTile */[Type] = 10;
#endif
	}

#if COMPILE_ERROR
	public override int SaplingGrowthType(ref int style)/* tModPorter Note: Removed. Use ModTree.SaplingGrowthType */ { return -1; }
#endif

	public override bool IsTileDangerous(int i, int j, Player player) {
		return false;
	}

	public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) { return true; /* comment */ }

	public override bool RightClick(int i, int j) { return false; /* comment */ }

	public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
		// not-yet-implemented
		drawData.tileLight *= 0.5f;
		// instead-expect
#if COMPILE_ERROR
		drawColor *= 0.5f;
#endif

		// Textbook usage of nextSpecialDrawIndex, reduced to one method in 1.4
		// not-yet-implemented
		Main.instance.TilesRenderer.AddSpecialLegacyPoint(i, j);
		// instead-expect
#if COMPILE_ERROR
		Main.specX[nextSpecialDrawIndex] = i;
		Main.specY[nextSpecialDrawIndex] = j;
		nextSpecialDrawIndex++;
#endif
	}

	public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) { /* comment */ }
}