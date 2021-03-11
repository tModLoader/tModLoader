using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Terraria.ModLoader
{
	public readonly struct PosData<T>
	{
		public class SparseLookupBuilder 
		{
			private readonly List<PosData<T>> list;
			private PosData<T> last;

			public SparseLookupBuilder(int capacity = 1048576) {
				list = new List<PosData<T>>(capacity);
				last = new PosData<T>(-1, default);
			}

			public void Add(int pos, T value) {
				if (pos <= last.pos)
					throw new ArgumentException($"Must build in ascending index order. Prev: {last.pos}, pos: {pos}");

				if (!EqualityComparer<T>.Default.Equals(value, last.value))
					list.Add(last = new PosData<T>(pos, value));
			}

			public PosData<T>[] Build() => list.ToArray();
		}

		public class SparseLookupReader
		{
			private readonly PosData<T>[] data;
			private PosData<T> current;
			private int nextIdx;

			public SparseLookupReader(PosData<T>[] data) {
				this.data = data;
				current = new PosData<T>(-1, default);
				nextIdx = 0;
			}

			public T Get(int pos) {
				if (pos <= current.pos)
					throw new ArgumentException($"Must read in ascending index order. Prev: {current.pos}, pos: {pos}");

				while (nextIdx < data.Length && data[nextIdx].pos <= pos)
					current = data[nextIdx++];

				return current.value;
			}
		}


		public readonly int pos;
		public readonly T value;

		public int X => pos / Main.maxTilesY;
		public int Y => pos % Main.maxTilesY;

		public PosData(int pos, T value) {
			this.pos = pos;
			this.value = value;
		}

		public PosData(int x, int y, T value) : this(PosData.CoordsToPos(x, y), value) { }
	}

	public static class PosData
	{
		public static int CoordsToPos(int x, int y) => x * Main.maxTilesY + y;

		/// <summary>
		/// Searches for the interval posMap[i].posID < posID < posMap[i + 1].posID and is preferable to NearbyBinarySearchPosMap for ordered data.
		/// </summary>
		/// <param name="posMap"></param>
		/// <param name="posID"></param>
		/// <returns></returns>
		public static int FindIndex<T>(this PosData<T>[] posMap, int pos) {
			int minimum = -1, maximum = posMap.Length;
			while (maximum - minimum > 1) {
				int split = (minimum + maximum) / 2;

				if (posMap[split].pos <= pos)
					minimum = split;
				else
					maximum = split;
			}

			return minimum;
		}

		public static int FindIndex<T>(this PosData<T>[] posMap, int x, int y) => posMap.FindIndex(CoordsToPos(x, y));

		public static T Lookup<T>(this PosData<T>[] posMap, int pos) => posMap.FindIndex(pos) switch {
			-1 => default,
			int i => posMap[i].value
		};

		public static T Lookup<T>(this PosData<T>[] posMap, int x, int y) => posMap.Lookup(CoordsToPos(x, y));

		/// <summary>
		/// Searches around the provided point to check for the nearest entry in the map, giving equal weight to Y and X.
		/// </summary>
		/// <param name="posMap"></param>
		/// <param name="pt"></param>
		/// <param name="distance"> The distance between the provided Point and nearby entry </param>
		/// <returns> True if successfully found an entry nearby </returns>
		public static bool NearbyBinarySearchPosMap(PosIndex[] posMap, Point pt, int distance, out int mapIndex) {
			int minimum = 0, maximum = posMap.Length - 1;
			mapIndex = -1;

			int d1 = distance + 1; int d2 = distance + 1; byte iterationsX = 0;
			while ((d1 > distance || d2 > distance) && iterationsX < 15) {
				iterationsX++;
				GetCoords(posMap[maximum].posID, out int bigX, out var bigY);
				d1 = Math.Abs(bigX - pt.X);

				GetCoords(posMap[minimum].posID, out int smlX, out var smlY);
				d2 = Math.Abs(pt.X - smlX);

				if (d2 <= d1) {
					maximum = (maximum - minimum) / 2;
				}
				else {
					minimum = (maximum - minimum) / 2;
				}
			}

			if (iterationsX == 15) {
				return false;
			}

			int d4 = distance * distance + 1; 
			for (int i = minimum; i < maximum; i++) {
				GetCoords(posMap[i].posID, out int x, out var y);
				int d3 = (int)(Math.Pow((x - pt.X), 2) + Math.Pow((y - pt.Y), 2));
				if (d3 < d4) {
					d4 = d3;
					mapIndex = i;
				}
			}

			if (d4 == distance * distance + 1) {
				return false;
			}

			return true;
		}
	}
}