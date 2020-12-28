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
				// Check if the particular infoTag doesn't contain mod information, if so skip
				if (!infoTag.ContainsKey("mod")) {
					infos.Add(null);
					canRestore.Add(0);
					continue;
				}

				// Load Unloaded Tile Info
				string modName = infoTag.GetString("mod");
				string name = infoTag.GetString("name");
				bool IsChest = infoTag.GetBool("IsChest");
				bool IsSolid = infoTag.GetBool("IsSolid");
				bool frameImportant = infoTag.ContainsKey("frameX");
				var info = frameImportant ?
					new UnloadedTileInfo(modName, name, infoTag.GetShort("frameX"), infoTag.GetShort("frameY"),
						IsChest, IsSolid) :
					new UnloadedTileInfo(modName, name, IsChest, IsSolid);
				infos.Add(info);

				/* Check if the Unloaded Tile can be restored; if so, add its Type to list and set flag to 
				// restore one or more Tiles. */
				int type = ModContent.TryFind(modName, name, out ModTile tile) ? tile.Type : 0;
				canRestore.Add((ushort)type);
				if (type != 0)
					canRestoreFlag = true;
			}
			// If restoration should occur during this load cycle, then do so
			if (canRestoreFlag) {
				RestoreTiles(canRestore);

				// Cleanup Info list to remove restored tiles
				for (int k = 0; k < canRestore.Count; k++) {
					if (canRestore[k] > 0) {
						infos[k] = null;
					}
				}
			}

			// If there is any tiles pending from failure to load in TileIO, handle them
			if (pendingInfos.Count > 0) {
				ConfirmPendingInfo();
			}
		}

		private void RestoreTiles(List<ushort> canRestore) {
			// Load instances of UnloadedTile
			ushort unloadedTile = ModContent.Find<ModTile>("ModLoader/UnloadedTile").Type;
			ushort unloadedNonSolidTile = ModContent.Find<ModTile>("ModLoader/UnloadedNonSolidTile").Type;
			ushort unloadedChest = ModContent.Find<ModTile>("ModLoader/UnloadedChest").Type;

			// Loop through all tiles in world
			for (int x = 0; x < Main.maxTilesX; x++) {
				for (int y = 0; y < Main.maxTilesY; y++) {

					// If tile is of Type unloaded, then get frame data, and restore if frame data allows it
					if (Main.tile[x, y].type == unloadedTile || Main.tile[x, y].type == unloadedNonSolidTile || 
						Main.tile[x, y].type == unloadedChest) {

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
			// If the ID hasn't already been recorded in 'infos' list, add it, and create shortlist of net-new pending
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
				truePendingID.Add(nextID); // Shortlist of net-new pending
			}

			ushort pendingType = ModContent.Find<ModTile>("ModLoader/PendingUnloadedTile").Type;
			ushort unloadedTile = ModContent.Find<ModTile>("ModLoader/UnloadedTile").Type;
			ushort unloadedNonSolidTile = ModContent.Find<ModTile>("ModLoader/UnloadedNonSolidTile").Type;
			ushort unloadedChest = ModContent.Find<ModTile>("ModLoader/UnloadedChest").Type;
			ushort workingType;
			// For all tiles on the map
			for (int x = 0; x < Main.maxTilesX; x++) {
				for (int y = 0; y < Main.maxTilesY; y++) {
					// If the tile on the map is of type pending, then replace with Unloaded Object
					if (Main.tile[x, y].type == pendingType) {
						Tile tile = Main.tile[x, y];
						UnloadedTileFrame frame = new UnloadedTileFrame(tile.frameX, tile.frameY);

						// Switch between three different tiles depending on fields IsChest, IsSolid; may not work in MP
						int frameID = frame.FrameID;

						UnloadedTileInfo info = infos[frameID];
						frame = new UnloadedTileFrame(truePendingID[frameID]);

						workingType = unloadedTile;
						if (!info.IsSolid) {
							workingType = unloadedNonSolidTile;
						}
						if (info.IsChest) {
							workingType = unloadedChest;
						} 
						// Place TileObject
						tile.type = workingType;
						tile.frameX = frame.FrameX;
						tile.frameY = frame.FrameY;
					}
				}
			}
		}
	}
}
