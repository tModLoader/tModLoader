using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.ModLoader.Default;

namespace Terraria.ModLoader.IO;

internal static partial class TileIO
{
	//*********** Legacy Tile, Walls Load for converting 1.3 to 1.4*******************************************//
	internal static class TileIOFlags
	{
		internal const byte None = 0;
		internal const byte ModTile = 1;
		internal const byte FrameXInt16 = 2;
		internal const byte FrameYInt16 = 4;
		internal const byte TileColor = 8;
		internal const byte ModWall = 16;
		internal const byte WallColor = 32;
		internal const byte NextTilesAreSame = 64;
		internal const byte NextModTile = 128;
	}

	internal static void LoadLegacy(TagCompound tag, TileEntry[] tileEntriesLookup, WallEntry[] wallEntriesLookup)
	{
		if (!tag.ContainsKey("data")) {
			return;
		}

		// Retrieve Locational-Specific Data from 'Data' and apply
		using var reader = new BinaryReader(tag.GetByteArray("data").ToMemoryStream());
		ReadTileData(reader, tileEntriesLookup, wallEntriesLookup, out var tilePosMapList, out var wallPosMapList);
		Tiles.unloadedEntryLookup = tilePosMapList.ToArray();
		Walls.unloadedEntryLookup = wallPosMapList.ToArray();
	}

	internal static void ReadTileData(BinaryReader reader, TileEntry[] tileEntriesLookup, WallEntry[] wallEntriesLookup, out List<PosData<ushort>> wallPosMapList, out List<PosData<ushort>> tilePosMapList)
	{
		int i = 0;
		int j = 0;
		byte skip;
		bool nextModTile = false;
		tilePosMapList = new List<PosData<ushort>>();
		wallPosMapList = new List<PosData<ushort>>();

		// Access indexed shortlist of all tile locations with mod data on either of wall or tiles
		do {
			// Skip vanilla tiles
			if (!nextModTile) {
				skip = reader.ReadByte();

				while (skip == 255) {
					for (byte k = 0; k < 255; k++) {
						if (!NextLocation(ref i, ref j)) {
							return; //Skip over vanilla tiles
						}
					}

					skip = reader.ReadByte();
				}

				for (byte k = 0; k < skip; k++) {
					if (!NextLocation(ref i, ref j)) {
						return;
					}
				}
			}
			else {
				nextModTile = false;
			}

			// Load modded tiles
			ReadModTile(ref i, ref j, reader, ref nextModTile, tilePosMapList, wallPosMapList, tileEntriesLookup, wallEntriesLookup);
		}
		while (NextLocation(ref i, ref j));
	}

	internal static void ReadModTile(ref int i, ref int j, BinaryReader reader, ref bool nextModTile, List<PosData<ushort>> wallPosMapList, List<PosData<ushort>> tilePosMapList, TileEntry[] tileEntriesLookup, WallEntry[] wallEntriesLookup)
	{
		// Access Stored 8bit Flags
		byte flags;
		ushort saveType;
		flags = reader.ReadByte();

		// Read Tiles
		Tile tile = Main.tile[i, j];

		if ((flags & TileIOFlags.ModTile) == TileIOFlags.ModTile) {
			tile.active(true);

			saveType = (ushort)(reader.ReadUInt16());

			var tEntry = tileEntriesLookup[saveType];
			tile.type = tEntry.loadedType;

			// Implement tile frames
			if (tEntry.frameImportant) {
				if ((flags & TileIOFlags.FrameXInt16) == TileIOFlags.FrameXInt16) {
					tile.frameX = reader.ReadInt16();
				}
				else {
					tile.frameX = reader.ReadByte();
				}
				if ((flags & TileIOFlags.FrameYInt16) == TileIOFlags.FrameYInt16) {
					tile.frameY = reader.ReadInt16();
				}
				else {
					tile.frameY = reader.ReadByte();
				}
			}
			else {
				tile.frameX = -1;
				tile.frameY = -1;
			}

			if ((flags & TileIOFlags.TileColor) == TileIOFlags.TileColor) {
				tile.color(reader.ReadByte());
			}

			if (tEntry.IsUnloaded) {
				tilePosMapList.Add(new PosData<ushort>(PosData.CoordsToPos(i, j), tEntry.type));
			}

			WorldGen.tileCounts[tile.type] += j <= Main.worldSurface ? 5 : 1;
		}

		if ((flags & TileIOFlags.ModWall) == TileIOFlags.ModWall) {
			saveType = (ushort)(reader.ReadUInt16());

			var wEntry = wallEntriesLookup[saveType];
			tile.wall = wEntry.loadedType;

			if (wEntry.IsUnloaded) {
				wallPosMapList.Add(new PosData<ushort>(PosData.CoordsToPos(i, j), wEntry.type));
			}

			if ((flags & TileIOFlags.WallColor) == TileIOFlags.WallColor) {
				tile.wallColor(reader.ReadByte());
			}
		}

		// Handle re-occurence, up to 256 counts.
		if ((flags & TileIOFlags.NextTilesAreSame) == TileIOFlags.NextTilesAreSame) {
			byte sameCount = reader.ReadByte(); //how many are the same

			for (byte k = 0; k < sameCount; k++) { // for all copy-paste tiles
				NextLocation(ref i, ref j); // move i,j to the next tile, with vertical being priority

				Main.tile[i, j].CopyFrom(tile);
				WorldGen.tileCounts[tile.type] += j <= Main.worldSurface ? 5 : 1;
			}
		}

		if ((flags & TileIOFlags.NextModTile) == TileIOFlags.NextModTile) {
			nextModTile = true;
		}
	}

	/// <summary>
	/// Increases the provided x and y coordinates to the next location in accordance with order-sensitive position IDs.
	/// Typically used in clustering duplicate data across multiple consecutive locations, such as in ModLoader.TileIO
	/// </summary>
	/// <param name="x"></param>
	/// <param name="y"></param>
	/// <returns> False if x and y cannot be increased further (end of the world)  </returns>
	private static bool NextLocation(ref int x, ref int y)
	{
		y++;
		if (y >= Main.maxTilesY) {
			y = 0;
			x++;
			if (x >= Main.maxTilesX) {
				return false;
			}
		}
		return true;
	}
}
