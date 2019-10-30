using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default
{
	class MysteryTilesWorld : ModWorld
	{
		private List<MysteryTileInfo> infos = new List<MysteryTileInfo>();
		internal List<MysteryTileInfo> pendingInfos = new List<MysteryTileInfo>();

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
					new MysteryTileInfo(modName, name, infoTag.GetShort("frameX"), infoTag.GetShort("frameY")) :
					new MysteryTileInfo(modName, name);
				infos.Add(info);

				int type = ModLoader.GetMod(modName)?.TileType(name) ?? 0;
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

		public override void LoadLegacy(BinaryReader reader) {
			var list = new List<TagCompound>();
			int count = reader.ReadInt32();
			for (int k = 0; k < count; k++) {
				string modName = reader.ReadString();
				if (modName.Length == 0) {
					list.Add(new TagCompound());
				}
				else {
					var tag = new TagCompound {
						["mod"] = modName,
						["name"] = reader.ReadString(),
					};
					if (reader.ReadBoolean()) {
						tag.Set("frameX", reader.ReadInt16());
						tag.Set("frameY", reader.ReadInt16());
					}
					list.Add(tag);
				}
			}
			Load(new TagCompound { ["list"] = list });
		}

		private void RestoreTiles(List<ushort> canRestore) {
			ushort mysteryType = (ushort)ModContent.GetInstance<ModLoaderMod>().TileType("MysteryTile");
			for (int x = 0; x < Main.maxTilesX; x++) {
				for (int y = 0; y < Main.maxTilesY; y++) {
					if (Main.tile[x, y].type == mysteryType) {
						Tile tile = Main.tile[x, y];
						MysteryTileFrame frame = new MysteryTileFrame(tile.frameX, tile.frameY);
						int frameID = frame.FrameID;
						if (canRestore[frameID] > 0) {
							MysteryTileInfo info = infos[frameID];
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
			ushort pendingType = (ushort)ModContent.GetInstance<ModLoaderMod>().TileType("PendingMysteryTile");
			ushort mysteryType = (ushort)ModContent.GetInstance<ModLoaderMod>().TileType("MysteryTile");
			for (int x = 0; x < Main.maxTilesX; x++) {
				for (int y = 0; y < Main.maxTilesY; y++) {
					if (Main.tile[x, y].type == pendingType) {
						Tile tile = Main.tile[x, y];
						MysteryTileFrame frame = new MysteryTileFrame(tile.frameX, tile.frameY);
						frame = new MysteryTileFrame(truePendingID[frame.FrameID]);
						tile.type = mysteryType;
						tile.frameX = frame.FrameX;
						tile.frameY = frame.FrameY;
					}
				}
			}
		}
	}
}
