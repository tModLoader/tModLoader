using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.ID;
using Terraria.ModLoader.Default;

namespace Terraria.ModLoader.IO
{
	internal static partial class TileIO
	{
		static string tileEntries = "tileMap";
		static string wallEntries = "wallMap";
		static string tileData = "tileData";
		static string wallData = "wallData";

		internal static void LoadBasics(TagCompound tag) {
			//TODO: Finish implementing this, and possibly refactor
			legacyLoad = false; uTileList.Clear(); uWallList.Clear();

			IOSaveLoadSet<TileEntry> tiles = IOSaveLoadSet<TileEntry>.Create();
			IOSaveLoadSet<WallEntry> walls = IOSaveLoadSet<WallEntry>.Create();

			PreLoad<TileEntry, ModTile>(tag, ref tiles, tileEntries);
			PreLoad<WallEntry, ModWall>(tag, ref walls, wallEntries);

			if (legacyLoad) {
				LoadLegacy(tag, tiles, walls);
			}
			else {
				var reader = new BinaryReader(new MemoryStream(tag.GetByteArray(tileData)));
				LoadData<TileEntry, ModTile>(reader, ref tiles);
				reader = new BinaryReader(new MemoryStream(tag.GetByteArray(wallData)));
				LoadData<WallEntry, ModWall>(reader, ref walls);
			}

			uTileList.AddRange(tiles.unloaded.list);
			uWallList.AddRange(walls.unloaded.list);
			WorldIO.ValidateSigns(); //call this at end
		}

		internal static TagCompound SaveBasics() {
			//TODO: Finish implementing this, and possibly refactor.
			bool[] hasTiles = new bool[TileLoader.TileCount];
			bool[] hasWalls = new bool[WallLoader.WallCount];

			ListAndKeys<TileEntry> tiles = ListAndKeys<TileEntry>.Create();
			ListAndKeys<WallEntry> walls = ListAndKeys<WallEntry>.Create();

			TagCompound tag =  new TagCompound {
				[tileData] = SaveData<TileEntry>(hasTiles),
				[tileEntries] = PostSave<TileEntry>(tiles, hasTiles),
				["u" + tileEntries] = uTileList.Select(entry => entry?.Save() ?? new TagCompound()).ToList(),
				
				[wallData] = SaveData<WallEntry>(hasWalls),
				[wallEntries] = PostSave<WallEntry>(walls, hasWalls),
				["u" + wallEntries] = uWallList.Select(entry => entry?.Save() ?? new TagCompound()).ToList()
			};

			return tag;
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

		internal static List<TagCompound> PostSave<T>(ListAndKeys<T> table, bool[] hasObj) where T : ModEntry, new() {
			ushort count = 0;

			ushort vanillaCount = 0;
			bool isTile = typeof(T) == typeof(TileEntry);
			bool isWall = typeof(T) == typeof(WallEntry);

			if (isWall)
				vanillaCount = WallID.Count;
			else if (isTile)
				vanillaCount = TileID.Count;

			for (ushort type = vanillaCount; type < hasObj.Length; type++) {
				// Skip Tile Types that aren't active in world
				if (!hasObj[type])
					continue;
				T entry = new T();

				if (isTile) {
					var obj = TileLoader.GetTile(type);
					entry.SetData((ushort)type, obj.Mod.Name, obj.Name, obj.vanillaFallbackOnModDeletion, GetUnloadedType<T>(type));
					(entry as TileEntry).frameImportant = Main.tileFrameImportant[type];
				}
				else if (isWall) {
					var obj = WallLoader.GetWall(type);
					entry.SetData((ushort)type, obj.Mod.Name, obj.Name, obj.vanillaFallbackOnModDeletion, GetUnloadedType<T>(type));
				}
				
				table.list.Add(entry);
				
				table.keyDict.Add((short)type, count++);
			}

			// Return if null
			if (table.list.Count == 0)
				return null;

			return table.list.Select(entry => entry?.Save() ?? new TagCompound()).ToList();
		}

		internal static void PreLoad<T, O>(TagCompound tag, ref IOSaveLoadSet<T> table, string dataRef) where T : ModEntry, new() where O : ModBlockType  {
			LoadUnloaded<T, O>(tag, ref table, dataRef);

			// If there is no modded data saved, skip
			if (!tag.ContainsKey(dataRef))
				return;

			// Retrieve Basic Tile Type Data from saved Tile Map, and store in table
			foreach (var objTag in tag.GetList<TagCompound>(dataRef)) {
				T entry = new T();
				entry.LoadData(objTag);
				ushort saveType = entry.id;

				ushort newID = ModContent.TryFind<O>(entry.modName, entry.name, out O obj) ? obj.Type : (ushort)0;

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

		internal static void LoadUnloaded<T, O>(TagCompound tag, ref IOSaveLoadSet<T> table, string dataRef) where T : ModEntry, new() where O : ModBlockType {
			// If there is no unloaded data saved, skip
			if (!tag.ContainsKey("u" + dataRef))
				return;

			short uCount = 1;
			foreach (var objTag in tag.GetList<TagCompound>("u" + dataRef)) {
				T entry = new T();
				entry.LoadData(objTag);

				ushort restoreType = ModContent.TryFind(entry.modName, entry.name, out O obj) ? obj.Type : (ushort)0;
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

		internal static byte[] SaveData<T>(bool[] hasObj) {
			var ms = new MemoryStream();
			var writer = new BinaryWriter(ms);

			bool isWall = typeof(T) == typeof(WallEntry);
			bool isTile = typeof(T) == typeof(TileEntry);
			ushort type = 0;
			ushort[] unloadedTypes = null;
			PosIndexer.PosKey[] posMap = null;

			if (isTile) {
				unloadedTypes = unloadedTileIDs;
				posMap = uTilePosMap;
			}
			else if (isWall) {
				unloadedTypes = unloadedWallIDs;
				posMap = uWallPosMap;
			}

			int sameCount = 0;
			for (int x = 0; x < Main.maxTilesX; x++) {
				for (int y = 0; y < Main.maxTilesY; y++) {
					// Skip accounted for
					if (sameCount > 0) {
						sameCount--;
						continue;
					}

					sameCount = -1;
					Tile tile = Main.tile[x, y];
					int i = x, j = y;

					if (isTile)
						type = tile.type;
					else if (isWall)
						type = tile.wall;

					// Skip Vanilla tiles
					if (isTile) {
						while (!tile.active() || tile.type < TileID.Count) {
							sameCount++;
							if (!NextTile(ref i, ref j))
								break;

							tile = Main.tile[i, j];
						}
					} else if (isWall) {
						while (tile.wall < WallID.Count) {
							sameCount++;
							if (!NextTile(ref i, ref j))
								break;

							tile = Main.tile[i, j];
						}
					}
					if (sameCount >= 0) {
						writer.Write((ushort)0);
						writer.Write(sameCount);
						continue;
					}

					// Write Locational data
					hasObj[type] = true;

					WriteKey(writer, type, x, y, posMap, unloadedTypes);

					if (isTile) 
						writer.Write(tile.color());
					else if (isWall) 
						writer.Write(tile.wallColor());

					if (isTile) {
						if (unloadedTileIDs.Contains(type)) {
							if (uTileList[PosIndexer.FloorGetKeyFromPos(posMap, x, y)].frameImportant) {
								writer.WriteVarInt(tile.frameX);
								writer.WriteVarInt(tile.frameY);
							}
						}
						else if (Main.tileFrameImportant[type]) {
							writer.WriteVarInt(tile.frameX);
							writer.WriteVarInt(tile.frameY);
						}
					}

					// Skip like-for-like tiles
					i = x; j = y;
					Tile currTile = Main.tile[i, j];
					int m = -1, n = -1;

					if (unloadedTypes.Contains(type)) {
						m = i;
						n = j;
						PosIndexer.MoveToNextCoordsInMap(posMap, ref m, ref n);
					}

					while (areSame(currTile, tile, isTile, isWall) && !(i == m && j == n)) {
						sameCount++;
						if (!NextTile(ref i, ref j))
							break;

						currTile = Main.tile[i, j];
					}
					writer.Write(sameCount);
				}
			}

			return ms.ToArray();
		}

		internal static void WriteKey(BinaryWriter writer, ushort type, int x, int y, PosIndexer.PosKey[] posMap, ushort[] unloadTypes) {
			if (!unloadTypes.Contains(type)) {
				writer.Write(type);
				return;
			}

			writer.Write((ushort)ushort.MaxValue);

			writer.Write(PosIndexer.FloorGetKeyFromPos(posMap, x, y));
		}

		private static bool areSame(Tile currTile, Tile tile, bool isTile, bool isWall) {
			if (isTile) {
				if (tile.type != currTile.type)
					return false;
				else if (Main.tileFrameImportant[currTile.type] != Main.tileFrameImportant[tile.type])
					return false;

				if (Main.tileFrameImportant[currTile.type]) {
					if (currTile.frameX != tile.frameX || currTile.frameY != tile.frameY) {
						return false;
					}
				}

				if (tile.color() != currTile.color())
					return false;

				return true;
			}

			else if (isWall) {
				if (tile.wall != currTile.wall)
					return false;

				if (tile.wallColor() != currTile.wallColor())
					return false;

				return true;
			}

			return false;
		}

		internal static void LoadData<T, O>(BinaryReader reader, ref IOSaveLoadSet<T> table) where T : ModEntry where O : ModBlockType {
			int sameCount = 0;
			List<PosIndexer.PosKey> posMapList = new List<PosIndexer.PosKey>();
			T entry;

			bool isWall = typeof(T) == typeof(WallEntry);
			bool isTile = typeof(T) == typeof(TileEntry);

			for (int x = 0; x < Main.maxTilesX; x++) {
				for (int y = 0; y < Main.maxTilesY; y++) {
					// Skip handled tiles
					if (sameCount > 0) {
						sameCount--;
						continue;
					}

					ushort saveType = reader.ReadUInt16();
					Tile tile = Main.tile[x, y];

					// Skip over vanilla
					if (saveType == 0) {
						sameCount = reader.ReadInt32();
						continue;
					}

					// Access tile information to set the type.
					ushort type = 0;
					if (table.loaded.keyDict.TryGetValue((short)saveType, out ushort key)) {
						entry = table.loaded.list[key];
						type = entry.id;
					}
					else { // Is currently unloaded
						if (!table.unloaded.keyDict.TryGetValue((short)saveType, out key)) { // Loading previously unloaded tile
							key = reader.ReadUInt16(); // Get the stored key
							
							// If it can be restored, restore it using key.
							if (table.restored.keyDict.TryGetValue((short)key, out ushort rKey)) {
								entry = table.restored.list[rKey];
								type = entry.id;
							}

							// If it can't be restored, re-setup unloaded
							else {
								table.unloaded.keyDict.TryGetValue((short)-key, out ushort uKey);
								entry = table.unloaded.list[uKey];
								type = ModContent.Find<O>(entry.unloadedType).Type;
								PosIndexer.MapPosToInfo(posMapList, uKey, x, y);
							}
						}

						// Else Create new unloaded setup
						else {
							entry = table.unloaded.list[key];
							type = ModContent.Find<O>(entry.unloadedType).Type;
							PosIndexer.MapPosToInfo(posMapList, key, x, y);
						}
					}

					if (isTile) {
						tile.active(true);
						tile.type = type;
						tile.color(reader.ReadByte());
						if ((entry as TileEntry).frameImportant) {
							tile.frameX = reader.ReadInt16();
							tile.frameY = reader.ReadInt16();
						}
					}
					else if (isWall) {
						tile.wall = type;
						tile.wallColor(reader.ReadByte());
					}
					
					sameCount = reader.ReadInt32();
					int i = x, j = y;
					for (int c = 0; c < sameCount; c++) {
						if (!NextTile(ref i, ref j))
							break;

						Tile currTile = Main.tile[i, j];
						objCopy<T>(currTile, tile, entry, isWall, isTile);
					}
				}
			}

			if (isTile)
				uTilePosMap = posMapList.ToArray();
			else if (isWall)
				uWallPosMap = posMapList.ToArray();
		}

		private static bool NextTile(ref int i, ref int j) {
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

		private static void objCopy<T>(Tile currTile, Tile tile, T entry, bool isWall, bool isTile) {
			if (isTile) {
				currTile.active(true);
				currTile.color(tile.color());
				currTile.type = tile.type;
				if ((entry as TileEntry).frameImportant) {
					currTile.frameX = tile.frameX;
					currTile.frameY = tile.frameY;
				}
			}
			else if (isWall) {
				currTile.wallColor(tile.wallColor());
				currTile.wall = tile.wall;
			}
		}

		internal static ushort[] unloadedTileIDs => new ushort[5] {
			ModContent.Find<ModTile>("ModLoader/UnloadedTile").Type,
			ModContent.Find<ModTile>("ModLoader/UnloadedNonSolidTile").Type,
			ModContent.Find<ModTile>("ModLoader/UnloadedSemiSolidTile").Type,
			ModContent.Find<ModTile>("ModLoader/UnloadedChest").Type,
			ModContent.Find<ModTile>("ModLoader/UnloadedDresser").Type
		};

		internal static ushort[] unloadedWallIDs => new ushort[1] { ModContent.Find<ModWall>("ModLoader/UnloadedWall").Type };

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
