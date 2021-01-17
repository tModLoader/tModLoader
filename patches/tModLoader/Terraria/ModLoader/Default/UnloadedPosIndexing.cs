using System.Collections.Generic;
namespace Terraria.ModLoader.Default
{
	internal class UnloadedPosIndexing
	{
		public int posID;

		public UnloadedPosIndexing(int posX, int posY) {
			this.posID = posX * Main.maxTilesY + posY; // Order determined in accordance with increasing Y in TileIO ReadModData such that PosID is ordered numerically
		}

		public void SaveInfoToPos(UnloadedInfo info, List<UnloadedInfo> infos, Dictionary<int, int> posMap) {
			int pendingID = infos.IndexOf(info);
			if (pendingID < 0) {
				pendingID = 0;
				while (pendingID < infos.Count && infos[pendingID] != null)
					pendingID++;
				if (pendingID == infos.Count)
					infos.Add(info);
				else
					infos[pendingID] = info;
			}
			posMap[posID] = pendingID;
		}
	}
}