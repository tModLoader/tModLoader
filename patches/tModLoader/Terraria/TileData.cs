using System;
using System.Linq;
using System.Reflection;

namespace Terraria
{
	public interface ITileData { }

	internal static class TileData
	{
		internal delegate void SetLengthDelegate(uint tilemapId, int length);
		internal delegate void CopySingleDelegate(uint sourceTilemapId, uint sourceIndex, uint destinationTilemapId, uint destinationIndex);
		internal delegate void CopyMultipleDelegate(uint sourceTilemapId, uint sourceIndex, uint destinationTilemapId, uint destinationIndex, int length);

		internal static SetLengthDelegate SetLength;
		internal static CopySingleDelegate CopySingle;
		internal static CopyMultipleDelegate CopyMultiple;

		static TileData() {
			// Initialize vanilla types. Probably temporary implementation.
			foreach (var type in Assembly.GetExecutingAssembly().GetTypes()) {
				if (type.IsAbstract || !type.IsValueType || !type.GetInterfaces().Contains(typeof(ITileData))) {
					continue;
				}

				typeof(TileData<>).MakeGenericType(type).TypeInitializer.Invoke(null, null);
			}
		}
	}

	internal static class TileData<T> where T : struct, ITileData
	{
		public static T[][] DataByTilemapId;

		static TileData() {
			TileData.SetLength += SetLength;
			TileData.CopySingle += CopySingle;
			TileData.CopyMultiple += CopyMultiple;
		}

		public static void SetLength(uint tilemapId, int length) {
			if (DataByTilemapId == null || DataByTilemapId.Length <= tilemapId) {
				Array.Resize(ref DataByTilemapId, (int)tilemapId + 1);
			}

			Array.Resize(ref DataByTilemapId[tilemapId], length);
		}

		public static void CopySingle(uint sourceTilemapId, uint sourceIndex, uint destinationTilemapId, uint destinationIndex) {
			DataByTilemapId[sourceTilemapId][sourceIndex] = DataByTilemapId[destinationTilemapId][destinationIndex];
		}

		public static void CopyMultiple(uint sourceTilemapId, uint sourceIndex, uint destinationTilemapId, uint destinationIndex, int length) {
			Array.Copy(DataByTilemapId[sourceTilemapId], (int)sourceIndex, DataByTilemapId[destinationTilemapId], (int)destinationIndex, length);
		}
	}
}
