using System;
using System.Collections.Generic;
using System.IO;
using Terraria.ID;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.Default;
using Terraria.ModLoader.Exceptions;

namespace Terraria.ModLoader.IO;

public static class ItemIO
{
	//replace netID writes in Terraria.Player.SavePlayer
	//in Terraria.IO.WorldFile.SaveChests include IsModItem for no-item check
	internal static void WriteVanillaID(Item item, BinaryWriter writer)
		=> writer.Write(item.ModItem != null ? 0 : item.netID);

	internal static void WriteShortVanillaID(Item item, BinaryWriter writer)
		=> WriteShortVanillaID(item.netID, writer);

	internal static void WriteShortVanillaID(int id, BinaryWriter writer)
		=> writer.Write((short)(id >= ItemID.Count ? 0 : id));

	internal static void WriteShortVanillaStack(Item item, BinaryWriter writer)
		=> WriteShortVanillaStack(item.stack, writer);

	internal static void WriteShortVanillaStack(int stack, BinaryWriter writer)
		=> writer.Write((short)(stack > short.MaxValue ? short.MaxValue : stack));

	internal static void WriteByteVanillaPrefix(Item item, BinaryWriter writer)
		=> WriteByteVanillaPrefix(item.prefix, writer);

	internal static void WriteByteVanillaPrefix(int prefix, BinaryWriter writer)
		=> writer.Write((byte)(prefix >= PrefixID.Count ? 0 : prefix));

	public static TagCompound Save(Item item) => Save(item, SaveGlobals(item));

	public static TagCompound Save(Item item, List<TagCompound> globalData)
	{
		var tag = new TagCompound();

		if (item.type <= 0)
			return tag;

		if (item.ModItem == null) {
			tag.Set("mod", "Terraria");
			tag.Set("id", item.netID);
		}
		else {
			tag.Set("mod", item.ModItem.Mod.Name);
			tag.Set("name", item.ModItem.Name);

			var saveData = new TagCompound();
			item.ModItem.SaveData(saveData);

			if (saveData.Count > 0)
				tag.Set("data", saveData);
		}


		if (PrefixLoader.GetPrefix(item.prefix) is ModPrefix modPrefix) {
			if (modPrefix is UnloadedPrefix) {
				UnloadedGlobalItem unloadedGlobalItem = item.GetGlobalItem<UnloadedGlobalItem>();
				tag.Set("modPrefixMod", unloadedGlobalItem.ModPrefixMod);
				tag.Set("modPrefixName", unloadedGlobalItem.ModPrefixName);
			}
			else {
				tag.Set("modPrefixMod", modPrefix.Mod.Name);
				tag.Set("modPrefixName", modPrefix.Name);
			}
		}
		else if (item.prefix != 0 && item.prefix < PrefixID.Count) {
			tag.Set("prefix", (byte)item.prefix);
		}

		if (item.stack > 1)
			tag.Set("stack", item.stack);

		if (item.favorited)
			tag.Set("fav", true);

		tag.Set("globalData", globalData);

		return tag;
	}

	public static void Load(Item item, TagCompound tag)
	{
		string modName = tag.GetString("mod");
		if (modName == "") {
			item.netDefaults(0);
			return;
		}

		if (modName == "Terraria") {
			item.netDefaults(tag.GetInt("id"));
		}
		else {
			if (ModContent.TryFind(modName, tag.GetString("name"), out ModItem modItem)) {
				item.SetDefaults(modItem.Type);
				item.ModItem.LoadData(tag.GetCompound("data"));
			}
			else {
				item.SetDefaults(ModContent.ItemType<UnloadedItem>());
				((UnloadedItem)item.ModItem).Setup(tag);
			}
		}

		LoadModdedPrefix(item, tag);

		item.stack = tag.Get<int?>("stack") ?? 1;
		item.favorited = tag.GetBool("fav");

		if (item.ModItem is not UnloadedItem)
			LoadGlobals(item, tag.GetList<TagCompound>("globalData"));
	}

	internal static void LoadModdedPrefix(Item item, TagCompound tag)
	{
		if (tag.ContainsKey("modPrefixMod") && tag.ContainsKey("modPrefixName")) {
			string modPrefixMod = tag.GetString("modPrefixMod");
			string modPrefixName = tag.GetString("modPrefixName");
			if (ModContent.TryFind(modPrefixMod, modPrefixName, out ModPrefix prefix)) {
				item.Prefix(prefix.Type);
			}
			else {
				item.Prefix(ModContent.PrefixType<UnloadedPrefix>());
				UnloadedGlobalItem unloadedGlobalItem = item.GetGlobalItem<UnloadedGlobalItem>();
				unloadedGlobalItem.ModPrefixMod = modPrefixMod;
				unloadedGlobalItem.ModPrefixName = modPrefixName;
			}
		}
		else if (tag.ContainsKey("prefix")) {
			item.Prefix(tag.GetByte("prefix"));
		}
	}

	public static Item Load(TagCompound tag)
	{
		var item = new Item();
		Load(item, tag);
		return item;
	}

	internal static List<TagCompound> SaveGlobals(Item item)
	{
		if (item.ModItem is UnloadedItem)
			return null; // UnloadedItems cannot have global data

		var list = new List<TagCompound>();

		var saveData = new TagCompound();

		foreach (var g in ItemLoader.HookSaveData.Enumerate(item)) {
			if (g is UnloadedGlobalItem unloadedGlobalItem) {
				list.AddRange(unloadedGlobalItem.data);
				continue;
			}

			g.SaveData(item, saveData);
			if (saveData.Count == 0)
				continue;

			list.Add(new TagCompound {
				["mod"] = g.Mod.Name,
				["name"] = g.Name,
				["data"] = saveData
			});
			saveData = new TagCompound();
		}

		return list.Count > 0 ? list : null;
	}

	internal static void LoadGlobals(Item item, IList<TagCompound> list)
	{
		foreach (var tag in list) {
			if (ModContent.TryFind(tag.GetString("mod"), tag.GetString("name"), out GlobalItem globalItemBase) && item.TryGetGlobalItem(globalItemBase, out var globalItem)) {
				try {
					globalItem.LoadData(item, tag.GetCompound("data"));
				}
				catch (Exception e) {
					throw new CustomModDataException(globalItem.Mod, $"Error in reading custom player data for {globalItem.FullName}", e);
				}
			}
			else {
				// Unloaded or no longer valid on an item (e.g. through AppliesToEntity)
				item.GetGlobalItem<UnloadedGlobalItem>().data.Add(tag);
			}
		}
	}

	public static void Send(Item item, BinaryWriter writer, bool writeStack = false, bool writeFavorite = false)
	{
		writer.Write7BitEncodedInt(item.netID);
		writer.Write7BitEncodedInt(item.prefix);

		if (writeStack)
			writer.Write7BitEncodedInt(item.stack);

		if (writeFavorite)
			writer.Write(item.favorited);

		SendModData(item, writer);
	}

	public static void Receive(Item item, BinaryReader reader, bool readStack = false, bool readFavorite = false)
	{
		item.netDefaults(reader.Read7BitEncodedInt());
		item.Prefix(reader.Read7BitEncodedInt());

		if (readStack)
			item.stack = reader.Read7BitEncodedInt();

		if (readFavorite)
			item.favorited = reader.ReadBoolean();

		ReceiveModData(item, reader);
	}

	public static Item Receive(BinaryReader reader, bool readStack = false, bool readFavorite = false)
	{
		var item = new Item();

		Receive(item, reader, readStack, readFavorite);

		return item;
	}

	public static void SendModData(Item item, BinaryWriter writer)
	{
		if (item.IsAir)
			return;

		writer.SafeWrite(w => item.ModItem?.NetSend(w));

		foreach (var g in ItemLoader.HookNetSend.Enumerate(item)) {
			writer.SafeWrite(w => g.NetSend(item, w));
		}
	}

	public static void ReceiveModData(Item item, BinaryReader reader)
	{
		if (item.IsAir)
			return;

		try {
			reader.SafeRead(r => item.ModItem?.NetReceive(r));
		}
		catch (IOException e) {
			Logging.tML.Error(e.ToString());
			Logging.tML.Error($"Above IOException error caused by {item.ModItem.Name} from the {item.ModItem.Mod.Name} mod.");
		}

		foreach (var g in ItemLoader.HookNetReceive.Enumerate(item)) {
			try {
				reader.SafeRead(r => g.NetReceive(item, r));
			}
			catch (IOException e) {
				Logging.tML.Error(e.ToString());
				Logging.tML.Error($"Above IOException error caused by {g.Name} from the {g.Mod.Name} mod while reading {item.Name}.");
			}
		}
	}

	internal static byte[] LegacyModData(int type, BinaryReader reader, bool hasGlobalSaving = true)
	{
		using (MemoryStream memoryStream = new MemoryStream()) {
			using (BinaryWriter writer = new BinaryWriter(memoryStream)) {
				if (type >= ItemID.Count) {
					ushort length = reader.ReadUInt16();
					writer.Write(length);
					writer.Write(reader.ReadBytes(length));
				}
				if (hasGlobalSaving) {
					ushort count = reader.ReadUInt16();
					writer.Write(count);
					for (int k = 0; k < count; k++) {
						writer.Write(reader.ReadString());
						writer.Write(reader.ReadString());
						ushort length = reader.ReadUInt16();
						writer.Write(length);
						writer.Write(reader.ReadBytes(length));
					}
				}
			}
			return memoryStream.ToArray();
		}
	}

	public static string ToBase64(Item item)
	{
		MemoryStream ms = new MemoryStream();
		TagIO.ToStream(Save(item), ms, true);
		return Convert.ToBase64String(ms.ToArray());
	}

	public static Item FromBase64(string base64)
	{
		return Load(TagIO.FromStream(Convert.FromBase64String(base64).ToMemoryStream(), true));
	}
}
