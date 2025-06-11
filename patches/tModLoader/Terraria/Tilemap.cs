using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Terraria;

public readonly struct Tilemap
{
	public readonly ushort Width;
	public readonly ushort Height;

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
			return new((uint)(y + (x * Height)));
#endif
		}
		internal set {
			throw new InvalidOperationException("Cannot set Tilemap tiles. Only used to init null tiles in Vanilla (which don't exist anymore)");
		}
	}
	
	public Tile this[Point pos] => this[pos.X, pos.Y];

	public Tile this[DataStructures.Point16 pos] => this[pos.X, pos.Y];

	internal Tilemap(ushort width, ushort height)
	{
		Width = width;
		Height = height;
		TileData.SetLength((uint)width * height);
	}

	public void ClearEverything() => TileData.ClearEverything();

	public T[] GetData<T>() where T : unmanaged, ITileData => TileData<T>.data;
}
