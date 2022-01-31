using System;
using System.Runtime.CompilerServices;

namespace Terraria
{
	public readonly struct Tilemap
	{
		public readonly uint Width;
		public readonly uint Height;

		public Tile this[int x, int y] {
			[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
			get {
				if ((uint)x >= Width || (uint)y >= Height) {
					throw new IndexOutOfRangeException();

					// The informative version is unfortunately terrible for performance (makes worldgen take 2.15x longer)
					// throw new IndexOutOfRangeException($"({x}, {y}). Map size ({Width}, {Height})");
				}

				return new((uint)(y + (x * Height)));
			}

			// Should have some function...
			internal set { }
		}

		internal Tilemap(uint width, uint height) {
			Width = width;
			Height = height;
			TileData.SetLength(width * height);
		}

		public void ClearEverything() => TileData.ClearEverything();
	}
}
