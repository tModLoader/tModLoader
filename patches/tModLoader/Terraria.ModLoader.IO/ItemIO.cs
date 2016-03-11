using System.IO;
using Terraria.ModLoader.Default;

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
				byte[] data;
				using (MemoryStream memoryStream = new MemoryStream()) {
					using (BinaryWriter customWriter = new BinaryWriter(memoryStream))
						item.modItem.SaveCustomData(customWriter);

					data = memoryStream.ToArray();
				}
				writer.Write((ushort)data.Length);
				writer.Write(data);
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
			if (modName == "Vanilla")
			{
				item.netDefaults(reader.ReadInt16());
			}
			else
			{
				string itemName = reader.ReadString();
				byte[] data = reader.ReadBytes(reader.ReadUInt16());

				var type = ModLoader.GetMod(modName)?.ItemType(itemName) ?? 0;
				if (type != 0)
				{
					item.netDefaults(type);
					if (data.Length > 0)
						using (BinaryReader customReader = new BinaryReader(new MemoryStream(data)))
							item.modItem.LoadCustomData(customReader);
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
	}
}
