using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.ID;
using Terraria.ModLoader.Default;

namespace Terraria.ModLoader.IO
{
	internal static partial class TileIO {
		// todo, resize arrays or content tags
		public static List<ushort> IsUnloadedTile = new List<ushort>();
		public static List<ushort> IsUnloadedWall = new List<ushort>();

		const string tileEntriesKey = "tileMap";
		const string wallEntriesKey = "wallMap";
		const string tileDataKey = "tileData";
		const string wallDataKey = "wallData";

		public static TileEntry[] tileEntries;
		public static PosData<ushort>[] unloadedTileLookup;

		public static WallEntry[] wallEntries;
		public static PosData<ushort>[] unloadedWallLookup;

		//NOTE: LoadBasics can't be separated into LoadWalls() and LoadTiles() because of LoadLegacy.
		internal static void LoadBasics(TagCompound tag) {
			tileEntries = LoadEntries<TileEntry, ModTile>(tag.GetList<TileEntry>(tileEntriesKey), out var tileEntriesLookup);
			wallEntries = LoadEntries<WallEntry, ModWall>(tag.GetList<WallEntry>(wallEntriesKey), out var wallEntriesLookup);

			if (legacyLoad) {
				LoadLegacy(tag, tileEntriesLookup, wallEntriesLookup);
			}
			else {
				var reader = new BinaryReader(new MemoryStream(tag.GetByteArray(tileDataKey)));
				unloadedTileLookup = LoadData<TileEntry, ModTile>(reader, tileEntriesLookup);

				reader = new BinaryReader(new MemoryStream(tag.GetByteArray(wallDataKey)));
				unloadedWallLookup = LoadData<WallEntry, ModWall>(reader, wallEntriesLookup);
			}

			WorldIO.ValidateSigns(); //call this at end
		}

		//TODO: This can likely be refactored to be SaveWalls() and SaveTiles(), but is left as is to mirror LoadBasics()
		internal static TagCompound SaveBasics() {
			// Handle worldgen
			if (tileEntries == null) {
				tileEntries = CreateEntries<TileEntry, ModTile>().ToArray();
			}
			if (wallEntries == null) {
				wallEntries = CreateEntries<WallEntry, ModWall>().ToArray();
			}

			return new TagCompound {
				[tileDataKey] = SaveData(tileEntries, unloadedTileLookup, out var hasTiles),
				[tileEntriesKey] = tileEntries.Where(e => e != null && hasTiles[e.type]).ToList(),

				[wallDataKey] = SaveData(wallEntries, unloadedWallLookup, out var hasWalls),
				[wallEntriesKey] = wallEntries.Where(e => e != null && hasWalls[e.type]).ToList(),
			};
		}

		static bool legacyLoad = false;

		internal static bool canPurgeOldData => false; //for deleting unloaded mod data in a save; should point to UI flag; temp false

		internal static T[] LoadEntries<T, B>(IList<T> savedEntryList, out T[] savedEntryLookup) where T : ModEntry, new() where B : ModBlockType {
			var entries = CreateEntries<T, B>();

			// Return if there is no saved mod blocks in world.
			if (savedEntryList.Count == 0) {
				savedEntryLookup = null;
				return null;
			}

			// Load entries from save.
			savedEntryLookup = new T[savedEntryList.Max(e => e.type) + 1];

			// Check saved entries
			foreach (var entry in savedEntryList) {
				// if the unloadedType doesn't exist (IE string length is 0) than set legacy flag
				if (entry.unloadedType.Length == 0 && !legacyLoad) {
					legacyLoad = true;
				}

				// If the saved entry can be found among the loaded blocks, then add it to the loaded blocks lookup directly
				if (ModContent.TryFind(entry.modName, entry.name, out B block)) {
					savedEntryLookup[entry.type] = entries[block.Type];
				}

				// If it can't be found, than set the save entry to say its unloaded, and add entry to the end of the entries list
				else {
					entries.Add(entry);
					entry.unloadedIndex = (ushort)(entries.Count - 1);
					savedEntryLookup[entry.type] = entries[entries.Count - 1];
				}
			}

			return entries.ToArray();
		}

		internal static List<T> CreateEntries<T, B>() where T : ModEntry, new() where B : ModBlockType {
			bool isWall = typeof(T) == typeof(WallEntry);
			bool isTile = typeof(T) == typeof(TileEntry);

			int count = 0;
			List<B> list = new List<B>();
			List<ushort> unloadedTypes = new List<ushort>();

			if (isTile) {
				count = TileLoader.TileCount;
				list = (List<B>)TileLoader.tiles;
				unloadedTypes = IsUnloadedTile;
			}
			else if (isWall) {
				count = WallLoader.WallCount;
				list = (List<B>)WallLoader.walls;
				unloadedTypes = IsUnloadedWall;
			}

			var entries = Enumerable.Repeat<T>(null, count).ToList();
			// Create entries for all loaded tiles (vanilla included?), and store in entries list.
			foreach (var block in list) {
				if (!unloadedTypes.Contains(block.Type)) {
					T temp = new T();
					temp.SetData<B>(block);
					entries[block.Type] = temp;
				}
			}

			return entries;
		}

		internal delegate void SetColour(Tile t, byte val);
		internal delegate void SetBlockType(Tile t, ushort val);

		internal static PosData<ushort>[] LoadData<T, O>(BinaryReader reader, T[] savedEntryLookup) where T : ModEntry where O : ModBlockType {
			var builder = new PosData<ushort>.OrderedSparseLookupBuilder();

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

					var entry = savedEntryLookup[saveType];

					// Set the type to either the existing type or the unloaded type
					ushort type = entry.type;
					if (entry.unloadedIndex > 0) {
						builder.Add(x, y, entry.unloadedIndex);
						// Because legacy load does not have a concept of unloadedType, we get the value of the unloaded Type here instead.
						type = ModContent.Find<O>(entry.unloadedType).Type;
					}

					Tile tile = Main.tile[x, y];
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

			return builder.Build();
		}


		internal delegate byte GetColour(Tile t);
		internal delegate ushort GetBlockType(Tile t);
		internal delegate bool BlockIsIgnorable(Tile t);

		internal static byte[] SaveData<T>(T[] entries, PosData<ushort>[] unloadedLookup, out bool[] hasObj) where T : ModEntry {
			var ms = new MemoryStream();
			var writer = new BinaryWriter(ms);

			GetColour colour = null;
			GetBlockType oType = null;
			BlockIsIgnorable isIgnorable = null;

			bool isWall = typeof(T) == typeof(WallEntry);
			bool isTile = typeof(T) == typeof(TileEntry);
			// Use pathing flags to pre-fetch variables.
			if (isTile) {
				colour = (t) => t.color();
				oType = (t) => t.type;
				isIgnorable = (t) => !t.active() || t.type < TileID.Count;
			}
			else if (isWall) {
				colour = (t) => t.wallColor();
				oType = (t) => t.wall;
				isIgnorable = (t) => t.wall < WallID.Count;
			}

			var unloadedReader = new PosData<ushort>.OrderedSparseLookupReader(unloadedLookup);
			hasObj = new bool[entries.Length];

			for (int x = 0; x < Main.maxTilesX; x++) {
				for (int y = 0; y < Main.maxTilesY; y++) {
					Tile tile = Main.tile[x, y];
					ushort type = oType(tile);

					// Skip Vanilla tiles
					if (isIgnorable(tile)) {
						writer.Write((ushort)0);
						continue;
					}

					if (entries[type] == null) { // Is an unloaded block
						type = unloadedReader.Get(x, y); // Get the "type", which is going to be outside the bounds of TileLoader.
					}
					
					// Write Locational data
					hasObj[type] = true;
					writer.Write(type);
					writer.Write(colour(tile));

					if (isTile && (entries[type] as TileEntry).frameImportant) {
						writer.Write(tile.frameX);
						writer.Write(tile.frameY);
					}
				}
			}

			return ms.ToArray();
		}

		internal static string GetUnloadedType<B>(int type) {
			if (typeof(B) == typeof(WallEntry))
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
