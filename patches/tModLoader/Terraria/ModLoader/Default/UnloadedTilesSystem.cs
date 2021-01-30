using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default
{
	[LegacyName("UnloadedTilesWorld")]
	internal class UnloadedTilesSystem : ModSystem
	{
		/// <summary>
		/// Tile-<see cref="UnloadedTileInfo"/>s that are not able to be restored in the current state of the world (and saved for the next world load)
		/// </summary>
		internal List<UnloadedInfo> tileInfos = new List<UnloadedInfo>();

		/// <summary>
		/// Use a dictionary mapping coordinates of tile infos from <see cref="tileInfos"/>
		/// </summary>
		internal SortedDictionary<int, int> tileInfoMap = new SortedDictionary<int, int>();

		/// <summary>
		/// Tile-<see cref="UnloadedChestInfo"/>s that are not able to be restored in the current state of the world (and saved for the next world load)
		/// </summary>
		internal List<UnloadedInfo> chestInfos = new List<UnloadedInfo>();

		/// <summary>
		/// Use a dictionary mapping coordinates of chest infos from <see cref="chestInfos"/>
		/// </summary>
		internal SortedDictionary<int, int> chestInfoMap = new SortedDictionary<int, int>();

		/// <summary>
		/// Wall-<see cref="UnloadedWallInfo"/>s that are not able to be restored in the current state of the world (and saved for the next world load)
		/// </summary>
		internal List<UnloadedInfo> wallInfos = new List<UnloadedInfo>();

		/// <summary>
		/// Use a dictionary mapping coordinates of walls infos from <see cref="wallInfos"/>
		/// </summary>
		internal SortedDictionary<int, int> wallInfoMap = new SortedDictionary<int, int>();

		internal static ushort UnloadedTile => ModContent.Find<ModTile>("ModLoader/UnloadedTile").Type;
		internal static ushort UnloadedNonSolidTile => ModContent.Find<ModTile>("ModLoader/UnloadedNonSolidTile").Type;
		internal static ushort UnloadedSemiSolidTile => ModContent.Find<ModTile>("ModLoader/UnloadedSemiSolidTile").Type;
		internal static ushort UnloadedChest => ModContent.Find<ModTile>("ModLoader/UnloadedChest").Type;
		internal static ushort UnloadedDresser => ModContent.Find<ModTile>("ModLoader/UnloadedDresser").Type;
		internal static ushort UnloadedWallType => ModContent.Find<ModWall>("ModLoader/UnloadedWall").Type;

		/// These values are synced to match UpdateUnloadedInfos <see cref="UpdateUnloaded"/> 
		internal static byte TilesIndex = 0;
		internal static byte WallsIndex = 1;
		internal static byte ChestIndex = 2;

		public override void OnWorldLoad() {
			tileInfos.Clear();
			tileInfoMap.Clear();

			wallInfos.Clear();
			wallInfoMap.Clear();

			chestInfos.Clear();
			chestInfoMap.Clear();
		}

		public override TagCompound SaveWorldData() {
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

		public override void LoadWorldData(TagCompound tag) {
			bool[] canRestoreFlag = new bool[] { false, false, false };

			// Process tiles
			UpdateUnloaded tileUpdater = new UpdateUnloaded(tileInfos);
			tileUpdater.UpdateInfos(tag.GetList<TagCompound>("tileList"), TilesIndex);
			canRestoreFlag[TilesIndex] = tileUpdater.canRestoreFlag;
			tileUpdater.UpdateMaps(tag.GetList<TagCompound>("tilePosIndex"), tileInfoMap);

			// Process Walls
			UpdateUnloaded wallUpdater = new UpdateUnloaded(wallInfos);
			wallUpdater.UpdateInfos(tag.GetList<TagCompound>("wallList"), WallsIndex);
			canRestoreFlag[WallsIndex] = wallUpdater.canRestoreFlag;
			wallUpdater.UpdateMaps(tag.GetList<TagCompound>("wallPosIndex"), wallInfoMap);

			// Process chests
			UpdateUnloaded chestUpdater = new UpdateUnloaded(chestInfos);
			chestUpdater.UpdateInfos(tag.GetList<TagCompound>("chestList"), ChestIndex);
			canRestoreFlag[ChestIndex] = chestUpdater.canRestoreFlag;
			chestUpdater.UpdateMaps(tag.GetList<TagCompound>("chestPosIndex"), chestInfoMap);

			// If restoration should occur during this load cycle, then do so
			RestoreTilesAndWalls(tileUpdater.canRestore, wallUpdater.canRestore, chestUpdater.canRestore, canRestoreFlag);

			// Cleanup data remnants
			tileUpdater.CleanupMaps(tileInfoMap);
			tileUpdater.CleanupInfos();
			wallUpdater.CleanupMaps(wallInfoMap);
			wallUpdater.CleanupInfos();
			chestUpdater.CleanupMaps(chestInfoMap);
			chestUpdater.CleanupInfos();
		}

		/// <summary>
		/// Converts unloaded tiles and walls to their original type
		/// </summary>
		/// <param name="canRestoreTiles"> List of tile types that can be restored, indexed by the tiles frameID through <see cref="UnloadedTileFrame"/> </param>
		/// <param name="canRestoreWalls"> List of wall types that can be restored, indexed by index of corresponding info through <see cref="wallInfos"/> </param>
		/// <param name="canRestoreChests"> List of chest types that can be restored, indexed by index of corresponding info through <see cref="chestInfos"/> </param>
		/// <param name="canRestoreTilesFlag"> <see langword="true"/> if atleast one tile type isn't 0</param>
		/// <param name="canRestoreWallsFlag"> <see langword="true"/> if atleast one wall type isn't 0 </param>
		/// <param name="canRestoreChestsFlag"> <see langword="true"/> if atleast one chest type isn't 0 </param>
		private void RestoreTilesAndWalls(List<ushort> canRestoreTiles, List<ushort> canRestoreWalls, List<ushort> canRestoreChests, bool[] canRestoreFlag) {
			// Return if there's nothing to restore
			if (!canRestoreFlag.Contains(true))
				return;

			// Load instances of UnloadedTile
			ushort unloadedTile = UnloadedTile;
			ushort unloadedNonSolidTile = UnloadedNonSolidTile;
			ushort unloadedSemiSolidTile = UnloadedSemiSolidTile;
			ushort unloadedChest = UnloadedChest;
			ushort unloadedDresser = UnloadedDresser;
			ushort unloadedWallType = UnloadedWallType;

			// Loop through all tiles in world	
			for (int x = 0; x < Main.maxTilesX; x++) {
				for (int y = 0; y < Main.maxTilesY; y++) {
					Tile tile = Main.tile[x, y];

					// If tile is of Type unloaded, restore original by position mapping
					if (canRestoreFlag[TilesIndex] && (tile.type == unloadedTile || tile.type == unloadedNonSolidTile || tile.type == unloadedSemiSolidTile)) {
						var posIndex = new UnloadedPosIndexing(x, y);
						int infoID = posIndex.FloorGetValue(tileInfoMap);

						if (canRestoreTiles[infoID] > 0) {
							tile.type = canRestoreTiles[infoID];
						}
					}

					// If Tile is a Chest, Replace the chest with original by referencing position mapping 
					if (canRestoreFlag[ChestIndex] && (tile.type == unloadedChest || tile.type == unloadedDresser)) {
						var posIndex = new UnloadedPosIndexing(x, y);
						int infoID = posIndex.FloorGetValue(chestInfoMap);

						if (canRestoreChests[infoID] > 0) {
							if (tile.type == unloadedDresser)
								WorldGen.PlaceDresserDirect(x + 1, y + 1, canRestoreChests[infoID], 0, -1);

							if (tile.type == unloadedChest) {
								UnloadedInfo info = chestInfos[infoID];
								WorldGen.PlaceChestDirect(x, y + 1, canRestoreChests[infoID], tile.frameX / 36, -1);
							}
						}
					}

					// If tile has a wall, restore original by position mapping
					if (canRestoreFlag[WallsIndex] && (tile.wall == unloadedWallType)) {
						var posIndex = new UnloadedPosIndexing(x, y);
						int infoID = posIndex.FloorGetValue(wallInfoMap);

						if (canRestoreWalls[infoID] > 0) {
							tile.wall = canRestoreWalls[infoID];
						}
					}
				}
			}
		}
	}
}