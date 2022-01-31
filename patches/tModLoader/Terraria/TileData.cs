using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Terraria
{
	public interface ITileData { }

	internal static class TileData
	{
		internal static Action OnClearEverything;
		internal static Action<uint> OnSetLength;
		internal static Action<uint> OnClearSingle;
		internal static Action<uint, uint> OnCopySingle;

		static TileData() {
			// Initialize vanilla types. Probably temporary implementation.
			foreach (var type in Assembly.GetExecutingAssembly().GetTypes()) {
				if (type.IsAbstract || !type.IsValueType || !type.GetInterfaces().Contains(typeof(ITileData))) {
					continue;
				}

				typeof(TileData<>).MakeGenericType(type).TypeInitializer.Invoke(null, null);
			}
		}
		internal static void ClearEverything() => OnClearEverything();
		internal static void SetLength(uint len) => OnSetLength(len);
		internal static void ClearSingle(uint index) => OnClearSingle(index);
		internal static void CopySingle(uint sourceIndex, uint destinationIndex) => OnCopySingle(sourceIndex, destinationIndex);
	}

	internal static unsafe class TileData<T> where T : unmanaged, ITileData
	{
		public static T[] data { get; private set; }
		public static T* ptr { get; private set; }

		private static GCHandle handle;

		static TileData() {
			TileData.OnClearEverything += ClearEverything;
			TileData.OnSetLength += SetLength;
			TileData.OnCopySingle += CopySingle;
			TileData.OnClearSingle += ClearSingle;
		}

		public static void ClearEverything() {
			Array.Clear(data);
		}

		private static unsafe void SetLength(uint len) {
			if (data != null)
				handle.Free();

			data = new T[len];
			handle = GCHandle.Alloc(data, GCHandleType.Pinned);
			ptr = (T*)handle.AddrOfPinnedObject().ToPointer();
		}

		private static unsafe void ClearSingle(uint index) {
			ptr[index] = default;
		}

		private static unsafe void CopySingle(uint sourceIndex, uint destinationIndex) {
			ptr[destinationIndex] = ptr[sourceIndex];
		}
	}
}
