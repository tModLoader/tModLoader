using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Terraria.ModLoader
{
	public static class PosIndexer
	{
		public struct PosIndex
		{
			public int posID;
			public ushort indexID;
		}

		/// <summary>
		/// Gets a Position ID based on the x,y position. If using in an order sensitive case, see NextLocation.
		/// </summary>
		/// <param name="posX"></param>
		/// <param name="posY"></param>
		/// <returns></returns>
		public static int GetPosID(int posX, int posY) {
			return posX * Main.maxTilesY + posY;
		}

		/// <summary>
		/// Gets the decomposed coordinates from the Position ID. 
		/// </summary>
		/// <param name="posID"></param>
		/// <param name="posX"></param>
		/// <param name="posY"></param>
		public static void GetCoords(int posID, out int posX, out int posY) {
			posY = posID % Main.maxTilesY;
			posX = posID / Main.maxTilesY;
		}

		/// <summary>
		/// Increases the provided x and y coordinates to the next location in accordance with order-sensitive position IDs.
		/// Typically used in clustering duplicate data across multiple consecutive locations, such as in ModLoader.TileIO 
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns> False if x and y cannot be increased further (end of the world)  </returns>
		public static bool NextLocation(ref int x, ref int y) {
			y++;
			if (y >= Main.maxTilesY) {
				y = 0;
				x++;
				if (x >= Main.maxTilesX) {
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Adds the x,y position and key to the position map. Receives posMap as a list.
		/// </summary>
		/// <param name="posMap"></param>
		/// <param name="posID"></param>
		/// <param name="key"></param>
		public static void MapPosToInfo(List<PosIndex> posMap, ushort key, int x, int y) {
			posMap.Add(new PosIndex {
				posID = GetPosID(x, y),
				indexID = key
			});
		}

		/// <summary>
		/// Gets the Key for a given x,y position, using the position map.
		/// Assumes the position map is being stored during run time as an array.
		/// </summary>
		/// <param name="posMap"></param>
		/// <param name="posID"></param>
		/// <returns></returns>
		public static ushort GetKeyFromPos(PosIndex[] posMap, int x, int y) {
			if (posMap.Length == 0) {
				return 0;
			}

			int index = BinarySearchPosMap(posMap, GetPosID(x, y));
			return posMap[index].indexID;
		}

		/// <summary>
		/// Gets the nearest Key, rounding down for the given PosID, in the position map. 
		/// Assumes the position map is being stored during run time as an array.
		/// Used in TileIO.
		/// </summary>
		/// <param name="posMap"></param>
		/// <param name="posID"></param>
		/// <returns></returns>
		public static ushort FloorGetKeyFromPos(PosIndex[] posMap, int x, int y) {
			if (posMap.Length == 0) {
				return 0;
			}

			int index = FloorBinarySearchPosMap(posMap, GetPosID(x, y));
			return posMap[index].indexID;
		}

		/// <summary>
		/// Takes current coords of x and y, and increases them to the next location in the position map
		/// </summary>
		/// <param name="posMap"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public static int MoveToNextCoordsInMap(PosIndex[] posMap, ref int x, ref int y) {
			if (posMap.Length == 0) {
				return 0;
			}

			int index = FloorBinarySearchPosMap(posMap, GetPosID(x, y));
			index = System.Math.Min(index + 1, posMap.Length - 1);
			GetCoords(posMap[index].posID, out x, out y);
			return index;
		}

		/// <summary>
		/// Searches for the interval posMap[i].posID < posID < posMap[i + 1].posID and is preferable to NearbyBinarySearchPosMap for ordered data.
		/// </summary>
		/// <param name="posMap"></param>
		/// <param name="posID"></param>
		/// <returns></returns>
		public static int FloorBinarySearchPosMap (PosIndex[] posMap, int posID) {
			int minimum = 0, maximum = posMap.Length;
			while (maximum - minimum > 1) {
				int split = (minimum + maximum) / 2;

				if (split == posMap.Length - 1) {
					break;
				}

				if (posMap[split].posID <= posID) {
					minimum = split;
				}

				if (posMap[split + 1].posID > posID) {
					maximum = split;
				}

			}
			return minimum;
		}

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

		/// <summary>
		/// Searches for posMap[i].posID == posID 
		/// </summary>
		/// <param name="posMap"></param>
		/// <param name="posID"></param>
		/// <returns></returns>
		private static int BinarySearchPosMap(PosIndex[] posMap, int posID) {
			int minimum = 0, maximum = posMap.Length, split;
			do {
				split = (minimum + maximum) / 2;

				if (posMap[split].posID <= posID) {
					minimum = split;
				}
				else {
					maximum = split;
				}
			} while (posMap[split].posID !=  posID);
			return split;
		}
	}
}