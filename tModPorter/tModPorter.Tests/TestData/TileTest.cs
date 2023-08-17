using Terraria;

public class TileTest
{
	void UseTileMembers() {
		Main.tile[0, 0].type = 0;

		Tile tile = new Tile();

		if (tile.active()) {
			tile.type = 0;
			tile.wall = 0;
		}

		if (tile.nactive()) {
			tile.frameX = 0;
			tile.frameY = 0;
			tile.wallFrameX = 0;
			tile.wallFrameY = 0;
		}

		tile.active(!tile.active());
		tile.inActive(!tile.inActive());
		tile.actuator(!tile.actuator());

		byte b = tile.slope();
		tile.slope(b);
		byte blockType = tile.blockType();
		tile.slope(tile.slope() == 2 ? 1 : 0);

		tile.halfBrick(!tile.halfBrick());
		tile.color(tile.color() + 1);
		tile.wallColor(tile.wallColor() + 1);
		tile.frameNumber(tile.frameNumber() + 1);
		tile.wallFrameNumber(tile.wallFrameNumber() + 1);
		tile.wallFrameX(tile.wallFrameX() + 1);
		tile.wallFrameY(tile.wallFrameY() + 1);
		tile.wire(!tile.wire());
		tile.wire2(!tile.wire2());
		tile.wire3(!tile.wire3());
		tile.wire4(!tile.wire4());
		tile.checkingLiquid(!tile.checkingLiquid());
		tile.skipLiquid(!tile.skipLiquid());

		bool slopey = tile.topSlope() && tile.leftSlope() || tile.rightSlope() && tile.bottomSlope();
		bool compBlockTypeWithConstant = tile.blockType() == 0 || tile.blockType() == 1 || tile.blockType() == 2 || tile.blockType() == 3 || tile.blockType() == 4 || tile.blockType() == 5;
		compBlockTypeWithConstant = tile.blockType() > 1 || tile.blockType() <= 4;

		if (tile.nactive()) { }
		if (tile.HasSameSlope(tile)) { }
		if (tile.isTheSameAs(tile)) { }
	}

	void TileLiquid(Tile tile, Tile tile2, byte liquidType) {
		tile.liquidType(0);
		tile.liquidType(1);
		tile.liquidType(2);
		tile.liquidType(liquidType);

		tile.liquid = 255;
		liquidType = tile.liquidType();
		tile.lava(true);
		tile.honey(true);

		if (tile.lava() || tile.honey()) { }
		if (tile.lava() != tile2.lava()) { }
		if (!tile.lava()) { }

		tile.lava(false);
		tile.honey(false);
		tile.lava(liquid > 0);
	}
}