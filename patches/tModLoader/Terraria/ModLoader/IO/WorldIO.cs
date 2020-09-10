using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
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
		internal static void Save(string path, bool isCloudSave) {
			path = Path.ChangeExtension(path, ".twld");
			if (FileUtilities.Exists(path, isCloudSave))
				FileUtilities.Copy(path, path + ".bak", isCloudSave);

			var tag = new TagCompound {
				["chests"] = SaveChests(),
				["tiles"] = TileIO.SaveTiles(),
				["containers"] = TileIO.SaveContainers(),
				["npcs"] = SaveNPCs(),
				["tileEntities"] = TileIO.SaveTileEntities(),
				["killCounts"] = SaveNPCKillCounts(),
				["anglerQuest"] = SaveAnglerQuest(),
				["townManager"] = SaveTownManager(),
				["modData"] = SaveModData()
			};

			var stream = new MemoryStream();
			TagIO.ToStream(tag, stream);
			var data = stream.ToArray();
			FileUtilities.Write(path, data, data.Length, isCloudSave);
		}
		//add near end of Terraria.IO.WorldFile.loadWorld before setting failure and success
		internal static void Load(string path, bool isCloudSave) {
			customDataFail = null;
			path = Path.ChangeExtension(path, ".twld");

			if (!FileUtilities.Exists(path, isCloudSave))
				return;

			byte[] buf = FileUtilities.ReadAllBytes(path, isCloudSave);

			if (buf[0] != 0x1F || buf[1] != 0x8B) {
				//LoadLegacy(buf);
				return;
			}

			var tag = TagIO.FromStream(new MemoryStream(buf));
			LoadChests(tag.GetList<TagCompound>("chests"));
			TileIO.LoadTiles(tag.GetCompound("tiles"));
			TileIO.LoadContainers(tag.GetCompound("containers"));
			LoadNPCs(tag.GetList<TagCompound>("npcs"));
			try {
				TileIO.LoadTileEntities(tag.GetList<TagCompound>("tileEntities"));
			}
			catch (CustomModDataException e) {
				customDataFail = e;
				throw;
			}
			LoadNPCKillCounts(tag.GetList<TagCompound>("killCounts"));
			LoadAnglerQuest(tag.GetCompound("anglerQuest"));
			LoadTownManager(tag.GetList<TagCompound>("townManager"));
			try {
				LoadModData(tag.GetList<TagCompound>("modData"));
			}
			catch (CustomModDataException e) {
				customDataFail = e;
				throw;
			}
		}

		internal static List<TagCompound> SaveChests() {
			var list = new List<TagCompound>();
			for (int k = 0; k < 1000; k++) {
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

		internal static void LoadChests(IList<TagCompound> list) {
			foreach (var tag in list) {
				int x = tag.GetInt("x");
				int y = tag.GetInt("y");
				int chest = Chest.FindChest(x, y);
				if (chest < 0)
					chest = Chest.CreateChest(x, y);
				if (chest >= 0)
					PlayerIO.LoadInventory(Main.chest[chest].item, tag.GetList<TagCompound>("items"));
			}
		}

		internal static List<TagCompound> SaveNPCs() {
			var list = new List<TagCompound>();
			for (int k = 0; k < Main.npc.Length; k++) {
				NPC npc = Main.npc[k];
				if (npc.active && NPCLoader.IsModNPC(npc)) {
					if (npc.townNPC) {
						TagCompound tag = new TagCompound {
							["mod"] = npc.modNPC.Mod.Name,
							["name"] = npc.modNPC.Name,
							["displayName"] = npc.GivenName,
							["x"] = npc.position.X,
							["y"] = npc.position.Y,
							["homeless"] = npc.homeless,
							["homeTileX"] = npc.homeTileX,
							["homeTileY"] = npc.homeTileY
						};
						list.Add(tag);
					}
					else if (NPCID.Sets.SavesAndLoads[npc.type]) {
						TagCompound tag = new TagCompound {
							["mod"] = npc.modNPC.Mod.Name,
							["name"] = npc.modNPC.Name,
							["x"] = npc.position.X,
							["y"] = npc.position.Y
						};
						list.Add(tag);
					}
				}
			}
			return list;
		}

		internal static void LoadNPCs(IList<TagCompound> list) {
			if (list == null) {
				return;
			}
			int nextFreeNPC = 0;
			foreach (TagCompound tag in list) {
				if (ModContent.TryFind(tag.GetString("mod"), tag.GetString("name"), out ModNPC modNpc)) {
					while (nextFreeNPC < 200 && Main.npc[nextFreeNPC].active) {
						nextFreeNPC++;
					}
					if (nextFreeNPC >= 200) {
						break;
					}
					NPC npc = Main.npc[nextFreeNPC];
					npc.SetDefaults(modNpc.Type);
					npc.position.X = tag.GetFloat("x");
					npc.position.Y = tag.GetFloat("y");
					if (npc.townNPC) {
						npc.GivenName = tag.GetString("displayName");
						npc.homeless = tag.GetBool("homeless");
						npc.homeTileX = tag.GetInt("homeTileX");
						npc.homeTileY = tag.GetInt("homeTileY");
					}
				}
				else {
					ModContent.GetInstance<UnloadedWorld>().unloadedNPCs.Add(tag);
				}
			}
		}

		internal static List<TagCompound> SaveNPCKillCounts() {
			var list = new List<TagCompound>();
			for (int type = NPCID.Count; type < NPCLoader.NPCCount; type++) {
				if (NPC.killCount[type] <= 0)
					continue;

				list.Add(new TagCompound {
					["mod"] = NPCLoader.GetNPC(type).Mod.Name,
					["name"] = NPCLoader.GetNPC(type).Name,
					["count"] = NPC.killCount[type]
				});
			}
			return list;
		}

		internal static void LoadNPCKillCounts(IList<TagCompound> list) {
			foreach (var tag in list) {
				if (ModContent.TryFind(tag.GetString("mod"), tag.GetString("name"), out ModNPC modNpc)) {
					NPC.killCount[modNpc.Type] = tag.GetInt("count");
				}
				else {
					ModContent.GetInstance<UnloadedWorld>().unloadedKillCounts.Add(tag);
				}
			}
		}

		internal static TagCompound SaveAnglerQuest() {
			if (Main.anglerQuest < ItemLoader.vanillaQuestFishCount)
				return null;

			int type = Main.anglerQuestItemNetIDs[Main.anglerQuest];
			var modItem = ItemLoader.GetItem(type);

			return new TagCompound {
				["mod"] = modItem.Mod.Name,
				["itemName"] = modItem.Name
			};
		}

		internal static void LoadAnglerQuest(TagCompound tag) {
			// Don't try to load modded angler quest item if there isn't one
			if (!tag.ContainsKey("mod")) {
				return;
			}
			if (ModContent.TryFind(tag.GetString("mod"), tag.GetString("itemName"), out ModItem modItem)) {
				for (int k = 0; k < Main.anglerQuestItemNetIDs.Length; k++) {
					if (Main.anglerQuestItemNetIDs[k] == modItem.Type) {
						Main.anglerQuest = k;
						return;
					}
				}
			}
			Main.AnglerQuestSwap();
		}

		internal static List<TagCompound> SaveTownManager() {
			var list = new List<TagCompound>();
			foreach (Tuple<int, Point> pair in WorldGen.TownManager._roomLocationPairs) {
				if (pair.Item1 >= NPCID.Count) {
					ModNPC npc = NPCLoader.GetNPC(pair.Item1);
					TagCompound tag = new TagCompound {
						["mod"] = npc.Mod.Name,
						["name"] = npc.Name,
						["x"] = pair.Item2.X,
						["y"] = pair.Item2.Y
					};
					list.Add(tag);
				}
			}
			return list;
		}

		internal static void LoadTownManager(IList<TagCompound> list) {
			if (list == null) {
				return;
			}
			foreach (TagCompound tag in list) {
				if (ModContent.TryFind(tag.GetString("mod"), tag.GetString("name"), out ModNPC modNpc)) {
					Point location = new Point(tag.GetInt("x"), tag.GetInt("y"));
					WorldGen.TownManager._roomLocationPairs.Add(Tuple.Create(modNpc.Type, location));
					WorldGen.TownManager._hasRoom[modNpc.Type] = true;
				}
			}
		}

		internal static List<TagCompound> SaveModData() {
			var list = new List<TagCompound>();
			foreach (var modWorld in WorldHooks.worlds) {
				var data = modWorld.Save();
				if (data == null)
					continue;

				list.Add(new TagCompound {
					["mod"] = modWorld.Mod.Name,
					["name"] = modWorld.Name,
					["data"] = data
				});
			}
			return list;
		}

		internal static void LoadModData(IList<TagCompound> list) {
			foreach (var tag in list) {
				if (ModContent.TryFind(tag.GetString("mod"), tag.GetString("name"), out ModWorld modWorld)) {
					try {
						modWorld.Load(tag.GetCompound("data"));
					}
					catch (Exception e) {
						throw new CustomModDataException(modWorld.Mod,
							"Error in reading custom world data for " + modWorld.Mod.Name, e);
					}
				}
				else {
					ModContent.GetInstance<UnloadedWorld>().data.Add(tag);
				}
			}
		}

		public static void SendModData(BinaryWriter writer) {
			foreach (var modWorld in WorldHooks.NetWorlds)
				writer.SafeWrite(w => modWorld.NetSend(w));
		}

		public static void ReceiveModData(BinaryReader reader) {
			foreach (var modWorld in WorldHooks.NetWorlds) {
				try {
					reader.SafeRead(r => modWorld.NetReceive(r));
				}
				catch (IOException) {
					Logging.tML.Error($"Above IOException error caused by {modWorld.Name} from the {modWorld.Mod.Name} mod.");
				}
			}
		}

		public static void ValidateSigns() {
			for (int i = 0; i < Main.sign.Length; i++) {
				if (Main.sign[i] != null) {
					Tile tile = Main.tile[Main.sign[i].x, Main.sign[i].y];
					if (!(tile.active() && Main.tileSign[(int)tile.type])) {
						Main.sign[i] = null;
					}
				}
			}
		}

		//add to end of Terraria.IO.WorldFileData.MoveToCloud
		internal static void MoveToCloud(string localPath, string cloudPath) {
			localPath = Path.ChangeExtension(localPath, ".twld");
			cloudPath = Path.ChangeExtension(cloudPath, ".twld");
			if (File.Exists(localPath)) {
				FileUtilities.MoveToCloud(localPath, cloudPath);
			}
		}
		//add to end of Terraria.IO.WorldFileData.MoveToLocal
		internal static void MoveToLocal(string cloudPath, string localPath) {
			cloudPath = Path.ChangeExtension(cloudPath, ".twld");
			localPath = Path.ChangeExtension(localPath, ".twld");
			if (FileUtilities.Exists(cloudPath, true)) {
				FileUtilities.MoveToLocal(cloudPath, localPath);
			}
		}
		//in Terraria.Main.DrawMenu in menuMode == 200 add after moving .bak file
		internal static void LoadBackup(string path, bool cloudSave) {
			path = Path.ChangeExtension(path, ".twld");
			if (FileUtilities.Exists(path + ".bak", cloudSave)) {
				FileUtilities.Move(path + ".bak", path, cloudSave, true);
			}
		}
		//in Terraria.WorldGen.do_playWorldCallback add this after moving .bak file
		internal static void LoadDedServBackup(string path, bool cloudSave) {
			path = Path.ChangeExtension(path, ".twld");
			if (FileUtilities.Exists(path, cloudSave)) {
				FileUtilities.Copy(path, path + ".bad", cloudSave, true);
			}
			if (FileUtilities.Exists(path + ".bak", cloudSave)) {
				FileUtilities.Copy(path + ".bak", path, cloudSave, true);
				FileUtilities.Delete(path + ".bak", cloudSave);
			}
		}
		//in Terraria.WorldGen.do_playWorldCallback add this after returning .bak file
		internal static void RevertDedServBackup(string path, bool cloudSave) {
			path = Path.ChangeExtension(path, ".twld");
			if (FileUtilities.Exists(path, cloudSave)) {
				FileUtilities.Copy(path, path + ".bak", cloudSave, true);
			}
			if (FileUtilities.Exists(path + ".bad", cloudSave)) {
				FileUtilities.Copy(path + ".bad", path, cloudSave, true);
				FileUtilities.Delete(path + ".bad", cloudSave);
			}
		}
		//in Terraria.Main.EraseWorld before reloading worlds add
		//  WorldIO.EraseWorld(Main.WorldList[i].Path, Main.WorldList[i].IsCloudSave);
		internal static void EraseWorld(string path, bool cloudSave) {
			path = Path.ChangeExtension(path, ".twld");
			if (!cloudSave) {
#if WINDOWS
				FileOperationAPIWrapper.MoveToRecycleBin(path);
				FileOperationAPIWrapper.MoveToRecycleBin(path + ".bak");
#else
				File.Delete(path);
				File.Delete(path + ".bak");
#endif
			}
			else if (SocialAPI.Cloud != null) {
				SocialAPI.Cloud.Delete(path);
			}
		}
	}
}
