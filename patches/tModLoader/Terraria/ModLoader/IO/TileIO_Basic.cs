using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.ID;

namespace Terraria.ModLoader.IO;

internal static partial class TileIO
{
	public abstract class IOImpl<TBlock, TEntry> where TBlock : ModBlockType where TEntry : ModBlockEntry
	{
		public readonly string entriesKey;
		public readonly string dataKey;

		public TEntry[] entries;
		public PosData<ushort>[] unloadedEntryLookup;

		public List<ushort> unloadedTypes = new List<ushort>();

		protected IOImpl(string entriesKey, string dataKey)
		{
			this.entriesKey = entriesKey;
			this.dataKey = dataKey;
		}

		protected abstract int LoadedBlockCount { get; }

		protected abstract IEnumerable<TBlock> LoadedBlocks { get; }

		protected abstract TEntry ConvertBlockToEntry(TBlock block);

		private List<TEntry> CreateEntries()
		{
			var entries = Enumerable.Repeat<TEntry>(null, LoadedBlockCount).ToList();
			// Create entries for all loaded tiles (vanilla included?), and store in entries list.
			foreach (var block in LoadedBlocks) {
				if (!unloadedTypes.Contains(block.Type)) {
					entries[block.Type] = ConvertBlockToEntry(block);
				}
			}

			return entries;
		}

		public void LoadEntries(TagCompound tag, out TEntry[] savedEntryLookup)
		{
			var savedEntryList = tag.GetList<TEntry>(entriesKey);
			var entries = CreateEntries();

			// Return if there is no saved mod blocks in world.
			if (savedEntryList.Count == 0) {
				savedEntryLookup = null;
			}
			else {
				// Load entries from save, and pathing variables
				savedEntryLookup = new TEntry[savedEntryList.Max(e => e.type) + 1];

				// Check saved entries
				foreach (var entry in savedEntryList) {
					savedEntryLookup[entry.type] = entry;

					// If the saved entry can be found among the loaded blocks, then use its loadedType
					if (ModContent.TryFind(entry.modName, entry.name, out TBlock block)) {
						entry.type = entry.loadedType = block.Type;
					}
					else { // If it can't be found, then add entry to the end of the entries list and set the loadedType to the unloaded placeholder
						entry.type = (ushort)entries.Count;
						entry.loadedType = (ModContent.TryFind(entry.unloadedType, out TBlock unloadedBlock) ? unloadedBlock : entry.DefaultUnloadedPlaceholder).Type;
						entries.Add(entry);
					}
				}
			}

			this.entries = entries.ToArray();
		}

		protected abstract void ReadData(Tile tile, TEntry entry, BinaryReader reader);

		public void LoadData(TagCompound tag, TEntry[] savedEntryLookup)
		{
			if (!tag.ContainsKey(dataKey)) {
				return;
			}

			using var reader = new BinaryReader(tag.GetByteArray(dataKey).ToMemoryStream());
			var builder = new PosData<ushort>.OrderedSparseLookupBuilder();

			for (int x = 0; x < Main.maxTilesX; x++) {
				for (int y = 0; y < Main.maxTilesY; y++) {
					ushort saveType = reader.ReadUInt16();
					if (saveType == 0) {
						continue;
					}

					var entry = savedEntryLookup[saveType];
					ReadData(Main.tile[x, y], entry, reader);

					if (entry.IsUnloaded) {
						// When an entry is using an unloaded placeholder, the saved entry is copied to the end of our current entries list (after all loaded tiles)
						// We store this 'type' (index of the copied entry) in sparse tile data storage, and use it to retrieve the correct entry at saving time (rather than saving an unloaded tile permanently)
						builder.Add(x, y, entry.type);
					}
				}
			}

			unloadedEntryLookup = builder.Build();
		}

		public void Save(TagCompound tag)
		{
			if (entries == null) {
				entries = CreateEntries().ToArray();
			}

			tag[dataKey] = SaveData(out var hasBlocks);
			tag[entriesKey] = SelectEntries(hasBlocks, entries).ToList();
		}

		private IEnumerable<TEntry> SelectEntries(bool[] select, TEntry[] entries)
		{
			for (int i = 0; i < select.Length; i++)
				if (select[i])
					yield return entries[i];
		}

		protected abstract ushort GetModBlockType(Tile tile);

		protected abstract void WriteData(BinaryWriter writer, Tile tile, TEntry entry);

		public byte[] SaveData(out bool[] hasObj)
		{
			using var ms = new MemoryStream();
			var writer = new BinaryWriter(ms);

			var unloadedReader = new PosData<ushort>.OrderedSparseLookupReader(unloadedEntryLookup);
			hasObj = new bool[entries.Length];

			for (int x = 0; x < Main.maxTilesX; x++) {
				for (int y = 0; y < Main.maxTilesY; y++) {
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

		public void Clear()
		{
			// make sure data from the last loaded world doesn't carry over into the next one
			entries = null;
			unloadedEntryLookup = null;
		}
	}

	public class TileIOImpl : IOImpl<ModTile, TileEntry>
	{
		public TileIOImpl() : base("tileMap", "tileData") { }

		protected override int LoadedBlockCount => TileLoader.TileCount;

		protected override IEnumerable<ModTile> LoadedBlocks => TileLoader.tiles;

		protected override TileEntry ConvertBlockToEntry(ModTile tile) => new TileEntry(tile);

		protected override ushort GetModBlockType(Tile tile) => tile.active() && tile.type >= TileID.Count ? tile.type : (ushort)0;

		protected override void ReadData(Tile tile, TileEntry entry, BinaryReader reader)
		{
			tile.type = entry.loadedType;
			tile.color(reader.ReadByte());

			// Set remaining tile data
			tile.active(true);
			if (entry.frameImportant) {
				tile.frameX = reader.ReadInt16();
				tile.frameY = reader.ReadInt16();
			}
		}

		protected override void WriteData(BinaryWriter writer, Tile tile, TileEntry entry)
		{
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
		public WallIOImpl() : base("wallMap", "wallData") { }

		protected override int LoadedBlockCount => WallLoader.WallCount;

		protected override IEnumerable<ModWall> LoadedBlocks => WallLoader.walls;

		protected override WallEntry ConvertBlockToEntry(ModWall wall) => new WallEntry(wall);
		protected override ushort GetModBlockType(Tile tile) => tile.wall >= WallID.Count ? tile.wall : (ushort)0;

		protected override void ReadData(Tile tile, WallEntry entry, BinaryReader reader)
		{
			tile.wall = entry.loadedType;
			tile.wallColor(reader.ReadByte());
		}

		protected override void WriteData(BinaryWriter writer, Tile tile, WallEntry entry)
		{
			writer.Write(entry.type);
			writer.Write(tile.wallColor());
		}
	}

	internal static TileIOImpl Tiles = new TileIOImpl();
	internal static WallIOImpl Walls = new WallIOImpl();

	//NOTE: LoadBasics can't be separated into LoadWalls() and LoadTiles() because of LoadLegacy.
	internal static void LoadBasics(TagCompound tag)
	{
		Tiles.LoadEntries(tag, out var tileEntriesLookup);
		Walls.LoadEntries(tag, out var wallEntriesLookup);

		if (!tag.ContainsKey("wallData")) {
			LoadLegacy(tag, tileEntriesLookup, wallEntriesLookup);
		}
		else {
			Tiles.LoadData(tag, tileEntriesLookup);
			Walls.LoadData(tag, wallEntriesLookup);
		}

		WorldIO.ValidateSigns(); //call this at end
	}

	//TODO: This can likely be refactored to be SaveWalls() and SaveTiles(), but is left as is to mirror LoadBasics()
	internal static TagCompound SaveBasics()
	{
		var tag = new TagCompound();
		Tiles.Save(tag);
		Walls.Save(tag);
		return tag;
	}

	internal static void ResetUnloadedTypes()
	{
		Tiles.unloadedTypes.Clear();
		Walls.unloadedTypes.Clear();
	}

	internal static void ClearWorld()
	{
		Tiles.Clear();
		Walls.Clear();
	}
}
