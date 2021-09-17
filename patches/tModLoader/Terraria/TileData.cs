using System;
using System.Linq;
using System.Reflection;

namespace Terraria
{
	public interface ITileData { }

	internal static class TileData
	{
		public delegate void SetLengthDelegate(uint tilemapId, int length);

		public static SetLengthDelegate SetLength;

		static TileData() {
			// Initialize vanilla types
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
		}

		public static void SetLength(uint tilemapId, int length) {
			if (DataByTilemapId == null || DataByTilemapId.Length <= tilemapId) {
				Array.Resize(ref DataByTilemapId, (int)tilemapId + 1);
			}

			Array.Resize(ref DataByTilemapId[tilemapId], length);
		}
	}
}
