using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System.IO;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader
{
	public static class ConfigManager
	{
		// These are THE configs.
		internal static readonly IDictionary<Mod, List<ModConfig>> Configs = new Dictionary<Mod, List<ModConfig>>();

		// This copy of Configs stores instances present during load. Its only use in detecting if a reload is needed.
		private static readonly IDictionary<Mod, List<ModConfig>> LoadTimeConfigs = new Dictionary<Mod, List<ModConfig>>();

		internal static readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings { Formatting = Formatting.Indented, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, ObjectCreationHandling = ObjectCreationHandling.Replace };

		public static readonly string ModConfigPath = Path.Combine(Main.SavePath, "Mod Configs");

		internal static void Add(ModConfig config)
		{
			string filename = config.mod.Name + "_" + config.Name + ".json";
			string path = Path.Combine(ModConfigPath, filename);
			string json = "{}";
			if (File.Exists(path))
			{
				using (StreamReader r = new StreamReader(path))
				{
					json = r.ReadToEnd();
				}
			}
			// use PopulateObject instead of Deserialize throughout so reference stays same
			JsonConvert.PopulateObject(json, config, serializerSettings);

			List<ModConfig> configList;
			if (!Configs.TryGetValue(config.mod, out configList))
				Configs.Add(config.mod, configList = new List<ModConfig>());
			configList.Add(config);

			config.PostAutoLoad();

			// Maintain a backup of LoadTime Configs.
			List<ModConfig> configList2;
			if (!LoadTimeConfigs.TryGetValue(config.mod, out configList2))
				LoadTimeConfigs.Add(config.mod, configList2 = new List<ModConfig>());
			configList2.Add(config.Clone());
		}
		
		internal static void Load(ModConfig config)
		{
			string filename = config.mod.Name + "_" + config.Name + ".json";
			string path = Path.Combine(ModConfigPath, filename);
			string json = "{}";
			if (File.Exists(path))
			{
				using (StreamReader r = new StreamReader(path))
				{
					json = r.ReadToEnd();
				}
			}
			JsonConvert.PopulateObject(json, config, serializerSettings);
		}

		internal static void Save(ModConfig config)
		{
			Directory.CreateDirectory(ModConfigPath);
			string filename = config.mod.Name + "_" + config.Name + ".json";
			string path = Path.Combine(ModConfigPath, filename);
			string json = JsonConvert.SerializeObject(config, serializerSettings);
			File.WriteAllText(path, json);
			config.PostSave();
		}

		internal static void Unload()
		{
			Configs.Clear();
			LoadTimeConfigs.Clear();
		}

		// pending changes are stored (in a variable? dictionary?)?? when synced from server.
		// Save personal?
		// replace values or config instance?
		// if needs reload, reload
		// hmm, mods.enabled persists after joining, so maybe server config jsons can persist for now until we redo that.
		//internal static bool NeedsReload()
		//{
		//	foreach (var entry in Configs.Keys)
		//	{
		//		if (ModNeedsReload(entry))
		//		{
		//			return true;
		//		}
		//	}
		//	return false;
		//}

		internal static bool ModNeedsReload(Mod mod)
		{
			if (Configs.ContainsKey(mod))
			{
				var configs = Configs[mod];
				var loadTimeConfigs = LoadTimeConfigs[mod];
				for (int i = 0; i < configs.Count; i++)
				{
					if (configs[i].NeedsReload(loadTimeConfigs[i]))
					{
						return true;
					}
				}
			}
			return false;
		}

		// GetConfig...returns the config instance

		// ReloadPrep?
		// 

		// Save
	}
}
