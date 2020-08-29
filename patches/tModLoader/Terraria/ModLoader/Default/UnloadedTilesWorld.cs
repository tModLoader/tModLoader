using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default
{
	class UnloadedTilesWorld : ModWorld
	{
		internal List<UnloadedTileInfo> infos = new List<UnloadedTileInfo>();
		internal List<UnloadedTileInfo> pendingInfos = new List<UnloadedTileInfo>();

		public override void Initialize() {
			infos.Clear();
			pendingInfos.Clear();
		}

		public override TagCompound Save() {
			return new TagCompound {
				["list"] = infos.Select(info => info?.Save() ?? new TagCompound()).ToList()
			};
		}

		public override void Load(TagCompound tag) {
			List<ushort> canRestore = new List<ushort>();
			bool canRestoreFlag = false;
			foreach (var infoTag in tag.GetList<TagCompound>("list")) {
				if (!infoTag.ContainsKey("mod")) {
					infos.Add(null);
					canRestore.Add(0);
					continue;
				}

				string modName = infoTag.GetString("mod");
				string name = infoTag.GetString("name");
				bool frameImportant = infoTag.ContainsKey("frameX");
				var info = frameImportant ?
					new UnloadedTileInfo(modName, name, infoTag.GetShort("frameX"), infoTag.GetShort("frameY")) :
					new UnloadedTileInfo(modName, name);
				infos.Add(info);

				int type = ModContent.TryFind(modName, name, out ModTile tile) ? tile.Type : 0;
				canRestore.Add((ushort)type);
				if (type != 0)
					canRestoreFlag = true;
			}
			if (canRestoreFlag) {
				RestoreTiles(canRestore);
				for (int k = 0; k < canRestore.Count; k++) {
					if (canRestore[k] > 0) {
						infos[k] = null;
					}
				}
			}
			if (pendingInfos.Count > 0) {
				ConfirmPendingInfo();
			}
		}

		private void RestoreTiles(List<ushort> canRestore) {
			ushort unloadedType = ModContent.Find<ModTile>("ModLoader/UnloadedTile").Type;
			for (int x = 0; x < Main.maxTilesX; x++) {
				for (int y = 0; y < Main.maxTilesY; y++) {
					if (Main.tile[x, y].type == unloadedType) {
						Tile tile = Main.tile[x, y];
						UnloadedTileFrame frame = new UnloadedTileFrame(tile.frameX, tile.frameY);
						int frameID = frame.FrameID;
						if (canRestore[frameID] > 0) {
							UnloadedTileInfo info = infos[frameID];
							tile.type = canRestore[frameID];
							tile.frameX = info.frameX;
							tile.frameY = info.frameY;
						}
					}
				}
			}
		}

		private void ConfirmPendingInfo() {
			List<int> truePendingID = new List<int>();
			int nextID = 0;
			for (int k = 0; k < pendingInfos.Count; k++) {
				while (nextID < infos.Count && infos[nextID] != null) {
					nextID++;
				}
				if (nextID == infos.Count) {
					infos.Add(pendingInfos[k]);
				}
				else {
					infos[nextID] = pendingInfos[k];
				}
				truePendingID.Add(nextID);
			}
			ushort pendingType = ModContent.Find<ModTile>("ModLoader/PendingUnloadedTile").Type;
			ushort unloadedType = ModContent.Find<ModTile>("ModLoader/UnloadedTile").Type;
			for (int x = 0; x < Main.maxTilesX; x++) {
				for (int y = 0; y < Main.maxTilesY; y++) {
					if (Main.tile[x, y].type == pendingType) {
						Tile tile = Main.tile[x, y];
						UnloadedTileFrame frame = new UnloadedTileFrame(tile.frameX, tile.frameY);
						frame = new UnloadedTileFrame(truePendingID[frame.FrameID]);
						tile.type = unloadedType;
						tile.frameX = frame.FrameX;
						tile.frameY = frame.FrameY;
					}
				}
			}
		}
	}
}
