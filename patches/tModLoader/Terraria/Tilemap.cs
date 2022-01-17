using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Terraria
{
	public class Tilemap
	{
		private static readonly Queue<uint> FreeTilemapIndices = new();
		private static uint NextId = 1; // Id 0 is invalid.

		private readonly uint Id;

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

				return new(Id, (uint)(y + (x * Height)));
			}

			// Should have some function...
			internal set { }
		}

		public Tilemap(uint width, uint height) {
			Width = width;
			Height = height;

			lock (FreeTilemapIndices) {
				if (FreeTilemapIndices.Count > 0) {
					Id = FreeTilemapIndices.Dequeue();
				}
				else {
					Id = NextId++;
				}

				TileData.SetLength(Id, width * height);
			}
		}

		~Tilemap() {
			lock (FreeTilemapIndices) {
				FreeTilemapIndices.Enqueue(Id);
				TileData.SetLength(Id, 0);
			}
		}
	}
}
