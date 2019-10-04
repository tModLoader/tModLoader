using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader.Default;
using Terraria.ModLoader.Engine;
using Terraria.ModLoader.Exceptions;
using Terraria.Social;
using Terraria.Utilities;

namespace Terraria.ModLoader.IO
{
	internal static class PlayerIO
	{
		internal static void WriteVanillaHairDye(short hairDye, BinaryWriter writer) {
			writer.Write((byte)(hairDye > EffectsTracker.vanillaHairShaderCount ? 0 : hairDye));
		}

		//make Terraria.Player.ENCRYPTION_KEY internal
		//add to end of Terraria.Player.SavePlayer
		internal static void Save(Player player, string path, bool isCloudSave) {
			path = Path.ChangeExtension(path, ".tplr");
			if (FileUtilities.Exists(path, isCloudSave))
				FileUtilities.Copy(path, path + ".bak", isCloudSave);

			var tag = new TagCompound {
				["armor"] = SaveInventory(player.armor),
				["dye"] = SaveInventory(player.dye),
				["inventory"] = SaveInventory(player.inventory),
				["miscEquips"] = SaveInventory(player.miscEquips),
				["miscDyes"] = SaveInventory(player.miscDyes),
				["bank"] = SaveInventory(player.bank.item),
				["bank2"] = SaveInventory(player.bank2.item),
				["bank3"] = SaveInventory(player.bank3.item),
				["hairDye"] = SaveHairDye(player.hairDye),
				["modData"] = SaveModData(player),
				["modBuffs"] = SaveModBuffs(player),
				["usedMods"] = SaveUsedMods(player)
			};

			using (Stream stream = isCloudSave ? (Stream)new MemoryStream() : (Stream)new FileStream(path, FileMode.Create)) {
				TagIO.ToStream(tag, stream);
				if (isCloudSave && SocialAPI.Cloud != null)
					SocialAPI.Cloud.Write(path, ((MemoryStream)stream).ToArray());
			}
		}
		//add near end of Terraria.Player.LoadPlayer before accessory check
		internal static void Load(Player player, string path, bool isCloudSave) {
			path = Path.ChangeExtension(path, ".tplr");
			if (!FileUtilities.Exists(path, isCloudSave))
				return;

			var buf = FileUtilities.ReadAllBytes(path, isCloudSave);
			if (buf[0] != 0x1F || buf[1] != 0x8B) {
				LoadLegacy(player, buf);
				return;
			}

			var tag = TagIO.FromStream(new MemoryStream(buf));
			LoadInventory(player.armor, tag.GetList<TagCompound>("armor"));
			LoadInventory(player.dye, tag.GetList<TagCompound>("dye"));
			LoadInventory(player.inventory, tag.GetList<TagCompound>("inventory"));
			LoadInventory(player.miscEquips, tag.GetList<TagCompound>("miscEquips"));
			LoadInventory(player.miscDyes, tag.GetList<TagCompound>("miscDyes"));
			LoadInventory(player.bank.item, tag.GetList<TagCompound>("bank"));
			LoadInventory(player.bank2.item, tag.GetList<TagCompound>("bank2"));
			LoadInventory(player.bank3.item, tag.GetList<TagCompound>("bank3"));
			LoadHairDye(player, tag.GetString("hairDye"));
			LoadModData(player, tag.GetList<TagCompound>("modData"));
			LoadModBuffs(player, tag.GetList<TagCompound>("modBuffs"));
			LoadUsedMods(player, tag.GetList<string>("usedMods"));
		}

		public static List<TagCompound> SaveInventory(Item[] inv) {
			var list = new List<TagCompound>();
			for (int k = 0; k < inv.Length; k++) {
				if (ItemLoader.NeedsModSaving(inv[k])) {
					var tag = ItemIO.Save(inv[k]);
					tag.Set("slot", (short)k);
					list.Add(tag);
				}
			}
			return list.Count > 0 ? list : null;
		}

		public static void LoadInventory(Item[] inv, IList<TagCompound> list) {
			foreach (var tag in list)
				inv[tag.GetShort("slot")] = ItemIO.Load(tag);
		}

		public static string SaveHairDye(short hairDye) {
			if (hairDye <= EffectsTracker.vanillaHairShaderCount)
				return "";

			int itemId = GameShaders.Hair._reverseShaderLookupDictionary[hairDye];
			var modItem = ItemLoader.GetItem(itemId);
			return modItem.mod.Name + '/' + modItem.Name;
		}

		public static void LoadHairDye(Player player, string hairDyeItemName) {
			if (hairDyeItemName == "")
				return;

			// no mystery hair dye at this stage
			ModContent.SplitName(hairDyeItemName, out string modName, out string itemName);
			var modItem = ModLoader.GetMod(modName)?.GetItem(itemName);
			if (modItem != null)
				player.hairDye = (byte)GameShaders.Hair.GetShaderIdFromItemId(modItem.item.type);
		}

		internal static List<TagCompound> SaveModData(Player player) {
			var list = new List<TagCompound>();
			foreach (var modPlayer in player.modPlayers) {
				var data = modPlayer.Save();
				if (data == null)
					continue;

				list.Add(new TagCompound {
					["mod"] = modPlayer.mod.Name,
					["name"] = modPlayer.Name,
					["data"] = data
				});
			}
			return list;
		}

		internal static void LoadModData(Player player, IList<TagCompound> list) {
			foreach (var tag in list) {
				var mod = ModLoader.GetMod(tag.GetString("mod"));
				var modPlayer = mod == null ? null : player.GetModPlayer(mod, tag.GetString("name"));
				if (modPlayer != null) {
					try {
						if (tag.ContainsKey("legacyData"))
							modPlayer.LoadLegacy(new BinaryReader(new MemoryStream(tag.GetByteArray("legacyData"))));
						else
							modPlayer.Load(tag.GetCompound("data"));
					}
					catch (Exception e) {
						throw new CustomModDataException(mod,
							"Error in reading custom player data for " + mod.Name, e);
					}
				}
				else {
					player.GetModPlayer<MysteryPlayer>().data.Add(tag);
				}
			}
		}

		internal static List<TagCompound> SaveModBuffs(Player player) {
			var list = new List<TagCompound>();
			for (int k = 0; k < Player.MaxBuffs; k++) {
				int buff = player.buffType[k];
				if (buff == 0 || Main.buffNoSave[buff])
					continue;

				if (BuffLoader.IsModBuff(buff)) {
					var modBuff = BuffLoader.GetBuff(buff);
					list.Add(new TagCompound {
						["mod"] = modBuff.mod.Name,
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

		internal static void LoadModBuffs(Player player, IList<TagCompound> list) {
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
					int type = modName == "Terraria" ? tag.GetInt("id") : ModLoader.GetMod(modName)?.BuffType(tag.GetString("name")) ?? 0;
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
				var mod = ModLoader.GetMod(tag.GetString("mod"));
				int type = mod?.BuffType(tag.GetString("name")) ?? 0;
				if (type == 0)
					continue;

				int index = Math.Min(tag.GetByte("index"), buffCount);
				Array.Copy(player.buffType, index, player.buffType, index + 1, Player.MaxBuffs - index - 1);
				Array.Copy(player.buffTime, index, player.buffTime, index + 1, Player.MaxBuffs - index - 1);
				player.buffType[index] = type;
				player.buffTime[index] = tag.GetInt("time");
			}
		}

		private static void LoadLegacy(Player player, byte[] buffer) {
			const int numFlagBytes = 2;
			RijndaelManaged rijndaelManaged = new RijndaelManaged();
			rijndaelManaged.Padding = PaddingMode.None;
			using (MemoryStream stream = new MemoryStream(buffer)) {
				using (CryptoStream cryptoStream = new CryptoStream(stream, rijndaelManaged.CreateDecryptor(Player.ENCRYPTION_KEY, Player.ENCRYPTION_KEY), CryptoStreamMode.Read)) {
					using (BinaryReader reader = new BinaryReader(cryptoStream)) {
						byte limit = reader.ReadByte();
						if (limit == 0) {
							return;
						}
						byte[] flags = reader.ReadBytes(limit);
						if (flags.Length < numFlagBytes) {
							Array.Resize(ref flags, numFlagBytes);
						}
						LoadLegacyModPlayer(player, flags, reader);
					}
				}
			}
		}

		private static void LoadLegacyModPlayer(Player player, byte[] flags, BinaryReader reader) {
			if ((flags[0] & 1) == 1) {
				ItemIO.LoadLegacyInventory(player.armor, reader);
			}
			if ((flags[0] & 2) == 2) {
				ItemIO.LoadLegacyInventory(player.dye, reader);
			}
			if ((flags[0] & 4) == 4) {
				ItemIO.LoadLegacyInventory(player.inventory, reader, true, true);
			}
			if ((flags[0] & 8) == 8) {
				ItemIO.LoadLegacyInventory(player.miscEquips, reader);
			}
			if ((flags[0] & 16) == 16) {
				ItemIO.LoadLegacyInventory(player.miscDyes, reader);
			}
			if ((flags[0] & 32) == 32) {
				ItemIO.LoadLegacyInventory(player.bank.item, reader, true);
			}
			if ((flags[0] & 64) == 64) {
				ItemIO.LoadLegacyInventory(player.bank2.item, reader, true);
			}
			if ((flags[0] & 128) == 128) {
				LoadLegacyModData(player, reader);
			}
			if ((flags[1] & 1) == 1) {
				LoadLegacyModBuffs(player, reader);
			}
		}

		private static void LoadLegacyModData(Player player, BinaryReader reader) {
			int count = reader.ReadUInt16();
			for (int k = 0; k < count; k++) {
				string modName = reader.ReadString();
				string name = reader.ReadString();
				byte[] data = reader.ReadBytes(reader.ReadUInt16());
				Mod mod = ModLoader.GetMod(modName);
				ModPlayer modPlayer = mod == null ? null : player.GetModPlayer(mod, name);
				if (modPlayer != null) {
					using (MemoryStream stream = new MemoryStream(data)) {
						using (BinaryReader customReader = new BinaryReader(stream)) {
							try {
								modPlayer.LoadLegacy(customReader);
							}
							catch (Exception e) {
								throw new CustomModDataException(mod,
									"Error in reading custom player data for " + mod.Name, e);
							}
						}
					}
				}
				else {
					var tag = new TagCompound {
						["mod"] = modName,
						["name"] = name,
						["legacyData"] = data
					};
					player.GetModPlayer<MysteryPlayer>().data.Add(tag);
				}
			}
		}

		private static void LoadLegacyModBuffs(Player player, BinaryReader reader) {
			int num = reader.ReadByte();
			int minusIndex = 0;
			for (int k = 0; k < num; k++) {
				int index = reader.ReadByte() - minusIndex;
				string modName = reader.ReadString();
				string name = reader.ReadString();
				int time = reader.ReadInt32();
				Mod mod = ModLoader.GetMod(modName);
				int type = mod == null ? 0 : mod.BuffType(name);
				if (type > 0) {
					for (int j = Player.MaxBuffs - 1; j > index; j--) {
						player.buffType[j] = player.buffType[j - 1];
						player.buffTime[j] = player.buffTime[j - 1];
					}
					player.buffType[index] = type;
					player.buffTime[index] = time;
				}
				else {
					minusIndex++;
				}
			}
			for (int k = 1; k < Player.MaxBuffs; k++) {
				if (player.buffType[k] > 0) {
					int j = k - 1;
					while (player.buffType[j] == 0) {
						player.buffType[j] = player.buffType[j + 1];
						player.buffTime[j] = player.buffTime[j + 1];
						player.buffType[j + 1] = 0;
						player.buffTime[j + 1] = 0;
						j--;
					}
				}
			}
		}

		internal static void LoadUsedMods(Player player, IList<string> usedMods) {
			player.usedMods = usedMods;
		}

		internal static List<string> SaveUsedMods(Player player) {
			return ModLoader.Mods.Select(m => m.Name).Except(new[] { "ModLoader" }).ToList();
		}

		//add to end of Terraria.IO.PlayerFileData.MoveToCloud
		internal static void MoveToCloud(string localPath, string cloudPath) {
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
		internal static void MoveToLocal(string cloudPath, string localPath) {
			cloudPath = Path.ChangeExtension(cloudPath, ".tplr");
			localPath = Path.ChangeExtension(localPath, ".tplr");
			if (FileUtilities.Exists(cloudPath, true)) {
				FileUtilities.MoveToLocal(cloudPath, localPath);
			}
		}
		//add to Terraria.Player.GetFileData after moving vanilla .bak file
		internal static void LoadBackup(string path, bool cloudSave) {
			path = Path.ChangeExtension(path, ".tplr");
			if (FileUtilities.Exists(path + ".bak", cloudSave)) {
				FileUtilities.Move(path + ".bak", path, cloudSave, true);
			}
		}
		//in Terraria.Main.ErasePlayer between the two try catches add
		//  PlayerIO.ErasePlayer(Main.PlayerList[i].Path, Main.PlayerList[i].IsCloudSave);
		internal static void ErasePlayer(string path, bool cloudSave) {
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
}
