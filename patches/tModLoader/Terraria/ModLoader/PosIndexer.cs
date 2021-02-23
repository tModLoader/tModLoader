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
			int index = FloorBinarySearchPosMap(posMap, GetPosID(x, y));	

			return posMap[index].indexID;
		}

		/// <summary>
		/// Takes current coords of x and y, and increases them to the next location in the position map
		/// </summary>
		/// <param name="posMap"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public static void MoveToNextCoordsInMap(PosIndex[] posMap, ref int x, ref int y) {
			int index = FloorBinarySearchPosMap(posMap, GetPosID(x, y));
			index = System.Math.Min(index + 1, posMap.Length - 1);
			GetCoords(posMap[index].posID, out x, out y);
		}

		/// <summary>
		/// Searches for the interval posMap[i].posID < posID < posMap[i + 1].posID 
		/// </summary>
		/// <param name="posMap"></param>
		/// <param name="posID"></param>
		/// <returns></returns>
		private static int FloorBinarySearchPosMap (PosIndex[] posMap, int posID) {
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