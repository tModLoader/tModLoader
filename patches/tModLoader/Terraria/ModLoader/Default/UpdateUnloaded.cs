using System.Collections.Generic;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default
{
	internal class UpdateUnloaded //TODO: Make better use of this.
	{
		internal static bool canRestoreFlag;
		internal readonly List<ushort> canRestore = new List<ushort>();
		internal readonly List<UnloadedInfo> infos;
		private byte context;

		internal bool canPurge = false; //for deleting unloaded mod data in a save; should point to UI flag; temp false
		
		
		public UpdateUnloaded(List<UnloadedInfo> infos, byte context) {
			this.infos = infos;
			this.context = context;
		}

		public void AddInfos(UnloadedInfo info) {
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
		}

		public void UpdateInfos(IList<TagCompound> prevList) {
			//NOTE: infos and canRestore lists are same length so the indices match later for RestoreTilesAndWalls
			
			foreach (var uInfo in infos) {
				ushort type = 0;

				if (type == 0 && canPurge)
					type = uInfo.fallbackType;

				canRestore.Add(type);

				if (type != 0)
					canRestoreFlag = true;
			}

			foreach (var infoTag in prevList) {
				if (!infoTag.ContainsKey("mod")) {
					// infos entries get nulled out once restored, leading to an empty tag. This aligns CanRestore and Infos
					infos.Add(null);
					canRestore.Add(0);
					continue;
				}

				string modName = infoTag.GetString("mod");
				string name = infoTag.GetString("name");
				ushort fallbackType = infoTag.Get<ushort>("fallbackType");

				var info = new UnloadedInfo(modName, name, fallbackType);

				infos.Add(info);

				//TODO: find a way to remove the typing sensitivity so this class is truly generic and can eliminate index
				ushort type = 0;

				if (context == TileIO.TilesContext)
					type = ModContent.TryFind(modName, name, out ModTile tile) ? tile.Type : (ushort)0;
				else if (context == TileIO.WallsContext)
					type = ModContent.TryFind(modName, name, out ModWall wall) ? wall.Type : (ushort)0;

				if (type == 0 && canPurge)
					type = infoTag.Get<ushort>("fallbackType");

				canRestore.Add(type);

				if (type != 0)
					canRestoreFlag = true;
			}
		}

		//NOTE: Can this be simplified further?
		public void Restore(List<TileIO.posMap> posMap ) {
			if (!canRestoreFlag)
				return;

			foreach (var entry in posMap) {
				var posIndex = new UnloadedPosIndexing(entry.posID);
				posIndex.GetCoords(out int x, out int y);
				int infoID = entry.infoID;

				ushort restoreID = canRestore[infoID];
				if (restoreID <= 0) {
					continue;
				}

				Tile tile = Main.tile[x, y];
					
				if (context == TileIO.TilesContext) {
					ushort uID = tile.type;

					do {
						tile.type = restoreID;

						if (!TileIO.NextTile(ref x, ref y))
							break;

						tile = Main.tile[x, y];
					} while (tile.type == uID);
				}

				else if (context == TileIO.WallsContext) {
					ushort uID = tile.wall;

					do {
						tile.wall = restoreID;

						if (!TileIO.NextTile(ref x, ref y)) 
							break;

						tile = Main.tile[x, y];
					} while (tile.wall == uID);
				}
			}

			this.CleanupMaps(posMap);
			this.CleanupInfos();
		}
				
		public void CleanupMaps(List<TileIO.posMap> posMap) {
			if (!canRestoreFlag) {
				return;
			}

			var nullable = new List<TileIO.posMap>();

			foreach (var entry in posMap) {
				if (canRestore[entry.infoID] > 0) {
					nullable.Add(entry);
				}
			}

			foreach (var entry in nullable) {
				posMap.Remove(entry);
			}
		}

		public void CleanupInfos() {
			if (!canRestoreFlag) {
				return;
			}

			for (int k = 0; k < canRestore.Count; k++) {
				if (canRestore[k] > 0)
					infos[k] = null;
			}
		}
	}
}
