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
		/// Use a dictionary mapping coordinates of tile infos from <see cref="tileInfos"/>
		/// </summary>
		internal Dictionary<int, int> tileInfoMap = new Dictionary<int, int>();

		/// <summary>
		/// Tile-<see cref="UnloadedChestInfo"/>s that are not able to be restored in the current state of the world (and saved for the next world load)
		/// </summary>
		internal List<UnloadedChestInfo> chestInfos = new List<UnloadedChestInfo>();

		/// <summary>
		/// Populated during <see cref="TileIO.ReadModTile"/>, detecting chests that lost their loaded mod, to then turn them into unloaded tiles
		/// </summary>
		internal List<UnloadedChestInfo> pendingChestInfos = new List<UnloadedChestInfo>();

		/// <summary>
		/// Use a dictionary mapping coordinates of chest infos from <see cref="chestInfos"/>
		/// </summary>
		internal Dictionary<int, int> chestInfoMap = new Dictionary<int, int>();

		/// <summary>
		/// Wall-<see cref="UnloadedWallInfo"/>s that are not able to be restored in the current state of the world (and saved for the next world load)
		/// </summary>
		internal List<UnloadedWallInfo> wallInfos = new List<UnloadedWallInfo>();

		/// <summary>
		/// Populated during <see cref="TileIO.ReadModTile"/>, detecting walls that lost their loaded mod, to then turn them into unloaded walls
		/// </summary>
		internal List<UnloadedWallInfo> pendingWallInfos = new List<UnloadedWallInfo>();

		/// <summary>
		/// Use a dictionary mapping coordinates of walls infos from <see cref="wallInfos"/>
		/// </summary>
		internal Dictionary<int, int> wallInfoMap = new Dictionary<int, int>();


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
			tileInfoMap.Clear();

			wallInfos.Clear();
			pendingWallInfos.Clear();
			wallInfoMap.Clear();

			chestInfos.Clear();
			pendingChestInfos.Clear();
			chestInfoMap.Clear();
		}

		public override TagCompound Save() { 
			return new TagCompound {
				["tileList"] = tileInfos.Select(info => info?.Save() ?? new TagCompound()).ToList(),
				["tilePosIndex"] = tileInfoMap.Select(pair => new TagCompound {
					["posID"] = pair.Key,
					["infoID"] = pair.Value
				}).ToList(),
				["wallList"] = wallInfos.Select(info => info?.Save() ?? new TagCompound()).ToList(),
				["wallPosIndex"] = wallInfoMap.Select(pair => new TagCompound {
					["posID"] = pair.Key,
					["infoID"] = pair.Value
				}).ToList(),
				["chestList"] = chestInfos.Select(info => info?.Save() ?? new TagCompound()).ToList(),
				["chestPosIndex"] = chestInfoMap.Select(pair => new TagCompound {
					["posID"] = pair.Key,
					["infoID"] = pair.Value
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
			UnloadedInfoUpdate update;

			//NOTE: infos and canRestore[] are same length so the indices match later for RestoreTilesAndWalls

			// Process tiles
			var tileList = tag.GetList<TagCompound>("tileList");
			update = new UnloadedInfoUpdate(tileList,'t');
			canRestoreTilesFlag = update.canRestoreFlag;
			canRestoreTiles = update.canRestore;

			// Process Walls
			var wallList = tag.GetList<TagCompound>("wallList");
			update = new UnloadedInfoUpdate(wallList, 'w');
			canRestoreWallsFlag = update.canRestoreFlag;
			canRestoreWalls = update.canRestore;

			// Process chests
			var chestList = tag.GetList<TagCompound>("chestList");
			update = new UnloadedInfoUpdate(chestList, 'c');
			canRestoreChestsFlag = update.canRestoreFlag;
			canRestoreChests = update.canRestore;

			// Prcoess chest Info Mapping
			var chestPosIndex = tag.GetList<TagCompound>("chestPosIndex");
			foreach (var posTag in chestPosIndex) {
				int PosID = posTag.Get<int>("PosID");
				var infoID = posTag.Get<int>("infoID");
				chestInfoMap[PosID] = infoID;
			}
			// Prcoess wall Info Mapping
			var wallPosIndex = tag.GetList<TagCompound>("wallPosIndex");
			foreach (var posTag in wallPosIndex) {
				int PosID = posTag.Get<int>("PosID");
				var infoID = posTag.Get<int>("infoID");
				wallInfoMap[PosID] = infoID;
			}
			// Prcoess wall Info Mapping
			var tilePosIndex = tag.GetList<TagCompound>("tilePosIndex");
			foreach (var posTag in tilePosIndex) {
				int PosID = posTag.Get<int>("PosID");
				var infoID = posTag.Get<int>("infoID");
				tileInfoMap[PosID] = infoID;
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
			// Resolve pending objects
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
			bool canRestoreTilesFlag, bool canRestoreWallsFlag, bool canRestoreChestsFlag) 
		{
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

					// If tile is of Type unloaded, restore original by position mapping
					Tile tile = Main.tile[x, y];
					if (canRestoreTilesFlag && (tile.type == unloadedTile || tile.type == unloadedNonSolidTile)) {
						int PosID = new UnloadedPosIndexing(x, y).PosID;
						tileInfoMap.TryGetValue(PosID, out int infoID);
						if (canRestoreTiles[infoID] > 0) {
							UnloadedTileInfo info = tileInfos[infoID];
							tile.type = canRestoreTiles[infoID];
							tile.frameX = info.frameX;
							tile.frameY = info.frameY;
							tileInfoMap.Remove(PosID);
						}
					}
					// If Tile is a Chest, Replace the chest with original by referencing position mapping 
					if (canRestoreChestsFlag && (tile.type == unloadedChest)) {
						int PosID = new UnloadedPosIndexing(x, y).PosID;
						chestInfoMap.TryGetValue(PosID, out int infoID);
						if (canRestoreChests[infoID] > 0) {
							UnloadedChestInfo info = chestInfos[infoID];
							WorldGen.PlaceChestDirect(x, y+1, canRestoreChests[infoID], 0, -1);
							chestInfoMap.Remove(PosID);
						}
					}
					// If tile has a wall, restore original by position mapping
					if (canRestoreWallsFlag && tile.wall == unloadedWallType) {
						int PosID = new UnloadedPosIndexing(x, y).PosID;
						tileInfoMap.TryGetValue(PosID, out int infoID);
						if (canRestoreWalls[infoID] > 0) {
							UnloadedTileInfo info = tileInfos[infoID];
							tile.wall = canRestoreWalls[infoID];
							wallInfoMap.Remove(PosID);
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

			nextID = 0;
			for (int k = 0; k < pendingChestInfos.Count; k++) {
				while (nextID < chestInfos.Count && chestInfos[nextID] != null)
					nextID++;

				if (nextID == chestInfos.Count)
					chestInfos.Add(pendingChestInfos[k]);
				else
					chestInfos[nextID] = pendingChestInfos[k];
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
						tile.type = unloadedTile;
					}
					if (confirmTileInfo && tile.type == pendingNSTile) {
						tile.type = unloadedNSTile;
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
