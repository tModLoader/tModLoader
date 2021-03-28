using System.Collections.Generic;
using System.Linq;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default
{
	[LegacyName("MysteryTilesWorld")]
	class UnloadedTilesSystem : ModSystem
	{
		internal static List<LegacyUnloadedTileInfo> infos = new List<LegacyUnloadedTileInfo>();
		//internal List<LegacyUnloadedTileInfo> pendingInfos = new List<LegacyUnloadedTileInfo>();

		public override void OnWorldLoad() {
			infos.Clear();
			//pendingInfos.Clear();
			converted.Clear();
		}

		/*
		public override TagCompound SaveWorldData() {
			return new TagCompound {
				["list"] = infos.Select(info => info?.Save() ?? new TagCompound()).ToList()
			};
		}
		*/

		public override void LoadWorldData(TagCompound tag) {
			//List<ushort> canRestore = new List<ushort>();
			//bool canRestoreFlag = false;

			foreach (var infoTag in tag.GetList<TagCompound>("list")) {
				if (!infoTag.ContainsKey("mod")) {
					infos.Add(null);
					//canRestore.Add(0);
					continue;
				}

				string modName = infoTag.GetString("mod");
				string name = infoTag.GetString("name");
				bool frameImportant = infoTag.ContainsKey("frameX");
				var info = frameImportant ?
					new LegacyUnloadedTileInfo(modName, name, infoTag.GetShort("frameX"), infoTag.GetShort("frameY")) :
					new LegacyUnloadedTileInfo(modName, name);

				infos.Add(info);

				//int type = ModContent.TryFind(modName, name, out ModTile tile) ? tile.Type : 0;

				//canRestore.Add((ushort)type);

				//if (type != 0)
				//	canRestoreFlag = true;
			}

			if (infos.Count > 0) {
				ConvertTiles();
			}

			/*if (canRestoreFlag) {
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
			*/
		}

		/*
		private void RestoreTiles(List<ushort> canRestore) {
			ushort unloadedType = ModContent.Find<ModTile>("ModLoader/UnloadedTile").Type;
			for (int x = 0; x < Main.maxTilesX; x++) {
				for (int y = 0; y < Main.maxTilesY; y++) {
					if (Main.tile[x, y].type == unloadedType) {
						Tile tile = Main.tile[x, y];
						LegacyUnloadedTileFrame frame = new LegacyUnloadedTileFrame(tile.frameX, tile.frameY);
						int frameID = frame.FrameID;
						if (canRestore[frameID] > 0) {
							LegacyUnloadedTileInfo info = infos[frameID];
							tile.type = canRestore[frameID];
							tile.frameX = info.frameX;
							tile.frameY = info.frameY;
						}
					}
				}
			}
		} 
		*/

		internal static Dictionary<int, ushort> converted = new Dictionary<int, ushort>();

		internal void ConvertTiles() {
			var legacyEntries = TileIO.Tiles.entries.ToList();

			var builder = new PosData<ushort>.OrderedSparseLookupBuilder();
			var unloadedReader = new PosData<ushort>.OrderedSparseLookupReader(TileIO.Tiles.unloadedEntryLookup);

			for (int x = 0; x < Main.maxTilesX; x++) {
				for (int y = 0; y < Main.maxTilesY; y++) {
					Tile tile = Main.tile[x, y];
					if (!(tile.active() && tile.type >= TileID.Count))
						continue;

					ushort type = tile.type;
					if (TileIO.Tiles.entries[type] == null) { // Is an unloaded block
						type = unloadedReader.Get(x, y);

						if (legacyEntries[type].modName.Equals("ModLoader")) { // Was saved as an unloaded block
							ConvertTile(tile, legacyEntries, out type);
						}
						builder.Add(x, y, type);
					}

				}
			}

			TileIO.Tiles.entries = legacyEntries.ToArray();
			TileIO.Tiles.unloadedEntryLookup = builder.Build();
		}

		internal void ConvertTile(Tile tile, List<TileEntry> entries, out ushort type) {
			LegacyUnloadedTileFrame frame = new LegacyUnloadedTileFrame(tile.frameX, tile.frameY);
			int frameID = frame.FrameID;

			if (converted.TryGetValue(frameID, out type))
				return;

			LegacyUnloadedTileInfo info = infos[frameID];
			var entry = new TileEntry(TileLoader.GetTile(tile.type));
			entry.name = info.name;
			entry.modName = info.modName;
			entry.frameImportant = info.frameX > -1;
			entry.type = type = (ushort)entries.Count;

			entries.Add(entry);
			converted.Add(frameID, type);
		}

		/*
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
						LegacyUnloadedTileFrame frame = new LegacyUnloadedTileFrame(tile.frameX, tile.frameY);
						frame = new LegacyUnloadedTileFrame(truePendingID[frame.FrameID]);
						tile.type = unloadedType;
						tile.frameX = frame.FrameX;
						tile.frameY = frame.FrameY;
					}
				}
			}
		}
		*/
	}
}