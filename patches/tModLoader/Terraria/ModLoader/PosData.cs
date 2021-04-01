using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Terraria.ModLoader
{
	public struct PosData<T>
	{
		public abstract class OrderedSparseInserter {
			internal PosData<T> last;
			internal List<PosData<T>> list;
			internal bool compressEqualValues;
			internal bool insertDefaultEntries;


			/// <summary>
			/// Safely adds the provided value to the lookups at the given (x,y) position.
			/// </summary>
			public void SafeAdd(int x, int y, T value) => SafeAdd(PosData.CoordsToPos(x, y), value);

			/// <summary>
			/// Safely adds the provided value to the lookups at the given (pos) position.
			/// To get the pos, <see cref="PosData.CoordsToPos(int, int)"/>
			/// </summary>
			public virtual void SafeAdd(int pos, T Value) { }

			internal void Add(int pos, T value) {
				if (compressEqualValues) {
					if (insertDefaultEntries && pos >= last.pos + 2) {
						// make sure the values between last.pos and pos are 'empty' by ensuring at two positions later than last. 
						// note that this won't make a new entry if last.value is default, and will update last if it does make a new value
						Add(last.pos + 1, default);
					}

					if (EqualityComparer<T>.Default.Equals(value, last.value))
						return;
				}

				list.Add(last = new PosData<T>(pos, value));
			}

			/// <summary>
			/// Constructs the lookups array based on items added to the builders in <see cref="PosData{T}"/>.
			/// Returns <see cref="PosData{T}"/>[] for lookups
			/// </summary>
			public virtual PosData<T>[] Build() => list.ToArray();
		}

		/// <summary>
		/// Efficient builder for <see cref="PosData{T}"/>[] lookups covering the whole world.
		/// Must add elements in ascending pos order.
		/// </summary>
		public class OrderedSparseLookupBuilder : OrderedSparseInserter
		{
			/// <summary>
			/// Use <paramref name="compressEqualValues"/> to produce a smaller lookup which won't work with <see cref="PosData.LookupExact"/>
			/// When using <paramref name="compressEqualValues"/> without <paramref name="insertDefaultEntries"/>,
			/// unspecified positions will default to the value of the previous specified position
			/// </summary>
			/// <param name="capacity">Defaults to 1M entries to reduce reallocations. Final built collection will be smaller. </param>
			/// <param name="compressEqualValues">Reduces the size of the map, but gives unspecified positions a value.</param>
			/// <param name="insertDefaultEntries">Ensures unspecified positions are assigned a default value when used with <paramref name="compressEqualValues"/></param>
			public OrderedSparseLookupBuilder(int capacity = 1048576, bool compressEqualValues = true, bool insertDefaultEntries = false) {
				list = new List<PosData<T>>(capacity);
				last = nullPosData;
				this.compressEqualValues = compressEqualValues;
				this.insertDefaultEntries = insertDefaultEntries;
			}

			public override void SafeAdd(int pos, T value) {
				if (pos <= last.pos)
					throw new ArgumentException($"Must build in ascending index order. Prev: {last.pos}, pos: {pos}");

				Add(pos, value);
			}
		}

		/// <summary>
		/// Efficient Merger for <see cref="PosData{T}"/>[] lookups merging on an existing lookup array.
		/// Must add new elements in ascending pos order.
		/// </summary>
		public class OrderedSparseLookupMerger : OrderedSparseInserter
		{
			private readonly List<PosData<T>> targetListUpper;
			private readonly List<PosData<T>> targetListLower;
			
			private readonly PosData<T>[] target;
			private readonly int yTop, yBottom;
			private readonly int finalPos;

			private int lastIndex;

			/// <summary>
			/// Used <paramref name="compressedEqualValues"/> to produce a smaller lookup which won't work with <see cref="PosData.LookupExact"/>
			/// When used <paramref name="compressedEqualValues"/> without <paramref name="insertDefaultEntries"/>,
			/// unspecified positions will default to the value of the previous specified position
			/// The x,y parameters will define the rectangle over which you are inserting new entries - the smaller the rectangle, the faster it runs.
			/// </summary>
			/// <param name="target"> The original array that you wish to merge new entries in to </param>
			/// <param name="compressedEqualValues"> Reduces the size of the map, but gives unspecified positions a value.</param>
			/// <param name="insertedDefaultEntries">Ensures unspecified positions are assigned a default value when used with <paramref name="compressEqualValues"/></param>
			public OrderedSparseLookupMerger(PosData<T>[] target, int xLeft, int xRight, int yTop, int yBottom, bool compressedEqualValues = true, bool insertedDefaultEntries = false) {
				this.target = target;
				this.yTop = yTop;
				this.yBottom = yBottom;
				finalPos = PosData.CoordsToPos(xRight, yBottom);
				list = new List<PosData<T>>(1048576 - target.Length);

				int lowerLoc = PosData.CoordsToPos(xLeft, yTop);
				lastIndex = PosData.FindIndex(target, lowerLoc);
				targetListLower = new List<PosData<T>>();
				if (lastIndex >= 0) {
					targetListLower = target.Take(lastIndex).ToList();
				}
				
				int upperLoc = PosData.CoordsToPos(xRight, yBottom);
				var upper = PosData.FindIndex(target, upperLoc);
				targetListUpper = new List<PosData<T>>();
				if (upper >= 0) {
					targetListUpper = target.Skip(upper + 1).ToList();
				}

				last = insertedDefaultEntries || lastIndex < 0 ? new PosData<T>(lowerLoc, default) : target[lastIndex];
				compressEqualValues = compressedEqualValues;
				insertDefaultEntries = insertedDefaultEntries;
			}

			public override void SafeAdd(int pos, T value) {
				if (pos <= last.pos)
					throw new ArgumentException($"Must build in ascending index order. Prev: {last.pos}, pos: {pos}");

				MergeOldEntries(pos);

				Add(pos, value);
			}

			public override PosData<T>[] Build() {
				MergeOldEntries(finalPos);
				targetListLower.AddRange(list);
				targetListLower.AddRange(targetListUpper);
				return targetListLower.ToArray();
			}

			private void MergeOldEntries(int pos) {
				int y = target[lastIndex + 1].Y;
				while (target[lastIndex + 1].pos <= pos) {
					if (!(y >= yTop && y <= yBottom)) { // Not inside the rectangle, re-add direct
						y = target[++lastIndex + 1].Y;
						Add(target[lastIndex].pos, target[lastIndex].value);
					}
					else {
						while (y <= yBottom && y >= yTop) { // Find the last entry within the rectangle
							y = target[++lastIndex + 1].Y;
						}
						Add(PosData.CoordsToPos(target[lastIndex].X, yBottom + 1), target[lastIndex].value); // Place the last entry outside rectangle in case is still used
					}
				}
			}
		}

		// Enumeration class to access data in a Sparse Lookup System
		public class OrderedSparseLookupReader
		{
			private readonly PosData<T>[] data;
			private readonly bool hasEqualValueCompression;
			private PosData<T> current;
			private int nextIdx;

			public OrderedSparseLookupReader(PosData<T>[] data, bool hasEqualValueCompression = true) {
				this.data = data;
				this.hasEqualValueCompression = hasEqualValueCompression;
				current = nullPosData;
				nextIdx = 0;
			}

			public T Get(int x, int y) => Get(PosData.CoordsToPos(x, y));

			public T Get(int pos) {
				if (pos <= current.pos)
					throw new ArgumentException($"Must read in ascending index order. Prev: {current.pos}, pos: {pos}");

				while (nextIdx < data.Length && data[nextIdx].pos <= pos)
					current = data[nextIdx++];

				if (!hasEqualValueCompression && current.pos != pos)
					throw new KeyNotFoundException($"Position does not exist in map. {pos} (X: {current.X}, Y: {current.Y})");

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

		/// <summary>
		/// Searches for the value i for which <code>posMap[i].pos &lt; CoordsToPos(x, y) &lt; posMap[i + 1].pos</code>
		/// </summary>
		/// <returns>The index of the nearest entry with <see cref="PosData{T}.pos"/> &lt;= CoordsToPos(x, y) or -1 if CoordsToPos(x, y) &lt; <paramref name="posMap"/>[0].pos</returns>
		public static int FindIndex<T>(this PosData<T>[] posMap, int x, int y) => posMap.FindIndex(CoordsToPos(x, y));

		/// <summary>
		/// Searches for the value i for which <code>posMap[i].pos &lt; pos &lt; posMap[i + 1].pos</code>
		/// </summary>
		/// <returns>The index of the nearest entry with <see cref="PosData{T}.pos"/> &lt;= <paramref name="pos"/> or -1 if <paramref name="pos"/> &lt; <paramref name="posMap"/>[0].pos</returns>
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

		/// <summary>
		/// Raw lookup function. Always returns the raw entry in the position map, or <see cref="PosData{T}.nullPosData"/> if it wasn't found.
		/// </summary>
		public static PosData<T> Find<T>(this PosData<T>[] posMap, int pos) => posMap.FindIndex(pos) switch {
			int i => i < 0 ? PosData<T>.nullPosData : posMap[i]
		};

		/// <summary>
		/// General purpose lookup function. Always returns a value (even if that value is `default`).
		/// See <see cref="PosData{T}.OrderedSparseLookupBuilder.OrderedSparseLookupBuilder(int, bool, bool)"/>for more info
		/// </summary>
		public static T Lookup<T>(this PosData<T>[] posMap, int x, int y) => posMap.Lookup(CoordsToPos(x, y));

		/// <summary>
		/// General purpose lookup function. Always returns a value (even if that value is `default`).
		/// See <see cref="PosData{T}.OrderedSparseLookupBuilder.OrderedSparseLookupBuilder(int, bool, bool)"/>for more info
		/// </summary>
		public static T Lookup<T>(this PosData<T>[] posMap, int pos) => posMap.Find(pos).value;

		/// <summary>
		/// For use with uncompressed sparse data lookups. Checks that the exact position exists in the lookup table.
		/// </summary>
		public static bool LookupExact<T>(this PosData<T>[] posMap, int x, int y, out T data) => posMap.LookupExact(CoordsToPos(x, y), out data);

		/// <summary>
		/// For use with uncompressed sparse data lookups. Checks that the exact position exists in the lookup table.
		/// </summary>
		public static bool LookupExact<T>(this PosData<T>[] posMap, int pos, out T data) {
			var posData = posMap.Find(pos);
			if (posData.pos != pos) {
				data = default;
				return false;
			}

			data = posData.value;
			return true;
		}

		/// <summary>
		/// Searches around the provided point to check for the nearest entry in the map for OrdereredSparse data
		/// Doesn't work with 'compressed' lookups from <see cref="PosData{T}.OrderedSparseLookupBuilder"/>
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