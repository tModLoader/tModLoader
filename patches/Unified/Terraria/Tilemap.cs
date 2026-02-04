using System;
using System.Runtime.CompilerServices;
using Terraria.DataStructures;
using Terraria.IO;

namespace Terraria;

public readonly struct Tilemap
{
	public readonly ushort Width;
	public readonly ushort Height;

	// Unsafe getters ignore bounds checks for scenarios where it's known to be
	// safe.

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Tile UnsafeGet(int x, int y)
	{
		return new Tile((uint)(y + (x * Height)));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Tile UnsafeGet(Point p)
	{
		return UnsafeGet(p.X, p.Y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Tile UnsafeGet(Point16 p)
	{
		return UnsafeGet(p.X, p.Y);
	}

	public Tile this[int x, int y] {
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		get {
			if ((uint)x >= Width || (uint)y >= Height) {
				throw new IndexOutOfRangeException();

				// The informative version is unfortunately terrible for performance (makes worldgen take 2.15x longer)
				// throw new IndexOutOfRangeException($"({x}, {y}). Map size ({Width}, {Height})");
			}

			return new Tile((uint)(y + (x * Height)));
		}
		internal set {
			/*
			throw new InvalidOperationException("Cannot set Tilemap tiles. Only used to init null tiles in Vanilla (which don't exist anymore)");
			*/
		}
	}

	public Tile this[Point pos] => this[pos.X, pos.Y];

	public Tile this[DataStructures.Point16 pos] => this[pos.X, pos.Y];

	internal Tilemap(ushort width, ushort height)
	{
		Width = width;
		Height = height;
		TileData.Length = (uint)width * height;
	}

	public void ClearEverything() => TileData.ClearEverything();

	public T[] GetData<T>() where T : unmanaged, ITileData => TileData<T>.Data;
}
