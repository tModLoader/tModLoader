using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MonoMod.Core.Platforms;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Default;
using Terraria.ModLoader.Engine;
using Terraria.ModLoader.Exceptions;
using Terraria.ModLoader.UI;
using Terraria.Utilities;

namespace Terraria.ModLoader.IO;

internal static class PlayerIO
{
	internal static void WriteByteVanillaHairDye(int hairDye, BinaryWriter writer)
	{
		writer.Write((byte)(hairDye > EffectsTracker.vanillaHairShaderCount ? 0 : hairDye));
	}

	internal static void WriteVanillaHair(int hair, BinaryWriter writer)
	{
		writer.Write(hair >= HairID.Count ? 0 : hair);
	}

	//make Terraria.Player.ENCRYPTION_KEY internal
	//add to end of Terraria.Player.SavePlayer
	internal static void Save(TagCompound tag, string path, bool isCloudSave)
	{
		path = Path.ChangeExtension(path, ".tplr");
		if (FileUtilities.Exists(path, isCloudSave))
			FileUtilities.Copy(path, path + ".bak", isCloudSave);

		FileUtilities.WriteTagCompound(path, isCloudSave, tag);
	}

	internal static TagCompound SaveData(Player player)
	{
		var _temporaryItemSlots = new[] { Main.mouseItem, Main.CreativeMenu.GetItemByIndex(0), Main.guideItem, Main.reforgeItem }; // Don't use player._temporaryItemSlots directly, it expects nulls. It also isn't what is saved in vanilla, which is relevant with Player.SerializedClone. It is only what is used to load in data.

		return new TagCompound {
			["armor"] = SaveInventory(player.armor),
			["dye"] = SaveInventory(player.dye),
			["loadouts"] = SaveLoadouts(player.Loadouts),
			["inventory"] = SaveInventory(player.inventory),
			["miscEquips"] = SaveInventory(player.miscEquips),
			["miscDyes"] = SaveInventory(player.miscDyes),
			["bank"] = SaveInventory(player.bank.item),
			["bank2"] = SaveInventory(player.bank2.item),
			["bank3"] = SaveInventory(player.bank3.item),
			["bank4"] = SaveInventory(player.bank4.item),
			["temporaryItemSlots"] = SaveInventory(_temporaryItemSlots),
			["hairDye"] = SaveHairDye(player.hairDye),
			["research"] = SaveResearch(player),
			["modData"] = SaveModData(player),
			["modBuffs"] = SaveModBuffs(player),
			["infoDisplays"] = SaveInfoDisplays(player),
			["builderToggles"] = SaveBuilderToggles(player),
			["usedMods"] = SaveUsedMods(player),
			["usedModPack"] = SaveUsedModPack(player),
			["hair"] = SaveHair(player.hair)
		};
	}

	//add near end of Terraria.Player.LoadPlayer before accessory check
	internal static void Load(Player player, TagCompound tag)
	{
		LoadInventory(player.armor, tag.GetList<TagCompound>("armor"));
		LoadInventory(player.dye, tag.GetList<TagCompound>("dye"));
		LoadLoadouts(player.Loadouts, tag.GetCompound("loadouts"));
		LoadInventory(player.inventory, tag.GetList<TagCompound>("inventory"));
		LoadInventory(player.miscEquips, tag.GetList<TagCompound>("miscEquips"));
		LoadInventory(player.miscDyes, tag.GetList<TagCompound>("miscDyes"));
		LoadInventory(player.bank.item, tag.GetList<TagCompound>("bank"));
		LoadInventory(player.bank2.item, tag.GetList<TagCompound>("bank2"));
		LoadInventory(player.bank3.item, tag.GetList<TagCompound>("bank3"));
		LoadInventory(player.bank4.item, tag.GetList<TagCompound>("bank4"));
		LoadInventory(player._temporaryItemSlots, tag.GetList<TagCompound>("temporaryItemSlots"));
		LoadHairDye(player, tag.GetString("hairDye"));
		LoadResearch(player, tag.GetList<TagCompound>("research"));
		LoadModData(player, tag.GetList<TagCompound>("modData"));
		LoadModBuffs(player, tag.GetList<TagCompound>("modBuffs"));
		LoadInfoDisplays(player, tag.GetList<string>("infoDisplays"));
		LoadBuilderToggles(player, tag.GetList<TagCompound>("builderToggles"));
		LoadUsedMods(player, tag.GetList<string>("usedMods"));
		LoadUsedModPack(player, tag.GetString("usedModPack"));
		LoadHair(player, tag.GetString("hair"));
	}

	internal static byte[] ReadDataBytes(string path, bool isCloudSave)
	{
		path = Path.ChangeExtension(path, ".tplr");
		if (!FileUtilities.Exists(path, isCloudSave))
			return null;

		return FileUtilities.ReadAllBytes(path, isCloudSave);
	}

	public static List<TagCompound> SaveInventory(Item[] inv)
	{
		var list = new List<TagCompound>();
		for (int k = 0; k < inv.Length; k++) {
			var globalData = ItemIO.SaveGlobals(inv[k]);
			if (globalData != null || ItemLoader.NeedsModSaving(inv[k])) {
				var tag = ItemIO.Save(inv[k], globalData);
				if (tag.Count != 0) {
					tag.Set("slot", (short)k);
					list.Add(tag);
				}
			}
		}
		return list.Count > 0 ? list : null;
	}

	public static void LoadInventory(Item[] inv, IList<TagCompound> list)
	{
		foreach (var tag in list)
			inv[tag.GetShort("slot")] = ItemIO.Load(tag);
	}

	public static List<TagCompound> SaveResearch(Player player)
	{
		var list = new List<TagCompound>();
		var dictionary = new Dictionary<int, int>(player.creativeTracker.ItemSacrifices._sacrificesCountByItemIdCache);

		foreach (var item in dictionary) {
			ModItem modItem = ItemLoader.GetItem(item.Key);

			if (modItem != null) {
				TagCompound tag = new TagCompound {
					["mod"] = modItem.Mod.Name,
					["name"] = modItem.Name,
					["sacrificeCount"] = item.Value
				};

				list.Add(tag);
			}
		}

		return list.Count > 0 ? list : null;
	}

	public static void LoadResearch(Player player, IList<TagCompound> list)
	{
		foreach (var tag in list) {
			if (!tag.ContainsKey("mod") || !tag.ContainsKey("name"))
				continue; // Discard tags from previous insufficient implementation pre-alpha so they are not carried over to unloadedResearch

			string modName = tag.GetString("mod");
			string modItemName = tag.GetString("name");

			if (ModContent.TryFind(modName, modItemName, out ModItem modItem)) {
				int netId = modItem.Type;
				string persistentId = ContentSamples.ItemPersistentIdsByNetIds[netId];

				int sacrificeCount = tag.GetInt("sacrificeCount");
				var itemSacrifices = player.creativeTracker.ItemSacrifices;
				itemSacrifices._sacrificeCountByItemPersistentId[persistentId] = sacrificeCount;
				itemSacrifices._sacrificesCountByItemIdCache[netId] = sacrificeCount;
			}
			else {
				player.GetModPlayer<UnloadedPlayer>().unloadedResearch.Add(tag);
			}
		}
	}

	public static string SaveHairDye(int hairDye)
	{
		if (hairDye <= EffectsTracker.vanillaHairShaderCount)
			return "";

		int itemId = GameShaders.Hair._reverseShaderLookupDictionary[hairDye];
		var modItem = ItemLoader.GetItem(itemId);

		return modItem.FullName;
	}

	public static void LoadHairDye(Player player, string hairDyeItemName)
	{
		if (hairDyeItemName == "")
			return;

		// no mystery hair dye at this stage
		if (ModContent.TryFind<ModItem>(hairDyeItemName, out var modItem))
			player.hairDye = (byte)GameShaders.Hair.GetShaderIdFromItemId(modItem.Type);
	}

	public static string SaveHair(int hair)
	{
		if (hair < HairID.Count)
			return "";

		return HairLoader.GetHair(hair).FullName;
	}

	public static void LoadHair(Player player, string hairName)
	{
		if (hairName == "")
			return;

		if (ModContent.TryFind<ModHair>(hairName, out var modHair))
			player.hair = modHair.Type;
	}

	internal static List<TagCompound> SaveModData(Player player)
	{
		var list = new List<TagCompound>();

		var saveData = new TagCompound();

		foreach (var modPlayer in player.modPlayers) {
			try {
				modPlayer.SaveData(saveData);
			}
			catch (Exception e) {
				// Unlike LoadData, we don't throw error because we don't want users to lose game progress.
				var message = NetworkText.FromKey("tModLoader.SavePlayerDataExceptionWarning", modPlayer.Name, modPlayer.Mod.Name, "\n\n" + e.ToString());
				Utils.HandleSaveErrorMessageLogging(message, broadcast: false);

				list.Add(new TagCompound {
					["mod"] = modPlayer.Mod.Name,
					["name"] = modPlayer.Name,
					["error"] = e.ToString()
				});

				saveData = new TagCompound();
				continue; // don't want to save half-broken data, that could compound errors.
			}

			if (saveData.Count == 0)
				continue;

			list.Add(new TagCompound {
				["mod"] = modPlayer.Mod.Name,
				["name"] = modPlayer.Name,
				["data"] = saveData
			});
			saveData = new TagCompound();
		}

		return list;
	}

	internal static void LoadModData(Player player, IList<TagCompound> list)
	{
		foreach (var tag in list) {
			string modName = tag.GetString("mod");
			string modPlayerName = tag.GetString("name");

			if (tag.TryGet<string>("error", out string errorMessage)) {
				player.ModSaveErrors[$"{modName}/{modPlayerName}.SaveData"] = errorMessage;
				continue;
			}

			if (ModContent.TryFind<ModPlayer>(modName, modPlayerName, out var modPlayerBase)) {
				var modPlayer = player.GetModPlayer(modPlayerBase);

				try {
					modPlayer.LoadData(tag.GetCompound("data"));
				}
				catch (Exception e) {
					var mod = modPlayer.Mod;

					throw new CustomModDataException(mod,
						"Error in reading custom player data for " + mod.Name, e);
				}
			}
			else {
				player.GetModPlayer<UnloadedPlayer>().data.Add(tag);
			}
		}
	}

	internal static List<TagCompound> SaveModBuffs(Player player)
	{
		var list = new List<TagCompound>();
		for (int k = 0; k < Player.MaxBuffs; k++) {
			int buff = player.buffType[k];
			if (buff == 0 || Main.buffNoSave[buff])
				continue;

			if (BuffLoader.IsModBuff(buff)) {
				var modBuff = BuffLoader.GetBuff(buff);
				list.Add(new TagCompound {
					["mod"] = modBuff.Mod.Name,
					["name"] = modBuff.Name,
					["time"] = player.buffTime[k]
				});
			}
			else {
				list.Add(new TagCompound {
					["mod"] = "Terraria",
					["id"] = buff,
					["time"] = player.buffTime[k]
				});
			}
		}
		return list;
	}

	internal static void LoadModBuffs(Player player, IList<TagCompound> list)
	{
		//buffs list is guaranteed to be compacted
		int buffCount = Player.MaxBuffs;
		while (buffCount > 0 && player.buffType[buffCount - 1] == 0)
			buffCount--;

		if (buffCount == 0) {
			//always the case since vanilla buff saving was disabled, when extra buff slots were added
			foreach (var tag in list) {
				if (buffCount == Player.MaxBuffs)
					return;

				var modName = tag.GetString("mod");
				int type = modName == "Terraria" ? tag.GetInt("id") : ModContent.TryFind(modName, tag.GetString("name"), out ModBuff buff) ? buff.Type : 0;
				if (type > 0) {
					player.buffType[buffCount] = type;
					player.buffTime[buffCount] = tag.GetInt("time");
					buffCount++;
				}
			}
			return;
		}

		//legacy code path
		//iterate the list in reverse, insert each buff at its index and push the buffs after it up a slot
		foreach (var tag in list.Reverse()) {
			if (!ModContent.TryFind(tag.GetString("mod"), tag.GetString("name"), out ModBuff buff))
				continue;

			int index = Math.Min(tag.GetByte("index"), buffCount);
			Array.Copy(player.buffType, index, player.buffType, index + 1, Player.MaxBuffs - index - 1);
			Array.Copy(player.buffTime, index, player.buffTime, index + 1, Player.MaxBuffs - index - 1);
			player.buffType[index] = buff.Type;
			player.buffTime[index] = tag.GetInt("time");
		}
	}

	internal static List<string> SaveInfoDisplays(Player player)
	{
		var hidden = new List<string>();
		for (int i = 0; i < InfoDisplayLoader.InfoDisplays.Count; i++) {
			if(!(InfoDisplayLoader.InfoDisplays[i] is VanillaInfoDisplay)) {
				if (player.hideInfo[i])
					hidden.Add(InfoDisplayLoader.InfoDisplays[i].FullName);
			}
		}
		return hidden;
	}

	internal static void LoadInfoDisplays(Player player, IList<string> hidden)
	{
		for (int i = 0; i < InfoDisplayLoader.InfoDisplays.Count; i++) {
			if (!(InfoDisplayLoader.InfoDisplays[i] is VanillaInfoDisplay)) {
				if (hidden.Contains(InfoDisplayLoader.InfoDisplays[i].FullName))
					player.hideInfo[i] = true;
			}
		}
	}

	internal static List<TagCompound> SaveBuilderToggles(Player player)
	{
		return BuilderToggleLoader.BuilderToggles
			.Where(x=> x is not VanillaBuilderToggle)
			.Select(x=> new TagCompound {
				["fullName"] = x.FullName,
				["currentState"] = player.builderAccStatus[x.Type] // Can't use x.CurrentState, that is LocalPlayer.
			}).ToList();
	}

	internal static void LoadBuilderToggles(Player player, IList<TagCompound> list)
	{
		foreach (var tag in list) {
			var fullname = tag.GetString("fullName");
			var entryIndex = BuilderToggleLoader.BuilderToggles.FindIndex(x => x.FullName == fullname);
			if (entryIndex != -1) {
				player.builderAccStatus[entryIndex] = tag.GetInt("currentState");
			}
		}

		// Could revert state to 0 if state is now invalid. This approach probably won't work since ModifyNumberOfStates probably relies on player inventory update.
		/*for (int i = 0; i < BuilderToggleLoader.BuilderToggles.Count; i++) {
			BuilderToggle builderToggle = BuilderToggleLoader.BuilderToggles[i];
			int numberOfStates = builderToggle.NumberOfStates;
			BuilderToggleLoader.ModifyNumberOfStates(builderToggle, ref numberOfStates);
			if (player.builderAccStatus[i] >= numberOfStates)
				player.builderAccStatus[i] = 0;
		}*/
	}

	internal static void LoadUsedMods(Player player, IList<string> usedMods)
	{
		player.usedMods = usedMods;
	}

	internal static List<string> SaveUsedMods(Player player)
	{
		return ModLoader.Mods.Select(m => m.Name).Except(new[] { "ModLoader" }).ToList();
	}

	internal static void LoadUsedModPack(Player player, string modpack)
	{
		player.modPack = string.IsNullOrEmpty(modpack) ? null : modpack; // tag.GetString returns "" even though null
	}

	internal static string SaveUsedModPack(Player player)
	{
		return Path.GetFileNameWithoutExtension(Core.ModOrganizer.ModPackActive);
	}

	internal static TagCompound SaveLoadouts(EquipmentLoadout[] equipLoadouts)
	{
		TagCompound loadouts = new();

		for (int i = 0; i < equipLoadouts.Length; i++) {
			loadouts[$"loadout{i}Armor"] = SaveInventory(equipLoadouts[i].Armor);
			loadouts[$"loadout{i}Dye"] = SaveInventory(equipLoadouts[i].Dye);
		}

		return loadouts;
	}

	internal static void LoadLoadouts(EquipmentLoadout[] loadouts, TagCompound loadoutTag)
	{
		for (int i = 0; i < loadouts.Length; i++) {
			LoadInventory(loadouts[i].Armor, loadoutTag.GetList<TagCompound>($"loadout{i}Armor"));
			LoadInventory(loadouts[i].Dye, loadoutTag.GetList<TagCompound>($"loadout{i}Dye"));
		}
	}

	//add to end of Terraria.IO.PlayerFileData.MoveToCloud
	internal static void MoveToCloud(string localPath, string cloudPath)
	{
		localPath = Path.ChangeExtension(localPath, ".tplr");
		cloudPath = Path.ChangeExtension(cloudPath, ".tplr");
		if (File.Exists(localPath)) {
			FileUtilities.MoveToCloud(localPath, cloudPath);
		}
	}
	//add to end of Terraria.IO.PlayerFileData.MoveToLocal
	//in Terraria.IO.PlayerFileData.MoveToLocal before iterating through map files add
	//  matchPattern = Regex.Escape(Main.CloudPlayerPath) + "/" + Regex.Escape(fileName) + "/.+\\.tmap";
	//  files.AddRange(SocialAPI.Cloud.GetFiles(matchPattern));
	internal static void MoveToLocal(string cloudPath, string localPath)
	{
		cloudPath = Path.ChangeExtension(cloudPath, ".tplr");
		localPath = Path.ChangeExtension(localPath, ".tplr");
		if (FileUtilities.Exists(cloudPath, true)) {
			FileUtilities.MoveToLocal(cloudPath, localPath);
		}
	}
	//add to Terraria.Player.GetFileData after moving vanilla .bak file
	internal static void LoadBackup(string path, bool cloudSave)
	{
		path = Path.ChangeExtension(path, ".tplr");
		if (FileUtilities.Exists(path + ".bak", cloudSave)) {
			FileUtilities.Move(path + ".bak", path, cloudSave, true);
		}
	}
	//in Terraria.Main.ErasePlayer between the two try catches add
	//  PlayerIO.ErasePlayer(Main.PlayerList[i].Path, Main.PlayerList[i].IsCloudSave);
	internal static void ErasePlayer(string path, bool cloudSave)
	{
		path = Path.ChangeExtension(path, ".tplr");
		try {
			FileUtilities.Delete(path, cloudSave);
			FileUtilities.Delete(path + ".bak", cloudSave);
		}
		catch {
			//just copying the Terraria code which also has an empty catch
		}
	}
}
