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
		internal List<UnloadedTileInfo> infos = new List<UnloadedTileInfo>();

		/// <summary>
		/// Populated during <see cref="TileIO.ReadModTile"/>, detecting tiles that lost their loaded mod, to then turn them into unloaded tiles
		/// </summary>
		internal List<UnloadedTileInfo> pendingInfos = new List<UnloadedTileInfo>();

		/// <summary>
		/// Wall-<see cref="UnloadedTileInfo"/>s that are not able to be restored in the current state of the world (and saved for the next world load)
		/// </summary>
		internal List<UnloadedTileInfo> wallInfos = new List<UnloadedTileInfo>();

		/// <summary>
		/// Populated during <see cref="TileIO.ReadModTile"/>, detecting walls that lost their loaded mod, to then turn them into unloaded walls
		/// </summary>
		internal List<UnloadedTileInfo> pendingWallInfos = new List<UnloadedTileInfo>();

		/// <summary>
		/// Because walls don't have "memory" in the form of frameX/Y that can be misused to store IDs, use a dictionary mapping coordinates of walls infos from <see cref="wallInfos"/>
		/// </summary>
		internal Dictionary<Point16, UnloadedTileInfo> wallCoordsToWallInfos = new Dictionary<Point16, UnloadedTileInfo>();

		internal static ushort UnloadedType => ModContent.Find<ModTile>("ModLoader/UnloadedTile").Type;

		internal static ushort PendingType => ModContent.Find<ModTile>("ModLoader/PendingUnloadedTile").Type;

		internal static ushort UnloadedWallType => ModContent.Find<ModWall>("ModLoader/UnloadedWall").Type;

		internal static ushort PendingWallType => ModContent.Find<ModWall>("ModLoader/PendingUnloadedWall").Type;

		public override void Initialize() {
			infos.Clear();
			pendingInfos.Clear();
			wallInfos.Clear();
			pendingWallInfos.Clear();
			wallCoordsToWallInfos.Clear();
		}

		public override TagCompound Save() {
			return new TagCompound {
				["list"] = infos.Select(info => info?.Save() ?? new TagCompound()).ToList(),
				["wallList"] = wallInfos.Select(info => info?.Save() ?? new TagCompound()).ToList(),
				["wallCoordsList"] = wallCoordsToWallInfos.Select(pair => new TagCompound {
					["coords"] = pair.Key,
					["info"] = pair.Value.Save()
				}).ToList()

			};
		}

		public override void Load(TagCompound tag) {
			List<ushort> canRestore = new List<ushort>(); //List of types that are now loadable again
			List<ushort> canRestoreWalls = new List<ushort>();
			bool canRestoreFlag = false; //true if atleast one previously unloaded type is now loadable again
			bool canRestoreWallsFlag = false;

			var list = tag.GetList<TagCompound>("list");
			foreach (var infoTag in list) {
				if (!infoTag.ContainsKey("mod")) {
					//infos entries get nulled out once restored, leading to an empty tag. This reverts it
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

				//Check if the previously unloaded tile is now loadable again
				ushort type = ModContent.TryFind(modName, name, out ModTile tile) ? tile.Type : (ushort)0;
				canRestore.Add(type);
				if (type != 0)
					canRestoreFlag = true;
			}
			//infos and canRestoreFlag are same length so the indices match later for RestoreTilesAndWalls

			var wallList = tag.GetList<TagCompound>("wallList");
			foreach (var infoTag in wallList) {
				if (!infoTag.ContainsKey("mod")) {
					wallInfos.Add(null);
					canRestoreWalls.Add(0);
					continue;
				}

				string modName = infoTag.GetString("mod");
				string name = infoTag.GetString("name");
				var info = new UnloadedTileInfo(modName, name);
				wallInfos.Add(info);

				ushort type = ModContent.TryFind(modName, name, out ModWall wall) ? wall.Type : (ushort)0;
				canRestoreWalls.Add(type);
				if (type != 0)
					canRestoreWallsFlag = true;
			}

			var wallCoordsList = tag.GetList<TagCompound>("wallCoordsList");
			foreach (var coordsTag in wallCoordsList) {
				Point16 coords = coordsTag.Get<Point16>("coords");
				var infoTag = coordsTag.Get<TagCompound>("info");

				string modName = infoTag.GetString("mod");
				string name = infoTag.GetString("name");
				var info = new UnloadedTileInfo(modName, name);

				wallCoordsToWallInfos[coords] = info;
			}

			RestoreTilesAndWalls(canRestore, canRestoreWalls, canRestoreFlag, canRestoreWallsFlag);
			// If restoration should occur during this load cycle, then do so
			if (canRestoreFlag) {
				for (int k = 0; k < canRestore.Count; k++) {
					if (canRestore[k] > 0)
						infos[k] = null; //Restored infos don't need to be saved
				}
			}
			if (canRestoreWallsFlag) {
				for (int k = 0; k < canRestoreWalls.Count; k++) {
					if (canRestoreWalls[k] > 0)
						wallInfos[k] = null;
				}
			}

			ConfirmPendingInfo();
		}

		/// <summary>
		/// Converts unloaded tiles and walls to their original type
		/// </summary>
		/// <param name="canRestore">List of tile types that can be restored, indexed by the tiles frameID through <see cref="UnloadedTileFrame"/></param>
		/// <param name="canRestoreWalls">List of wall types that can be restored, indexed by index of corresponding info through <see cref="wallInfos"/></param>
		/// <param name="canRestoreFlag"><see langword="true"/> if atleast one tile type isn't 0</param>
		/// <param name="canRestoreWallsFlag"><see langword="true"/> if atleast one wall type isn't 0 </param>
		private void RestoreTilesAndWalls(List<ushort> canRestore, List<ushort> canRestoreWalls, bool canRestoreFlag, bool canRestoreWallsFlag) {
			if (!(canRestoreFlag || canRestoreWallsFlag))
				return; //Nothing to restore

			// Load instances of UnloadedTile
			ushort unloadedTile = ModContent.Find<ModTile>("ModLoader/UnloadedTile").Type;
			ushort unloadedNonSolidTile = ModContent.Find<ModTile>("ModLoader/UnloadedNonSolidTile").Type;
			ushort unloadedChest = ModContent.Find<ModTile>("ModLoader/UnloadedChest").Type;
			ushort unloadedWallType = UnloadedWallType;

			// Loop through all tiles in world	
			for (int x = 0; x < Main.maxTilesX; x++) {
				for (int y = 0; y < Main.maxTilesY; y++) {

					// If tile is of Type unloaded, then get frame data, and restore if frame data allows it
					Tile tile = Main.tile[x, y];
					if (canRestoreFlag && (tile.type == unloadedTile || tile.type == unloadedNonSolidTile ||
						Main.tile[x, y].type == unloadedChest)) {
						UnloadedTileFrame frame = new UnloadedTileFrame(tile.frameX, tile.frameY);
						int frameID = frame.FrameID;
						if (canRestore[frameID] > 0) {
							UnloadedTileInfo info = infos[frameID];
							tile.type = canRestore[frameID];
							tile.frameX = info.frameX;
							tile.frameY = info.frameY;
						}
					}
					if (canRestoreWallsFlag && tile.wall == unloadedWallType) {
						Point16 coords = new Point16(x, y);
						if (wallCoordsToWallInfos.TryGetValue(coords, out UnloadedTileInfo info) &&
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
			bool confirmTileInfo = pendingInfos.Count > 0;
			bool confirmWallInfo = pendingWallInfos.Count > 0;
			if (!(confirmTileInfo || confirmWallInfo))
				return; //Nothing to confirm

			List<int> truePendingID = new List<int>();
			int nextID = 0;
			// If the ID hasn't already been recorded in 'infos' list, add it, and create shortlist of net-new pending
			for (int k = 0; k < pendingInfos.Count; k++) {
				while (nextID < infos.Count && infos[nextID] != null)
					nextID++;

				if (nextID == infos.Count)
					infos.Add(pendingInfos[k]);
				else
					infos[nextID] = pendingInfos[k];

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

			ushort pendingType = ModContent.Find<ModTile>("ModLoader/PendingUnloadedTile").Type;
			ushort unloadedTile = ModContent.Find<ModTile>("ModLoader/UnloadedTile").Type;
			ushort unloadedNonSolidTile = ModContent.Find<ModTile>("ModLoader/UnloadedNonSolidTile").Type;
			ushort unloadedChest = ModContent.Find<ModTile>("ModLoader/UnloadedChest").Type;
			ushort workingType;

			ushort pendingWallType = PendingWallType;
			ushort unloadedWallType = UnloadedWallType;
			
			// For all tiles on the map
			for (int x = 0; x < Main.maxTilesX; x++) {
				for (int y = 0; y < Main.maxTilesY; y++) {
					Tile tile = Main.tile[x, y];
					// If the tile on the map is of type pending, then replace with Unloaded Object
					if (confirmTileInfo && tile.type == pendingType) {
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
					if (confirmWallInfo && tile.wall == pendingWallType)
						tile.wall = unloadedWallType;
				}
			}
		}
	}
}
