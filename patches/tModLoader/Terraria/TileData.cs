using System;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

namespace Terraria;

public interface ITileData { }

internal static class TileData
{
	internal static Action OnClearEverything;
	internal static Action<uint> OnSetLength;
	internal static Action<uint> OnClearSingle;
	internal static Action<uint, uint> OnCopySingle;

	internal static uint Length { get; private set; }
	internal static void SetLength(uint len)
	{
		Length = len;
		OnSetLength?.Invoke(len);
	}

	internal static void ClearEverything() => OnClearEverything?.Invoke();
	internal static void ClearSingle(uint index) => OnClearSingle?.Invoke(index);
	internal static void CopySingle(uint sourceIndex, uint destinationIndex) => OnCopySingle?.Invoke(sourceIndex, destinationIndex);
}

internal static unsafe class TileData<T> where T : unmanaged, ITileData
{
	public static T[] data { get; private set; }
	public static T* ptr { get; private set; }

	private static GCHandle handle;

	static TileData()
	{
		TileData.OnSetLength += SetLength;
		TileData.OnClearEverything += ClearEverything;
		TileData.OnCopySingle += CopySingle;
		TileData.OnClearSingle += ClearSingle;
		AssemblyLoadContext.GetLoadContext(typeof(T).Assembly).Unloading += _ => Unload();

		SetLength(TileData.Length);
	}

	private static void Unload()
	{
		TileData.OnSetLength -= SetLength;
		TileData.OnClearEverything -= ClearEverything;
		TileData.OnCopySingle -= CopySingle;
		TileData.OnClearSingle -= ClearSingle;
		if (data != null) {
			handle.Free();
			data = null;
		}
	}

	public static void ClearEverything()
	{
		Array.Clear(data);
	}

	private static unsafe void SetLength(uint len)
	{
		if (data != null)
			handle.Free();

		data = new T[len];
		handle = GCHandle.Alloc(data, GCHandleType.Pinned);
		ptr = (T*)handle.AddrOfPinnedObject().ToPointer();
	}

	private static unsafe void ClearSingle(uint index)
	{
		ptr[index] = default;
	}

	private static unsafe void CopySingle(uint sourceIndex, uint destinationIndex)
	{
		ptr[destinationIndex] = ptr[sourceIndex];
	}
}
