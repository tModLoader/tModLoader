using System;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

namespace Terraria;

public interface ITileData;

internal static class TileData
{
	internal static Action OnClearEverything;
	internal static Action<uint> OnSetLength;
	internal static Action<uint> OnClearSingle;
	internal static Action<uint, uint> OnCopySingle;

	internal static uint Length {
		get;

		set {
			field = value;
			OnSetLength?.Invoke(value);
		}
	}

	internal static void ClearEverything() => OnClearEverything?.Invoke();

	internal static void ClearSingle(uint index) => OnClearSingle?.Invoke(index);

	internal static void CopySingle(uint sourceIndex, uint destinationIndex) => OnCopySingle?.Invoke(sourceIndex, destinationIndex);
}

internal static unsafe class TileData<T> where T : unmanaged, ITileData
{
	public static T[] Data { get; private set; }

	public static T* Ptr { get; private set; }

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
		if (Data != null) {
			handle.Free();
			Data = null;
		}
	}

	public static void ClearEverything()
	{
		Array.Clear(Data);
	}

	private static void SetLength(uint len)
	{
		if (Data != null)
			handle.Free();

		Data = new T[len];
		handle = GCHandle.Alloc(Data, GCHandleType.Pinned);
		Ptr = (T*)handle.AddrOfPinnedObject().ToPointer();
	}

	private static void ClearSingle(uint index)
	{
		Ptr[index] = default;
	}

	private static void CopySingle(uint sourceIndex, uint destinationIndex)
	{
		Ptr[destinationIndex] = Ptr[sourceIndex];
	}
}
