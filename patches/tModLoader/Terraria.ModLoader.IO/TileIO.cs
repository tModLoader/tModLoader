using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Terraria.ModLoader.IO
{
	internal static class TileIO
	{
		//in Terraria.IO.WorldFile.SaveWorldTiles add type check to tile.active() check and wall check
		internal struct Tables
		{
			internal IDictionary<ushort, ushort> tiles;
			internal IDictionary<ushort, bool> frameImportant;
			internal IDictionary<ushort, ushort> walls;

			internal static Tables Create()
			{
				Tables tables = new Tables();
				tables.tiles = new Dictionary<ushort, ushort>();
				tables.frameImportant = new Dictionary<ushort, bool>();
				tables.walls = new Dictionary<ushort, ushort>();
				return tables;
			}
		}

		internal static bool WriteTiles(BinaryWriter writer)
		{
			IList<ushort> types = new List<ushort>();
			IList<ushort> walls = new List<ushort>();
			for (int i = 0; i < Main.maxTilesX; i++)
			{
				for (int j = 0; j < Main.maxTilesY; j++)
				{
					Tile tile = Main.tile[i, j];
					if (tile.active() && tile.type >= TileID.Count)
					{
						types.Add(tile.type);
					}
					if (tile.wall >= WallID.Count)
					{
						walls.Add(tile.wall);
					}
				}
			}
			if (types.Count > 0 || walls.Count > 0)
			{
				writer.Write((ushort)types.Count);
				foreach (ushort type in types)
				{
					writer.Write(type);
					ModTile modTile = TileLoader.GetTile(type);
					writer.Write(modTile.mod.Name);
					writer.Write(modTile.Name);
					writer.Write(Main.tileFrameImportant[type]);
				}
				writer.Write((ushort)walls.Count);
				foreach (ushort wall in walls)
				{
					writer.Write(wall);
					ModWall modWall = WallLoader.GetWall(wall);
					writer.Write(modWall.mod.Name);
					writer.Write(modWall.Name);
				}
				WriteTileData(writer);
				return true;
			}
			return false;
		}

		internal static void ReadTiles(BinaryReader reader)
		{
			Tables tables = Tables.Create();
			ushort count = reader.ReadUInt16();
			for (int k = 0; k < count; k++)
			{
				ushort type = reader.ReadUInt16();
				string modName = reader.ReadString();
				string name = reader.ReadString();
				Mod mod = ModLoader.GetMod(modName);
				tables.tiles[type] = mod == null ? (ushort)0 : (ushort)mod.TileType(name);
				tables.frameImportant[type] = reader.ReadBoolean();
			}
			count = reader.ReadUInt16();
			for (int k = 0; k < count; k++)
			{
				ushort wall = reader.ReadUInt16();
				string modName = reader.ReadString();
				string name = reader.ReadString();
				Mod mod = ModLoader.GetMod(modName);
				tables.walls[wall] = mod == null ? (ushort)0 : (ushort)mod.WallType(name);
			}
			ReadTileData(reader, tables);
		}

		internal static void WriteTileData(BinaryWriter writer)
		{
			byte skip = 0;
			bool nextModTile = false;
			int i = 0;
			int j = 0;
			do
			{
				Tile tile = Main.tile[i, j];
				if (HasModData(tile))
				{
					if (!nextModTile)
					{
						writer.Write(skip);
						skip = 0;
					}
					else
					{
						nextModTile = false;
					}
					WriteModTile(ref i, ref j, writer, ref nextModTile);
				}
				else
				{
					skip++;
					if (skip == 255)
					{
						writer.Write(skip);
						skip = 0;
					}
				}
			}
			while (NextTile(ref i, ref j));
			if (skip > 0)
			{
				writer.Write(skip);
			}
		}

		internal static void ReadTileData(BinaryReader reader, Tables tables)
		{
			int i = 0;
			int j = 0;
			bool nextModTile = false;
			do
			{
				if (!nextModTile)
				{
					byte skip = reader.ReadByte();
					while (skip == 255)
					{
						for (byte k = 0; k < 255; k++)
						{
							if (!NextTile(ref i, ref j))
							{
								return;
							}
						}
						skip = reader.ReadByte();
					}
					for (byte k = 0; k < skip; k++)
					{
						if (!NextTile(ref i, ref j))
						{
							return;
						}
					}
				}
				else
				{
					nextModTile = false;
				}
				ReadModTile(ref i, ref j, tables, reader, ref nextModTile);
			}
			while (NextTile(ref i, ref j));
		}

		internal static void WriteModTile(ref int i, ref int j, BinaryWriter writer, ref bool nextModTile)
		{
			Tile tile = Main.tile[i, j];
			byte flags = 0;
			byte[] data = new byte[11];
			int index = 1;
			if (tile.active() && tile.type >= TileID.Count)
			{
				flags |= 1;
				data[index] = (byte)tile.type;
				index++;
				data[index] = (byte)(tile.type >> 8);
				index++;
				if (Main.tileFrameImportant[tile.type])
				{
					data[index] = (byte)tile.frameX;
					index++;
					if (tile.frameX >= 256)
					{
						flags |= 2;
						data[index] = (byte)(tile.frameX >> 8);
						index++;
					}
					data[index] = (byte)tile.frameY;
					index++;
					if (tile.frameY >= 256)
					{
						flags |= 4;
						data[index] = (byte)(tile.frameY >> 8);
						index++;
					}
				}
				if (tile.color() != 0)
				{
					flags |= 8;
					data[index] = tile.color();
					index++;
				}
			}
			if (tile.wall >= WallID.Count)
			{
				flags |= 16;
				data[index] = (byte)tile.wall;
				index++;
				data[index] = (byte)(tile.wall >> 8);
				index++;
				if (tile.wallColor() != 0)
				{
					flags |= 32;
					data[index] = tile.wallColor();
					index++;
				}
			}
			int nextI = i;
			int nextJ = j;
			byte sameCount = 0;
			while (NextTile(ref nextI, ref nextJ))
			{
				if (tile.isTheSameAs(Main.tile[nextI, nextJ]) && sameCount < 255)
				{
					sameCount++;
					i = nextI;
					j = nextJ;
				}
				else if (HasModData(Main.tile[nextI, nextJ]))
				{
					flags |= 128;
					nextModTile = true;
					break;
				}
				else
				{
					break;
				}
			}
			if (sameCount > 0)
			{
				flags |= 64;
				data[index] = sameCount;
				index++;
			}
			data[0] = flags;
			writer.Write(data, 0, index);
		}

		internal static void ReadModTile(ref int i, ref int j, Tables tables, BinaryReader reader, ref bool nextModTile)
		{
			byte flags;
			flags = reader.ReadByte();
			Tile tile = Main.tile[i, j];
			if ((flags & 1) == 1)
			{
				tile.active(true);
				ushort saveType = reader.ReadUInt16();
				tile.type = tables.tiles[saveType];
				if (tables.frameImportant[saveType])
				{
					if ((flags & 2) == 2)
					{
						tile.frameX = reader.ReadInt16();
					}
					else
					{
						tile.frameX = reader.ReadByte();
					}
					if ((flags & 4) == 4)
					{
						tile.frameY = reader.ReadInt16();
					}
					else
					{
						tile.frameY = reader.ReadByte();
					}
				}
				else
				{
					tile.frameX = -1;
					tile.frameY = -1;
				}
				if ((flags & 8) == 8)
				{
					tile.color(reader.ReadByte());
				}
				WorldGen.tileCounts[tile.type] += j <= Main.worldSurface ? 5 : 1;
			}
			if ((flags & 16) == 16)
			{
				tile.wall = tables.walls[reader.ReadUInt16()];
				if ((flags & 32) == 32)
				{
					tile.wallColor(reader.ReadByte());
				}
			}
			if ((flags & 64) == 64)
			{
				byte sameCount = reader.ReadByte();
				for (byte k = 0; k < sameCount; k++)
				{
					NextTile(ref i, ref j);
					Main.tile[i, j].CopyFrom(tile);
					WorldGen.tileCounts[tile.type] += j <= Main.worldSurface ? 5 : 1;
				}
			}
			if ((flags & 128) == 128)
			{
				nextModTile = true;
			}
		}

		private static bool HasModData(Tile tile)
		{
			return (tile.active() && tile.type >= TileID.Count) || tile.wall >= WallID.Count;
		}

		private static bool NextTile(ref int i, ref int j)
		{
			j++;
			if (j >= Main.maxTilesY)
			{
				j = 0;
				i++;
				if (i >= Main.maxTilesX)
				{
					return false;
				}
			}
			return true;
		}
	}
}
