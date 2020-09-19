using System;
using System.Collections.Generic;
using System.IO;
using Terraria.ID;
using Terraria.ModLoader.Default;
using Terraria.ModLoader.Exceptions;

namespace Terraria.ModLoader.IO
{
	public static class ItemIO
	{
		//replace netID writes in Terraria.Player.SavePlayer
		//in Terraria.IO.WorldFile.SaveChests include IsModItem for no-item check
		internal static void WriteVanillaID(Item item, BinaryWriter writer) {
			writer.Write(item.modItem != null ? 0 : item.netID);
		}

		public static TagCompound Save(Item item) {
			var tag = new TagCompound();
			if (item.type <= 0)
				return tag;

			if (item.modItem == null) {
				tag.Set("mod", "Terraria");
				tag.Set("id", item.netID);
			}
			else {
				tag.Set("mod", item.modItem.Mod.Name);
				tag.Set("name", item.modItem.Name);
				tag.Set("data", item.modItem.Save());
			}

			if (item.prefix != 0 && item.prefix < PrefixID.Count)
				tag.Set("prefix", item.prefix);

			if (item.prefix >= PrefixID.Count) {
				ModPrefix modPrefix = ModPrefix.GetPrefix(item.prefix);
				if (modPrefix != null) {
					tag.Set("modPrefixMod", modPrefix.Mod.Name);
					tag.Set("modPrefixName", modPrefix.Name);
				}
			}

			if (item.stack > 1)
				tag.Set("stack", item.stack);

			if (item.favorited)
				tag.Set("fav", true);

			tag.Set("globalData", SaveGlobals(item));

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
					item.modItem.Load(tag.GetCompound("data"));
				}
				else {
					item.SetDefaults(ModContent.ItemType<UnloadedItem>());
					((UnloadedItem)item.modItem).Setup(tag);
				}
			}

			if (tag.ContainsKey("modPrefixMod") && tag.ContainsKey("modPrefixName")) {
				item.Prefix(ModContent.TryFind(tag.GetString("modPrefixMod"), tag.GetString("modPrefixName"), out ModPrefix prefix) ? prefix.Type : 0);
			}
			else if (tag.ContainsKey("prefix")) {
				item.Prefix(tag.GetByte("prefix"));
			}
			item.stack = tag.Get<int?>("stack") ?? 1;
			item.favorited = tag.GetBool("fav");

			if (!(item.modItem is UnloadedItem))
				LoadGlobals(item, tag.GetList<TagCompound>("globalData"));
		}

		public static Item Load(TagCompound tag) {
			var item = new Item();
			Load(item, tag);
			return item;
		}

		internal static List<TagCompound> SaveGlobals(Item item) {
			if (item.modItem is UnloadedItem)
				return null; // UnloadedItems cannot have global data

			var list = new List<TagCompound>();
			foreach (var globalItem in ItemLoader.globalItems) {
				var globalItemInstance = globalItem.Instance(item);
				if (!globalItemInstance.NeedsSaving(item))
					continue;

				list.Add(new TagCompound {
					["mod"] = globalItemInstance.Mod.Name,
					["name"] = globalItemInstance.Name,
					["data"] = globalItemInstance.Save(item)
				});
			}
			return list.Count > 0 ? list : null;
		}

		internal static void LoadGlobals(Item item, IList<TagCompound> list) {
			foreach (var tag in list) {
				if (ModContent.TryFind(tag.GetString("mod"), tag.GetString("name"), out GlobalItem globalItem)) {
					var globalItemInstance = globalItem.Instance(item);
					try {
						globalItemInstance.Load(item, tag.GetCompound("data"));
					}
					catch (Exception e) {
						throw new CustomModDataException(globalItem.Mod, $"Error in reading custom player data for {globalItem.FullName}", e);
					}
				}
				else {
					item.GetGlobalItem<UnloadedGlobalItem>().data.Add(tag);
				}
			}
		}

		public static void Send(Item item, BinaryWriter writer, bool writeStack = false, bool writeFavourite = false) {
			writer.Write((short)item.netID);
			writer.Write(item.prefix);
			if (writeStack) writer.Write((short)item.stack);
			if (writeFavourite) writer.Write(item.favorited);
			SendModData(item, writer);
		}

		public static void Receive(Item item, BinaryReader reader, bool readStack = false, bool readFavorite = false) {
			item.netDefaults(reader.ReadInt16());
			item.Prefix(reader.ReadByte());
			if (readStack) item.stack = reader.ReadInt16();
			if (readFavorite) item.favorited = reader.ReadBoolean();
			ReceiveModData(item, reader);
		}

		public static Item Receive(BinaryReader reader, bool readStack = false, bool readFavorite = false) {
			var item = new Item();
			Receive(item, reader, readStack, readFavorite);
			return item;
		}

		public static void SendModData(Item item, BinaryWriter writer) {
			if (item.IsAir) return;
			writer.SafeWrite(w => item.modItem?.NetSend(w));
			foreach (var globalItem in ItemLoader.NetGlobals)
				writer.SafeWrite(w => globalItem.Instance(item).NetSend(item, w));
		}

		public static void ReceiveModData(Item item, BinaryReader reader) {
			if (item.IsAir) return;
			try {
				reader.SafeRead(r => item.modItem?.NetReceive(r));
			}
			catch (IOException) {
				Logging.tML.Error($"Above IOException error caused by {item.modItem.Name} from the {item.modItem.Mod.Name} mod.");
			}

			foreach (var globalItem in ItemLoader.NetGlobals) {
				try {
					reader.SafeRead(r => globalItem.Instance(item).NetReceive(item, r));
				}
				catch (IOException) {
					Logging.tML.Error($"Above IOException error caused by {globalItem.Name} from the {globalItem.Mod.Name} mod while reading {item.Name}.");
				}
			}
		}

		internal static byte[] LegacyModData(int type, BinaryReader reader, bool hasGlobalSaving = true) {
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

		public static string ToBase64(Item item) {
			MemoryStream ms = new MemoryStream();
			TagIO.ToStream(ItemIO.Save(item), ms, true);
			return Convert.ToBase64String(ms.ToArray());
		}

		public static Item FromBase64(string base64) {
			MemoryStream ms = new MemoryStream(Convert.FromBase64String(base64));
			return ItemIO.Load(TagIO.FromStream(ms, true));
		}
	}
}
