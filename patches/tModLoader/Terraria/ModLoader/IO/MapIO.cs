using System;
using System.Collections.Generic;
using System.IO;
using Terraria.Map;
using Terraria.Social;
using Terraria.Utilities;

namespace Terraria.ModLoader.IO;

internal static class MapIO
{
	//in Terraria.Map.MapHelper.SaveMap at end of try block call MapIO.WriteModFile(text, isCloudSave);
	internal static void WriteModFile(string path, bool isCloudSave)
	{
		path = Path.ChangeExtension(path, ".tmap");
		bool hasModData;
		byte[] data;
		using (MemoryStream stream = new MemoryStream()) {
			using (BinaryWriter writer = new BinaryWriter(stream)) {
				hasModData = WriteModMap(writer);
				writer.Flush();
				data = stream.ToArray();
			}
		}
		if (hasModData) {
			FileUtilities.WriteAllBytes(path, data, isCloudSave);
		}
		else {
			if (isCloudSave && SocialAPI.Cloud != null) {
				SocialAPI.Cloud.Delete(path);
			}
			else {
				File.Delete(path);
			}
		}
	}
	//in Terraria.Map.WorldMap.Load after calling MapHelper.load methods
	//  call MapIO.ReadModFile(text2, isCloudSave);
	internal static void ReadModFile(string path, bool isCloudSave)
	{
		path = Path.ChangeExtension(path, ".tmap");
		if (!FileUtilities.Exists(path, isCloudSave)) {
			return;
		}

		ReadModMap(new BinaryReader(FileUtilities.ReadAllBytes(path, isCloudSave).ToMemoryStream()));
	}

	internal static bool WriteModMap(BinaryWriter writer)
	{
		ISet<ushort> types = new HashSet<ushort>();
		for (int i = 0; i < Main.maxTilesX; i++) {
			for (int j = 0; j < Main.maxTilesY; j++) {
				ushort type = Main.Map[i, j].Type;
				if (type >= MapHelper.modPosition) {
					types.Add(type);
				}
			}
		}
		if (types.Count == 0) {
			return false;
		}
		writer.Write((ushort)types.Count);
		foreach (ushort type in types) {
			writer.Write(type);
			if (MapLoader.entryToTile.ContainsKey(type)) {
				ModTile tile = TileLoader.GetTile(MapLoader.entryToTile[type]);
				writer.Write(true);
				writer.Write(tile.Mod.Name);
				writer.Write(tile.Name);
				writer.Write((ushort)(type - MapHelper.tileLookup[tile.Type]));
			}
			else if (MapLoader.entryToWall.ContainsKey(type)) {
				ModWall wall = WallLoader.GetWall(MapLoader.entryToWall[type]);
				writer.Write(false);
				writer.Write(wall.Mod.Name);
				writer.Write(wall.Name);
				writer.Write((ushort)(type - MapHelper.wallLookup[wall.Type]));
			}
			else {
				writer.Write(true);
				writer.Write("");
				writer.Write("");
				writer.Write((ushort)0);
			}
		}
		WriteMapData(writer);
		return true;
	}

	internal static void ReadModMap(BinaryReader reader)
	{
		IDictionary<ushort, ushort> table = new Dictionary<ushort, ushort>();
		ushort count = reader.ReadUInt16();
		for (ushort k = 0; k < count; k++) {
			ushort type = reader.ReadUInt16();
			bool isTile = reader.ReadBoolean();
			string modName = reader.ReadString();
			string name = reader.ReadString();
			ushort option = reader.ReadUInt16();
			ushort newType = 0;
			if (isTile) {
				if (ModContent.TryFind(modName, name, out ModTile tile)) {
					if (option >= MapLoader.modTileOptions(tile.Type)) {
						option = 0;
					}
					newType = (ushort)MapHelper.TileToLookup(tile.Type, option);
				}
			}
			else {
				if (ModContent.TryFind(modName, name, out ModWall wall)) {
					if (option >= MapLoader.modWallOptions(wall.Type)) {
						option = 0;
					}
					newType = (ushort)(MapHelper.wallLookup[wall.Type] + option);
				}
			}
			table[type] = newType;
		}
		ReadMapData(reader, table);
	}

	internal static void WriteMapData(BinaryWriter writer)
	{
		byte skip = 0;
		bool nextModTile = false;
		int i = 0;
		int j = 0;
		do {
			MapTile tile = Main.Map[i, j];
			if (tile.Type >= MapHelper.modPosition && tile.Light > 18) {
				if (!nextModTile) {
					writer.Write(skip);
					skip = 0;
				}
				else {
					nextModTile = false;
				}
				WriteMapTile(ref i, ref j, writer, ref nextModTile);
			}
			else {
				skip++;
				if (skip == 255) {
					writer.Write(skip);
					skip = 0;
				}
			}
		}
		while (NextTile(ref i, ref j));
		if (skip > 0) {
			writer.Write(skip);
		}
	}

	internal static void ReadMapData(BinaryReader reader, IDictionary<ushort, ushort> table)
	{
		int i = 0;
		int j = 0;
		bool nextModTile = false;
		do {
			if (!nextModTile) {
				byte skip = reader.ReadByte();
				while (skip == 255) {
					for (byte k = 0; k < 255; k++) {
						if (!NextTile(ref i, ref j)) {
							return;
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
			ReadMapTile(ref i, ref j, table, reader, ref nextModTile);
		}
		while (NextTile(ref i, ref j));
	}

	internal static void WriteMapTile(ref int i, ref int j, BinaryWriter writer, ref bool nextModTile)
	{
		MapTile tile = Main.Map[i, j];
		byte flags = 0;
		byte[] data = new byte[9];
		int index = 1;
		data[index] = (byte)tile.Type;
		index++;
		data[index] = (byte)(tile.Type >> 8);
		index++;
		if (tile.Light < 255) {
			flags |= 1;
			data[index] = tile.Light;
			index++;
		}
		if (tile.Color > 0) {
			flags |= 2;
			data[index] = tile.Color;
			index++;
		}
		int nextI = i;
		int nextJ = j;
		uint sameCount = 0;
		while (NextTile(ref nextI, ref nextJ)) {
			MapTile nextTile = Main.Map[nextI, nextJ];
			if (tile.Equals(ref nextTile) && sameCount < UInt32.MaxValue) {
				sameCount++;
				i = nextI;
				j = nextJ;
			}
			else if (nextTile.Type >= MapHelper.modPosition && nextTile.Light > 18) {
				flags |= 32;
				nextModTile = true;
				break;
			}
			else {
				break;
			}
		}
		if (sameCount > 0) {
			flags |= 4;
			data[index] = (byte)sameCount;
			index++;
			if (sameCount > 255) {
				flags |= 8;
				data[index] = (byte)(sameCount >> 8);
				index++;
				if (sameCount > UInt16.MaxValue) {
					flags |= 16;
					data[index] = (byte)(sameCount >> 16);
					index++;
					data[index] = (byte)(sameCount >> 24);
					index++;
				}
			}
		}
		data[0] = flags;
		writer.Write(data, 0, index);
	}

	internal static void ReadMapTile(ref int i, ref int j, IDictionary<ushort, ushort> table,
		BinaryReader reader, ref bool nextModTile)
	{
		byte flags = reader.ReadByte();
		ushort type = table[reader.ReadUInt16()];
		byte light = (flags & 1) == 1 ? reader.ReadByte() : (byte)255;
		byte color = (flags & 2) == 2 ? reader.ReadByte() : (byte)0;
		MapTile tile = MapTile.Create(type, light, color);
		Main.Map.SetTile(i, j, ref tile);
		if ((flags & 4) == 4) {
			uint sameCount;
			if ((flags & 16) == 16) {
				sameCount = reader.ReadUInt32();
			}
			else if ((flags & 8) == 8) {
				sameCount = reader.ReadUInt16();
			}
			else {
				sameCount = reader.ReadByte();
			}
			for (uint k = 0; k < sameCount; k++) {
				NextTile(ref i, ref j);
				tile = MapTile.Create(type, light, color);
				Main.Map.SetTile(i, j, ref tile);
			}
		}
		if ((flags & 32) == 32) {
			nextModTile = true;
		}
	}

	private static bool NextTile(ref int i, ref int j)
	{
		j++;
		if (j >= Main.maxTilesY) {
			j = 0;
			i++;
			if (i >= Main.maxTilesX) {
				return false;
			}
		}
		return true;
	}
}
