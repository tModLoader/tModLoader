using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.ID;

namespace Terraria.ModLoader.IO
{
	internal static partial class TileIO {
		public abstract class IOImpl<TBlock, TEntry> where TBlock : ModBlockType where TEntry : ModEntry
		{
			public readonly string entriesKey;
			public readonly string dataKey;
			public readonly string chunkXKey;
			public readonly string chunkYKey;

			public TEntry[] entries;
			public PosData<ushort>[] unloadedEntryLookup;

			public List<ushort> unloadedTypes = new List<ushort>();

			protected IOImpl(string entriesKey, string dataKey, string chunkXKey, string chunkYKey) {
				this.entriesKey = entriesKey;
				this.dataKey = dataKey;
				this.chunkYKey = chunkYKey;
				this.chunkXKey = chunkXKey;
			}

			protected abstract int LoadedBlockCount { get; }

			protected abstract IEnumerable<TBlock> LoadedBlocks { get; }

			protected abstract TEntry ConvertBlockToEntry(TBlock block);

			internal List<TEntry> CreateEntries() {
				var entries = Enumerable.Repeat<TEntry>(null, LoadedBlockCount).ToList();
				// Create entries for all loaded tiles (vanilla included?), and store in entries list.
				foreach (var block in LoadedBlocks) {
					if (!unloadedTypes.Contains(block.Type)) {
						entries[block.Type] = ConvertBlockToEntry(block);
					}
				}

				return entries;
			}

			public void LoadEntries(TagCompound tag, out TEntry[] savedEntryLookup, List<TEntry> entries) {
				var savedEntryList = tag.GetList<TEntry>(entriesKey);

				// Return if there is no saved mod blocks in world.
				if (savedEntryList.Count == 0) {
					savedEntryLookup = null;
				}
				else {
					// Load entries from save, and pathing variables
					savedEntryLookup = new TEntry[savedEntryList.Max(e => e.type) + 1];

					// Check saved entries
					foreach (var entry in savedEntryList) {
						// If the saved entry can be found among the loaded blocks, then use the entry created for the loaded block
						if (ModContent.TryFind(entry.modName, entry.name, out TBlock block)) {
							savedEntryLookup[entry.type] = entries[block.Type];
						}
						else { // If it can't be found, then add entry to the end of the entries list and set the loadedType to the unloaded placeholder
							savedEntryLookup[entry.type] = entry;
							entry.type = (ushort)entries.Count;
							entry.loadedType = canPurgeOldData ? entry.vanillaReplacementType : ModContent.Find<TBlock>(entry.unloadedType).Type;
							entries.Add(entry);
						}
					}
				}

				this.entries =  entries.ToArray();
			}

			protected abstract void ReadData(Tile tile, TEntry entry, BinaryReader reader);

			public void LoadData<TInserter>(TagCompound tag, TEntry[] savedEntryLookup, TInserter inserter, int xL = 0, int xR = -1, int yT = 0, int yB = -1) where TInserter : PosData<ushort>.OrderedSparseInserter {
				using var reader = new BinaryReader(new MemoryStream(tag.GetByteArray(dataKey)));
				if (xR < 0) {
					xR = Main.maxTilesX;
				}
				if (yB < 0) {
					yB = Main.maxTilesY;
				}

				for (int x = xL; x < xR; x++) {
					for (int y = yT; y < yB; y++) {
						ushort saveType = reader.ReadUInt16();
						if (saveType == 0) {
							continue;
						}

						var entry = savedEntryLookup[saveType];

						// Set the type to either the existing type or the unloaded type
						if (entry.IsUnloaded && !canPurgeOldData) {
							inserter.SafeAdd(x, y, entry.type);
						}

						ReadData(Main.tile[x, y], entry, reader);
					}
				}

				unloadedEntryLookup = inserter.Build();
			}

			public void Save(TagCompound tag) {
				if (entries == null) {
					entries = CreateEntries().ToArray();
					// Something something Run LoadEntries for all schmatics used in world gen here
				}

				tag[dataKey] = SaveData(out var hasBlocks);
				tag[entriesKey] = SelectEntries(hasBlocks, entries).ToList();
			}

			private IEnumerable<TEntry> SelectEntries(bool[] select, TEntry[] entries) {
				for (int i = 0; i < select.Length; i++)
					if (select[i])
						yield return entries[i];
			}

			protected abstract ushort GetModBlockType(Tile tile);

			protected abstract void WriteData(BinaryWriter writer, Tile tile, TEntry entry);

			public byte[] SaveData(out bool[] hasObj, int xL = 0, int xR = -1, int yT = 0, int yB = -1) {
				using var ms = new MemoryStream();
				var writer = new BinaryWriter(ms);

				if (xR < 0) {
					xR = Main.maxTilesX;
				}
				if (yB < 0) {
					yB = Main.maxTilesY;
				}

				var unloadedReader = new PosData<ushort>.OrderedSparseLookupReader(unloadedEntryLookup);
				hasObj = new bool[entries.Length];

				for (int x = xL; x < xR; x++) {
					for (int y = yT; y < yB; y++) {
						Tile tile = Main.tile[x, y];

						int type = GetModBlockType(tile);
						// Skip Vanilla tiles
						if (type == 0) {
							writer.Write((ushort)0);
							continue;
						}

						if (entries[type] == null) { // Is an unloaded block
							type = unloadedReader.Get(x, y); // Get the "type", which is going to be outside the bounds of TileLoader.
						}

						// Write Locational data
						hasObj[type] = true;
						WriteData(writer, tile, entries[type]);
					}
				}

				return ms.ToArray();
			}

			public void PostExitWorldCleanup() {
				// make sure data from the last loaded world doesn't carry over into the next one
				entries = null;
				unloadedEntryLookup = null;
			}

			public void LoadChunk(int xLeft, int yTop, TagCompound tag) {
				LoadEntries(tag, out var tileEntriesLookup, entries.ToList());
				int xRight = tag.Get<int>(chunkXKey) + xLeft;
				int yBottom = tag.Get<int>(chunkYKey) + yTop;

				LoadData(tag, tileEntriesLookup, 
					new PosData<ushort>.OrderedSparseLookupMerger(
						unloadedEntryLookup, xLeft, xRight, yTop, yBottom 
					), 
					xLeft, xRight, yTop, yBottom
				);

				WorldIO.ValidateSigns(); //call this at end
			}

			public void SaveChunk(TagCompound tag, int xLeft, int yTop, int xSize, int ySize) {
				tag[chunkXKey] = xSize;
				tag[chunkYKey] = ySize;
				tag[dataKey] = SaveData(out var hasBlocks, xLeft, yTop, xSize + xLeft, ySize + yTop);
				tag[entriesKey] = SelectEntries(hasBlocks, entries).ToList();
			}
		}

		public class TileIOImpl : IOImpl<ModTile, TileEntry>
		{
			public TileIOImpl() : base("tileMap", "tileData", "tileXSize", "tileYSize") { }

			protected override int LoadedBlockCount => TileLoader.TileCount;

			protected override IEnumerable<ModTile> LoadedBlocks => TileLoader.tiles;

			protected override TileEntry ConvertBlockToEntry(ModTile tile) => new TileEntry(tile);

			protected override ushort GetModBlockType(Tile tile) => tile.active() && tile.type >= TileID.Count ? tile.type : (ushort)0;

			protected override void ReadData(Tile tile, TileEntry entry, BinaryReader reader) {
				tile.type = entry.loadedType;
				tile.color(reader.ReadByte());

				// Set remaining tile data
				tile.active(true);
				if (entry.frameImportant) {
					tile.frameX = reader.ReadInt16();
					tile.frameY = reader.ReadInt16();
				}
			}

			protected override void WriteData(BinaryWriter writer, Tile tile, TileEntry entry) {
				writer.Write(entry.type);
				writer.Write(tile.color());

				if (entry.frameImportant) {
					writer.Write(tile.frameX);
					writer.Write(tile.frameY);
				}
			}
		}

		public class WallIOImpl : IOImpl<ModWall, WallEntry>
		{
			public WallIOImpl() : base("wallMap", "wallData", "wallXSize", "wallYSize") { }

			protected override int LoadedBlockCount => WallLoader.WallCount;

			protected override IEnumerable<ModWall> LoadedBlocks => WallLoader.walls;

			protected override WallEntry ConvertBlockToEntry(ModWall wall) => new WallEntry(wall);
			protected override ushort GetModBlockType(Tile tile) => tile.wall >= WallID.Count ? tile.wall : (ushort)0;

			protected override void ReadData(Tile tile, WallEntry entry, BinaryReader reader) {
				tile.wall = entry.loadedType;
				tile.wallColor(reader.ReadByte());
			}

			protected override void WriteData(BinaryWriter writer, Tile tile, WallEntry entry) {
				writer.Write(entry.type);
				writer.Write(tile.wallColor());
			}
		}

		internal static TileIOImpl Tiles = new TileIOImpl();
		internal static WallIOImpl Walls = new WallIOImpl();

		internal static bool canPurgeOldData => false; //for deleting unloaded mod data in a save; should point to UI flag; temp false

		//NOTE: LoadBasics can't be separated into LoadWalls() and LoadTiles() because of LoadLegacy.
		internal static void LoadBasics(TagCompound tag) {

			Tiles.LoadEntries(tag, out var tileEntriesLookup, Tiles.CreateEntries());
			Walls.LoadEntries(tag, out var wallEntriesLookup, Walls.CreateEntries());

			if (!tag.ContainsKey("wallData")) {
				LoadLegacy(tag, tileEntriesLookup, wallEntriesLookup);
			}
			else {
				Tiles.LoadData(tag, tileEntriesLookup, new PosData<ushort>.OrderedSparseLookupBuilder());
				Walls.LoadData(tag, wallEntriesLookup, new PosData<ushort>.OrderedSparseLookupBuilder());
			}

			WorldIO.ValidateSigns(); //call this at end
		}

		//TODO: This can likely be refactored to be SaveWalls() and SaveTiles(), but is left as is to mirror LoadBasics()
		internal static TagCompound SaveBasics() {
			var tag = new TagCompound();
			Tiles.Save(tag);
			Walls.Save(tag);
			return tag;
		}

		internal static void LoadChunk(TagCompound tag, int xLeft, int yTop) {
			Tiles.LoadChunk(xLeft, yTop, tag);
			Walls.LoadChunk(xLeft, yTop, tag);
		}

		internal static TagCompound SaveChunk(int xLeft, int yTop, int xSize, int ySize) {
			TagCompound tag = new TagCompound();
			Tiles.SaveChunk(tag, xLeft, yTop, xSize, ySize);
			Walls.SaveChunk(tag, xLeft, yTop, xSize, ySize);
			return tag;
		}

		// Called to ensure proper loading/unloaded behaviour with respect to Unloaded Placeholders
		internal static void ResizeArrays() {
			Tiles.unloadedTypes.Clear();
			Walls.unloadedTypes.Clear();
		}

		internal static void PostExitWorldCleanup() {
			Tiles.PostExitWorldCleanup();
			Walls.PostExitWorldCleanup();
		}
	}
}
