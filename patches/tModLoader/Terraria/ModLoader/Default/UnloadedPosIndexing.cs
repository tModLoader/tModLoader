using System.Collections.Generic;
namespace Terraria.ModLoader.Default
{
	internal class UnloadedPosIndexing
	{
		public int posID;

		public UnloadedPosIndexing(int posX, int posY) {
			// The order is determined in accordance with increasing Y in TileIO ReadModData such that PosID is ordered numerically
			posID = posX * Main.maxTilesY + posY;
		}

		public UnloadedPosIndexing(int posID) {
			this.posID = posID;
		}

		public void GetCoords(out int posX, out int posY) {
			posY = posID % Main.maxTilesY;
			posX = posID / Main.maxTilesY;
		}

		public void MapPosToInfo(UnloadedInfo info, List<UnloadedInfo> infos, SortedDictionary<int, int> posMap) {
			int pendingID = infos.IndexOf(info);
			posMap[posID] = pendingID;
		}

		public int FloorGetValue(SortedDictionary<int, int> posMap) {
			var keys = posMap.Keys;
			int floorKey = 0;

			foreach (int testKey in keys) {
				if (testKey > posID)
					break;

				floorKey = testKey;
			}

			posMap.TryGetValue(floorKey, out int value);

			return value;
		}
	}
}