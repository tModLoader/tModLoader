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

	
		/// These values are synced to match UpdateUnloadedInfos <see cref="UpdateUnloaded"/> 
		internal static byte TilesContext = 0;
		internal static byte WallsContext = 1;
		internal static byte ChestContext = 2;

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
			UpdateUnloaded tileUpdater = new UpdateUnloaded(tileInfos, TilesContext);
			tileUpdater.UpdateInfos(tag.GetList<TagCompound>("tileList"));
			tileUpdater.UpdateMaps(tag.GetList<TagCompound>("tilePosIndex"), tileInfoMap);
			tileUpdater.Restore(tileInfoMap);
			tileUpdater.CleanupMaps(tileInfoMap);
			tileUpdater.CleanupInfos();

			// Process Walls
			UpdateUnloaded wallUpdater = new UpdateUnloaded(wallInfos, WallsContext);
			wallUpdater.UpdateInfos(tag.GetList<TagCompound>("wallList"));
			wallUpdater.UpdateMaps(tag.GetList<TagCompound>("wallPosIndex"), wallInfoMap);
			wallUpdater.Restore(wallInfoMap);
			wallUpdater.CleanupMaps(wallInfoMap);
			wallUpdater.CleanupInfos();

			// Process chests
			UpdateUnloaded chestUpdater = new UpdateUnloaded(chestInfos, ChestContext);
			chestUpdater.UpdateInfos(tag.GetList<TagCompound>("chestList"));
			chestUpdater.UpdateMaps(tag.GetList<TagCompound>("chestPosIndex"), chestInfoMap);
			chestUpdater.Restore(chestInfoMap);
			chestUpdater.CleanupMaps(chestInfoMap);
			chestUpdater.CleanupInfos();
		}
	}
}