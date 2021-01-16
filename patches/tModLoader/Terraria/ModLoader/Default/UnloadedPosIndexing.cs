namespace Terraria.ModLoader.Default
{
	internal class UnloadedPosIndexing
	{
		public int posID;

		public UnloadedPosIndexing(int posX, int posY) {
			this.posID = posX * Main.maxTilesY + posY; // Order determined in accordance with increasing Y in TileIO ReadModData such that PosID is ordered numerically
		}

		public void SaveChestInfoToPos(UnloadedChestInfo info) {
			UnloadedTilesSystem modSystem = ModContent.GetInstance<UnloadedTilesSystem>();
			var infos = modSystem.chestInfos;
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
			modSystem.chestInfoMap[posID] = pendingID;
		}

		public void SaveTileInfoToPos(UnloadedTileInfo info) {
			UnloadedTilesSystem modSystem = ModContent.GetInstance<UnloadedTilesSystem>();
			var infos = modSystem.tileInfos;
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
			modSystem.tileInfoMap[posID] = pendingID;
		}

		public void SaveWallInfoToPos(UnloadedWallInfo info) {
			UnloadedTilesSystem modSystem = ModContent.GetInstance<UnloadedTilesSystem>();
			var infos = modSystem.wallInfos;
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
			modSystem.wallInfoMap[posID] = pendingID;
		}
	}
}