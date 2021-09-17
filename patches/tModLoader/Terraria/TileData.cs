using System;
using System.Linq;
using System.Reflection;

namespace Terraria
{
	public interface ITileData { }

	internal static class TileData
	{
		internal static Action<uint, int> OnSetLength;
		internal static Action<uint, uint> OnClearSingle;
		internal static Action<uint, uint, int> OnClearMultiple;
		internal static Action<uint, uint, uint, uint> OnCopySingle;
		internal static Action<uint, uint, uint, uint, int> OnCopyMultiple;

		static TileData() {
			// Initialize vanilla types. Probably temporary implementation.
			foreach (var type in Assembly.GetExecutingAssembly().GetTypes()) {
				if (type.IsAbstract || !type.IsValueType || !type.GetInterfaces().Contains(typeof(ITileData))) {
					continue;
				}

				typeof(TileData<>).MakeGenericType(type).TypeInitializer.Invoke(null, null);
			}
		}

		public static void SetLength(uint tilemapId, int length)
			=> OnSetLength?.Invoke(tilemapId, length);

		public static void ClearSingle(uint tilemapId, uint index)
			=> OnClearSingle?.Invoke(tilemapId, index);

		public static void ClearMultiple(uint tilemapId, uint index, int length)
			=> OnClearMultiple?.Invoke(tilemapId, index, length);

		public static void CopySingle(uint sourceTilemapId, uint sourceIndex, uint destinationTilemapId, uint destinationIndex)
			=> OnCopySingle?.Invoke(sourceTilemapId, sourceIndex, destinationTilemapId, destinationIndex);

		public static void CopyMultiple(uint sourceTilemapId, uint sourceIndex, uint destinationTilemapId, uint destinationIndex, int length)
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

		public static void SetLength(uint tilemapId, int length) {
			if (DataByTilemapId == null || DataByTilemapId.Length <= tilemapId) {
				Array.Resize(ref DataByTilemapId, (int)tilemapId + 1);
			}

			Array.Resize(ref DataByTilemapId[tilemapId], length);
		}

		public static void ClearSingle(uint tilemapId, uint index) {
			DataByTilemapId[tilemapId][index] = default;
		}

		public static void ClearMultiple(uint tilemapId, uint index, int length) {
			Array.Clear(DataByTilemapId[tilemapId], (int)index, length);
		}

		public static void CopySingle(uint sourceTilemapId, uint sourceIndex, uint destinationTilemapId, uint destinationIndex) {
			DataByTilemapId[destinationTilemapId][destinationIndex] = DataByTilemapId[sourceTilemapId][sourceIndex];
		}

		public static void CopyMultiple(uint sourceTilemapId, uint sourceIndex, uint destinationTilemapId, uint destinationIndex, int length) {
			Array.Copy(DataByTilemapId[sourceTilemapId], (int)sourceIndex, DataByTilemapId[destinationTilemapId], (int)destinationIndex, length);
		}
	}
}
