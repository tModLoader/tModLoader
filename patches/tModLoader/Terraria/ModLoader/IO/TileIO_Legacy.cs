using System.IO;
using Terraria.ModLoader.Default;

namespace Terraria.ModLoader.IO
{
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

		internal static void LoadLegacy(TagCompound tag) { 
			// Retrieve Locational-Specific Data from 'Data' and apply
			using (var memoryStream = new MemoryStream(tag.GetByteArray("data")))
			using (var reader = new BinaryReader(memoryStream))
				ReadTileData(reader);

			WorldIO.ValidateSigns();
		}

		internal static void ReadTileData(BinaryReader reader) {
			int i = 0;
			int j = 0;
			byte skip;
			bool nextModTile = false;

			// Access indexed shortlist of all tile locations with mod data on either of wall or tiles
			do {
				// Skip vanilla tiles
				if (!nextModTile) {
					skip = reader.ReadByte();
					
					while (skip == 255) {
						for (byte k = 0; k < 255; k++) {
							if (!NextTile(ref i, ref j)) {
								return; //Skip over vanilla tiles
							}
						}

						skip = reader.ReadByte();
					}

					for (byte k = 0; k < skip; k++) {
						if (!NextTile(ref i, ref j)) {
							return;
						}
					}
				}
				else {
					nextModTile = false;
				}

				// Load modded tiles
				ReadModTile(ref i, ref j, reader, ref nextModTile);
			}
			while (NextTile(ref i, ref j));
		}

		internal static void ReadModTile(ref int i, ref int j, BinaryReader reader, ref bool nextModTile) {
			// Access Stored 8bit Flags
			byte flags;
			flags = reader.ReadByte();

			ushort saveType, key;
			TileEntry tEntry;
			WallEntry wEntry;

			// Read Tiles
			Tile tile = Main.tile[i, j];

			if ((flags & TileIOFlags.ModTile) == TileIOFlags.ModTile) {
				tile.active(true);

				saveType = (ushort)(reader.ReadUInt16());

				if (saveKeyTiles.TryGetValue(saveType, out key)) {
					tEntry = tileList[key];
					tile.type = tEntry.id;
				}
				else {
					unloadedKeyTiles.TryGetValue(saveType, out key);
					tEntry = uTileList[key];
					tile.type = unloadedTileIDs[0];
					PosIndexer.MapPosToInfo(tilePosMapList, key, i, j);
				}

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

				WorldGen.tileCounts[tile.type] += j <= Main.worldSurface ? 5 : 1;
			}

			if ((flags & TileIOFlags.ModWall) == TileIOFlags.ModWall) {
				saveType = (ushort)(reader.ReadUInt16());

				saveKeyWalls.TryGetValue(saveType, out key);

				if (key != 0) {
					wEntry = wallList[key];
					tile.wall = wEntry.id;
				}
				else {
					unloadedKeyWalls.TryGetValue(saveType, out key);
					tile.wall = ModContent.Find<ModWall>("ModLoader/UnloadedWall").Type;
					PosIndexer.MapPosToInfo(wallPosMapList, key, i, j);
				}

				if ((flags & TileIOFlags.WallColor) == TileIOFlags.WallColor) {
					tile.wallColor(reader.ReadByte());
				}
			}

			// Handle re-occurence, up to 256 counts.
			if ((flags & TileIOFlags.NextTilesAreSame) == TileIOFlags.NextTilesAreSame) {
				byte sameCount = reader.ReadByte(); //how many are the same

				for (byte k = 0; k < sameCount; k++) { // for all copy-paste tiles
					NextTile(ref i, ref j); // move i,j to the next tile, with vertical being priority

					Main.tile[i, j].CopyFrom(tile);
					WorldGen.tileCounts[tile.type] += j <= Main.worldSurface ? 5 : 1;
				}
			}

			if ((flags & TileIOFlags.NextModTile) == TileIOFlags.NextModTile) {
				nextModTile = true;
			}
		}
	}
}
