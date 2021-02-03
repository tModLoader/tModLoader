using System.Collections.Generic;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default
{
	internal class UnloadedPosIndexing //Requires identity to enable adding support of labelling to multiplayer. Should make static otherwise.
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

		public void MapPosToInfo(List<UnloadedInfo> infos, List<TileIO.posMap> posMap, List<TileIO.posMap> prevPosMap = null, UnloadedInfo info = null)  {
			ushort pendingID = 0;
			if (info == null && prevPosMap.Count > 0) {
				pendingID = (ushort) (FloorGetValue(prevPosMap.ToArray()) + infos.Count);
			}
			else {
				pendingID = (ushort) infos.IndexOf(info);
			}
			
			posMap.Add(new TileIO.posMap {
				posID = this.posID,
				infoID = pendingID
			});
		}

		public ushort FloorGetValue(TileIO.posMap[] posMap) {
			
			int index = BinarySearchPosMap(posMap);	

			return posMap[index].infoID;
		}

		private int BinarySearchPosMap (TileIO.posMap[] posMap) {
			int minimum = 0, maximum = posMap.Length;
			// Binary search for interval containing posID
			while (maximum - minimum > 1) {
				int split = (minimum + maximum) / 2;

				if (split == posMap.Length - 1) {
					break;
				}

				if (posMap[split].posID <= this.posID) {
					minimum = split;
				}

				if (posMap[split + 1].posID > this.posID) {
					maximum = split;
				}

			}
			return minimum;
		}
	}
}