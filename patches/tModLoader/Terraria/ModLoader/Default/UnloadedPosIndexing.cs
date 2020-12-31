namespace Terraria.ModLoader.Default
{
	internal class UnloadedPosIndexing
	{
		public int PosID;

		public UnloadedPosIndexing(int posX, int posY) {
			this.PosID = posY * Main.maxTilesX + posX;
		}

		public void SaveChestInfoToPos(UnloadedChestInfo info) {
			UnloadedTilesWorld modWorld = ModContent.GetInstance<UnloadedTilesWorld>();
			int pendingID = modWorld.pendingChestInfos.IndexOf(info);
			if (pendingID < 0) {
				pendingID = modWorld.pendingChestInfos.Count;
				modWorld.pendingChestInfos.Add(info);
			}
			modWorld.chestInfoMap[PosID] = pendingID;
		}

		public void SaveTileInfoToPos(UnloadedTileInfo info) {
			UnloadedTilesWorld modWorld = ModContent.GetInstance<UnloadedTilesWorld>();
			int pendingID = modWorld.pendingTileInfos.IndexOf(info);
			if (pendingID < 0) {
				pendingID = modWorld.pendingTileInfos.Count;
				modWorld.pendingTileInfos.Add(info);
			}
			modWorld.tileInfoMap[PosID] = pendingID;
		}

		public void SaveWallInfoToPos(UnloadedWallInfo info) {
			UnloadedTilesWorld modWorld = ModContent.GetInstance<UnloadedTilesWorld>();
			int pendingID = modWorld.pendingWallInfos.IndexOf(info);
			if (pendingID < 0) {
				pendingID = modWorld.pendingWallInfos.Count;
				modWorld.pendingWallInfos.Add(info);
			}
			modWorld.wallInfoMap[PosID] = pendingID;
		}

	}
}
