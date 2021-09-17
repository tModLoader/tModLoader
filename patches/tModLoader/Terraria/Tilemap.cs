using System;
using System.Collections.Generic;

namespace Terraria
{
	public class Tilemap : IDisposable
	{
		internal static Queue<uint> FreeTilemapIndices = new();
		internal static uint NextId = 1; // Id 0 is invalid.
		internal static object IdThreadLock = new();

		internal uint Id;

		public int Width { get; private set; }
		public int Height { get; private set; }

		public Tile this[int x, int y] {
			get => new(Id, (uint)((y * Width) + x));

			// Should have some function...
			internal set { }
		}

		public Tilemap(int width, int height) {
			CheckSize(width, height);

			Width = width;
			Height = height;

			lock (IdThreadLock) {
				if (FreeTilemapIndices.Count > 0) {
					Id = FreeTilemapIndices.Dequeue();
				}
				else {
					Id = NextId++;
				}

				TileData.SetLength?.Invoke(Id, width * height);
			}
		}

		public void Resize(int width, int height) {
			CheckSize(width, height);

			lock (IdThreadLock) {
				TileData.SetLength?.Invoke(Id, width * height);
			}
		}

		public void Dispose() {
			if (Id == 0) {
				return;
			}

			lock (IdThreadLock) {
				FreeTilemapIndices.Enqueue(Id);

				Id = 0;
				Width = 0;
				Height = 0;

				TileData.SetLength?.Invoke(Id, 0);
			}
		}

		private static void CheckSize(int width, int height) {
			if (width <= 0) {
				throw new ArgumentException($"Argument {nameof(width)} must be more than 0.");
			}

			if (height <= 0) {
				throw new ArgumentException($"Argument {nameof(height)} must be more than 0.");
			}
		}
	}
}
