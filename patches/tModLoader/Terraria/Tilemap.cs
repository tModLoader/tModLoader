using System;
using System.Runtime.CompilerServices;
using System.Transactions;
using Microsoft.Xna.Framework;

namespace Terraria;

public struct Tilemap : IDisposable
{
	private bool disposed;

	public readonly ushort Width;
	public readonly ushort Height;
	public readonly uint Offset;

	public Tile this[int x, int y] {
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		get {
			if ((uint)x >= Width || (uint)y >= Height) {
				throw new IndexOutOfRangeException();

				// The informative version is unfortunately terrible for performance (makes worldgen take 2.15x longer)
				// throw new IndexOutOfRangeException($"({x}, {y}). Map size ({Width}, {Height})");
			}
#if TILE_X_Y
			return new((ushort)x, (ushort)y, (uint)(y + (x * Height)));
#else
			return new((uint)(y + (x * Height) + Offset));
#endif
		}
		internal set {
			throw new InvalidOperationException("Cannot set Tilemap tiles. Only used to init null tiles in Vanilla (which don't exist anymore)");
		}
	}
	
	public Tile this[Point pos] => this[pos.X, pos.Y];

	public Tile this[DataStructures.Point16 pos] => this[pos.X, pos.Y];

	public Tilemap(ushort width, ushort height)
	{
		Width = width;
		Height = height;
		Offset = TileData.AddTilemap(this);
	}

	public void Clear()
	{
		if (disposed) {
			throw new ObjectDisposedException(GetType().Name);
		}
		TileData.ClearTilemap(this);
	}

	public void CopyTo(Tilemap other)
	{
		if (disposed) {
			throw new ObjectDisposedException(GetType().Name);
		}
		TileData.CopyTilemap(this, other);
	}

	public static Span<T> GetData<T>() where T : unmanaged, ITileData => TileData<T>.data;

	public void Dispose()
	{
		if (!disposed) {
			TileData.RemoveTilemap(this);
			disposed = true;
		}
	}
}
