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

		byte b = (byte)tile.Slope;
		tile.Slope = (SlopeType)b;
		tile.Slope = (SlopeType)(tile.Slope == (SlopeType)2 ? 1 : 0);

		tile.IsHalfBlock = !tile.IsHalfBlock;
		tile.TileColor = (byte)(tile.TileColor + 1);
		tile.WallColor = (byte)(tile.WallColor + 1);
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
		byte blockType = (byte)tile.BlockType;
		bool compBlockTypeWithConstant = tile.BlockType == 0 || tile.BlockType == (BlockType)1 || tile.BlockType == (BlockType)2 || tile.BlockType == (BlockType)3 || tile.BlockType == (BlockType)4 || tile.BlockType == (BlockType)5;
		compBlockTypeWithConstant = tile.BlockType > (BlockType)1 || tile.BlockType <= (BlockType)4;

		if (tile.HasUnactuatedTile) { }
		if (tile.BlockType == tile.BlockType) { }
#if COMPILE_ERROR
		if (tile.isTheSameAs/* https://github.com/tModLoader/tModLoader/wiki/Update-Migration-Guide#tiles */(tile)) { }
#endif
	}

	void TileLiquid(Tile tile, Tile tile2, byte liquid) {
		tile.LiquidType = 0;
		tile.LiquidType = 1;
		tile.LiquidType = 2;
		tile.LiquidType = liquid;

		liquid = (byte)tile.LiquidType;
		tile.LiquidType = LiquidID.Lava;
		tile.LiquidType = LiquidID.Honey;

		if (tile.LiquidType == LiquidID.Lava || tile.LiquidType == LiquidID.Honey) { }
		if ((tile.LiquidType == LiquidID.Lava) != (tile2.LiquidType == LiquidID.Lava)) { }
		if (tile.LiquidType != LiquidID.Lava) { }

#if COMPILE_ERROR
		tile.lava/* Suggestion: LiquidType = LiquidID.Water */(false);
		tile.honey/* Suggestion: LiquidType = LiquidID.Water */(false);
		tile.lava/* Suggestion: LiquidType = ... */(liquid > 0);
#endif
	}

#if COMPILE_ERROR
	void TileNullable() {
		Tile tile = null; /* Tiles can no-longer be null. Suggestion: replace 'null' with 'default' and remove all null checks.  */
		if (tile == null) { }
		if (tile != null) {
			tile.HasTile = false;
		}
		tile.Active = true;

		int type = tile.active() ? tile.TileType : 0;
		type = tile.TileType ?? 0;
		type = tile.WallFrameNumber ?? 0;
	}
#endif
}