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

		internal delegate byte GetColour(Tile t);
		internal delegate ushort GetBlockType(Tile t);
		internal delegate bool BlockIsIgnorable(Tile t);

		internal static byte[] SaveData<T>(bool[] hasObj) {
			var ms = new MemoryStream();
			var writer = new BinaryWriter(ms);

			ushort[] unloadedTypes = null;
			PosIndexer.PosIndex[] posMap = null;
			GetColour colour = null;
			GetBlockType oType = null;
			BlockIsIgnorable isIgnorable = null;

			bool isWall = typeof(T) == typeof(WallEntry);
			bool isTile = typeof(T) == typeof(TileEntry);
			// Use pathing flags to pre-fetch variables.
			if (isTile) {
				unloadedTypes = unloadedTileIDs;
				posMap = uTilePosMap;
				colour = (t) => t.color();
				oType = (t) => t.type;
				isIgnorable = (t) => !t.active() || t.type < TileID.Count;
			}
			else if (isWall) {
				unloadedTypes = unloadedWallIDs;
				posMap = uWallPosMap;
				colour = (t) => t.wallColor();
				oType = (t) => t.wall;
				isIgnorable = (t) => t.wall < WallID.Count;
			}

			for (int x = 0; x < Main.maxTilesX; x++) {
				for (int y = 0; y < Main.maxTilesY; y++) {
					Tile tile = Main.tile[x, y];
					ushort type = oType(tile);

					// Skip Vanilla tiles
					if (isIgnorable(tile)) {
						writer.Write((ushort)0);
						continue;
					}

					// Write Locational data
					hasObj[type] = true;
					WriteKey(writer, type, x, y, posMap, unloadedTypes);
					writer.Write(colour(tile));

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

		internal delegate void SetColour(Tile t, byte val);
		internal delegate void SetBlockType(Tile t, ushort val);

		internal static void LoadData<T, O>(BinaryReader reader, ref IOSaveLoadSet<T> table) where T : ModEntry where O : ModBlockType {
			List<PosIndexer.PosIndex> posMapList = new List<PosIndexer.PosIndex>();
			ushort prevUnloadedKey = ushort.MaxValue;
			ushort prevKey = ushort.MaxValue;
			SetColour colour = null;
			SetBlockType oType = null;

			// Create pathing flags based on type of T
			bool isWall = typeof(T) == typeof(WallEntry);
			bool isTile = typeof(T) == typeof(TileEntry);
			// Use pathing flags to pre-fetch variables.
			if (isWall) {
				colour = (t, val) => t.wallColor(val);
				oType = (t, val) => t.wall = val;
			}
			else if (isTile) {
				colour = (t, val) => t.color(val);
				oType = (t, val) => t.type = val;
			}

			for (int x = 0; x < Main.maxTilesX; x++) {
				for (int y = 0; y < Main.maxTilesY; y++) {
					ushort saveType = reader.ReadUInt16();
					if (saveType == 0) {
						continue;
					}

					Tile tile = Main.tile[x, y];
					LoadType<T, O>(saveType, reader, x, y, ref prevUnloadedKey, ref prevKey, ref posMapList, ref table, out var type, out var entry);
					oType(tile, type);
					colour(tile, reader.ReadByte());

					// Set remaining tile data
					if (isTile) {
						tile.active(true);
						if ((entry as TileEntry).frameImportant) {
							tile.frameX = reader.ReadInt16();
							tile.frameY = reader.ReadInt16();
						}
					}
				}
			}

			// Store the position maps in array format so that future searches are faster.
			if (isTile)
				uTilePosMap = posMapList.ToArray();
			else if (isWall)
				uWallPosMap = posMapList.ToArray();
		}

		private static void LoadType<T, O>(ushort saveType, BinaryReader reader, int x, int y, ref ushort prevUnloadedKey, ref ushort prevKey, ref List<PosIndexer.PosIndex> posMapList, ref IOSaveLoadSet<T> table, out ushort type, out T entry) where T : ModEntry where O : ModBlockType {
			type = 0;
			//WARNING: This section is a logical challenge to read without using the comments. 
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
						// If an indexer has already been made for this same tile, use the prev key.
						if (-key == prevUnloadedKey) {
							entry = table.unloaded.list[prevUnloadedKey];
							type = ModContent.Find<O>(entry.unloadedType).Type;
						}
						// Else create the indexer.
						else {
							table.unloaded.keyDict.TryGetValue((short)-key, out ushort uKey);
							prevUnloadedKey = uKey;
							entry = table.unloaded.list[uKey];
							type = ModContent.Find<O>(entry.unloadedType).Type;
							PosIndexer.MapPosToInfo(posMapList, uKey, x, y);
						}
					}
				}
				// Else create new unloaded setup for this newly unloaded object
				else {
					// If an indexer has already been made for this same tile, use the prev key.
					if (key == prevKey) {
						entry = table.unloaded.list[prevKey];
						type = ModContent.Find<O>(entry.unloadedType).Type;
					}
					// Else create the indexer.
					else {
						prevKey = key;
						entry = table.unloaded.list[key];
						type = ModContent.Find<O>(entry.unloadedType).Type;
						PosIndexer.MapPosToInfo(posMapList, key, x, y);
					}
				}
			}
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
