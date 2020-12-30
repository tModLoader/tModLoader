using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default
{
	internal class UnloadedTilesWorld : ModWorld
	{
		/// <summary>
		/// Tile-<see cref="UnloadedTileInfo"/>s that are not able to be restored in the current state of the world (and saved for the next world load)
		/// </summary>
		internal List<UnloadedTileInfo> tileInfos = new List<UnloadedTileInfo>();

		/// <summary>
		/// Populated during <see cref="TileIO.ReadModTile"/>, detecting tiles that lost their loaded mod, to then turn them into unloaded tiles
		/// </summary>
		internal List<UnloadedTileInfo> pendingTileInfos = new List<UnloadedTileInfo>();

		/// <summary>
		/// Tile-<see cref="UnloadedChestInfo"/>s that are not able to be restored in the current state of the world (and saved for the next world load)
		/// </summary>
		internal List<UnloadedChestInfo> chestInfos = new List<UnloadedChestInfo>();

		/// <summary>
		/// Populated during <see cref="TileIO.ReadModTile"/>, detecting chests that lost their loaded mod, to then turn them into unloaded tiles
		/// </summary>
		internal List<UnloadedChestInfo> pendingChestInfos = new List<UnloadedChestInfo>();

		/// <summary>
		/// Wall-<see cref="UnloadedWallInfo"/>s that are not able to be restored in the current state of the world (and saved for the next world load)
		/// </summary>
		internal List<UnloadedWallInfo> wallInfos = new List<UnloadedWallInfo>();

		/// <summary>
		/// Populated during <see cref="TileIO.ReadModTile"/>, detecting walls that lost their loaded mod, to then turn them into unloaded walls
		/// </summary>
		internal List<UnloadedWallInfo> pendingWallInfos = new List<UnloadedWallInfo>();

		/// <summary>
		/// Because walls don't have "memory" in the form of frameX/Y that can be misused to store IDs, use a dictionary mapping coordinates of walls infos from <see cref="wallInfos"/>
		/// </summary>
		internal Dictionary<Point16, UnloadedWallInfo> wallCoordsToWallInfos = new Dictionary<Point16, UnloadedWallInfo>();

		/// <summary>
		/// Because chests don't have "static memory" in the form of frameX/Y that can be misused to store IDs, use a dictionary mapping coordinates of walls infos from <see cref="wallInfos"/>
		/// </summary>
		/// 

		internal Dictionary<int, int> chestCoordsToChestInfos = new Dictionary<int, int>();

		internal static ushort UnloadedTile => ModContent.Find<ModTile>("ModLoader/UnloadedTile").Type;

		internal static ushort PendingTile => ModContent.Find<ModTile>("ModLoader/PendingUnloadedTile").Type;

		internal static ushort UnloadedNonSolidTile => ModContent.Find<ModTile>("ModLoader/UnloadedNonSolidTile").Type;

		internal static ushort PendingNonSolidTile => ModContent.Find<ModTile>("ModLoader/PendingUnloadedNonSolidTile").Type;

		internal static ushort UnloadedChest => ModContent.Find<ModTile>("ModLoader/UnloadedChest").Type;

		internal static ushort PendingChest => ModContent.Find<ModTile>("ModLoader/PendingUnloadedChest").Type;

		internal static ushort UnloadedWallType => ModContent.Find<ModWall>("ModLoader/UnloadedWall").Type;

		internal static ushort PendingWallType => ModContent.Find<ModWall>("ModLoader/PendingUnloadedWall").Type;

		public override void Initialize() {
			tileInfos.Clear();
			pendingTileInfos.Clear();

			wallInfos.Clear();
			pendingWallInfos.Clear();
			wallCoordsToWallInfos.Clear();

			chestInfos.Clear();
			pendingChestInfos.Clear();
			chestCoordsToChestInfos.Clear();
		}

		public override TagCompound Save() { //TODO: Figure out what to do with this part
			return new TagCompound {
				["tileList"] = tileInfos.Select(info => info?.Save() ?? new TagCompound()).ToList(),
				["wallList"] = wallInfos.Select(info => info?.Save() ?? new TagCompound()).ToList(),
				["wallCoordsList"] = wallCoordsToWallInfos.Select(pair => new TagCompound {
					["coords"] = pair.Key,
					["info"] = pair.Value.Save()
				}).ToList(),
				["chestList"] = chestInfos.Select(info => info?.Save() ?? new TagCompound()).ToList(),
				["chestPosIndex"] = chestCoordsToChestInfos.Select(pair => new TagCompound {
					["PosID"] = pair.Key,
					["frameID"] = pair.Value
				}).ToList(),
			};
		}

		public override void Load(TagCompound tag) {
			List<ushort> canRestoreTiles = new List<ushort>(); // List of types that are now loadable again
			List<ushort> canRestoreWalls = new List<ushort>();
			List<ushort> canRestoreChests = new List<ushort>();
			bool canRestoreTilesFlag = false; // true if atleast one previously unloaded type is now loadable again
			bool canRestoreWallsFlag = false;
			bool canRestoreChestsFlag = false;

			//NOTE: infos and canRestore[] are same length so the indices match later for RestoreTilesAndWalls

			// Process tiles
			var tileList = tag.GetList<TagCompound>("tileList");
			foreach (var infoTag in tileList) {
				if (!infoTag.ContainsKey("mod")) {
					// infos entries get nulled out once restored, leading to an empty tag. This reverts it
					tileInfos.Add(null);
					canRestoreTiles.Add(0);
					continue;
				}

				// Repopulate Unloaded Tile Info
				string modName = infoTag.GetString("mod");
				string name = infoTag.GetString("name");
				bool IsSolid = infoTag.GetBool("IsSolid");
				bool frameImportant = infoTag.ContainsKey("frameX");
				var info = frameImportant ?
					new UnloadedTileInfo(modName, name, infoTag.GetShort("frameX"), infoTag.GetShort("frameY"),IsSolid) :
					new UnloadedTileInfo(modName, name, IsSolid);
				tileInfos.Add(info);

				// Check if the previously unloaded tile is now loadable again
				ushort type = ModContent.TryFind(modName, name, out ModTile tile) ? tile.Type : (ushort)0;
				canRestoreTiles.Add(type);
				if (type != 0)
					canRestoreTilesFlag = true;
			}

			// Process Walls
			var wallList = tag.GetList<TagCompound>("wallList");
			foreach (var infoTag in wallList) {
				if (!infoTag.ContainsKey("mod")) {
					wallInfos.Add(null);
					canRestoreWalls.Add(0);
					continue;
				}

				string modName = infoTag.GetString("mod");
				string name = infoTag.GetString("name");
				var info = new UnloadedWallInfo(modName, name);
				wallInfos.Add(info);

				ushort type = ModContent.TryFind(modName, name, out ModWall wall) ? wall.Type : (ushort)0;
				canRestoreWalls.Add(type);
				if (type != 0)
					canRestoreWallsFlag = true;
			}

			// Prcoess Walls Coordinates Conversion
			var wallCoordsList = tag.GetList<TagCompound>("wallCoordsList");
			foreach (var coordsTag in wallCoordsList) {
				Point16 coords = coordsTag.Get<Point16>("coords");
				var infoTag = coordsTag.Get<TagCompound>("info");

				string modName = infoTag.GetString("mod");
				string name = infoTag.GetString("name");
				var info = new UnloadedWallInfo(modName, name);

				wallCoordsToWallInfos[coords] = info;
			}

			// Process chests
			var chestList = tag.GetList<TagCompound>("chestList");
			foreach (var infoTag in chestList) {
				if (!infoTag.ContainsKey("mod")) {
					chestInfos.Add(null);
					canRestoreChests.Add(0);
					continue;
				}

				// Repopulate Unloaded Chest Info
				string modName = infoTag.GetString("mod");
				string name = infoTag.GetString("name");
				var info = new UnloadedChestInfo(modName, name);
				chestInfos.Add(info);

				// Check if the previously unloaded chest is now loadable again
				ushort type = ModContent.TryFind(modName, name, out ModTile tile) ? tile.Type : (ushort)0;
				canRestoreChests.Add(type);
				if (type != 0)
					canRestoreChestsFlag = true;
			}
			// Prcoess Chests Coordinates Conversion
			var chestPosIndex = tag.GetList<TagCompound>("chestPosIndex");
			foreach (var coordsTag in chestPosIndex) {
				int PosID = coordsTag.Get<int>("PosID");
				var frameID = coordsTag.Get<int>("frameID");
				chestCoordsToChestInfos[PosID] = frameID;
			}

			// If restoration should occur during this load cycle, then do so
			RestoreTilesAndWalls(canRestoreTiles, canRestoreWalls, canRestoreChests, canRestoreTilesFlag, canRestoreWallsFlag, canRestoreChestsFlag);
			
			// Cleanup infos to reflect restored content
			if (canRestoreTilesFlag) {
				for (int k = 0; k < canRestoreTiles.Count; k++) {
					if (canRestoreTiles[k] > 0)
						tileInfos[k] = null; //Restored infos don't need to be saved
				}
			}
			if (canRestoreWallsFlag) {
				for (int k = 0; k < canRestoreWalls.Count; k++) {
					if (canRestoreWalls[k] > 0)
						wallInfos[k] = null;
				}
			}
			if (canRestoreChestsFlag) {
				for (int k = 0; k < canRestoreChests.Count; k++) {
					if (canRestoreChests[k] > 0)
						chestInfos[k] = null;
				}
			}
			ConfirmPendingInfo();
		}

		/// <summary>
		/// Converts unloaded tiles and walls to their original type
		/// </summary>
		/// <param name="canRestoreTiles">List of tile types that can be restored, indexed by the tiles frameID through <see cref="UnloadedTileFrame"/></param>
		/// <param name="canRestoreWalls">List of wall types that can be restored, indexed by index of corresponding info through <see cref="wallInfos"/></param>
		/// <param name="canRestoreChests">List of chest types that can be restored, indexed by index of corresponding info through <see cref="chestInfos"/></param>
		/// <param name="canRestoreTilesFlag"><see langword="true"/> if atleast one tile type isn't 0</param>
		/// <param name="canRestoreWallsFlag"><see langword="true"/> if atleast one wall type isn't 0 </param>
		/// <param name="canRestoreChestsFlag"><see langword="true"/> if atleast one chest type isn't 0 </param>
		private void RestoreTilesAndWalls(List<ushort> canRestoreTiles, List<ushort> canRestoreWalls, List<ushort> canRestoreChests,
			bool canRestoreTilesFlag, bool canRestoreWallsFlag, bool canRestoreChestsFlag) {
			if (!(canRestoreTilesFlag || canRestoreWallsFlag || canRestoreChestsFlag))
				return; //Nothing to restore

			// Load instances of UnloadedTile
			ushort unloadedTile = UnloadedTile;
			ushort unloadedNonSolidTile = UnloadedNonSolidTile;
			ushort unloadedChest = UnloadedChest;
			ushort unloadedWallType = UnloadedWallType;

			// Loop through all tiles in world	
			for (int x = 0; x < Main.maxTilesX; x++) {
				for (int y = 0; y < Main.maxTilesY; y++) {

					// If tile is of Type unloaded, then get frame data, and restore if frame data allows it
					Tile tile = Main.tile[x, y];
					if (canRestoreTilesFlag && (tile.type == unloadedTile || tile.type == unloadedNonSolidTile)) {
						UnloadedTileFrame frame = new UnloadedTileFrame(tile.frameX, tile.frameY);
						int frameID = frame.FrameID;
						if (canRestoreTiles[frameID] > 0) {
							UnloadedTileInfo info = tileInfos[frameID];
							tile.type = canRestoreTiles[frameID];
							tile.frameX = info.frameX;
							tile.frameY = info.frameY;
						}
					}
					// If Tile is a Chest, Replace the chest with original. 
					if (canRestoreChestsFlag && (tile.type == unloadedChest)) {
						int PosID = y * Main.maxTilesX + x;
						chestCoordsToChestInfos.TryGetValue(PosID, out int frameID);
						if (canRestoreChests[frameID] > 0) {
							UnloadedChestInfo info = chestInfos[frameID];
							WorldGen.PlaceChestDirect(x, y+1, canRestoreChests[frameID], 0, -1);
						}
					}
					if (canRestoreWallsFlag && tile.wall == unloadedWallType) {
						Point16 coords = new Point16(x, y);
						if (wallCoordsToWallInfos.TryGetValue(coords, out UnloadedWallInfo info) &&
							wallInfos.IndexOf(info) is int index && index > -1 && canRestoreWalls[index] > 0) {
							//If info for these coords exists and it can be restored, restore and remove saved coords
							tile.wall = canRestoreWalls[index];
							//TODO remove/move
							wallCoordsToWallInfos.Remove(coords);
						}
					}
				}
			}
		}

		/// <summary>
		/// If there are pending tiles or walls (after a mod disable), convert them to unloaded, and refill <see cref="infos"/> and/or <see cref="wallInfos"/>
		/// </summary>
		private void ConfirmPendingInfo() {
			bool confirmTileInfo = pendingTileInfos.Count > 0;
			bool confirmWallInfo = pendingWallInfos.Count > 0;
			bool confirmChestInfo = pendingChestInfos.Count > 0;
			if (!(confirmTileInfo || confirmWallInfo || confirmChestInfo))
				return; //Nothing to confirm

			List<int> truePendingID = new List<int>();

			int nextID = 0;
			// If the ID hasn't already been recorded in 'infos' list, add it, and create shortlist of net-new pending
			for (int k = 0; k < pendingTileInfos.Count; k++) {
				while (nextID < tileInfos.Count && tileInfos[nextID] != null)
					nextID++;

				if (nextID == tileInfos.Count)
					tileInfos.Add(pendingTileInfos[k]);
				else
					tileInfos[nextID] = pendingTileInfos[k];

				truePendingID.Add(nextID); // Shortlist of net-new pending
			}

			nextID = 0;
			for (int k = 0; k < pendingWallInfos.Count; k++) {
				while (nextID < wallInfos.Count && wallInfos[nextID] != null)
					nextID++;

				if (nextID == wallInfos.Count)
					wallInfos.Add(pendingWallInfos[k]);
				else
					wallInfos[nextID] = pendingWallInfos[k];
			}

			ushort pendingTile = PendingTile;
			ushort pendingNSTile = PendingNonSolidTile;
			ushort pendingChest = PendingChest;
			ushort pendingWallType = PendingWallType;

			ushort unloadedTile = UnloadedTile;
			ushort unloadedNSTile = UnloadedNonSolidTile;
			ushort unloadedChest = UnloadedChest;
			ushort unloadedWallType = UnloadedWallType;
		
			// For all tiles on the map
			for (int x = 0; x < Main.maxTilesX; x++) {
				for (int y = 0; y < Main.maxTilesY; y++) {
					Tile tile = Main.tile[x, y];

					// Replace tiles like for like, so to speak
					if (confirmTileInfo && tile.type == pendingTile) {
						UnloadedTileFrame frame = new UnloadedTileFrame(tile.frameX, tile.frameY);
						int frameID = frame.FrameID;
						tile.type = unloadedTile;
						tile.frameX = frame.FrameX;
						tile.frameY = frame.FrameY;
					}
					if (confirmTileInfo && tile.type == pendingNSTile) {
						UnloadedTileFrame frame = new UnloadedTileFrame(tile.frameX, tile.frameY);
						int frameID = frame.FrameID;
						tile.type = unloadedNSTile;
						tile.frameX = frame.FrameX;
						tile.frameY = frame.FrameY;
					}
					if (confirmChestInfo && tile.type == pendingChest) {
						WorldGen.PlaceChestDirect(x, y + 1, unloadedChest, 0, -1);
					}
					if (confirmWallInfo && tile.wall == pendingWallType)
						tile.wall = unloadedWallType;
				}
			}
		}
	}
}
