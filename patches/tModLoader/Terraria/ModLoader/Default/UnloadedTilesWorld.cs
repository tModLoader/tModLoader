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
		/// Use a dictionary mapping coordinates of tile infos from <see cref="tileInfos"/>
		/// </summary>
		internal Dictionary<int, int> tileInfoMap = new Dictionary<int, int>();

		/// <summary>
		/// Tile-<see cref="UnloadedChestInfo"/>s that are not able to be restored in the current state of the world (and saved for the next world load)
		/// </summary>
		internal List<UnloadedChestInfo> chestInfos = new List<UnloadedChestInfo>();

		/// <summary>
		/// Use a dictionary mapping coordinates of chest infos from <see cref="chestInfos"/>
		/// </summary>
		internal Dictionary<int, int> chestInfoMap = new Dictionary<int, int>();

		/// <summary>
		/// Wall-<see cref="UnloadedWallInfo"/>s that are not able to be restored in the current state of the world (and saved for the next world load)
		/// </summary>
		internal List<UnloadedWallInfo> wallInfos = new List<UnloadedWallInfo>();

		/// <summary>
		/// Use a dictionary mapping coordinates of walls infos from <see cref="wallInfos"/>
		/// </summary>
		internal Dictionary<int, int> wallInfoMap = new Dictionary<int, int>();


		internal static ushort UnloadedTile => ModContent.Find<ModTile>("ModLoader/UnloadedTile").Type;

		internal static ushort UnloadedNonSolidTile => ModContent.Find<ModTile>("ModLoader/UnloadedNonSolidTile").Type;

		internal static ushort UnloadedChest => ModContent.Find<ModTile>("ModLoader/UnloadedChest").Type;

		internal static ushort UnloadedWallType => ModContent.Find<ModWall>("ModLoader/UnloadedWall").Type;


		public override void Initialize() {
			tileInfos.Clear();
			tileInfoMap.Clear();

			wallInfos.Clear();
			wallInfoMap.Clear();

			chestInfos.Clear();
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

			//NOTE: infos and canRestore[] are same length so the indices match later for RestoreTilesAndWalls
			
			ushort type = 0;
			// Process tiles
			var tileList = tag.GetList<TagCompound>("tileList");
			foreach (var infoTag in tileList) {
				if (!infoTag.ContainsKey("mod")) {
					// infos entries get nulled out once restored, leading to an empty tag. This reverts it
					tileInfos.Add(null);
					canRestoreTiles.Add(0);
					continue;
				}
				string modName = infoTag.GetString("mod");
				string name = infoTag.GetString("name");
				bool IsSolid = infoTag.GetBool("IsSolid");
				var tInfo =  new UnloadedTileInfo(modName, name);
				tileInfos.Add(tInfo);
				type = ModContent.TryFind(modName, name, out ModTile tile) ? tile.Type : (ushort)0;
				canRestoreTiles.Add(type);
				if (type != 0)
					canRestoreTilesFlag = true;
			}

			// Process Walls
			var wallList = tag.GetList<TagCompound>("wallList");
			foreach (var infoTag in wallList) {
				if (!infoTag.ContainsKey("mod")) {
					// infos entries get nulled out once restored, leading to an empty tag. This reverts it
					wallInfos.Add(null);
					canRestoreWalls.Add(0);
					continue;
				}
				string modName = infoTag.GetString("mod");
				string name = infoTag.GetString("name");
				var wInfo = new UnloadedWallInfo(modName, name);
				wallInfos.Add(wInfo);
				type = ModContent.TryFind(modName, name, out ModWall wall) ? wall.Type : (ushort)0;
				canRestoreWalls.Add(type);
				if (type != 0)
					canRestoreWallsFlag = true;
			}

			// Process chests
			var chestList = tag.GetList<TagCompound>("chestList");
			foreach (var infoTag in chestList) {
				if (!infoTag.ContainsKey("mod")) {
					// infos entries get nulled out once restored, leading to an empty tag. This reverts it
					chestInfos.Add(null);
					canRestoreChests.Add(0);
					continue;
				}
				string modName = infoTag.GetString("mod");
				string name = infoTag.GetString("name");
				var cInfo = new UnloadedChestInfo(modName, name);
				chestInfos.Add(cInfo);
				type = ModContent.TryFind(modName, name, out ModTile tile) ? tile.Type : (ushort)0;
				canRestoreChests.Add(type);
				if (type != 0)
					canRestoreChestsFlag = true;
			}

			// Prcoess chest Info Mapping
			var chestPosIndex = tag.GetList<TagCompound>("chestPosIndex");
			foreach (var posTag in chestPosIndex) {
				int PosID = posTag.Get<int>("posID");
				int infoID = posTag.Get<int>("infoID");
				chestInfoMap[PosID] = infoID;
			}
			// Prcoess wall Info Mapping
			var wallPosIndex = tag.GetList<TagCompound>("wallPosIndex");
			foreach (var posTag in wallPosIndex) {
				int PosID = posTag.Get<int>("posID");
				int infoID = posTag.Get<int>("infoID");
				wallInfoMap[PosID] = infoID;
			}
			// Prcoess wall Info Mapping
			var tilePosIndex = tag.GetList<TagCompound>("tilePosIndex");
			foreach (var posTag in tilePosIndex) {
				int PosID = posTag.Get<int>("posID");
				int infoID = posTag.Get<int>("infoID");
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
						int posID = new UnloadedPosIndexing(x, y).PosID;
						tileInfoMap.TryGetValue(posID, out int infoID);
						if (canRestoreTiles[infoID] > 0) {
							tile.type = canRestoreTiles[infoID];
						}
					}
					// If Tile is a Chest, Replace the chest with original by referencing position mapping 
					if (canRestoreChestsFlag && (tile.type == unloadedChest)) {
						int posID = new UnloadedPosIndexing(x, y).PosID;
						chestInfoMap.TryGetValue(posID, out int infoID);
						if (canRestoreChests[infoID] > 0) {
							UnloadedChestInfo info = chestInfos[infoID];
							WorldGen.PlaceChestDirect(x, y+1, canRestoreChests[infoID], 0, -1);
						}
					}
					// If tile has a wall, restore original by position mapping
					if (canRestoreWallsFlag && tile.wall == unloadedWallType) {
						int posID = new UnloadedPosIndexing(x, y).PosID;
						wallInfoMap.TryGetValue(posID, out int infoID);
						if (canRestoreWalls[infoID] > 0) {
							tile.wall = canRestoreWalls[infoID];
						}
					}
				}
			}
			// Prep dictionaries for cleanup by indexing nullable entries, then removing
			// tiles
			List<int> nullable = new List<int>();
			foreach (var entry in tileInfoMap) {
				if (canRestoreTiles[entry.Value] > 0) {
					nullable.Add(entry.Key);
				}
			}
			foreach (int posID in nullable) {
				tileInfoMap.Remove(posID);
			}
			// Chests
			nullable = new List<int>();
			foreach (var entry in chestInfoMap) {
				if (canRestoreChests[entry.Value] > 0) {
					nullable.Add(entry.Key);
				}
			}
			foreach (int posID in nullable) {
				chestInfoMap.Remove(posID);
			}
			// Walls
			nullable = new List<int>();
			foreach (var entry in wallInfoMap) {
				if (canRestoreWalls[entry.Value] > 0) {
					nullable.Add(entry.Key);
				}
			}
			foreach (int posID in nullable) {
				wallInfoMap.Remove(posID);
			}
		}
	}
}
