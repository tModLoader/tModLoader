using Terraria;

public class TileNullability // TODO, maybe, if ever
{
#if COMPILE_ERROR
	void TileNullable() {
		Tile tile = null/* tModPorter Suggestion: Tiles can no-longer be null. Replace 'null' with 'default' and remove all null checks. */;
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