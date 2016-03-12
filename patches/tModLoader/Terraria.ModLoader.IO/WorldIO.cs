using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ModLoader.Default;
using Terraria.ModLoader.Exceptions;
using Terraria.Social;
using Terraria.Utilities;

namespace Terraria.ModLoader.IO
{
	internal static class WorldIO
	{
		private const byte numByteFlags = 1;
		//add near end of Terraria.IO.WorldFile.saveWorld before releasing locks
		internal static void WriteModFile(string path, bool isCloudSave)
		{
			path = Path.ChangeExtension(path, ".twld");
			if (FileUtilities.Exists(path, isCloudSave))
			{
				FileUtilities.Copy(path, path + ".bak", isCloudSave, true);
			}
			byte[] flags;
			byte[] data;
			using (MemoryStream stream = new MemoryStream())
			{
				using (BinaryWriter writer = new BinaryWriter(stream))
				{
					flags = WriteModWorld(writer);
					writer.Flush();
					data = stream.ToArray();
				}
			}
			using (MemoryStream stream = new MemoryStream())
			{
				using (BinaryWriter writer = new BinaryWriter(stream))
				{
					byte limit;
					for (limit = (byte)flags.Length; limit > 0; limit--)
					{
						if (flags[limit - 1] != 0)
						{
							break;
						}
					}
					writer.Write(limit);
					if (limit > 0)
					{
						writer.Write(flags, 0, limit);
						writer.Write(data);
					}
					writer.Flush();
					data = stream.ToArray();
				}
			}
			FileUtilities.Write(path, data, data.Length, isCloudSave);
		}
		//add near end of Terraria.IO.WorldFile.loadWorld before setting failure and success
		internal static void ReadModFile(string path, bool isCloudSave)
		{
			path = Path.ChangeExtension(path, ".twld");
			if (!FileUtilities.Exists(path, isCloudSave))
			{
				return;
			}
			byte[] buffer = FileUtilities.ReadAllBytes(path, isCloudSave);
			using (MemoryStream stream = new MemoryStream(buffer))
			{
				using (BinaryReader reader = new BinaryReader(stream))
				{
					byte limit = reader.ReadByte();
					if (limit == 0)
					{
						return;
					}
					byte[] flags = reader.ReadBytes(limit);
					if (flags.Length < numByteFlags)
					{
						Array.Resize(ref flags, numByteFlags);
					}
					ReadModWorld(flags, reader);
				}
			}
		}

		internal static byte[] WriteModWorld(BinaryWriter writer)
		{
			byte[] flags = new byte[numByteFlags];
			if (WriteChests(writer))
			{
				flags[0] |= 1;
			}
			if (TileIO.WriteTiles(writer))
			{
				flags[0] |= 2;
			}
			if (WriteNPCKillCounts(writer))
			{
				flags[0] |= 4;
			}
			if (TileIO.WriteContainers(writer))
			{
				flags[0] |= 8;
			}
			if (WriteAnglerQuest(writer))
			{
				flags[0] |= 16;
			}
			if (WriteCustomData(writer))
			{
				flags[0] |= 32;
			}
			return flags;
		}

		internal static void ReadModWorld(byte[] flags, BinaryReader reader)
		{
			if (flags.Length == 0)
			{
				return;
			}
			if ((flags[0] & 1) == 1)
			{
				ReadChests(reader);
			}
			if ((flags[0] & 2) == 2)
			{
				TileIO.ReadTiles(reader);
			}
			if ((flags[0] & 4) == 4)
			{
				ReadNPCKillCounts(reader);
			}
			if ((flags[0] & 8) == 8)
			{
				TileIO.ReadContainers(reader);
			}
			if ((flags[0] & 16) == 16)
			{
				ReadAnglerQuest(reader);
			}
			if ((flags[0] & 32) == 32)
			{
				ReadCustomData(reader);
			}
		}

		internal static bool WriteChests(BinaryWriter writer)
		{
			short count = 0;
			byte[] data;
			using (MemoryStream stream = new MemoryStream())
			{
				using (BinaryWriter chestWriter = new BinaryWriter(stream))
				{
					for (int k = 0; k < 1000; k++)
					{
						Chest chest = Main.chest[k];
						if (chest != null)
						{
							if (WriteChest(chest, chestWriter))
							{
								count++;
							}
						}
					}
					chestWriter.Flush();
					data = stream.ToArray();
				}
			}
			if (count > 0)
			{
				writer.Write(count);
				writer.Write(data);
				return true;
			}
			return false;
		}

		internal static void ReadChests(BinaryReader reader)
		{
			short count = reader.ReadInt16();
			for (int k = 0; k < count; k++)
			{
				ReadChest(reader);
			}
		}

		internal static bool WriteChest(Chest chest, BinaryWriter writer)
		{
			bool flag;
			byte[] data;
			using (MemoryStream stream = new MemoryStream())
			{
				using (BinaryWriter invWriter = new BinaryWriter(stream))
				{
					flag = PlayerIO.WriteInventory(chest.item, invWriter, true);
					invWriter.Flush();
					data = stream.ToArray();
				}
			}
			if (flag)
			{
				writer.Write(chest.x);
				writer.Write(chest.y);
				writer.Write(data);
			}
			return flag;
		}

		internal static void ReadChest(BinaryReader reader)
		{
			int x = reader.ReadInt32();
			int y = reader.ReadInt32();
			int chest = Chest.FindChest(x, y);
			if (chest < 0)
			{
				chest = Chest.CreateChest(x, y);
			}
			if (chest >= 0)
			{
				PlayerIO.ReadInventory(Main.chest[chest].item, reader, true);
			}
			else
			{
				PlayerIO.ReadInventory(new Item[40], reader, true);
			}
		}

		internal static bool WriteNPCKillCounts(BinaryWriter writer)
		{
			byte[] data;
			ushort numCounts = 0;
			using (MemoryStream stream = new MemoryStream())
			{
				using (BinaryWriter countWriter = new BinaryWriter(stream))
				{
					foreach (int type in NPCLoader.npcs.Keys)
					{
						if (NPC.killCount[type] > 0)
						{
							countWriter.Write(NPCLoader.GetNPC(type).mod.Name);
							countWriter.Write(Main.npcName[type]);
							countWriter.Write(NPC.killCount[type]);
							numCounts++;
						}
					}
					countWriter.Flush();
					data = stream.ToArray();
				}
			}
			if (numCounts > 0)
			{
				writer.Write(numCounts);
				writer.Write(data);
				return true;
			}
			return false;
		}

		internal static void ReadNPCKillCounts(BinaryReader reader)
		{
			ushort numCounts = reader.ReadUInt16();
			for (ushort k = 0; k < numCounts; k++)
			{
				string modName = reader.ReadString();
				string name = reader.ReadString();
				int count = reader.ReadInt32();
				Mod mod = ModLoader.GetMod(modName);
				int type = mod == null ? 0 : mod.NPCType(name);
				if (type > 0)
				{
					NPC.killCount[type] = count;
				}
			}
		}

		internal static bool WriteAnglerQuest(BinaryWriter writer)
		{
			if (Main.anglerQuest >= ItemLoader.vanillaQuestFishCount)
			{
				int type = Main.anglerQuestItemNetIDs[Main.anglerQuest];
				ModItem modItem = ItemLoader.GetItem(type);
				writer.Write(modItem.mod.Name);
				writer.Write(Main.itemName[type]);
				return true;
			}
			return false;
		}

		internal static void ReadAnglerQuest(BinaryReader reader)
		{
			string modName = reader.ReadString();
			string name = reader.ReadString();
			Mod mod = ModLoader.GetMod(modName);
			int type = 0;
			if (mod != null)
			{
				type = mod.ItemType(name);
			}
			bool flag = false;
			if (type > 0)
			{
				for (int k = 0; k < Main.anglerQuestItemNetIDs.Length; k++)
				{
					if (Main.anglerQuestItemNetIDs[k] == type)
					{
						Main.anglerQuest = k;
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				Main.AnglerQuestSwap();
			}
		}

		internal static bool WriteCustomData(BinaryWriter writer)
		{
			ushort count = 0;
			byte[] data;
			using (MemoryStream stream = new MemoryStream())
			{
				using (BinaryWriter customWriter = new BinaryWriter(stream))
				{
					foreach (var modWorld in WorldHooks.worlds)
					{
						if (WriteCustomData(modWorld, customWriter))
						{
							count++;
						}
					}
					customWriter.Flush();
					data = stream.ToArray();
				}
			}
			if (count > 0)
			{
				writer.Write(count);
				writer.Write(data);
				return true;
			}
			return false;
		}

		internal static bool WriteCustomData(ModWorld modWorld, BinaryWriter writer)
		{
			byte[] data;
			using (MemoryStream stream = new MemoryStream())
			{
				using (BinaryWriter customWriter = new BinaryWriter(stream))
				{
					modWorld.SaveCustomData(customWriter);
					customWriter.Flush();
					data = stream.ToArray();
				}
			}
			if (data.Length > 0)
			{
				writer.Write(modWorld.mod.Name);
				writer.Write(modWorld.Name);
				writer.Write((ushort)data.Length);
				writer.Write(data);
				return true;
			}
			return false;
		}

		internal static void ReadCustomData(BinaryReader reader)
		{
			int count = reader.ReadUInt16();
			for (int k = 0; k < count; k++)
			{
				string modName = reader.ReadString();
				string name = reader.ReadString();
				byte[] data = reader.ReadBytes(reader.ReadUInt16());
				Mod mod = ModLoader.GetMod(modName);
				ModWorld modWorld = mod == null ? null : mod.GetModWorld(name);
				if (modWorld != null)
				{
					using (MemoryStream stream = new MemoryStream(data))
					{
						using (BinaryReader customReader = new BinaryReader(stream))
						{
                            try
                            {
                                modWorld.LoadCustomData(customReader);
                            }
                            catch (Exception e)
                            {
                                throw new CustomModDataException(
                                    "Error in reading custom world data for " + mod.Name, e);
                            }
						}
					}
					if (modName == "ModLoader" && name == "MysteryWorld")
					{
						((MysteryWorld)modWorld).RestoreData();
					}
				}
				else
				{
					ModWorld mystery = ModLoader.GetMod("ModLoader").GetModWorld("MysteryWorld");
					((MysteryWorld)mystery).AddData(modName, name, data);
				}
			}
		}
		//add to end of Terraria.IO.WorldFileData.MoveToCloud
		internal static void MoveToCloud(string localPath, string cloudPath)
		{
			localPath = Path.ChangeExtension(localPath, ".twld");
			cloudPath = Path.ChangeExtension(cloudPath, ".twld");
			if (File.Exists(localPath))
			{
				FileUtilities.MoveToCloud(localPath, cloudPath);
			}
		}
		//add to end of Terraria.IO.WorldFileData.MoveToLocal
		internal static void MoveToLocal(string cloudPath, string localPath)
		{
			cloudPath = Path.ChangeExtension(cloudPath, ".twld");
			localPath = Path.ChangeExtension(localPath, ".twld");
			if (FileUtilities.Exists(cloudPath, true))
			{
				FileUtilities.MoveToLocal(cloudPath, localPath);
			}
		}
		//in Terraria.Main.DrawMenu in menuMode == 200 add after moving .bak file
		internal static void LoadBackup(string path, bool cloudSave)
		{
			path = Path.ChangeExtension(path, ".twld");
			if (FileUtilities.Exists(path + ".bak", cloudSave))
			{
				FileUtilities.Move(path + ".bak", path, cloudSave, true);
			}
		}
		//in Terraria.WorldGen.do_playWorldCallback add this after moving .bak file
		internal static void LoadDedServBackup(string path, bool cloudSave)
		{
			path = Path.ChangeExtension(path, ".twld");
			if (FileUtilities.Exists(path, cloudSave))
			{
				FileUtilities.Copy(path, path + ".bad", cloudSave, true);
			}
			if (FileUtilities.Exists(path + ".bak", cloudSave))
			{
				FileUtilities.Copy(path + ".bak", path, cloudSave, true);
				FileUtilities.Delete(path + ".bak", cloudSave);
			}
		}
		//in Terraria.WorldGen.do_playWorldCallback add this after returning .bak file
		internal static void RevertDedServBackup(string path, bool cloudSave)
		{
			path = Path.ChangeExtension(path, ".twld");
			if (FileUtilities.Exists(path, cloudSave))
			{
				FileUtilities.Copy(path, path + ".bak", cloudSave, true);
			}
			if (FileUtilities.Exists(path + ".bad", cloudSave))
			{
				FileUtilities.Copy(path + ".bad", path, cloudSave, true);
				FileUtilities.Delete(path + ".bad", cloudSave);
			}
		}
		//in Terraria.Main.EraseWorld before reloading worlds add
		//  WorldIO.EraseWorld(Main.WorldList[i].Path, Main.WorldList[i].IsCloudSave);
		internal static void EraseWorld(string path, bool cloudSave)
		{
			path = Path.ChangeExtension(path, ".twld");
			if (!cloudSave)
			{
#if WINDOWS
				FileOperationAPIWrapper.MoveToRecycleBin(path);
				FileOperationAPIWrapper.MoveToRecycleBin(path + ".bak");
#else
				File.Delete(path);
				File.Delete(path + ".bak");
#endif
			}
			else if (SocialAPI.Cloud != null)
			{
				SocialAPI.Cloud.Delete(path);
			}
		}
	}
}
