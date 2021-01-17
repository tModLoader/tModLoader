using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default
{
	internal class UpdateUnloaded
	{
		internal bool canRestoreFlag;
		internal readonly List<ushort> canRestore = new List<ushort>();
		internal readonly List<UnloadedInfo> infos;

		internal bool canPurge = false; //for deleting unloaded mod data in a System; should point to UI flag; temp false

		/// These values are synced to match UnloadedTilesSystem <see cref="UnloadedTilesSystem"/> 
		internal static byte TilesIndex = 0;
		internal static byte WallsIndex = 1;
		internal static byte ChestIndex = 2;


		public UpdateUnloaded(List<UnloadedInfo> infos) {
			this.infos = infos;
		}

		public void UpdateInfos(IList<TagCompound> list, byte index) {
			//NOTE: infos and canRestore lists are same length so the indices match later for RestoreTilesAndWalls
			foreach (var infoTag in list) {
				if (!infoTag.ContainsKey("mod")) {
					// infos entries get nulled out once restored, leading to an empty tag. This aligns CanRestore and Infos
					infos.Add(null);
					canRestore.Add(0);
					continue;
				}

				string modName = infoTag.GetString("mod");
				string name = infoTag.GetString("name");
				ushort fallbackType = infoTag.Get<ushort>("fallbackType");
				TagCompound customData = infoTag.Get<TagCompound>("customData");

				var info = new UnloadedInfo(modName, name, fallbackType,customData);
				infos.Add(info);

				//TODO: find a way to remove the typing sensitivity so this class is truly generic and can eliminate index
				ushort type = 0;
				if (index == TilesIndex || index == ChestIndex) // is a tile
					type = ModContent.TryFind(modName, name, out ModTile tile) ? tile.Type : (ushort)0;
				else if (index == WallsIndex) // is a wall
					type = ModContent.TryFind(modName, name, out ModWall wall) ? wall.Type : (ushort)0;

				if ((type == 0) && canPurge)
					type = infoTag.Get<ushort>("fallbackType");

				canRestore.Add(type);
				if (type != 0)
					canRestoreFlag = true;
			}
		}

		public void UpdateMaps(IList<TagCompound> list, Dictionary<int,int> posMap) {
			foreach (var posTag in list) {
				int posID = posTag.Get<int>("posID");
				int infoID = posTag.Get<int>("infoID");
				posMap[posID] = infoID;

			}
		}

		public void CleanupMaps(Dictionary<int, int> infoMap) {
			if (canRestoreFlag) {
				List<int> nullable = new List<int>();
				foreach (var entry in infoMap) {
					if (canRestore[entry.Value] > 0) {
						nullable.Add(entry.Key);
					}
				}
				foreach (int posID in nullable) {
					infoMap.Remove(posID);
				}
			}
		}

		public void CleanupInfos() {
			if (canRestoreFlag) {
				UnloadedTilesSystem modSystem = ModContent.GetInstance<UnloadedTilesSystem>();
				for (int k = 0; k < canRestore.Count; k++) {
					if (canRestore[k] > 0)
						infos[k] = null;
				}
			}
		}
	}
}
