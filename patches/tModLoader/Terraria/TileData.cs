using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Terraria
{
	public interface ITileData { }

	internal static class TileData
	{
		internal static Action<uint, uint> OnSetLength;
		internal static Action<uint, uint> OnClearSingle;
		internal static Action<uint, uint, uint, uint> OnCopySingle;

		static TileData() {
			// Initialize vanilla types. Probably temporary implementation.
			foreach (var type in Assembly.GetExecutingAssembly().GetTypes()) {
				if (type.IsAbstract || !type.IsValueType || !type.GetInterfaces().Contains(typeof(ITileData))) {
					continue;
				}

				typeof(TileData<>).MakeGenericType(type).TypeInitializer.Invoke(null, null);
			}
		}
		internal static void SetLength(uint tilemapId, uint len)
			=> OnSetLength?.Invoke(tilemapId, len);

		public static void ClearSingle(uint tilemapId, uint index)
			=> OnClearSingle?.Invoke(tilemapId, index);

		public static void CopySingle(uint sourceTilemapId, uint sourceIndex, uint destinationTilemapId, uint destinationIndex)
			=> OnCopySingle?.Invoke(sourceTilemapId, sourceIndex, destinationTilemapId, destinationIndex);
	}

	internal static unsafe class TileData<T> where T : unmanaged, ITileData
	{
		public const int MaxTilemaps = 16;
		public static readonly T** DataByTilemapId = (T**) AllocGlobalAndInit(sizeof(T*) * MaxTilemaps);

		static TileData() {
			// incase anyone tries to use a 'default' Tile
			DataByTilemapId[0] = (T*) AllocGlobalAndInit(sizeof(T));

			TileData.OnSetLength += SetLength;
			TileData.OnCopySingle += CopySingle;
			TileData.OnClearSingle += ClearSingle;
		}

		internal static unsafe void SetLength(uint tilemapId, uint len) {
			ref T* tilemap = ref DataByTilemapId[tilemapId];
			uint size = (uint)(sizeof(T) * len);
			if (tilemap != null) {
				tilemap = (T*)Marshal.ReAllocHGlobal(new IntPtr(tilemap), new IntPtr(size)).ToPointer();
			}
			else {
				tilemap = (T*)Marshal.AllocHGlobal(new IntPtr(size)).ToPointer();
			}
			Unsafe.InitBlock(tilemap, 0, size);
		}

		public static unsafe void ClearSingle(uint tilemapId, uint index) {
			DataByTilemapId[tilemapId][index] = default;
		}

		public static unsafe void CopySingle(uint sourceTilemapId, uint sourceIndex, uint destinationTilemapId, uint destinationIndex) {
			DataByTilemapId[destinationTilemapId][destinationIndex] = DataByTilemapId[sourceTilemapId][sourceIndex];
		}

		private static void* AllocGlobalAndInit(int len) {
			void* ptr = Marshal.AllocHGlobal(len).ToPointer();
			Unsafe.InitBlock(ptr, 0, (uint)len);
			return ptr;
		}
	}
}
