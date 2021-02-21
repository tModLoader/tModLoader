using System.IO;
using System.Linq;
using Terraria.ID;
using Terraria.ModLoader.Default;

namespace Terraria.ModLoader.IO
{
	internal static partial class TileIO
	{
		internal static void SaveTileData(BinaryWriter writer, bool[] hasTile) {
			short sameCount = 0;
			for (int x = 0; x < Main.maxTilesX; x++) {
				for (int y = 0; y < Main.maxTilesY; y++) {
					// Skip accounted for tiles
					if (sameCount > 0) {
						sameCount--;
						continue;
					}

					sameCount = -1;
					Tile tile = Main.tile[x, y];
					int i = x, j = y;

					// Skip Vanilla tiles
					while (!tile.active() || tile.type < TileID.Count) {
						NextTile(ref i, ref j);
						tile = Main.tile[i, j];
						sameCount++;
					}
					if (sameCount >= 0) {
						writer.Write((ushort)0);
						writer.Write(sameCount);
						continue;
					}

					// Write Locational data
					hasTile[tile.type] = true;

					WriteTileKey(writer, tile.type, x, y);
					
					writer.Write(tile.color());

					if (Main.tileFrameImportant[tile.type]) {
						writer.WriteVarInt(tile.frameX);
						writer.WriteVarInt(tile.frameY);
					}

					// Skip like-for-like tiles
					Tile prevTile = tile;
					int m = -1, n = -1;
					
					if (unloadedTileIDs.Contains(tile.type)) {
						m = i; n = j;
						PosIndexer.MoveToNextCoordsInMap(tilePosMap, ref m, ref n);
					}
					
					while (prevTile.isTheSameAs(tile) && !(i == m && j == n)) {
						NextTile(ref i, ref j);
						tile = Main.tile[i, j];
						sameCount++;
					}
					writer.Write(sameCount);
				}
			}
		}

		internal static void LoadTileData(BinaryReader reader) {
			short sameCount = 0;
			TileEntry entry;
			for (int x = 0; x < Main.maxTilesX; x++) {
				for (int y = 0; y < Main.maxTilesY; y++) {
					// Skip accounted for tiles
					if (sameCount > 0) {
						sameCount--;
						continue;
					}

					ushort saveType = reader.ReadUInt16();
					Tile tile = Main.tile[x, y];

					// Skip over vanilla
					if (saveType == 0) {
						sameCount = reader.ReadInt16();
						continue;
					}

					// Access tile information
					if (saveKeyTiles.TryGetValue(saveType, out ushort key)) {
						entry = tileList[key];
						tile.type = entry.id;
					}
					else { // Is currently unloaded
						if (!unloadedKeyTiles.TryGetValue(saveType, out key)) { // Loading previously unloaded tile
							key = reader.ReadUInt16(); // Get the stored key
							// If it can be restored, restore it using key.
							if (restoreKeyTiles.TryGetValue(key, out ushort rKey)) {
								entry = rTileList[rKey];
								tile.type = entry.id;
							}
							// If it can't be restored, re-setup unloaded
							else {
								entry = uTileList[key];
								tile.type = ModContent.Find<ModTile>(entry.unloadedType).Type;
								PosIndexer.MapPosToInfo(tilePosMapList, key, x, y);
							}
						}

						// Create new unloaded setup
						else {
							
							entry = uTileList[key];
							tile.type = ModContent.Find<ModTile>(entry.unloadedType).Type;
							PosIndexer.MapPosToInfo(tilePosMapList, key, x, y);
						}
						

					}

					tile.color(reader.ReadByte());

					if (entry.frameImportant) {
						tile.frameX = reader.ReadInt16();
						tile.frameY = reader.ReadInt16();
					}
					
					sameCount = reader.ReadInt16();
				}
			}
		}

		internal static void WriteTileKey(BinaryWriter writer, ushort type, int x, int y) {
			if (!unloadedTileIDs.Contains(type)) {
				writer.Write(type);
				return;
			}

			writer.Write((ushort) tileList.Count);

			writer.Write(PosIndexer.FloorGetKeyFromPos(tilePosMap, x, y));
		}

		internal static bool NextTile(ref int i, ref int j) {
			j++;
			if (j >= Main.maxTilesY) {
				j = 0;
				i++;
				if (i >= Main.maxTilesX) {
					return false;
				}
			}
			return true;
		}

	}
}
