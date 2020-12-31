using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default
{
	public class UnloadedInfoUpdate
	{
		internal List<ushort> canRestore = new List<ushort>();
		internal bool canRestoreFlag = false;

		public UnloadedInfoUpdate(IList<TagCompound> list, char identity) {
			UnloadedTilesWorld modWorld = ModContent.GetInstance<UnloadedTilesWorld>();
			foreach (var infoTag in list) {
				if (!infoTag.ContainsKey("mod")) {
					// infos entries get nulled out once restored, leading to an empty tag. This reverts it
					switch (identity) 
					{
						case 't':
							modWorld.tileInfos.Add(null);
							break;
						case 'c':
							modWorld.chestInfos.Add(null);
							break;
						case 'w':
							modWorld.wallInfos.Add(null);
							break;
					}
					canRestore.Add((ushort)0);
					continue;
				}

				// Repopulate Unloaded Info
				string modName = infoTag.GetString("mod");
				string name = infoTag.GetString("name");
				switch (identity) {
					case 't':
						bool IsSolid = infoTag.GetBool("IsSolid");
						bool frameImportant = infoTag.ContainsKey("frameX");
						var tInfo = frameImportant ?
							new UnloadedTileInfo(modName, name, infoTag.GetShort("frameX"), infoTag.GetShort("frameY"), IsSolid) :
							new UnloadedTileInfo(modName, name, IsSolid);
						modWorld.tileInfos.Add(tInfo);
						break;
					case 'c':
						var cInfo = new UnloadedChestInfo(modName, name);
						modWorld.chestInfos.Add(cInfo);
						break;
					case 'w':
						var wInfo = new UnloadedWallInfo(modName, name);
						modWorld.wallInfos.Add(wInfo);
						break;
				}
				


				// Check if the previously unloaded tile is now loadable again
				ModTile tile;
				ushort type=0;
				switch (identity) {
					case 't':
						type = ModContent.TryFind(modName, name, out tile) ? tile.Type : (ushort)0;
						break;
					case 'c':
						type = ModContent.TryFind(modName, name, out tile) ? tile.Type : (ushort)0;
						break;
					case 'w':
						type = ModContent.TryFind(modName, name, out ModWall wall) ? wall.Type : (ushort)0;
						break;
				}
				canRestore.Add(type);
				if (type != 0)
					canRestoreFlag = true;
			}
		}
	}
}
