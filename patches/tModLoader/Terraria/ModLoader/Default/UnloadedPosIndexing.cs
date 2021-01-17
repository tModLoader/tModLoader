using System.Collections.Generic;
namespace Terraria.ModLoader.Default
{
	internal class UnloadedPosIndexing
	{
		public int posID;

		public UnloadedPosIndexing(int posX, int posY) {
			this.posID = posX * Main.maxTilesY + posY; // Order determined in accordance with increasing Y in TileIO ReadModData such that PosID is ordered numerically
		}

		public void SaveInfoToPos(UnloadedInfo info,byte index) {
			UnloadedTilesSystem modSystem = ModContent.GetInstance<UnloadedTilesSystem>();
			List<UnloadedInfo> infos = null;
			Dictionary<int, int> posMap = null;

			switch (index) {
				case 0:
					infos = modSystem.tileInfos;
					posMap = modSystem.tileInfoMap;
					break;
				case 1:
					infos = modSystem.wallInfos;
					posMap = modSystem.wallInfoMap;
					break;
				case 2:
					infos = modSystem.chestInfos;
					posMap = modSystem.chestInfoMap;
					break;
			}
			
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