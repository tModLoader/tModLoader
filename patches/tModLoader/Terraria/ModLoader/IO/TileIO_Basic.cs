using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.ID;
using Terraria.ModLoader.Default;

namespace Terraria.ModLoader.IO
{
	internal static partial class TileIO {
		// todo, resize arrays or content tags
		public static bool[] IsUnloadedTile = new bool[0];
		public static bool[] IsUnloadedWall = new bool[0];

		const string tileEntriesKey = "tileMap";
		const string wallEntriesKey = "wallMap";
		const string tileDataKey = "tileData";
		const string wallDataKey = "wallData";

		public static TileEntry[] tileEntries;
		public static PosData<ushort>[] unloadedTileLookup;

		public static WallEntry[] wallEntries;
		public static PosData<ushort>[] unloadedWallLookup;

		//NOTE: LoadBasics can't be separated into LoadWalls() and LoadTiles() because of LoadLegacy.
		static void LoadBasics(TagCompound tag) {
			tileEntries = LoadEntries<TileEntry, ModTile>(tag.GetList<TileEntry>(tileEntriesKey), out var savedTileEntries);
			wallEntries = LoadEntries<WallEntry, ModWall>(tag.GetList<WallEntry>(wallEntriesKey), out var savedWallEntries);

			var reader = new BinaryReader(new MemoryStream(tag.GetByteArray(tileDataKey)));
			unloadedTileLookup = LoadData<TileEntry, ModTile>(reader, savedTileEntries);

			reader = new BinaryReader(new MemoryStream(tag.GetByteArray(wallDataKey)));
			unloadedWallLookup = LoadData<WallEntry, ModWall>(reader, savedWallEntries);

			WorldIO.ValidateSigns(); //call this at end
		}

		//TODO: This can likely be refactored to be SaveWalls() and SaveTiles(), but is left as is to mirror LoadBasics()
		static TagCompound SaveBasics() {
			return new TagCompound {
				[tileDataKey] = SaveData(tileEntries, unloadedTileLookup, out var hasTiles),
				[tileEntriesKey] = tileEntries.Where(e => e != null && hasTiles[e.id]).ToList(),

				[wallDataKey] = SaveData(wallEntries, unloadedWallLookup, out var hasWalls),
				[wallEntriesKey] = wallEntries.Where(e => e != null && hasWalls[e.id]).ToList(),
			};
		}

		static bool legacyLoad = false;

		internal static bool canPurgeOldData => false; //for deleting unloaded mod data in a save; should point to UI flag; temp false

		internal static T[] LoadEntries<T, B>(IList<T> savedEntryList, out T[] savedEntryLookup) where T : ModEntry where B : ModBlockType {
			// todo: genericize
			var entries = Enumerable.Repeat<T>(null, TileLoader.TileCount).ToList();
			foreach (var tile in TileLoader.tiles)
				if (!IsUnloadedTile[tile.Type])
					entries[tile.Type] = new TileEntry(tile);


			savedEntryLookup = new T[savedEntryList.Max(e => e.id)];
			foreach (var entry in savedEntryList) {
				if (ModContent.TryFind(entry.modName, entry.name, out B block)) {
					savedEntryLookup[entry.id] = entries[block.Type];
				}
				else {
					entry.id = (ushort)entries.Count;
					entry.loadedType = ModContent.Find<B>(entry.unloadedType).Type;
					entries.Add(entry);
				}
			}

			return entries.ToArray();
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

			var unloadedReader = new PosData<ushort>.SparseLookupReader(unloadedLookup);
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

					if (entries[type] == null)
						type = unloadedReader.Get(x, y);

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

		internal delegate void SetColour(Tile t, byte val);
		internal delegate void SetBlockType(Tile t, ushort val);

		internal static PosData<ushort>[] LoadData<T, O>(BinaryReader reader, T[] savedEntryLookup) where T : ModEntry where O : ModBlockType {
			var builder = new PosData<ushort>.SparseLookupBuilder();

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
					if (entry.id != entry.loadedType)
						builder.Add(x, y, entry.id);

					Tile tile = Main.tile[x, y];
					oType(tile, entry.loadedType);
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
	}
}
