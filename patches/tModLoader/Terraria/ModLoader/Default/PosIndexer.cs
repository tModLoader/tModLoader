using System.Collections.Generic;

namespace Terraria.ModLoader.Default
{
	public static class PosIndexer
	{
		public struct PosKey
		{
			public int posID;
			public ushort infoID;
		}

		/// <summary>
		/// Gets a Position ID based on the x,y position. If using in an order sensitive case, y increases before x.
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
		/// Adds the x,y position and key to the position map. Receives posMap as a list.
		/// </summary>
		/// <param name="posMap"></param>
		/// <param name="posID"></param>
		/// <param name="key"></param>
		public static void MapPosToInfo(List<PosKey> posMap, ushort key, int x, int y) {
			posMap.Add(new PosKey {
				posID = GetPosID(x, y),
				infoID = key
			});
		}

		/// <summary>
		/// Gets the Key for a given x,y position, using the position map.
		/// Assumes the position map is being stored during run time as an array.
		/// </summary>
		/// <param name="posMap"></param>
		/// <param name="posID"></param>
		/// <returns></returns>
		public static ushort GetKeyFromPos(PosKey[] posMap, int x, int y) {
			int index = BinarySearchPosMap(posMap, GetPosID(x, y));

			return posMap[index].infoID;
		}

		/// <summary>
		/// Gets the nearest Key, rounding down for the given PosID, in the position map. 
		/// Assumes the position map is being stored during run time as an array.
		/// Used in TileIO.
		/// </summary>
		/// <param name="posMap"></param>
		/// <param name="posID"></param>
		/// <returns></returns>
		public static ushort FloorGetKeyFromPos(PosKey[] posMap, int x, int y) {
			int index = FloorBinarySearchPosMap(posMap, GetPosID(x, y));	

			return posMap[index].infoID;
		}

		/// <summary>
		/// Takes current coords of x and y, and increases them to the next location in the position map
		/// </summary>
		/// <param name="posMap"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public static void MoveToNextCoordsInMap(PosKey[] posMap, ref int x, ref int y) {
			int index = FloorBinarySearchPosMap(posMap, GetPosID(x, y));
			index = System.Math.Min(index + 1, posMap.Length);
			GetCoords(posMap[index].posID, out x, out y);
		}

		/// <summary>
		/// Searches for the interval posMap[i].posID < posID < posMap[i + 1].posID 
		/// </summary>
		/// <param name="posMap"></param>
		/// <param name="posID"></param>
		/// <returns></returns>
		private static int FloorBinarySearchPosMap (PosKey[] posMap, int posID) {
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
		private static int BinarySearchPosMap(PosKey[] posMap, int posID) {
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

		internal static void CleanupMap(ref PosKey[] posMap, bool[] keepCondition) {
			var temp = new List<PosKey>();

			for (int i = 0; i < posMap.Length; i++) {
				if (keepCondition[i]) {
					temp.Add(posMap[i]);
				}
			}

			posMap = temp.ToArray();
		}
	}
}