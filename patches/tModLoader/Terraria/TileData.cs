using System;
using System.Linq;
using System.Reflection;

namespace Terraria
{
	public interface ITileData { }

	internal static class TileData
	{
		internal static Action<int, int> OnSetLength;
		internal static Action<int, int> OnClearSingle;
		internal static Action<int, int, int> OnClearMultiple;
		internal static Action<int, int, int, int> OnCopySingle;
		internal static Action<int, int, int, int, int> OnCopyMultiple;

		static TileData() {
			// Initialize vanilla types. Probably temporary implementation.
			foreach (var type in Assembly.GetExecutingAssembly().GetTypes()) {
				if (type.IsAbstract || !type.IsValueType || !type.GetInterfaces().Contains(typeof(ITileData))) {
					continue;
				}

				typeof(TileData<>).MakeGenericType(type).TypeInitializer.Invoke(null, null);
			}
		}

		public static void SetLength(int tilemapId, int length)
			=> OnSetLength?.Invoke(tilemapId, length);

		public static void ClearSingle(int tilemapId, int index)
			=> OnClearSingle?.Invoke(tilemapId, index);

		public static void ClearMultiple(int tilemapId, int index, int length)
			=> OnClearMultiple?.Invoke(tilemapId, index, length);

		public static void CopySingle(int sourceTilemapId, int sourceIndex, int destinationTilemapId, int destinationIndex)
			=> OnCopySingle?.Invoke(sourceTilemapId, sourceIndex, destinationTilemapId, destinationIndex);

		public static void CopyMultiple(int sourceTilemapId, int sourceIndex, int destinationTilemapId, int destinationIndex, int length)
			=> OnCopyMultiple?.Invoke(sourceTilemapId, sourceIndex, destinationTilemapId, destinationIndex, length);
	}

	internal static class TileData<T> where T : struct, ITileData
	{
		public static T[][] DataByTilemapId;

		static TileData() {
			TileData.OnSetLength += SetLength;
			TileData.OnCopySingle += CopySingle;
			TileData.OnCopyMultiple += CopyMultiple;
			TileData.OnClearSingle += ClearSingle;
			TileData.OnClearMultiple += ClearMultiple;
		}

		public static void SetLength(int tilemapId, int length) {
			if (DataByTilemapId == null || DataByTilemapId.Length <= tilemapId) {
				Array.Resize(ref DataByTilemapId, tilemapId + 1);
			}

			Array.Resize(ref DataByTilemapId[tilemapId], length);
		}

		public static void ClearSingle(int tilemapId, int index) {
			DataByTilemapId[tilemapId][index] = default;
		}

		public static void ClearMultiple(int tilemapId, int index, int length) {
			Array.Clear(DataByTilemapId[tilemapId], index, length);
		}

		public static void CopySingle(int sourceTilemapId, int sourceIndex, int destinationTilemapId, int destinationIndex) {
			DataByTilemapId[destinationTilemapId][destinationIndex] = DataByTilemapId[sourceTilemapId][sourceIndex];
		}

		public static void CopyMultiple(int sourceTilemapId, int sourceIndex, int destinationTilemapId, int destinationIndex, int length) {
			Array.Copy(DataByTilemapId[sourceTilemapId], sourceIndex, DataByTilemapId[destinationTilemapId], destinationIndex, length);
		}
	}
}
