using Microsoft.Xna.Framework;
using ReLogic.OS;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Terraria.Chat;
using Terraria.GameContent.Events;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.ModLoader.Default;
using Terraria.ModLoader.Exceptions;
using Terraria.Social;
using Terraria.Utilities;

namespace Terraria.ModLoader.IO;

internal static class WorldIO
{
	/// <summary> Contains modded error messages from the world load attempt. </summary>
	public static CustomModDataException customDataFail;

	//add near end of Terraria.IO.WorldFile.saveWorld before releasing locks
	internal static void Save(string path, bool isCloudSave)
	{
		path = Path.ChangeExtension(path, ".twld");
		if (FileUtilities.Exists(path, isCloudSave))
			FileUtilities.Copy(path, path + ".bak", isCloudSave);

		Main.ActiveWorldFileData.ModSaveErrors.Clear();
		TagCompound header = SaveHeader();

		var body = new TagCompound {
			["chests"] = SaveChestInventory(),
			["tiles"] = TileIO.SaveBasics(),
			["containers"] = TileIO.SaveContainers(),
			["npcs"] = SaveNPCs(),
			["tileEntities"] = TileIO.SaveTileEntities(),
			["killCounts"] = SaveNPCKillCounts(),
			["bestiaryKills"] = SaveNPCBestiaryKills(),
			["bestiarySights"] = SaveNPCBestiarySights(),
			["bestiaryChats"] = SaveNPCBestiaryChats(),
			["anglerQuest"] = SaveAnglerQuest(),
			["townManager"] = SaveTownManager(),
			["modData"] = SaveModData(),
			["alteredVanillaFields"] = SaveAlteredVanillaFields()
		};

		TagCompound saveModDataErrors = new TagCompound();
		foreach (var error in Main.ActiveWorldFileData.ModSaveErrors) {
			saveModDataErrors[error.Key] = error.Value;
		}
		header["saveModDataErrors"] = saveModDataErrors;
		TagCompound tag = new TagCompound { ["0header"] = header };
		foreach (var bodyTag in body) {
			tag[bodyTag.Key] = bodyTag.Value;
		}

		FileUtilities.WriteTagCompound(path, isCloudSave, tag);
	}

	//add near end of Terraria.IO.WorldFile.loadWorld before setting failure and success
	internal static void Load(string path, bool isCloudSave)
	{
		customDataFail = null;
		path = Path.ChangeExtension(path, ".twld");

		if (!FileUtilities.Exists(path, isCloudSave))
			return;

		byte[] buf = FileUtilities.ReadAllBytes(path, isCloudSave);

		if (buf[0] != 0x1F || buf[1] != 0x8B) {
			throw new IOException($"{Path.GetFileName(path)}:: File Corrupted during Last Save Step. Aborting... ERROR: Missing NBT Header");
		}

		var tag = TagIO.FromStream(buf.ToMemoryStream());
		TileIO.LoadBasics(tag.GetCompound("tiles"));
		TileIO.LoadContainers(tag.GetCompound("containers"));
		LoadNPCs(tag.GetList<TagCompound>("npcs"));
		try {
			TileIO.LoadTileEntities(tag.GetList<TagCompound>("tileEntities"));
		}
		catch (CustomModDataException e) {
			customDataFail = e;
			throw;
		}
		LoadChestInventory(tag.GetList<TagCompound>("chests")); // Must occur after tiles are loaded
		LoadNPCKillCounts(tag.GetList<TagCompound>("killCounts"));
		LoadNPCBestiaryKills(tag.GetList<TagCompound>("bestiaryKills"));
		LoadNPCBestiarySights(tag.GetList<TagCompound>("bestiarySights"));
		LoadNPCBestiaryChats(tag.GetList<TagCompound>("bestiaryChats"));
		LoadAnglerQuest(tag.GetCompound("anglerQuest"));
		LoadTownManager(tag.GetList<TagCompound>("townManager"));
		try {
			LoadModData(tag.GetList<TagCompound>("modData"));
		}
		catch (CustomModDataException e) {
			customDataFail = e;
			throw;
		}
		LoadAlteredVanillaFields(tag.GetCompound("alteredVanillaFields"));

		if (Main.ActiveWorldFileData.ModSaveErrors.Any()) {
			string fullError = Utils.CreateSaveErrorMessage("tModLoader.WorldCustomDataSaveFail", Main.ActiveWorldFileData.ModSaveErrors).ToString();
			Utils.LogAndConsoleInfoMessage(fullError);
		}
	}

	internal static List<TagCompound> SaveChestInventory()
	{
		var list = new List<TagCompound>();

		const short MaxChestSaveCount = 8000; //As of Vanilla 1.4.0.1

		for (int k = 0; k < MaxChestSaveCount; k++) {
			var chest = Main.chest[k];
			if (chest == null) // chest doesn't exist
				continue;

			var itemTagListModded = PlayerIO.SaveInventory(chest.item); // list of mod only items in inventory
			if (itemTagListModded == null) // Doesn't need additional saving beyond vanilla
				continue;

			TagCompound tag = new TagCompound {
				["items"] = itemTagListModded,
				["x"] = chest.x,
				["y"] = chest.y,
			};

			list.Add(tag);
		}

		return list;
	}

	internal static void LoadChestInventory(IList<TagCompound> list)
	{
		foreach (var tag in list) {
			int cID = Chest.FindChest(tag.GetInt("x"), tag.GetInt("y"));

			if (cID >= 0) {
				var chest = Main.chest[cID];

				PlayerIO.LoadInventory(chest.item, tag.GetList<TagCompound>("items"));
			}
		}
	}

	internal static List<TagCompound> SaveNPCs()
	{
		var list = new List<TagCompound>();
		var data = new TagCompound();

		for (int index = 0; index < Main.maxNPCs; index++) {
			NPC npc = Main.npc[index];

			if (!npc.active || !NPCLoader.SavesAndLoads(npc)) {
				continue;
			}

			var globalData = new List<TagCompound>();

			foreach (var g in NPCLoader.HookSaveData.Enumerate(npc)) {
				if (g is UnloadedGlobalNPC unloadedGlobalNPC) {
					globalData.AddRange(unloadedGlobalNPC.data);
					continue;
				}

				g.SaveData(npc, data);
				if (data.Count == 0)
					continue;

				globalData.Add(new TagCompound {
					["mod"] = g.Mod.Name,
					["name"] = g.Name,
					["data"] = data
				});
				data = new TagCompound();
			}

			TagCompound tag;

			if (NPCLoader.IsModNPC(npc)) {
				npc.ModNPC.SaveData(data);

				tag = new TagCompound {
					["mod"] = npc.ModNPC.Mod.Name,
					["name"] = npc.ModNPC.Name
				};

				if (data.Count != 0) {
					tag["data"] = data;
					data = new TagCompound();
				}

				if (npc.townNPC) {
					tag["displayName"] = npc.GivenName;
					tag["homeless"] = npc.homeless;
					tag["homeTileX"] = npc.homeTileX;
					tag["homeTileY"] = npc.homeTileY;
					tag["isShimmered"] = NPC.ShimmeredTownNPCs[npc.type];
					tag["npcTownVariationIndex"] = npc.townNpcVariationIndex;
				}
			}
			else if (globalData.Count != 0) {
				tag = new TagCompound {
					["mod"] = "Terraria",
					["name"] = NPCID.Search.GetName(npc.type)
				};
			}
			else {
				continue;
			}

			tag["x"] = npc.position.X;
			tag["y"] = npc.position.Y;
			tag["globalData"] = globalData;

			list.Add(tag);
		}

		return list;
	}

	internal static void LoadNPCs(IList<TagCompound> list)
	{
		if (list == null) {
			return;
		}

		int nextFreeNPC = 0;

		foreach (TagCompound tag in list) {
			NPC npc = null;

			while (nextFreeNPC < Main.maxNPCs && Main.npc[nextFreeNPC].active) {
				nextFreeNPC++;
			}

			if (tag.GetString("mod") == "Terraria") {
				int npcId = NPCID.Search.GetId(tag.GetString("name"));
				float x = tag.GetFloat("x");
				float y = tag.GetFloat("y");

				int index;

				for (index = 0; index < Main.maxNPCs; index++) {
					npc = Main.npc[index];

					if (npc.active) {
						if (npc.type == npcId && npc.position.X == x && npc.position.Y == y)
							break;
					}
				}

				if (index == Main.maxNPCs) {
					if (nextFreeNPC == Main.maxNPCs) {
						ModContent.GetInstance<UnloadedSystem>().unloadedNPCs.Add(tag);
						continue;
					}
					else {
						npc = Main.npc[nextFreeNPC];
						npc.SetDefaults(npcId);
						npc.position = new Vector2(x, y);
					}
				}
			}
			else {
				if (!ModContent.TryFind(tag.GetString("mod"), tag.GetString("name"), out ModNPC modNpc)) {
					ModContent.GetInstance<UnloadedSystem>().unloadedNPCs.Add(tag);
					continue;
				}

				if (nextFreeNPC == Main.maxNPCs) {
					ModContent.GetInstance<UnloadedSystem>().unloadedNPCs.Add(tag);
					continue;
				}

				npc = Main.npc[nextFreeNPC];
				npc.SetDefaults(modNpc.Type);
				npc.position.X = tag.GetFloat("x");
				npc.position.Y = tag.GetFloat("y");

				if (npc.townNPC) {
					npc.GivenName = tag.GetString("displayName");
					npc.homeless = tag.GetBool("homeless");
					npc.homeTileX = tag.GetInt("homeTileX");
					npc.homeTileY = tag.GetInt("homeTileY");

					NPC.ShimmeredTownNPCs[modNpc.Type] = tag.GetBool("isShimmered");
					npc.townNpcVariationIndex = tag.GetInt("npcTownVariationIndex");
				}

				if (tag.ContainsKey("data")) {
					npc.ModNPC.LoadData((TagCompound)tag["data"]);
				}
			}
			LoadGlobals(npc, tag.GetList<TagCompound>("globalData"));
		}
	}

	private static void LoadGlobals(NPC npc, IList<TagCompound> list)
	{
		foreach (var tag in list) {
			if (ModContent.TryFind(tag.GetString("mod"), tag.GetString("name"), out GlobalNPC globalNPCBase) && npc.TryGetGlobalNPC(globalNPCBase, out var globalNPC)) {
				try {
					globalNPC.LoadData(npc, tag.GetCompound("data"));
				}
				catch (Exception inner) {
					throw new CustomModDataException(globalNPC.Mod, $"Error in reading custom player data for {tag.GetString("mod")}", inner);
				}
			}
			else {
				// Unloaded or no longer valid on an item (e.g. through AppliesToEntity)
				npc.GetGlobalNPC<UnloadedGlobalNPC>().data.Add(tag);
			}
		}
	}

	internal static List<TagCompound> SaveNPCKillCounts()
	{
		var list = new List<TagCompound>();
		for (int type = NPCID.Count; type < NPCLoader.NPCCount; type++) {
			int killCount = NPC.killCount[type];
			if (killCount <= 0)
				continue;

			ModNPC modNPC = NPCLoader.GetNPC(type);
			list.Add(new TagCompound {
				["mod"] = modNPC.Mod.Name,
				["name"] = modNPC.Name,
				["count"] = killCount
			});
		}
		return list;
	}

	internal static void LoadNPCKillCounts(IList<TagCompound> list)
	{
		foreach (var tag in list) {
			if (ModContent.TryFind(tag.GetString("mod"), tag.GetString("name"), out ModNPC modNpc)) {
				NPC.killCount[modNpc.Type] = tag.GetInt("count");
			}
			else {
				ModContent.GetInstance<UnloadedSystem>().unloadedKillCounts.Add(tag);
			}
		}
	}

	internal static List<TagCompound> SaveNPCBestiaryKills()
	{
		var list = new List<TagCompound>();
		for (int type = NPCID.Count; type < NPCLoader.NPCCount; type++) {
			int killCount = Main.BestiaryTracker.Kills.GetKillCount(ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[type]);
			if (killCount <= 0)
				continue;

			ModNPC modNPC = NPCLoader.GetNPC(type);
			list.Add(new TagCompound {
				["mod"] = modNPC.Mod.Name,
				["name"] = modNPC.Name,
				["count"] = killCount
			});
		}
		return list;
	}

	internal static void LoadNPCBestiaryKills(IList<TagCompound> list)
	{
		foreach (var tag in list) {
			if (ModContent.TryFind(tag.GetString("mod"), tag.GetString("name"), out ModNPC modNpc)) {
				string persistentId = ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[modNpc.Type];
				Main.BestiaryTracker.Kills.SetKillCountDirectly(persistentId, tag.GetInt("count"));
			}
			else {
				ModContent.GetInstance<UnloadedSystem>().unloadedBestiaryKills.Add(tag);
			}
		}
	}

	internal static List<TagCompound> SaveNPCBestiarySights()
	{
		var list = new List<TagCompound>();
		for (int type = NPCID.Count; type < NPCLoader.NPCCount; type++) {
			bool seen = Main.BestiaryTracker.Sights.GetWasNearbyBefore(ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[type]);
			if (!seen)
				continue;

			ModNPC modNPC = NPCLoader.GetNPC(type);
			list.Add(new TagCompound {
				["mod"] = modNPC.Mod.Name,
				["name"] = modNPC.Name
			});
		}
		return list;
	}

	internal static void LoadNPCBestiarySights(IList<TagCompound> list)
	{
		foreach (var tag in list) {
			if (ModContent.TryFind(tag.GetString("mod"), tag.GetString("name"), out ModNPC modNpc)) {
				string persistentId = ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[modNpc.Type];
				Main.BestiaryTracker.Sights.SetWasSeenDirectly(persistentId);
			}
			else {
				ModContent.GetInstance<UnloadedSystem>().unloadedBestiarySights.Add(tag);
			}
		}
	}

	internal static List<TagCompound> SaveNPCBestiaryChats()
	{
		var list = new List<TagCompound>();
		for (int type = NPCID.Count; type < NPCLoader.NPCCount; type++) {
			bool chatted = Main.BestiaryTracker.Chats.GetWasChatWith(ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[type]);
			if (!chatted)
				continue;

			ModNPC modNPC = NPCLoader.GetNPC(type);
			list.Add(new TagCompound {
				["mod"] = modNPC.Mod.Name,
				["name"] = modNPC.Name
			});
		}
		return list;
	}

	internal static void LoadNPCBestiaryChats(IList<TagCompound> list)
	{
		foreach (var tag in list) {
			if (ModContent.TryFind(tag.GetString("mod"), tag.GetString("name"), out ModNPC modNpc)) {
				string persistentId = ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[modNpc.Type];
				Main.BestiaryTracker.Chats.SetWasChatWithDirectly(persistentId);
			}
			else {
				ModContent.GetInstance<UnloadedSystem>().unloadedBestiaryChats.Add(tag);
			}
		}
	}

	internal static TagCompound SaveAnglerQuest()
	{
		if (Main.anglerQuest < ItemLoader.vanillaQuestFishCount)
			return null;

		int type = Main.anglerQuestItemNetIDs[Main.anglerQuest];
		var modItem = ItemLoader.GetItem(type);

		return new TagCompound {
			["mod"] = modItem.Mod.Name,
			["itemName"] = modItem.Name
		};
	}

	internal static void LoadAnglerQuest(TagCompound tag)
	{
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

	internal static List<TagCompound> SaveTownManager()
	{
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

	internal static void LoadTownManager(IList<TagCompound> list)
	{
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

	internal static List<TagCompound> SaveModData()
	{
		var list = new List<TagCompound>();

		var saveData = new TagCompound();

		foreach (var system in SystemLoader.Systems) {
			try {
				system.SaveWorldData(saveData);
			}
			catch (Exception e) {
				var message = NetworkText.FromKey("tModLoader.SaveWorldDataExceptionWarning", system.Name, system.Mod.Name, "\n\n" + e.ToString());
				Utils.HandleSaveErrorMessageLogging(message, broadcast: true);

				Main.ActiveWorldFileData.ModSaveErrors[$"{system.FullName}.SaveWorldData"] = e.ToString();

				saveData = new TagCompound();
				continue; // don't want to save half-broken data, that could compound errors.
			}

			if (saveData.Count == 0)
				continue;

			list.Add(new TagCompound {
				["mod"] = system.Mod.Name,
				["name"] = system.Name,
				["data"] = saveData
			});
			saveData = new TagCompound();
		}

		return list;
	}

	internal static void LoadModData(IList<TagCompound> list)
	{
		foreach (var tag in list) {
			if (ModContent.TryFind(tag.GetString("mod"), tag.GetString("name"), out ModSystem system)) {
				try {
					system.LoadWorldData(tag.GetCompound("data"));
				}
				catch (Exception e) {
					throw new CustomModDataException(system.Mod,
						"Error in reading custom world data for " + system.Mod.Name, e);
				}
			}
			else {
				ModContent.GetInstance<UnloadedSystem>().data.Add(tag);
			}
		}
	}

	internal static TagCompound SaveAlteredVanillaFields()
	{
		return new TagCompound {
			["timeCultists"] = CultistRitual.delay,
			["timeRain"] = Main.rainTime,
			["timeSandstorm"] = Sandstorm.TimeLeft
		};
	}

	internal static void LoadAlteredVanillaFields(TagCompound compound)
	{
		CultistRitual.delay = compound.GetDouble("timeCultists");
		Main.rainTime = compound.GetDouble("timeRain");
		Sandstorm.TimeLeft = compound.GetDouble("timeSandstorm");
	}

	public static void SendModData(BinaryWriter writer)
	{
		foreach (var system in SystemLoader.HookNetSend.Enumerate())
			writer.SafeWrite(w => system.NetSend(w));
	}

	public static void ReceiveModData(BinaryReader reader)
	{
		foreach (var system in SystemLoader.HookNetReceive.Enumerate()) {
			try {
				reader.SafeRead(r => system.NetReceive(r));
			}
			catch (IOException e) {
				Logging.tML.Error(e.ToString());
				Logging.tML.Error($"Above IOException error caused by {system.Name} from the {system.Mod.Name} mod.");
			}
		}
	}

	public static void ValidateSigns()
	{
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
	internal static void MoveToCloud(string localPath, string cloudPath)
	{
		localPath = Path.ChangeExtension(localPath, ".twld");
		cloudPath = Path.ChangeExtension(cloudPath, ".twld");
		if (File.Exists(localPath)) {
			FileUtilities.MoveToCloud(localPath, cloudPath);
		}
	}
	//add to end of Terraria.IO.WorldFileData.MoveToLocal
	internal static void MoveToLocal(string cloudPath, string localPath)
	{
		cloudPath = Path.ChangeExtension(cloudPath, ".twld");
		localPath = Path.ChangeExtension(localPath, ".twld");
		if (FileUtilities.Exists(cloudPath, true)) {
			FileUtilities.MoveToLocal(cloudPath, localPath);
		}
	}
	//in Terraria.Main.DrawMenu in menuMode == 200 add after moving .bak file
	internal static void LoadBackup(string path, bool cloudSave)
	{
		path = Path.ChangeExtension(path, ".twld");
		if (FileUtilities.Exists(path + ".bak", cloudSave)) {
			FileUtilities.Move(path + ".bak", path, cloudSave, true);
		}
	}
	//in Terraria.WorldGen.do_playWorldCallback add this after moving .bak file
	internal static void LoadDedServBackup(string path, bool cloudSave)
	{
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
	internal static void RevertDedServBackup(string path, bool cloudSave)
	{
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
	internal static void EraseWorld(string path, bool cloudSave)
	{
		path = Path.ChangeExtension(path, ".twld");
		if (!cloudSave) {
			Platform.Get<IPathService>().MoveToRecycleBin(path);
			Platform.Get<IPathService>().MoveToRecycleBin(path + ".bak");
		}
		else if (SocialAPI.Cloud != null) {
			SocialAPI.Cloud.Delete(path);
		}
	}

	private static TagCompound SaveHeader()
	{
		return new TagCompound {
			["modHeaders"] = SaveModHeaders(),
			["usedMods"] = SaveUsedMods(),
			["usedModPack"] = SaveUsedModPack(),
			["generatedWithMods"] = SaveGeneratedWithMods(),
		};
	}

	private static TagCompound SaveModHeaders()
	{
		var modHeaders = new TagCompound();

		var saveData = new TagCompound();
		foreach (var system in SystemLoader.Systems) {
			try {
				system.SaveWorldHeader(saveData);
			}
			catch (Exception e) {
				var message = NetworkText.FromKey("tModLoader.SaveWorldHeaderExceptionWarning", system.Name, system.Mod.Name);
				Utils.HandleSaveErrorMessageLogging(message, broadcast: true);

				Main.ActiveWorldFileData.ModSaveErrors[$"{system.FullName}.SaveWorldHeader"] = e.ToString();

				saveData = new TagCompound();
				continue; // don't want to save half-broken data, that could compound errors.
			}

			if (saveData.Count == 0)
				continue;

			modHeaders[system.FullName] = saveData;
			saveData = new TagCompound();
		}

		// preserve data for unloaded systems
		foreach (var entry in Main.ActiveWorldFileData.ModHeaders) {
			if (!ModContent.TryFind<ModSystem>(entry.Key, out _))
				modHeaders[entry.Key] = entry.Value;
		}

		return modHeaders;
	}

	internal static void ReadWorldHeader(WorldFileData data)
	{
		string path = Path.ChangeExtension(data.Path, ".twld");
		bool isCloudSave = data.IsCloudSave;

		if (!FileUtilities.Exists(path, isCloudSave))
			return;

		try {
			// this code hard-traverses the NBT format to read the just the first nested tag, if preset.
			// It relies on deterministic tag saving order for the header to be first.
			// Because the NBT format is not seekable, there is no way to skip reading the entire tag tree in order to find a specific sub-tag at an arbitrary path.

			using Stream stream = isCloudSave ? new MemoryStream(SocialAPI.Cloud.Read(path)) : new FileStream(path, FileMode.Open);
			using BinaryReader reader = new BigEndianReader(new GZipStream(stream, CompressionMode.Decompress));
			if (reader.ReadByte() != 10)
				throw new IOException("Root tag not a TagCompound");

			// ignore root tag name
			_ = TagIO.ReadTagImpl(8, reader);
			if (reader.ReadByte() != 10)
				return; // no header tag

			if ((string)TagIO.ReadTagImpl(8, reader) != "0header")
				return;

			LoadWorldHeader(data, (TagCompound)TagIO.ReadTagImpl(10, reader));
		}
		catch (Exception ex) {
			Logging.tML.Warn($"Error reading .twld header from: {path} (IsCloudSave={isCloudSave})", ex);
		}
	}

	private static void LoadWorldHeader(WorldFileData data, TagCompound tag)
	{
		LoadModHeaders(data, tag);
		LoadUsedMods(data, tag.GetList<string>("usedMods"));
		LoadUsedModPack(data, tag.GetString("usedModPack"));
		if (tag.ContainsKey("generatedWithMods")) // GetCompound will return an empty TagCompound instead of null. null and empty TagCompound have different meaning for this data.
			LoadGeneratedWithMods(data, tag.GetCompound("generatedWithMods"));
		LoadErrors(data, tag.GetCompound("saveModDataErrors"));
	}

	private static void LoadErrors(WorldFileData data, TagCompound tagCompound)
	{
		foreach (var entry in tagCompound) {
			data.ModSaveErrors[entry.Key] = (string)entry.Value;
		}
	}

	private static void LoadModHeaders(WorldFileData data, TagCompound tag)
	{
		data.ModHeaders = new Dictionary<string, TagCompound>();
		foreach (var entry in tag.GetCompound("modHeaders")) {
			string fullname = entry.Key;

			if (ModContent.TryFind<ModSystem>(fullname, out var system)) // handle legacy renames
				fullname = system.FullName;

			data.ModHeaders[fullname] = (TagCompound)entry.Value;
		}
	}

	internal static void LoadUsedMods(WorldFileData data, IList<string> usedMods)
	{
		data.usedMods = usedMods;
	}

	internal static List<string> SaveUsedMods()
	{
		return ModLoader.Mods.Select(m => m.Name).Except(new[] { "ModLoader" }).ToList();
	}

	internal static void LoadUsedModPack(WorldFileData data, string modpack)
	{
		data.modPack = string.IsNullOrEmpty(modpack) ? null : modpack; // tag.GetString returns "" even though null was saved.
	}

	internal static string SaveUsedModPack()
	{
		return Path.GetFileNameWithoutExtension(Core.ModOrganizer.ModPackActive);
	}

	internal static void LoadGeneratedWithMods(WorldFileData data, TagCompound tag)
	{
		data.modVersionsDuringWorldGen = new Dictionary<string, Version>();
		foreach (var item in tag) {
			// We can't use tag.Get<Version>(item.Key); because sometimes world headers are loaded before mods and custom serializers are loaded, such as with the -world server command line parameter.
			data.modVersionsDuringWorldGen[item.Key] = new Version((string)item.Value);
		}
	}

	internal static TagCompound SaveGeneratedWithMods()
	{
		if (Main.ActiveWorldFileData.modVersionsDuringWorldGen == null)
			return null;
		var tag = new TagCompound();
		foreach (var item in Main.ActiveWorldFileData.modVersionsDuringWorldGen) {
			tag[item.Key] = item.Value;
		}
		return tag;
	}
}
