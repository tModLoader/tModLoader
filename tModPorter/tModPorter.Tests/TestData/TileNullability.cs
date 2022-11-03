using Terraria;

public class TileNullability // TODO, maybe, if ever
{
#if COMPILE_ERROR
	void TileNullable() {
		Tile tile = null;
		if (tile == null) { }
		if (tile != null) {
			tile.active(false);
		}
		tile?.active(true);

		int type = tile?.active() ? tile.type : 0;
		type = tile?.type ?? 0;
		type = tile?.wallFrameNumber() ?? 0;
	}
#endif
}