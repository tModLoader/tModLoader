using System;
using System.Runtime.CompilerServices;
using System.Transactions;
using Microsoft.Xna.Framework;

namespace Terraria;

public struct Tilemap : IDisposable
{
	private bool disposed;

	public ushort Width { get; private set; }
	public ushort Height { get; private set; }
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

	/// <summary>
	/// Clears all tile data associated with this <see cref="Tilemap"/>.
	/// </summary>
	/// <exception cref="ObjectDisposedException"></exception>
	public void Clear()
	{
		if (disposed) {
			throw new ObjectDisposedException(GetType().Name);
		}
		TileData.ClearTilemap(this);
	}

	/// <summary>
	/// Copies tile data from this instance to <paramref name="other"/>.
	/// </summary>
	/// <param name="other"></param>
	/// <exception cref="ObjectDisposedException"></exception>
	/// <exception cref="ArgumentException"></exception>
	public void CopyTo(Tilemap other)
	{
		if (disposed) {
			throw new ObjectDisposedException(GetType().Name);
		}
		if (Width != other.Width || Height != other.Height) {
			throw new ArgumentException("The provided tilemaps must have the same width and height.");
		}
		TileData.CopyTilemap(this, other);
	}

	public static Span<T> GetData<T>() where T : unmanaged, ITileData => TileData<T>.data;

	/// <summary>
	/// Clears all tile data and frees the memory space used by this <see cref="Tilemap"/> instance.
	/// </summary>
	public void Dispose()
	{
		if (!disposed) {
			TileData.RemoveTilemap(this);
			Width = 0;
			Height = 0;
			disposed = true;
		}
	}
}
