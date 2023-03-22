using Terraria;
using Terraria.ID;

public class TileTest
{
	void UseTileMembers() {
		Main.tile[0, 0].TileType = 0;

		Tile tile = new Tile();

		if (tile.HasTile) {
			tile.TileType = 0;
			tile.WallType = 0;
		}

		if (tile.HasUnactuatedTile) {
			tile.TileFrameX = 0;
			tile.TileFrameY = 0;
			tile.WallFrameX = 0;
			tile.WallFrameY = 0;
		}

		tile.HasTile = !tile.HasTile;
		tile.IsActuated = !tile.IsActuated;
		tile.HasActuator = !tile.HasActuator;

#if COMPILE_ERROR // byte -> SlopeType, byte -> BlockType
		byte b = tile.Slope;
		tile.Slope = b;
		byte blockType = tile.BlockType;
#endif
		// not-yet-implemented
		tile.Slope = (SlopeType)(tile.Slope == (SlopeType)2 ? 1 : 0);
		// instead-expect
#if COMPILE_ERROR
		tile.Slope = tile.Slope == 2 ? 1 : 0;
#endif

		tile.IsHalfBlock = !tile.IsHalfBlock;
		// not-yet-implemented
		tile.TileColor = (byte)(tile.TileColor + 1);
		tile.WallColor = (byte)(tile.WallColor + 1);
		// instead-expect
#if COMPILE_ERROR
		tile.TileColor = tile.TileColor + 1;
		tile.WallColor = tile.WallColor + 1;
#endif
		tile.TileFrameNumber = tile.TileFrameNumber + 1;
		tile.WallFrameNumber = tile.WallFrameNumber + 1;
		tile.WallFrameX = tile.WallFrameX + 1;
		tile.WallFrameY = tile.WallFrameY + 1;
		tile.RedWire = !tile.RedWire;
		tile.BlueWire = !tile.BlueWire;
		tile.GreenWire = !tile.GreenWire;
		tile.YellowWire = !tile.YellowWire;
		tile.CheckingLiquid = !tile.CheckingLiquid;
		tile.SkipLiquid = !tile.SkipLiquid;

		bool slopey = tile.TopSlope && tile.LeftSlope || tile.RightSlope && tile.BottomSlope;
		// not-yet-implemented
		bool compBlockTypeWithConstant = tile.BlockType == 0 || tile.BlockType == (BlockType)1 || tile.BlockType == (BlockType)2 || tile.BlockType == (BlockType)3 || tile.BlockType == (BlockType)4 || tile.BlockType == (BlockType)5;
		compBlockTypeWithConstant = tile.BlockType > (BlockType)1 || tile.BlockType <= (BlockType)4;
		// instead-expect
#if COMPILE_ERROR
		bool compBlockTypeWithConstant = tile.BlockType == 0 || tile.BlockType == 1 || tile.BlockType == 2 || tile.BlockType == 3 || tile.BlockType == 4 || tile.BlockType == 5;
		compBlockTypeWithConstant = tile.BlockType > 1 || tile.BlockType <= 4;
#endif

		if (tile.HasUnactuatedTile) { }
		// not-yet-implemented
		if (tile.BlockType == tile.BlockType) { }
		// instead-expect
#if COMPILE_ERROR
		if (tile.HasSameSlope(tile)) { }
		if (tile.isTheSameAs(tile)/* tModPorter Suggestion: Read https://github.com/tModLoader/tModLoader/wiki/Update-Migration-Guide#tiles */) { }
#endif
	}

	void TileLiquid(Tile tile, Tile tile2, byte liquidType) {
		tile.LiquidType = 0;
		tile.LiquidType = 1;
		tile.LiquidType = 2;
		tile.LiquidType = liquidType;

		tile.LiquidAmount = 255;
#if COMPILE_ERROR // byte -> int
		liquidType = tile.LiquidType;
#endif
		tile.LiquidType = LiquidID.Lava;
		tile.LiquidType = LiquidID.Honey;

		if ((tile.LiquidType == LiquidID.Lava) || (tile.LiquidType == LiquidID.Honey)) { }
		if ((tile.LiquidType == LiquidID.Lava) != (tile2.LiquidType == LiquidID.Lava)) { }
		if (!(tile.LiquidType == LiquidID.Lava)) { }

#if COMPILE_ERROR
		tile.lava/* tModPorter Suggestion: LiquidType = ... */(false);
		tile.honey/* tModPorter Suggestion: LiquidType = ... */(false);
		tile.lava/* tModPorter Suggestion: LiquidType = ... */(liquid > 0);
#endif
	}
}