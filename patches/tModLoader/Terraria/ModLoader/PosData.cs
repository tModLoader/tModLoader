using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Terraria.ModLoader
{
	public struct PosData<T>
	{
		// Enumeration class to build an Ordered Sparse Lookup system
		public class OrderedSparseLookupBuilder 
		{
			private readonly List<PosData<T>> list;
			private PosData<T> last;

			public OrderedSparseLookupBuilder(int capacity = 1048576) {
				list = new List<PosData<T>>(capacity);
				last = nullPosData;
			}

			public void Add(int x, int y, T value) => Add(PosData.CoordsToPos(x, y), value);

			public void Add(int pos, T value) {
				if (pos <= last.pos)
					throw new ArgumentException($"Must build in ascending index order. Prev: {last.pos}, pos: {pos}");

				list.Add(new PosData<T>(pos, value));
			}

			public void ClusteredAdd(int x, int y, T value) => ClusteredAdd(PosData.CoordsToPos(x, y), value);

			public void ClusteredAdd(int pos, T value) {
				if (pos <= last.pos)
					throw new ArgumentException($"Must build in ascending index order. Prev: {last.pos}, pos: {pos}");

				if (!EqualityComparer<T>.Default.Equals(value, last.value))
					list.Add(last = new PosData<T>(pos, value));
			}

			public PosData<T>[] Build() => list.ToArray();
		}

		// Enumeration class to access data in a Sparse Lookup System
		public class OrderedSparseLookupReader
		{
			private readonly PosData<T>[] data;
			private PosData<T> current;
			private int nextIdx;

			public OrderedSparseLookupReader(PosData<T>[] data) {
				this.data = data;
				current = nullPosData;
				nextIdx = 0;
			}

			public T Get(int x, int y) => Get(PosData.CoordsToPos(x, y));

			public T Get(int pos) {
				if (pos <= current.pos)
					throw new ArgumentException($"Must read in ascending index order. Prev: {current.pos}, pos: {pos}");

				while (nextIdx < data.Length && data[nextIdx].pos <= pos)
					current = data[nextIdx++];

				return current.value;
			}
		}

		public readonly int pos;
		public T value;

		public int X => pos / Main.maxTilesY;
		public int Y => pos % Main.maxTilesY;

		public PosData(int pos, T value) {
			this.pos = pos;
			this.value = value;
		}

		public PosData(int x, int y, T value) : this(PosData.CoordsToPos(x, y), value) { }

		public static PosData<T> nullPosData = new PosData<T>(-1, default);
	}

	public static class PosData
	{
		/// <summary>
		/// Gets a Position ID based on the x,y position. If using in an order sensitive case, see NextLocation.
		/// </summary>
		/// <param name="posX"></param>
		/// <param name="posY"></param>
		/// <returns></returns>
		public static int CoordsToPos(int x, int y) => x * Main.maxTilesY + y;

		public static int FindIndex<T>(this PosData<T>[] posMap, int x, int y) => posMap.FindIndex(CoordsToPos(x, y));

		/// <summary>
		/// Searches for the interval posMap[i].posID < provided posID < posMap[i + 1].posID.
		/// </summary>
		/// <param name="posMap"></param>
		/// <param name="posID"></param>
		/// <returns></returns>
		public static int FindIndex<T>(this PosData<T>[] posMap, int pos) {
			int minimum = -1, maximum = posMap.Length;
			while (maximum - minimum > 1) {
				int split = (minimum + maximum) / 2;
				
				if (posMap[split].pos <= pos) { 
					minimum = split;
				}

				else {
					maximum = split;
				}
			}

			return minimum;
		}

		public static bool Lookup<T>(this PosData<T>[] posMap, int x, int y, out T data) => posMap.Lookup<T>(CoordsToPos(x, y), out data);

		public static bool Lookup<T>(this PosData<T>[] posMap, int pos, out T data) {
			var i = posMap.FindIndex(pos);
			bool fail = i == -1;
			data = fail ? default : posMap[i].value;
			return !fail;
		}

		public static bool LookupExact<T>(this PosData<T>[] posMap, int x, int y, out T data) => posMap.LookupExact<T>(CoordsToPos(x, y), out data);

		public static bool LookupExact<T>(this PosData<T>[] posMap, int pos, out T data) {
			var i = posMap.FindIndex(pos);
			bool fail = (i == -1) || (posMap[i].pos != pos);
			data = fail ? default : posMap[i].value;
			return !fail;
		}

		/// <summary>
		/// Searches around the provided point to check for the nearest entry in the map for OrdereredSparse data
		/// </summary>
		/// <param name="posMap"></param>
		/// <param name="pt"></param>
		/// <param name="distance"> The distance between the provided Point and nearby entry </param>
		/// <returns> True if successfully found an entry nearby </returns>
		public static bool NearbySearchOrderedPosMap<T>(PosData<T>[] posMap, Point pt, int distance, out PosData<T> entry) {
			entry = new PosData<T>(-1, default);

			// Check if posMap.Length is zero (typically due to modder building map themeselves), before executing everything else needlessly.
			if (posMap.Length == 0) {
				return false;
			}

			int minPos = CoordsToPos(Math.Max(pt.X - distance, 0), Math.Max(pt.Y - distance, 0));
			int maxPos = CoordsToPos(Math.Min(pt.X + distance, Main.maxTilesX - 1), Math.Min(pt.Y + distance, Main.maxTilesY - 1));

			// Check if there is even a hope of finding a nearby entry before doing searches. Requires Map is non-empty
			if (posMap[0].pos > maxPos || posMap[posMap.Length - 1].pos < minPos) {
				return false;
			}

			// this range [inclusive, inclusive] contains all the tiles in the square of radius distance around point (side length 2*distance+1)
			int minimum = Math.Max(posMap.FindIndex(minPos), 0);
			int maximum = Math.Max(posMap.FindIndex(maxPos), 0);

			int bestSqDist = distance * distance + 1;

			//TODO: this loop could search in both y directions from a centerpoint for each X, with earlier exit conditions, but this should do reasonably
			for (int i = minimum; i < maximum; i++) {
				var posData = posMap[i];
				int dy = posData.Y - pt.Y;
				if (dy < -distance || dy > distance)
					continue;

				int dx = posData.X - pt.X;
				int sqDist = dx * dx + dy * dy;
				if (sqDist < bestSqDist) {
					bestSqDist = sqDist;
					entry = posData;
				}
			}

			return entry.pos >= 0;
		}
	}
}