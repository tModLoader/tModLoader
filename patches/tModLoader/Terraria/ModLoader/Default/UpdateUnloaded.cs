using System.Collections.Generic;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default
{
	internal class UpdateUnloaded
	{
		internal bool canRestoreFlag;
		internal readonly List<ushort> canRestore = new List<ushort>();
		internal readonly List<UnloadedInfo> infos;
		private byte context;

		internal bool canPurge = false; //for deleting unloaded mod data in a System; should point to UI flag; temp false

		/// These values are synced to match UnloadedTilesSystem <see cref="UnloadedTilesSystem"/> 
		/// They are also a good way to find code that is context dependant
		internal static byte TilesContext = 0;
		internal static byte WallsContext = 1;
		internal static byte ChestContext = 2;

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

		public void UpdateInfos(IList<TagCompound> list) {
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

				var info = new UnloadedInfo(modName, name, fallbackType);

				infos.Add(info);

				//TODO: find a way to remove the typing sensitivity so this class is truly generic and can eliminate index
				ushort type = 0;

				if (context == TilesContext || context == ChestContext) // is a tile
					type = ModContent.TryFind(modName, name, out ModTile tile) ? tile.Type : (ushort)0;
				else if (context == WallsContext) // is a wall
					type = ModContent.TryFind(modName, name, out ModWall wall) ? wall.Type : (ushort)0;

				if (type == 0 && canPurge)
					type = infoTag.Get<ushort>("fallbackType");

				canRestore.Add(type);

				if (type != 0)
					canRestoreFlag = true;
			}
		}

		public void UpdateMaps(IList<TagCompound> list, SortedDictionary<int, int> posMap) {
			foreach (var posTag in list) {
				int posID = posTag.Get<int>("posID");
				int infoID = posTag.Get<int>("infoID");

				posMap[posID] = infoID;
			}
		}

		//TODO: Can this be simplified further?
		public void Restore(SortedDictionary<int, int> posMap) {
			if (!canRestoreFlag)
				return;

			foreach (var entry in posMap) {
				var posIndex = new UnloadedPosIndexing(entry.Key);
				posIndex.GetCoords(out int x, out int y);
				int infoID = entry.Key;

				ushort restoreID = canRestore[infoID];
				if (restoreID <= 0) {
					continue;
				}

				Tile tile = Main.tile[x, y];
					
				if (context == TilesContext) {
					ushort uID = tile.type;

					do {
						tile.type = restoreID;

						if (!NextTile(ref x, ref y))
							break;

						tile = Main.tile[x, y];
					} while (tile.type == uID);
				}

				else if (context == WallsContext) {
					ushort uID = tile.wall;

					do {
						tile.wall = restoreID;

						if (!NextTile(ref x, ref y)) 
							break;

						tile = Main.tile[x, y];
					} while (tile.wall == uID);
				}

				else if (context == ChestContext) {
					ushort uID = tile.type;

					do {
						if (tile.type == TileIO.UnloadedDresser) {
							WorldGen.PlaceDresserDirect(x + 1, y + 1, restoreID, 0, -1);
						}

						if (tile.type == TileIO.UnloadedChest) {
							WorldGen.PlaceChestDirect(x, y + 1, restoreID, tile.frameX / 36, -1);
						}

						if (!NextTile(ref x, ref y))
							break;

						tile = Main.tile[x, y];
					} while (tile.wall == uID);
				}
			}

			this.CleanupMaps(posMap);
			this.CleanupInfos();
		}

		//TODO, don't have this be a copy paste from tileIO.
		private static bool NextTile(ref int i, ref int j) {
			j++;
			if (j >= Main.maxTilesY) {
				j = 0;
				i++;
				if (i >= Main.maxTilesX) {
					return false;
				}
			}
			return true;
		}

		public void CleanupMaps(SortedDictionary<int, int> posMap) {
			if (!canRestoreFlag) {
				return;
			}

			var nullable = new List<int>();

			foreach (var entry in posMap) {
				if (canRestore[entry.Value] > 0) {
					nullable.Add(entry.Key);
				}
			}

			foreach (int posID in nullable) {
				posMap.Remove(posID);
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
