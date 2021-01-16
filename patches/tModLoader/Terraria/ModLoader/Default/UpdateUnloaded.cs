using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default
{
	internal class UpdateUnloaded
	{
		internal bool CanRestoreFlag;
		internal List<ushort> CanRestore = new List<ushort>();
		internal byte Index;

		internal bool CanPurge = false; //for deleting unloaded mod data in a System; should point to UI flag; temp false


		public UpdateUnloaded(byte index) {
			this.Index = index;
		}

		public void updateInfos(IList<TagCompound> list) {
			//NOTE: infos and canRestore lists are same length so the indices match later for RestoreTilesAndWalls
			UnloadedTilesSystem modSystem = ModContent.GetInstance<UnloadedTilesSystem>();
			foreach (var infoTag in list) {
				if (!infoTag.ContainsKey("mod")) {
					// infos entries get nulled out once restored, leading to an empty tag. This aligns CanRestore and Infos
					switch (Index) {
						case 0:
							modSystem.tileInfos.Add(null);
							break;
						case 1:
							modSystem.wallInfos.Add(null);
							break;
						case 2:
							modSystem.chestInfos.Add(null);
							break;
					}
					CanRestore.Add(0);
					continue;
				}

				string modName = infoTag.GetString("mod");
				string name = infoTag.GetString("name");
				ushort fallbackType = infoTag.Get<ushort>("fallbackType");

				ushort type = 0;
				ModTile tile;
				switch (Index) {
					case 0:
						var tInfo = new UnloadedTileInfo(modName, name, fallbackType);
						modSystem.tileInfos.Add(tInfo);
						type = ModContent.TryFind(modName, name, out tile) ? tile.Type : (ushort)0;
						break;
					case 1:
						var wInfo = new UnloadedWallInfo(modName, name);
						modSystem.wallInfos.Add(wInfo);
						type = ModContent.TryFind(modName, name, out ModWall wall) ? wall.Type : (ushort)0;
						break;
					case 2:
						byte chestStyle = infoTag.GetByte("style");
						var cInfo = new UnloadedChestInfo(modName, name, chestStyle);
						modSystem.chestInfos.Add(cInfo);
						type = ModContent.TryFind(modName, name, out tile) ? tile.Type : (ushort)0;
						break;
				}
				if ((type == 0) && CanPurge)
					type = infoTag.Get<ushort>("fallbackType");
				CanRestore.Add(type);
				if (type != 0)
					CanRestoreFlag = true;
			}
		}

		public void updateMaps(IList<TagCompound> list) {
			UnloadedTilesSystem modSystem = ModContent.GetInstance<UnloadedTilesSystem>();
			foreach (var posTag in list) {
				int PosID = posTag.Get<int>("posID");
				int infoID = posTag.Get<int>("infoID");
				switch (Index) {
					case 0:
						modSystem.tileInfoMap[PosID] = infoID;
						break;
					case 1:
						modSystem.wallInfoMap[PosID] = infoID;
						break;
					case 2:
						modSystem.chestInfoMap[PosID] = infoID;
						break;
				}

			}
		}

		public void cleanupMaps() {
			if (CanRestoreFlag) {
				UnloadedTilesSystem modSystem = ModContent.GetInstance<UnloadedTilesSystem>();
				List<int> nullable = new List<int>();
				Dictionary<int, int> infoMap = null;
				switch (Index) {
					case 0:
						infoMap = modSystem.tileInfoMap;
						break;
					case 1:
						infoMap = modSystem.wallInfoMap;
						break;
					case 2:
						infoMap = modSystem.chestInfoMap;
						break;
				}
				foreach (var entry in infoMap) {
					if (CanRestore[entry.Value] > 0) {
						nullable.Add(entry.Key);
					}
				}
				foreach (int posID in nullable) {
					infoMap.Remove(posID);
				}
			}
		}

		public void cleanupInfos() {
			if (CanRestoreFlag) {
				UnloadedTilesSystem modSystem = ModContent.GetInstance<UnloadedTilesSystem>();
				for (int k = 0; k < CanRestore.Count; k++) {
					if (CanRestore[k] > 0)
						switch (Index) {
							case 0:
								modSystem.tileInfos[k] = null;
								break;
							case 1:
								modSystem.wallInfos[k] = null;
								break;
							case 2:
								modSystem.chestInfos[k] = null;
								break;
						}
				}
			}
		}
	}
}
