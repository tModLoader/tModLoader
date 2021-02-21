using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.ID;
using Terraria.ModLoader.Default;

namespace Terraria.ModLoader.IO
{
	internal static partial class TileIO
	{
		internal static void LoadBasics(TagCompound tag) {
			legacyLoad = false; uTileList.Clear(); uWallList.Clear();

			IOSaveLoadSet<TileEntry> tiles = IOSaveLoadSet<TileEntry>.Create();
			IOSaveLoadSet<WallEntry> walls = IOSaveLoadSet<WallEntry>.Create();

			PreLoad<TileEntry>(tag, ref tiles);

			if (legacyLoad) {
				LoadLegacy(tag, tiles, walls);
			}
			else {
				var reader = new BinaryReader(new MemoryStream(tag.GetByteArray("tileData")));
				LoadData<TileEntry>(reader, ref tiles);
			}

			uTileList.AddRange(tiles.unloaded.list);
			WorldIO.ValidateSigns(); //call this at end
		}

		internal static TagCompound SaveBasics() {
			//TODO: Finish implementing this

			ListAndKeys<TileEntry> tiles = ListAndKeys<TileEntry>.Create();
			return new TagCompound {
				["tileMap"] = PreSave<TileEntry>(ref tiles),
				["unloadedTileEntries"] = uTileList.Select(entry => entry?.Save() ?? new TagCompound()).ToList(),
			};
		}

		static bool legacyLoad = false;

		internal static bool canPurgeOldData => false; //for deleting unloaded mod data in a save; should point to UI flag; temp false

		//TODO: Merge in to one persistent object for each entry type.
		internal static List<TileEntry> uTileList = new List<TileEntry>();
		internal static PosIndexer.PosKey[] uTilePosMap;
		internal static List<WallEntry> uWallList = new List<WallEntry>();
		internal static PosIndexer.PosKey[] uWallPosMap;

		internal struct ListAndKeys<T> {
			internal List<T> list;
			internal Dictionary<short, ushort> keyDict;

			internal static ListAndKeys<T> Create() {
				return new ListAndKeys<T> { list = new List<T>(), keyDict = new Dictionary<short, ushort>() };
			}
		}

		internal struct IOSaveLoadSet<T> {
			internal ListAndKeys<T> unloaded;
			internal ListAndKeys<T> loaded;
			internal ListAndKeys<T> restored;

			internal static IOSaveLoadSet<T> Create() {
				return new IOSaveLoadSet<T> {
					unloaded = ListAndKeys<T>.Create(),
					loaded = ListAndKeys<T>.Create(),
					restored = ListAndKeys<T>.Create()
				};
			}
		}

		internal static List<TagCompound> PreSave<T>(ref ListAndKeys<T> table) where T : ModEntry {
			bool[] hasTile = new bool[TileLoader.TileCount];

			ushort count = 0;
			for (ushort type = TileID.Count; type < hasTile.Length; type++) {
				// Skip Tile Types that aren't active in world
				if (!hasTile[type] || unloadedTileIDs.Contains(type))
					continue;

				var modTile = TileLoader.GetTile(type);

				T entry = new TileEntry((ushort)type, modTile.Mod.Name, modTile.Name, modTile.vanillaFallbackOnModDeletion, true, GetUnloadedType<T>(type));

				table.list.Add(entry);
				
				table.keyDict.Add((short)type, count++);
			}

			// Return if null
			if (table.list.Count == 0)
				return null;

			return table.list.Select(entry => entry?.Save() ?? new TagCompound()).ToList();
		}

		internal static void PreLoad<T>(TagCompound tag, ref IOSaveLoadSet<T> table) where T : ModEntry {
			LoadUnloaded<T>(tag, ref table);

			// If there is no modded data saved, skip
			if (!tag.ContainsKey("tileMap"))
				return;

			// Retrieve Basic Tile Type Data from saved Tile Map, and store in table
			foreach (var tileTag in tag.GetList<TagCompound>("tileMap")) {
				T entry = new TileEntry(tag);

				ushort saveType = entry.id;

				ushort newID = ModContent.TryFind(entry.modName, entry.name, out ModTile tile) ? tile.Type : (ushort)0;

				if (newID == 0) {
					table.unloaded.keyDict.Add((short)saveType, (ushort)table.unloaded.list.Count);
					table.unloaded.list.Add(entry);
				}
				else {
					entry.id = newID;
					table.loaded.keyDict.Add((short)saveType, (ushort)table.loaded.list.Count);
					table.loaded.list.Add(entry);
				}

				if (!legacyLoad && entry.unloadedType == null) {
					legacyLoad = true;
				}
			}
		}

		internal static void LoadUnloaded<T>(TagCompound tag, ref IOSaveLoadSet<T> table) where T : ModEntry {
			// If there is no unloaded data saved, skip
			if (!tag.ContainsKey("unloadedTileEntries"))
				return;

			// infos and canRestore lists are same length so the indices match later for Restore()
			short uCount = 1;
			foreach (var tileTag in tag.GetList<TagCompound>("unloadedTileEntries")) {
				T entry = new TileEntry(tag);

				ushort restoreType = ModContent.TryFind(entry.modName, entry.name, out ModTile tile) ? tile.Type : (ushort)0;
				if (restoreType == 0 && canPurgeOldData)
					restoreType = entry.fallbackID;

				uCount--;
				if (restoreType != 0) {
					table.restored.list.Add(entry);
					table.restored.keyDict.Add((short)-uCount, (ushort)table.restored.list.Count);
				}
				else {
					table.unloaded.list.Add(entry);
					table.unloaded.keyDict.Add(uCount, (ushort)uTileList.Count); // Use negative for existing entries to avoid conflicts
				}
			}
		}

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

					if (unloadedTileIDs.Contains(tile.type)) {
						if (uTileList[PosIndexer.FloorGetKeyFromPos(uTilePosMap, x, y)].frameImportant) {
							writer.WriteVarInt(tile.frameX);
							writer.WriteVarInt(tile.frameY);
						}
					}
					else if (Main.tileFrameImportant[tile.type]) {
						writer.WriteVarInt(tile.frameX);
						writer.WriteVarInt(tile.frameY);
					}

					//TODO: Reimplement same count to be not be doubling work of both walls and tiles and fundamentally flawed.
					// Skip like-for-like tiles
					Tile prevTile = tile;
					int m = -1, n = -1;

					if (unloadedTileIDs.Contains(tile.type)) {
						m = i;
						n = j;
						PosIndexer.MoveToNextCoordsInMap(uTilePosMap, ref m, ref n);
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

		internal static void WriteTileKey(BinaryWriter writer, ushort type, int x, int y) {
			if (!unloadedTileIDs.Contains(type)) {
				writer.Write(type);
				return;
			}

			writer.Write((ushort)ushort.MaxValue);

			writer.Write(PosIndexer.FloorGetKeyFromPos(uTilePosMap, x, y));
		}

		internal static void LoadData<T>(BinaryReader reader, ref IOSaveLoadSet<T> table) where T : ModEntry {
			short sameCount = 0;
			List<PosIndexer.PosKey> posMapList = new List<PosIndexer.PosKey>();
			T entry;

			for (int x = 0; x < Main.maxTilesX; x++) {
				for (int y = 0; y < Main.maxTilesY; y++) {
					// Skip vanilla tiles
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
					if (table.loaded.keyDict.TryGetValue((short)saveType, out ushort key)) {
						entry = table.loaded.list[key];
						tile.type = entry.id;
					}
					else { // Is currently unloaded
						if (!table.unloaded.keyDict.TryGetValue((short)saveType, out key)) { // Loading previously unloaded tile
							key = reader.ReadUInt16(); // Get the stored key
							
							// If it can be restored, restore it using key.
							if (table.restored.keyDict.TryGetValue((short)key, out ushort rKey)) {
								entry = table.restored.list[rKey];
								tile.type = entry.id;
							}

							// If it can't be restored, re-setup unloaded
							else {
								table.unloaded.keyDict.TryGetValue((short)-key, out ushort uKey);
								entry = table.unloaded.list[uKey];
								tile.type = ModContent.Find<ModTile>(entry.unloadedType).Type;
								PosIndexer.MapPosToInfo(posMapList, uKey, x, y);
							}
						}

						// Else Create new unloaded setup
						else {
							entry = table.unloaded.list[key];
							tile.type = ModContent.Find<ModTile>(entry.unloadedType).Type;
							PosIndexer.MapPosToInfo(posMapList, key, x, y);
						}
					}

					tile.color(reader.ReadByte());

					if (entry.frameImportant) {
						tile.frameX = reader.ReadInt16();
						tile.frameY = reader.ReadInt16();
					}

					//TODO: Reimplement same count to be not be doubling work of both walls and tiles and fundamentally flawed.
					sameCount = reader.ReadInt16();
					int i = x, j = y;
					for (int c = 0; c < sameCount; c++) {
						NextTile(ref i, ref j);
						Main.tile[i, j].CopyFrom(tile);
					}
				}
			}

			uTilePosMap = posMapList.ToArray();
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

		internal static ushort[] unloadedTileIDs => new ushort[5] {
			ModContent.Find<ModTile>("ModLoader/UnloadedTile").Type,
			ModContent.Find<ModTile>("ModLoader/UnloadedNonSolidTile").Type,
			ModContent.Find<ModTile>("ModLoader/UnloadedSemiSolidTile").Type,
			ModContent.Find<ModTile>("ModLoader/UnloadedChest").Type,
			ModContent.Find<ModTile>("ModLoader/UnloadedDresser").Type
		};

		internal static string GetUnloadedType<T>(int type) {
			if (typeof(T) == typeof(WallEntry))
				return "ModLoader/UnloadedWall";
			
			if (TileID.Sets.BasicChest[type])
				return "ModLoader/UnloadedChest";

			if (TileID.Sets.BasicDresser[type])
				return "ModLoader/UnloadedDresser";

			if (Main.tileSolidTop[type])
				return "ModLoader/UnloadedSemiSolidTile";

			if (!Main.tileSolid[type])
				return "ModLoader/UnloadedNonSolidTile";

			return "ModLoader/UnloadedTile";
		}
	}
}
