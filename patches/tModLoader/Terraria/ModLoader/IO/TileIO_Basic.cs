using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.ID;
using Terraria.ModLoader.Default;

namespace Terraria.ModLoader.IO
{
	internal static partial class TileIO {
		static string tileEntries = "tileMap";
		static string wallEntries = "wallMap";
		static string tileData = "tileData";
		static string wallData = "wallData";

		//NOTE: LoadBasics can't be separated into LoadWalls() and LoadTiles() because of LoadLegacy.
		internal static void LoadBasics(TagCompound tag) {
			legacyLoad = false; uTileList.Clear(); uWallList.Clear(); // call at beginning to remove previous world artifacts

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

		//TODO: This can likely be refactored to be SaveWalls() and SaveTiles(), but is left as is to mirror LoadBasics()
		internal static TagCompound SaveBasics() {
			bool[] hasTiles = new bool[TileLoader.TileCount];
			bool[] hasWalls = new bool[WallLoader.WallCount];

			ListAndKeys<TileEntry> tiles = ListAndKeys<TileEntry>.Create();
			ListAndKeys<WallEntry> walls = ListAndKeys<WallEntry>.Create();

			TagCompound tag = new TagCompound {
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

		//TODO: In the future, merge in to one persistent object for each entry type.
		internal static List<TileEntry> uTileList = new List<TileEntry>();
		internal static PosIndexer.PosIndex[] uTilePosMap;
		internal static List<WallEntry> uWallList = new List<WallEntry>();
		internal static PosIndexer.PosIndex[] uWallPosMap;

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

				// Create an entry representing the required data for restoration associated with the object
				if (isTile) {
					var obj = TileLoader.GetTile(type);
					entry.SetData((ushort)type, obj.Mod.Name, obj.Name, obj.vanillaFallbackOnModDeletion, GetUnloadedType<T>(type));
					(entry as TileEntry).frameImportant = Main.tileFrameImportant[type];
				}
				else if (isWall) {
					var obj = WallLoader.GetWall(type);
					entry.SetData((ushort)type, obj.Mod.Name, obj.Name, obj.vanillaFallbackOnModDeletion, GetUnloadedType<T>(type));
				}

				// Add entries to list, and add to dictionairy if needing to lookup the entry.
				table.list.Add(entry);
				table.keyDict.Add((short)type, count++);
			}

			// Return null if no entries.
			if (table.list.Count == 0)
				return null;

			return table.list.Select(entry => entry?.Save() ?? new TagCompound()).ToList();
		}

		internal static void PreLoad<T, O>(TagCompound tag, ref IOSaveLoadSet<T> table, string dataRef) where T : ModEntry, new() where O : ModBlockType {
			LoadUnloaded<T, O>(tag, ref table, dataRef);

			// If there is no modded data saved, skip
			if (!tag.ContainsKey(dataRef))
				return;

			// Retrieve Basic Data and store in entries list
			foreach (var objTag in tag.GetList<TagCompound>(dataRef)) {
				T entry = new T();
				entry.LoadData(objTag);
				ushort saveType = entry.id;

				// Check if object exists in loaded mods
				ushort newID = ModContent.TryFind<O>(entry.modName, entry.name, out O obj) ? obj.Type : (ushort)0;

				// If doesn't exist, store in unloaded list
				if (newID == 0) {
					table.unloaded.keyDict.Add((short)saveType, (ushort)table.unloaded.list.Count);
					table.unloaded.list.Add(entry);
				}
				// If exists, store in loaded list
				else {
					entry.id = newID;
					table.loaded.keyDict.Add((short)saveType, (ushort)table.loaded.list.Count);
					table.loaded.list.Add(entry);
				}

				// If the unloadedType field doesn't exist, then we know it is from pre-#1266 and should use Load-Legacy
				if (!legacyLoad && entry.unloadedType.Length == 0) {
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
				
				// Determine if object exists, and if not, checks if we're purging old unloaded data to force a vanilla restoration
				ushort restoreType = ModContent.TryFind(entry.modName, entry.name, out O obj) ? obj.Type : (ushort)0;
				if (restoreType == 0 && canPurgeOldData)
					restoreType = entry.fallbackID;

				uCount--;
				// if the object is loaded, add to restore list so it gets restored
				if (restoreType != 0) {
					table.restored.keyDict.Add((short)-uCount, (ushort)table.restored.list.Count);
					table.restored.list.Add(entry);
				}
				// if the object is unloaded, then re-add to unloaded list
				else {
					table.unloaded.keyDict.Add(uCount, (ushort)uTileList.Count); // Use negative for existing entries to avoid conflicts
					table.unloaded.list.Add(entry);
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
			PosIndexer.PosIndex[] posMap = null;

			// Use pathing flags to pre-fetch variables.
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
					//TODO: Check if can simplify this somehow in the future. The issue is the condition tile.active() 
					if (isTile) {
						while (!tile.active() || tile.type < TileID.Count) {
							sameCount++;
							if (NextTile(ref i, ref j, ref tile))
								break;
						}
					} else if (isWall) {
						while (tile.wall < WallID.Count) {
							sameCount++;
							if (NextTile(ref i, ref j, ref tile))
								break;
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
								writer.Write(tile.frameX);
								writer.Write(tile.frameY);
							}
						}
						else if (Main.tileFrameImportant[type]) {
							writer.Write(tile.frameX);
							writer.Write(tile.frameY);
						}
					}

					// Setup skipping/compressing for like-for-like tiles
					i = x; j = y;
					
					int m = -1, n = -1;
					if (unloadedTypes.Contains(type)) {
						m = i;
						n = j;
						PosIndexer.MoveToNextCoordsInMap(posMap, ref m, ref n);
					}

					Tile currTile = Main.tile[i, j];
					do {
						if (NextTile(ref i, ref j, ref currTile))
							break;
						if (!(i == m && j == n))
							break;
						
						sameCount++;
					} while (areSame(currTile, tile, isTile, isWall));
					
					writer.Write(sameCount);
				}
			}

			return ms.ToArray();
		}

		private static void WriteKey(BinaryWriter writer, ushort type, int x, int y, PosIndexer.PosIndex[] posMap, ushort[] unloadTypes) {
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
			List<PosIndexer.PosIndex> posMapList = new List<PosIndexer.PosIndex>();
			T entry;

			// Create pathing flags based on type of T
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

					// Access tile information to get the type.
					//WARNING: This section is a logical challenge to read without using the comments. 
					ushort type = 0;
					// Attempt to load the object as a loaded mod object
					if (table.loaded.keyDict.TryGetValue((short)saveType, out ushort key)) {
						entry = table.loaded.list[key];
						type = entry.id;
					}
					// Else, it is currently an unloaded object
					else {
						// If the tile did not become unloaded during this load cycle, then
						if (!table.unloaded.keyDict.TryGetValue((short)saveType, out key)) {
							// Get the stored key
							key = reader.ReadUInt16(); 

							// If it can be restored, restore it using key.
							if (table.restored.keyDict.TryGetValue((short)key, out ushort rKey)) {
								entry = table.restored.list[rKey];
								type = entry.id;
							}

							// If it can't be restored, re-setup as unloaded
							else {
								table.unloaded.keyDict.TryGetValue((short)-key, out ushort uKey);
								entry = table.unloaded.list[uKey];
								type = ModContent.Find<O>(entry.unloadedType).Type;
								PosIndexer.MapPosToInfo(posMapList, uKey, x, y);
							}
						}

						// Else create new unloaded setup for this newly unloaded object
						else {
							entry = table.unloaded.list[key];
							type = ModContent.Find<O>(entry.unloadedType).Type;
							PosIndexer.MapPosToInfo(posMapList, key, x, y);
						}
					}

					// Set data
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

					// Loop through carbon clones
					sameCount = reader.ReadInt32();
					int i = x, j = y;
					Tile currTile = Main.tile[i, j];
					for (int c = 0; c < sameCount; c++) {
						if (!NextTile(ref i, ref j, ref currTile))
							break;

						objCopy<T>(currTile, tile, entry, isWall, isTile);
					}
				}
			}

			// Store the position maps in array format so that future searches are faster.
			if (isTile)
				uTilePosMap = posMapList.ToArray();
			else if (isWall)
				uWallPosMap = posMapList.ToArray();
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

		private static bool NextTile(ref int i, ref int j, ref Tile tile) {
			if (!PosIndexer.NextLocation(ref i, ref j))
				return false;

			tile = Main.tile[i, j];
			return true;
		}

		private static ushort[] unloadedTileIDs => new ushort[5] {
			ModContent.Find<ModTile>("ModLoader/UnloadedSolidTile").Type,
			ModContent.Find<ModTile>("ModLoader/UnloadedNonSolidTile").Type,
			ModContent.Find<ModTile>("ModLoader/UnloadedSemiSolidTile").Type,
			ModContent.Find<ModTile>("ModLoader/UnloadedChest").Type,
			ModContent.Find<ModTile>("ModLoader/UnloadedDresser").Type
		};

		private static ushort[] unloadedWallIDs => new ushort[1] { ModContent.Find<ModWall>("ModLoader/UnloadedWall").Type };

		private static string GetUnloadedType<T>(int type) {
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

			return "ModLoader/UnloadedSolidTile";
		}
	}
}
