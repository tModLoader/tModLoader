namespace Terraria.ModLoader.Default
{
	internal class UnloadedPosIndexing
	{
		public int PosID;

		public UnloadedPosIndexing(int posX, int posY) {
			this.PosID = posX * Main.maxTilesY + posY; // Order determined in accordance with increasing Y in TileIO ReadModData such that PosID is ordered numerically
		}

		public void SaveChestInfoToPos(UnloadedChestInfo info) {
			UnloadedTilesWorld modWorld = ModContent.GetInstance<UnloadedTilesWorld>();
			var infos = modWorld.chestInfos;
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
			modWorld.chestInfoMap[PosID] = pendingID;
		}

		public void SaveTileInfoToPos(UnloadedTileInfo info) {
			UnloadedTilesWorld modWorld = ModContent.GetInstance<UnloadedTilesWorld>();
			var infos = modWorld.tileInfos;
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
			modWorld.tileInfoMap[PosID] = pendingID;
		}

		public void SaveWallInfoToPos(UnloadedWallInfo info) {
			UnloadedTilesWorld modWorld = ModContent.GetInstance<UnloadedTilesWorld>();
			var infos = modWorld.wallInfos;
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
			modWorld.wallInfoMap[PosID] = pendingID;
		}

	}
}
