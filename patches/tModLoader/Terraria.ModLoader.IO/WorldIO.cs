using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Terraria.ID;
using Terraria.ModLoader.Default;
using Terraria.ModLoader.Exceptions;
using Terraria.Social;
using Terraria.Utilities;

namespace Terraria.ModLoader.IO
{
	internal static class WorldIO
	{
		public static CustomModDataException customDataFail;

		//add near end of Terraria.IO.WorldFile.saveWorld before releasing locks
		internal static void Save(string path, bool isCloudSave)
		{
			path = Path.ChangeExtension(path, ".twld");
			if (FileUtilities.Exists(path, isCloudSave))
				FileUtilities.Copy(path, path + ".bak", isCloudSave);

			var tag = new TagCompound {
				["chests"] = SaveChests(),
				["tiles"] =  TileIO.SaveTiles(),
				["containers"] = TileIO.SaveContainers(),
				["killCounts"] = SaveNPCKillCounts(),
				["anglerQuest"] = SaveAnglerQuest(),
				["modData"] = SaveModData()
			};

			var stream = new MemoryStream();
			TagIO.ToStream(tag, stream);
			var data = stream.ToArray();
			FileUtilities.Write(path, data, data.Length, isCloudSave);
		}
		//add near end of Terraria.IO.WorldFile.loadWorld before setting failure and success
		internal static void Load(string path, bool isCloudSave)
		{
			customDataFail = null;
			path = Path.ChangeExtension(path, ".twld");
			if (!FileUtilities.Exists(path, isCloudSave))
				return;
			
			var buf = FileUtilities.ReadAllBytes(path, isCloudSave);
			if (buf[0] != 0x1F || buf[1] != 0x8B)
			{
				LoadLegacy(buf);
				return;
			}

			var tag = TagIO.FromStream(new MemoryStream(buf));
			LoadChests(tag.GetList<TagCompound>("chests"));
			TileIO.LoadTiles(tag.GetCompound("tiles"));
			TileIO.LoadContainers(tag.GetCompound("containers"));
			LoadNPCKillCounts(tag.GetList<TagCompound>("killCounts"));
			LoadAnglerQuest(tag.GetCompound("anglerQuest"));
			try
			{
				LoadModData(tag.GetList<TagCompound>("modData"));
			}
			catch (CustomModDataException e)
			{
				customDataFail = e;
				throw;
			}
		}

		internal static List<TagCompound> SaveChests()
		{
			var list = new List<TagCompound>();
			for (int k = 0; k < 1000; k++)
			{
				var chest = Main.chest[k];
				if (chest == null)
					continue;

				var itemTagList = PlayerIO.SaveInventory(chest.item);
				if (itemTagList == null) //doesn't need mod saving
					continue;

				list.Add(new TagCompound {
					["items"] = itemTagList,
					["x"] = chest.x,
					["y"] = chest.y
				});
			}
			return list;
		}

		internal static void LoadChests(IList<TagCompound> list)
		{
			foreach (var tag in list)
			{
				int x = tag.GetInt("x");
				int y = tag.GetInt("y");
				int chest = Chest.FindChest(x, y);
				if (chest < 0)
					chest = Chest.CreateChest(x, y);
				if (chest >= 0)
					PlayerIO.LoadInventory(Main.chest[chest].item, tag.GetList<TagCompound>("items"));
			}
		}

		internal static List<TagCompound> SaveNPCKillCounts()
		{
			var list = new List<TagCompound>();
			for (int type = NPCID.Count; type < NPCLoader.NPCCount; type++)
			{
				if (NPC.killCount[type] <= 0)
					continue;

				list.Add(new TagCompound {
					["mod"] = NPCLoader.GetNPC(type).mod.Name,
					["name"] = Main.npcName[type],
					["count"] = NPC.killCount[type]
				});
			}
			return list;
		}

		internal static void LoadNPCKillCounts(IList<TagCompound> list)
		{
			foreach (var tag in list)
			{
				Mod mod = ModLoader.GetMod(tag.GetString("mod"));
				int type = mod?.NPCType(tag.GetString("name")) ?? 0;
				if (type > 0)
					NPC.killCount[type] = tag.GetInt("count");
			}
		}

		internal static TagCompound SaveAnglerQuest()
		{
			if (Main.anglerQuest < ItemLoader.vanillaQuestFishCount)
				return null;

			int type = Main.anglerQuestItemNetIDs[Main.anglerQuest];
			var modItem = ItemLoader.GetItem(type);

			return new TagCompound {
				["mod"] = modItem.mod.Name,
				["itemName"] = Main.itemName[type]
			};
		}

		internal static void LoadAnglerQuest(TagCompound tag)
		{
			var mod = ModLoader.GetMod(tag.GetString("mod"));
			int type = mod?.ItemType(tag.GetString("itemName")) ?? 0;
			if (type > 0)
			{
				for (int k = 0; k < Main.anglerQuestItemNetIDs.Length; k++)
				{
					if (Main.anglerQuestItemNetIDs[k] == type)
					{
						Main.anglerQuest = k;
						return;
					}
				}
			}
			Main.AnglerQuestSwap();
		}

		internal static List<TagCompound> SaveModData()
		{
			var list = new List<TagCompound>();
			foreach (var modWorld in WorldHooks.worlds)
			{
				var data = modWorld.Save();
				if (data == null)
					continue;

				list.Add(new TagCompound {
					["mod"] = modWorld.mod.Name,
					["name"] = modWorld.Name,
					["data"] = data
				});
			}
			return list;
		}

		internal static void LoadModData(IList<TagCompound> list)
		{
			foreach (var tag in list)
			{
				var mod = ModLoader.GetMod(tag.GetString("mod"));
				var modWorld = mod?.GetModWorld(tag.GetString("name"));
				if (modWorld != null)
				{
					try
					{
						if (tag.HasTag("legacyData"))
							modWorld.LoadLegacy(new BinaryReader(new MemoryStream(tag.GetByteArray("legacyData"))));
						else
							modWorld.Load(tag.GetCompound("data"));
					}
					catch (Exception e)
					{
						throw new CustomModDataException(mod,
							"Error in reading custom world data for " + mod.Name, e);
					}
				}
				else
				{
					((MysteryWorld)ModLoader.GetMod("ModLoader").GetModWorld("MysteryWorld")).data.Add(tag);
				}
			}
		}

		public static void SendModData(BinaryWriter writer)
		{
			foreach (var modWorld in WorldHooks.NetWorlds)
				writer.SafeWrite(w => modWorld.NetSend(w));
		}

		public static void ReceiveModData(BinaryReader reader)
		{
			foreach (var modWorld in WorldHooks.NetWorlds)
			{
				try
				{
					reader.SafeRead(r => modWorld.NetReceive(r));
				}
				catch (IOException)
				{
					//TODO inform modder/user
				}
			}
		}

		private static void LoadLegacy(byte[] buffer)
		{
			const int numByteFlags = 1;
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
					try
					{
						LoadLegacyModWorld(flags, reader);
					}
					catch (CustomModDataException e)
					{
						customDataFail = e;
						throw;
					}
				}
			}
		}

		private static void LoadLegacyModWorld(byte[] flags, BinaryReader reader)
		{
			if (flags.Length == 0)
			{
				return;
			}
			if ((flags[0] & 1) == 1)
			{
				LoadLegacyChests(reader);
			}
			if ((flags[0] & 2) == 2)
			{
				TileIO.LoadLegacyTiles(reader);
			}
			if ((flags[0] & 4) == 4)
			{
				LoadLegacyNPCKillCounts(reader);
			}
			if ((flags[0] & 8) == 8)
			{
				TileIO.ReadContainers(reader);
			}
			if ((flags[0] & 16) == 16)
			{
				LoadLegacyAnglerQuest(reader);
			}
			if ((flags[0] & 32) == 32)
			{
				LoadLegacyModData(reader);
			}
		}

		private static void LoadLegacyChests(BinaryReader reader)
		{
			short count = reader.ReadInt16();
			for (int k = 0; k < count; k++)
			{
				LoadLegacyChest(reader);
			}
		}

		private static void LoadLegacyChest(BinaryReader reader)
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
				ItemIO.LoadLegacyInventory(Main.chest[chest].item, reader, true);
			}
			else
			{
				ItemIO.LoadLegacyInventory(new Item[40], reader, true);
			}
		}

		private static void LoadLegacyNPCKillCounts(BinaryReader reader)
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

		private static void LoadLegacyAnglerQuest(BinaryReader reader)
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

		private static void LoadLegacyModData(BinaryReader reader)
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
								modWorld.LoadLegacy(customReader);
							}
							catch (Exception e)
							{
								throw new CustomModDataException(mod,
									"Error in reading custom world data for " + mod.Name, e);
							}
						}
					}
				}
				else
				{
					var tag = new TagCompound {
						["mod"] = modName,
						["name"] = name,
						["legacyData"] = data
					};
					((MysteryWorld)ModLoader.GetMod("ModLoader").GetModWorld("MysteryWorld")).data.Add(tag);
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
