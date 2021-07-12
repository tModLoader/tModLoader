using Terraria.DataStructures;
using Terraria.ObjectData;

namespace Terraria.ModLoader
{
	public static class TileEntityUtils
	{
		public static Point16 TileTopLeft(int i, int j) {
			if (i >= 0 && i <= Main.maxTilesX && j >= 0 && j <= Main.maxTilesY) {
				Tile tile = Main.tile[i, j];

				int fX = 0;
				int fY = 0;

				if (tile != null) {
					TileObjectData data = TileObjectData.GetTileData(tile.type, 0);

					if (data != null) {
						int size = 16 + data.CoordinatePadding;

						fX = (tile.frameX % (size * data.Width)) / size;
						fY = (tile.frameY % (size * data.Height)) / size;
					}
				}

				return new Point16(i - fX, j - fY);
			}

			return Point16.NegativeOne;
		}

		public static Point16 TileTopLeft(Point16 position) => TileTopLeft(position.X, position.Y);

		public static T GetTileEntity<T>(Point16 position) where T : ModTileEntity {
			if (TileEntity.ByPosition.TryGetValue(TileTopLeft(position), out TileEntity te)) return (T)te;
			return null;
		}

		public static T GetTileEntity<T>(int i, int j) where T : ModTileEntity => GetTileEntity<T>(new Point16(i, j));

		public static bool TryGetTileEntity<T>(Point16 position, out T tileEntity) where T : ModTileEntity {
			if (TileEntity.ByPosition.TryGetValue(TileTopLeft(position), out TileEntity te)) {
				tileEntity = (T)te;
				return true;
			}

			tileEntity = null;
			return false;
		}

		public static bool TryGetTileEntity<T>(int i, int j, out T tileEntity) where T : ModTileEntity => TryGetTileEntity(new Point16(i, j), out tileEntity);
		
		public static bool IsTopLeft(this Tile tile)
		{
			int style = 0;
			int alt = 0;
			TileObjectData.GetTileInfo(tile, ref style, ref alt);
			TileObjectData data = TileObjectData.GetTileData(tile.type, style, alt);

			if (data != null) {
				int size = data.CoordinateWidth + data.CoordinatePadding;
				return tile.frameX % (data.Width * size) == 0 && tile.frameY % (data.Height * size) == 0;
			}

			return true;
		}
	}
}