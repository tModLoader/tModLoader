using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

namespace Terraria;

public interface ITileData { }

internal static class TileData
{
	private static readonly List<TileDataRegion> AvailableRegions = [];
	private static int ActiveTilemaps;

	internal static readonly object _syncRoot = new();
	internal static Action<uint> OnAddTilemap;
	internal static Action<uint, uint> OnRemoveTilemap;
	internal static Action<uint, uint> OnClearTilemap;
	internal static Action<uint, uint, uint> OnCopyTilemap;
	internal static Action<uint> OnClearSingle;
	internal static Action<uint, uint> OnCopySingle;

	internal static uint Count { get; private set; }
	internal static readonly uint InitialCapacity = (uint)(Main.maxTilesX * Main.maxTilesY);

	internal static uint AddTilemap(in Tilemap tilemap)
	{
		uint tilemapSize = (uint)(tilemap.Width * tilemap.Height);
		lock (_syncRoot) {
			ActiveTilemaps++;

			uint oldCount = Count;
			bool shouldGrow = true;

			foreach (TileDataRegion region in AvailableRegions) {
				if (region.Size == tilemapSize) {
					AvailableRegions.Remove(region);
					shouldGrow = false;
					break;
				}
			}

			if (shouldGrow) {
				Count += tilemapSize;
			}

			OnAddTilemap?.Invoke(tilemapSize);

			return oldCount;
		}
	}

	internal static void RemoveTilemap(in Tilemap tilemap)
	{
		uint tilemapSize = (uint)(tilemap.Width * tilemap.Height);
		lock (_syncRoot) {
			ActiveTilemaps--;

			if (ActiveTilemaps == 0) {
				// Clear freed regions since there aren't any active tilemaps
				AvailableRegions.Clear();
				Count = 0;
			}
			else {
				AvailableRegions.Add(new TileDataRegion(
					tilemapSize,
					tilemap.Offset));
			}

			OnRemoveTilemap?.Invoke(tilemapSize, tilemap.Offset);
		}
	}

	internal static void ClearTilemap(in Tilemap tilemap) => OnClearTilemap?.Invoke((uint)(tilemap.Width * tilemap.Height), tilemap.Offset);

	internal static void CopyTilemap(Tilemap from, Tilemap to)
	{
		if (from.Width != to.Width || from.Height != to.Height) {
			throw new ArgumentException("The tilemaps provided have unequal dimensions.");
		}
		OnCopyTilemap?.Invoke(from.Offset, to.Offset, (uint)(from.Width * from.Height));
	}

	internal static void ClearSingle(uint index) => OnClearSingle?.Invoke(index);
	internal static void CopySingle(uint sourceIndex, uint destinationIndex) => OnCopySingle?.Invoke(sourceIndex, destinationIndex);

	private record struct TileDataRegion(uint Size, uint Offset);
}

internal static unsafe class TileData<T> where T : unmanaged, ITileData
{
	private static uint _capacity;

	public static Span<T> data => new(ptr, (int)_capacity);
	public static T* ptr { get; private set; }

	public static uint Capacity {
		get => _capacity;
		set {
			ArgumentOutOfRangeException.ThrowIfLessThan(value, _capacity);
			if (value == _capacity)
				return;

			var oldByteCount = (nuint)(sizeof(T) * _capacity);
			var newByteCount = (nuint)(sizeof(T) * value);

			T* new_data = (T*)NativeMemory.AllocZeroed(newByteCount);

			if (ptr != null) {
				Buffer.MemoryCopy(ptr, new_data, oldByteCount, oldByteCount);
				NativeMemory.Free(ptr);
			}

			ptr = new_data;
			_capacity = value;
		}
	}

	static TileData()
	{
		TileData.OnAddTilemap += OnAddTilemap;
		TileData.OnRemoveTilemap += OnRemoveTilemap;
		TileData.OnClearTilemap += ClearTilemap;
		TileData.OnCopyTilemap += CopyTilemap;
		TileData.OnCopySingle += CopySingle;
		TileData.OnClearSingle += ClearSingle;
		AssemblyLoadContext.GetLoadContext(typeof(T).Assembly).Unloading += _ => UnloadAll();

		lock (TileData._syncRoot) {
			// Without the lock this could be called in the middle of a tilemap add/remove operation.
			OnAddTilemap(Math.Max(TileData.InitialCapacity, TileData.Count));
		}
	}

	private static void UnloadAll()
	{
		TileData.OnAddTilemap -= OnAddTilemap;
		TileData.OnRemoveTilemap -= OnRemoveTilemap;
		TileData.OnClearTilemap -= ClearTilemap;
		TileData.OnCopySingle -= CopySingle;
		TileData.OnClearSingle -= ClearSingle;
		TileData.OnCopyTilemap -= CopyTilemap;
		if (ptr != null) {
			NativeMemory.Free(ptr);
			_capacity = 0;
		}
	}

	public static void ClearTilemap(uint size, uint offset)
	{
		NativeMemory.Clear(ptr + offset, (nuint)(sizeof(T) * size));
	}

	private static void OnAddTilemap(uint size)
	{
		if (TileData.Count < _capacity) {
			return;
		}

		Capacity += Math.Min(size, TileData.Count - _capacity);
	}

	private static void OnRemoveTilemap(uint tilemapSize, uint offset)
	{
		ClearTilemap(tilemapSize, offset);
	}

	private static void ClearSingle(uint index)
	{
		ptr[index] = default;
	}

	private static void CopySingle(uint sourceIndex, uint destinationIndex)
	{
		ptr[destinationIndex] = ptr[sourceIndex];
	}

	private static void CopyTilemap(uint srcOffset, uint destOffset, uint size)
	{
		var byteCount = (nuint)(sizeof(T) * size);
		Buffer.MemoryCopy(ptr + srcOffset, ptr + destOffset, byteCount, byteCount);
	}
}
