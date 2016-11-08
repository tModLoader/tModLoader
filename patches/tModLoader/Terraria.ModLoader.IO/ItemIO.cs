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
		internal static void WriteVanillaID(Item item, BinaryWriter writer)
		{
			writer.Write(ItemLoader.IsModItem(item) ? 0 : item.netID);
		}

		public static TagCompound Save(Item item)
		{
			var tag = new TagCompound();
			if (item.type <= 0)
				return tag;

			if (item.modItem == null)
			{
				tag.SetTag("mod", "Terraria");
				tag.SetTag("id", item.netID);
			}
			else
			{
				tag.SetTag("mod", item.modItem.mod.Name);
				tag.SetTag("name", Main.itemName[item.type]);
				tag.SetTag("data", item.modItem.Save());
			}

			tag.SetTag("prefix", item.prefix);

			if (item.stack > 1)
				tag.SetTag("stack", item.stack);

			if (item.favorited)
				tag.SetTag("fav", true);

			tag.SetTag("globalData", SaveGlobals(item));

			return tag;
		}

		public static void Load(Item item, TagCompound tag)
		{
			if (tag.Count == 0)
			{
				item.netDefaults(0);
				return;
			}

			string modName = tag.GetString("mod");
			if (modName == "Terraria")
			{
				item.netDefaults(tag.GetInt("id"));
				if (tag.HasTag("legacyData"))
					LoadLegacyModData(item, tag.GetByteArray("legacyData"), tag.GetBool("hasGlobalSaving"));
			}
			else
			{
				int type = ModLoader.GetMod(modName)?.ItemType(tag.GetString("name")) ?? 0;
				if (type > 0)
				{
					item.netDefaults(type);
					if (tag.HasTag("legacyData"))
						LoadLegacyModData(item, tag.GetByteArray("legacyData"), tag.GetBool("hasGlobalSaving"));
					else
						item.modItem.Load(tag.GetCompound("data"));
				}
				else
				{
					item.netDefaults(ModLoader.GetMod("ModLoader").ItemType("MysteryItem"));
					((MysteryItem)item.modItem).Setup(tag);
				}
			}

			item.Prefix(tag.GetByte("prefix"));
			item.stack = tag.GetTag<int?>("stack") ?? 1;
			item.favorited = tag.GetBool("fav");

			if (!(item.modItem is MysteryItem))
				LoadGlobals(item, tag.GetList<TagCompound>("globalData"));
		}

		public static Item Load(TagCompound tag)
		{
			var item = new Item();
			Load(item, tag);
			return item;
		}

		internal static List<TagCompound> SaveGlobals(Item item)
		{
			if (item.modItem is MysteryItem)
				return null; //MysteryItems cannot have global data

			var list = new List<TagCompound>();
			foreach (var globalItem in ItemLoader.globalItems)
			{
				if (!globalItem.NeedsSaving(item))
					continue;

				list.Add(new TagCompound {
					["mod"] = globalItem.mod.Name,
					["name"] = globalItem.Name,
					["data"] = globalItem.Save(item)
				});
			}
			return list.Count > 0 ? list : null;
		}

		internal static void LoadGlobals(Item item, IList<TagCompound> list)
		{
			foreach (var tag in list)
			{
				var mod = ModLoader.GetMod(tag.GetString("mod"));
				var globalItem = mod?.GetGlobalItem(tag.GetString("name"));
				if (globalItem != null)
				{
					try
					{
						globalItem.Load(item, tag.GetCompound("data"));
					}
					catch (Exception e)
					{
						throw new CustomModDataException(mod,
							"Error in reading custom player data for " + mod.Name, e);
					}
				}
				else
				{
					item.GetModInfo<MysteryGlobalItemInfo>(ModLoader.GetMod("ModLoader")).data.Add(tag);
				}
			}
		}

		public static void Send(Item item, BinaryWriter writer, bool writeStack = false, bool writeFavourite = false)
		{
			writer.Write((short)item.netID);
			writer.Write(item.prefix);
			if (writeStack) writer.Write((short)item.stack);
			if (writeFavourite) writer.Write(item.favorited);
			SendModData(item, writer);
		}

		public static void Receive(Item item, BinaryReader reader, bool readStack = false, bool readFavorite = false)
		{
			item.netDefaults(reader.ReadInt16());
			item.Prefix(reader.ReadByte());
			if (readStack) item.stack = reader.ReadInt16();
			if (readFavorite) item.favorited = reader.ReadBoolean();
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
			writer.SafeWrite(w => item.modItem?.NetSend(w));
			foreach (var globalItem in ItemLoader.NetGlobals)
				writer.SafeWrite(w => globalItem.NetSend(item, w));
		}

		public static void ReceiveModData(Item item, BinaryReader reader)
		{
			try
			{
				reader.SafeRead(r => item.modItem?.NetRecieve(r));
			}
			catch (IOException)
			{
				//TODO inform modder/user
			}

			foreach (var globalItem in ItemLoader.NetGlobals)
			{
				try
				{
					reader.SafeRead(r => globalItem.NetReceive(item, r));
				}
				catch (IOException)
				{
					//TODO inform modder/user
				}
			}
		}

		public static void LoadLegacy(Item item, BinaryReader reader, bool readStack = false, bool readFavorite = false)
		{
			string modName = reader.ReadString();
			bool hasGlobalSaving = false;
			if (modName.Length == 0)
			{
				hasGlobalSaving = true;
				modName = reader.ReadString();
			}
			if (modName == "Terraria")
			{
				item.netDefaults(reader.ReadInt32());
				LoadLegacyModData(item, LegacyModData(item.type, reader, hasGlobalSaving), hasGlobalSaving);
			}
			else
			{
				string itemName = reader.ReadString();
				int type = ModLoader.GetMod(modName)?.ItemType(itemName) ?? 0;
				byte[] data = LegacyModData(type == 0 ? int.MaxValue : type, reader, hasGlobalSaving);
				if (type != 0)
				{
					item.netDefaults(type);
					LoadLegacyModData(item, data, hasGlobalSaving);
				}
				else
				{
					item.netDefaults(ModLoader.GetMod("ModLoader").ItemType("MysteryItem"));
					var tag = new TagCompound {
						["mod"] = modName,
						["name"] = itemName,
						["hasGlobalSaving"] = hasGlobalSaving,
						["legacyData"] = data
					};
					((MysteryItem)item.modItem).Setup(tag);
				}
			}

			item.Prefix(reader.ReadByte());

			if (readStack)
				item.stack = reader.ReadInt32();

			if (readFavorite)
				item.favorited = reader.ReadBoolean();
		}

		internal static byte[] LegacyModData(int type, BinaryReader reader, bool hasGlobalSaving = true)
		{
			using (MemoryStream memoryStream = new MemoryStream())
			{
				using (BinaryWriter writer = new BinaryWriter(memoryStream))
				{
					if (type >= ItemID.Count)
					{
						ushort length = reader.ReadUInt16();
						writer.Write(length);
						writer.Write(reader.ReadBytes(length));
					}
					if (hasGlobalSaving)
					{
						ushort count = reader.ReadUInt16();
						writer.Write(count);
						for (int k = 0; k < count; k++)
						{
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

		internal static void LoadLegacyModData(Item item, byte[] data, bool hasGlobalSaving = true)
		{
			using (BinaryReader reader = new BinaryReader(new MemoryStream(data)))
			{
				if (ItemLoader.IsModItem(item))
				{
					byte[] modData = reader.ReadBytes(reader.ReadUInt16());
					if (modData.Length > 0)
					{
						using (BinaryReader customReader = new BinaryReader(new MemoryStream(modData)))
						{
							try
							{
								item.modItem.LoadLegacy(customReader);
							}
							catch (Exception e)
							{
								throw new CustomModDataException(item.modItem.mod,
									"Error in reading custom item data for " + item.modItem.mod.Name, e);
							}
						}
					}
				}
				if (hasGlobalSaving)
				{
					int count = reader.ReadUInt16();
					for (int k = 0; k < count; k++)
					{
						string modName = reader.ReadString();
						string globalName = reader.ReadString();
						byte[] globalData = reader.ReadBytes(reader.ReadUInt16());
						GlobalItem globalItem = ModLoader.GetMod(modName)?.GetGlobalItem(globalName);
						//could support legacy global data in mystery globals but eh...
						if (globalItem != null && globalData.Length > 0)
						{
							using (BinaryReader customReader = new BinaryReader(new MemoryStream(globalData)))
							{
								try
								{
									globalItem.LoadLegacy(item, customReader);
								}
								catch (Exception e)
								{
									throw new CustomModDataException(globalItem.mod,
										"Error in reading custom global item data for " + globalItem.mod.Name, e);
								}
							}
						}
					}
				}
			}
		}

		public static void LoadLegacyInventory(Item[] inv, BinaryReader reader, bool readStack = false, bool readFavorite = false)
		{
			int count = reader.ReadUInt16();
			for (int k = 0; k < count; k++)
			{
				LoadLegacy(inv[reader.ReadUInt16()], reader, readStack, readFavorite);
			}
		}
	}
}
