using System.Collections.Generic;
using System.Linq;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default;

/// <summary>
/// Handles conversion of legacy 1.3 world unloaded tile TML TagCompound data to the newer 1.4+ systems.
/// </summary>
[LegacyName("MysteryTilesWorld", "UnloadedTilesSystem")]
internal partial class LegacyUnloadedTilesSystem : ModSystem
{
	private static readonly List<TileInfo> infos = new List<TileInfo>();
	private static readonly Dictionary<int, ushort> converted = new Dictionary<int, ushort>();

	public override void ClearWorld()
	{
		infos.Clear();
		converted.Clear();
	}

	public override void SaveWorldData(TagCompound tag)
	{
		// Nothing to do, system is legacy
	}

	public override void LoadWorldData(TagCompound tag)
	{
		foreach (var infoTag in tag.GetList<TagCompound>("list")) {
			if (!infoTag.ContainsKey("mod")) {
				infos.Add(TileInfo.Invalid);
				continue;
			}

			string modName = infoTag.GetString("mod");
			string name = infoTag.GetString("name");
			bool frameImportant = infoTag.ContainsKey("frameX");

			var info = frameImportant
				? new TileInfo(modName, name, infoTag.GetShort("frameX"), infoTag.GetShort("frameY"))
				: new TileInfo(modName, name);

			infos.Add(info);
		}

		if (infos.Count > 0) {
			ConvertTiles();
		}
	}

	internal void ConvertTiles()
	{
		var legacyEntries = TileIO.Tiles.entries.ToList();

		var builder = new PosData<ushort>.OrderedSparseLookupBuilder();
		var unloadedReader = new PosData<ushort>.OrderedSparseLookupReader(TileIO.Tiles.unloadedEntryLookup);

		for (int x = 0; x < Main.maxTilesX; x++) {
			for (int y = 0; y < Main.maxTilesY; y++) {
				Tile tile = Main.tile[x, y];

				if (!(tile.active() && tile.type >= TileID.Count))
					continue;

				ushort type = tile.type;

				if (TileIO.Tiles.entries[type] == null) { // Is an unloaded block
					type = unloadedReader.Get(x, y);

					if (legacyEntries[type].modName.Equals("ModLoader")) { // Was saved as an unloaded block
						ConvertTile(tile, legacyEntries, out type);
					}

					builder.Add(x, y, type);
				}

			}
		}

		TileIO.Tiles.entries = legacyEntries.ToArray();
		TileIO.Tiles.unloadedEntryLookup = builder.Build();
	}

	internal void ConvertTile(Tile tile, List<TileEntry> entries, out ushort type)
	{
		var frame = new TileFrame(tile.frameX, tile.frameY);
		int frameID = frame.FrameID;

		if (converted.TryGetValue(frameID, out type))
			return;

		TileInfo info = infos[frameID];
		var entry = new TileEntry(TileLoader.GetTile(tile.type)) {
			name = info.name,
			modName = info.modName,
			frameImportant = info.frameX > -1,
			type = type = (ushort)entries.Count
		};

		entries.Add(entry);
		converted.Add(frameID, type);
	}
}