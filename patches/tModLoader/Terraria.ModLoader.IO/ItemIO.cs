using System;
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

		public static void WriteItem(Item item, BinaryWriter writer, bool writeStack = false, bool writeFavorite = false)
		{
			if (ItemLoader.NeedsGlobalCustomSaving(item) > 0)
			{
				writer.Write("");
			}
			if (item.modItem == null)
			{
				writer.Write("Terraria");
				writer.Write(item.netID);
			}
			else
			{
				writer.Write(item.modItem.mod.Name);
				writer.Write(Main.itemName[item.type]);
			}
			SaveCustomData(item, writer, false);

			writer.Write(item.prefix);

			if (writeStack)
				writer.Write(item.stack);

			if (writeFavorite)
				writer.Write(item.favorited);
		}

		public static void ReadItem(Item item, BinaryReader reader, bool readStack = false, bool readFavorite = false)
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
				ReadCustomData(item, GetCustomData(item.type, reader, hasGlobalSaving), hasGlobalSaving);
			}
			else
			{
				string itemName = reader.ReadString();
				int type = ModLoader.GetMod(modName)?.ItemType(itemName) ?? 0;
				byte[] data = GetCustomData(type == 0 ? Int32.MaxValue : type, reader, hasGlobalSaving);
				if (type != 0)
				{
					item.netDefaults(type);
					ReadCustomData(item, data, hasGlobalSaving);
				}
				else
				{
					item.netDefaults(ModLoader.GetMod("ModLoader").ItemType("MysteryItem"));
					((MysteryItem)item.modItem).Setup(modName, itemName, data, hasGlobalSaving);
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

		public static void SaveCustomData(Item item, BinaryWriter writer, bool alwaysGlobal = true)
		{
			if (ItemLoader.IsModItem(item))
			{
				item.modItem.PreSaveCustomData();
				byte[] data;
				using (MemoryStream memoryStream = new MemoryStream())
				{
					using (BinaryWriter customWriter = new BinaryWriter(memoryStream))
					{
						item.modItem.SaveCustomData(customWriter);
					}
					data = memoryStream.ToArray();
				}
				writer.Write((ushort)data.Length);
				writer.Write(data);
			}
			int numGlobals = ItemLoader.NeedsGlobalCustomSaving(item);
			if (numGlobals > 0)
			{
				writer.Write((ushort)numGlobals);
				foreach (GlobalItem globalItem in ItemLoader.globalItems)
				{
					globalItem.PreSaveCustomData(item);
					if (globalItem.NeedsCustomSaving(item))
					{
						byte[] data;
						using (MemoryStream memoryStream = new MemoryStream())
						{
							using (BinaryWriter customWriter = new BinaryWriter(memoryStream))
							{
								globalItem.SaveCustomData(item, customWriter);
							}
							data = memoryStream.ToArray();
						}
						writer.Write(globalItem.mod.Name);
						writer.Write(globalItem.Name);
						writer.Write((ushort)data.Length);
						writer.Write(data);
					}
				}
			}
			else if (alwaysGlobal)
			{
				writer.Write((ushort)0);
			}
		}

		public static byte[] GetCustomData(int type, BinaryReader reader, bool hasGlobalSaving = true)
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

		public static void ReadCustomData(Item item, byte[] data, bool hasGlobalSaving = true)
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
				if (hasGlobalSaving)
				{
					int count = reader.ReadUInt16();
					for (int k = 0; k < count; k++)
					{
						string modName = reader.ReadString();
						string globalName = reader.ReadString();
						byte[] globalData = reader.ReadBytes(reader.ReadUInt16());
						GlobalItem globalItem = ModLoader.GetMod(modName)?.GetGlobalItem(globalName) ?? null;
						if (globalItem != null && globalData.Length > 0)
						{
							using (BinaryReader customReader = new BinaryReader(new MemoryStream(globalData)))
							{
								try
								{
									globalItem.LoadCustomData(item, customReader);
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
	}
}
