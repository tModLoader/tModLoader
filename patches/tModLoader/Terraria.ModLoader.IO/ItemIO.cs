using System;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Default;

namespace Terraria.ModLoader.IO
{
	internal static class ItemIO
	{
		//replace netID writes in Terraria.Player.SavePlayer
		//in Terraria.IO.WorldFile.SaveChests include IsModItem for no-item check
		internal static void WriteVanillaID(Item item, BinaryWriter writer)
		{
			writer.Write(ItemLoader.IsModItem(item) ? 0 : item.netID);
		}

		internal static bool WriteModItemSlot(Item[] inv, int slot, BinaryWriter writer, bool writeStack = false, bool writeFavorite = false)
		{
			Item item = inv[slot];
			if (ItemLoader.IsModItem(item))
			{
				writer.Write((ushort)slot);
				WriteModItem(item, writer);
				if (writeStack)
				{
					writer.Write(item.stack);
				}
				if (writeFavorite)
				{
					writer.Write(item.favorited);
				}
				return true;
			}
			return false;
		}

		internal static void ReadModItemSlot(Item[] inv, BinaryReader reader, bool readStack = false, bool readFavorite = false)
		{
			int slot = reader.ReadUInt16();
			Item item = inv[slot];
			ReadModItem(item, reader);
			if (readStack)
			{
				item.stack = reader.ReadInt32();
			}
			if (readFavorite)
			{
				item.favorited = reader.ReadBoolean();
			}
		}

		internal static void WriteModItem(Item item, BinaryWriter writer)
		{
			writer.Write(item.modItem.mod.Name);
			writer.Write(Main.itemName[item.type]);
			byte[] data;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				using (BinaryWriter customWriter = new BinaryWriter(memoryStream))
				{
					item.modItem.SaveCustomData(customWriter);
					customWriter.Flush();
					data = memoryStream.ToArray();
				}
			}
			writer.Write((ushort)data.Length);
			if (data.Length > 0)
			{
				writer.Write(data);
			}
			writer.Write(item.prefix);
		}

		internal static void ReadModItem(Item item, BinaryReader reader)
		{
			string modName = reader.ReadString();
			string itemName = reader.ReadString();
			Mod mod = ModLoader.GetMod(modName);
			int type = mod == null ? 0 : mod.ItemType(itemName);
			if (type != 0)
			{
				item.netDefaults(type);
				int dataLength = reader.ReadUInt16();
				if (dataLength > 0)
				{
					byte[] data = reader.ReadBytes(dataLength);
					using (MemoryStream memoryStream = new MemoryStream(data))
					{
						using (BinaryReader customReader = new BinaryReader(memoryStream))
						{
							item.modItem.LoadCustomData(customReader);
						}
					}
				}
				if (type == ModLoader.GetMod("ModLoader").ItemType("MysteryItem"))
				{
					MysteryItem mystery = item.modItem as MysteryItem;
					modName = mystery.GetModName();
					itemName = mystery.GetItemName();
					mod = ModLoader.GetMod(modName);
					type = mod == null ? 0 : mod.ItemType(itemName);
					if (type != 0)
					{
						item.netDefaults(type);
					}
				}
			}
			else
			{
				item.netDefaults(ModLoader.GetMod("ModLoader").ItemType("MysteryItem"));
				MysteryItem mystery = item.modItem as MysteryItem;
				mystery.SetModName(modName);
				mystery.SetItemName(itemName);
				reader.ReadBytes(reader.ReadUInt16());
			}
			item.Prefix(reader.ReadByte());
		}
	}
}
