using System;
using System.IO;
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

		public static void WriteItem(Item item, BinaryWriter writer, bool writeStack = false, bool writeFavorite = false)
		{
			if (item.modItem == null)
			{
				writer.Write("Terraria");
				writer.Write(item.netID);
			}
			else
			{
				writer.Write(item.modItem.mod.Name);
				writer.Write(Main.itemName[item.type]);
                SaveCustomData(item, writer);
			}
			
			writer.Write(item.prefix);

			if (writeStack)
				writer.Write(item.stack);

			if (writeFavorite)
				writer.Write(item.favorited);
		}

		public static void ReadItem(Item item, BinaryReader reader, bool readStack = false, bool readFavorite = false)
		{
			string modName = reader.ReadString();
			if (modName == "Terraria")
			{
				item.netDefaults(reader.ReadInt32());
			}
			else
			{
				string itemName = reader.ReadString();
				byte[] data = reader.ReadBytes(reader.ReadUInt16());

				var type = ModLoader.GetMod(modName)?.ItemType(itemName) ?? 0;
				if (type != 0)
				{
					item.netDefaults(type);
                    ReadCustomData(item, data);
				}
				else
				{
					item.netDefaults(ModLoader.GetMod("ModLoader").ItemType("MysteryItem"));
					((MysteryItem)item.modItem).Setup(modName, itemName, data);
				}
			}
			
			item.Prefix(reader.ReadByte());

			if (readStack)
				item.stack = reader.ReadInt32();

			if (readFavorite)
				item.favorited = reader.ReadBoolean();
		}

		public static Item ReadItem(BinaryReader reader, bool readStack = false, bool readFavorite = false)
		{
			var item = new Item();
			ReadItem(item, reader, readStack, readFavorite);
			return item;
        }

        public static void SaveCustomData(Item item, BinaryWriter writer)
        {
            if (ItemLoader.IsModItem(item))
            {
                byte[] data;
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (BinaryWriter customWriter = new BinaryWriter(memoryStream))
                        item.modItem.SaveCustomData(customWriter);

                    data = memoryStream.ToArray();
                }
                writer.Write((ushort)data.Length);
                writer.Write(data);
            }
        }

        public static void ReadCustomData(Item item, byte[] data)
        {
            if (data.Length > 0)
                using (BinaryReader customReader = new BinaryReader(new MemoryStream(data)))
                    try
                    {
                        item.modItem.LoadCustomData(customReader);
                    }
                    catch (Exception e)
                    {
                        throw new CustomModDataException(item.modItem.mod,
                            "Error in reading custom item data for " + item.modItem.mod.Name, e);
                    }
        }
    }
}
