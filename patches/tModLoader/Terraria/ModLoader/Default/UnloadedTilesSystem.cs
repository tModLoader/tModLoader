using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default
{
	[LegacyName("UnloadedTilesWorld")]
	internal class UnloadedTilesSystem : ModSystem {
		/// <summary>
		/// Tile-<see cref="UnloadedInfo"/>s that are not able to be restored in the current state of the world (and saved for the next world load)
		/// </summary>
		internal List<UnloadedInfo> tileInfos = new List<UnloadedInfo>();

		/// <summary>
		/// Wall-<see cref="UnloadedInfo"/>s that are not able to be restored in the current state of the world (and saved for the next world load)
		/// </summary>
		internal List<UnloadedInfo> wallInfos = new List<UnloadedInfo>();

		public override void OnWorldLoad() {
			tileInfos.Clear();
			wallInfos.Clear();
		}

		public override TagCompound SaveWorldData() {
			return new TagCompound {
				["tileInfos"] = tileInfos.Select(info => info?.Save() ?? new TagCompound()).ToList(),
				["wallInfos"] = wallInfos.Select(info => info?.Save() ?? new TagCompound()).ToList()
			};
		}

		public override void LoadWorldData(TagCompound tag) {
			// Process tiles
			tileInfos.AddRange(TileIO.tileInfos);

			UpdateUnloaded Updater = new UpdateUnloaded(tileInfos, TileIO.TilesContext);
			Updater.UpdateInfos(tag.GetList<TagCompound>("tileInfos"));
			Updater.Restore(TileIO.tileInfoMap);

			TileIO.tileInfos.Clear();
			TileIO.prevTileInfoMap.Clear();

			// Process Walls
			wallInfos.AddRange(TileIO.wallInfos);
			
			Updater = new UpdateUnloaded(wallInfos, TileIO.WallsContext);
			Updater.UpdateInfos(tag.GetList<TagCompound>("wallInfos"));
			Updater.Restore(TileIO.wallInfoMap);

			TileIO.wallInfos.Clear();
			TileIO.prevWallInfoMap.Clear();
		}
	}
}
