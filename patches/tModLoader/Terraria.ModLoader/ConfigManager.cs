using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System.IO;
using Terraria.ModLoader.IO;
using Terraria.ModLoader.Exceptions;
using Terraria.ID;
using System.Reflection;

namespace Terraria.ModLoader
{
	public static class ConfigManager
	{
		// These are THE configs.
		internal static readonly IDictionary<Mod, List<ModConfig>> Configs = new Dictionary<Mod, List<ModConfig>>();
		// Configs should never violate reload required.
		// Menu save should force reload

		// This copy of Configs stores instances present during load. Its only use in detecting if a reload is needed.
		private static readonly IDictionary<Mod, List<ModConfig>> LoadTimeConfigs = new Dictionary<Mod, List<ModConfig>>();

		internal static readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings
		{
			Formatting = Formatting.Indented,
			DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
			ObjectCreationHandling = ObjectCreationHandling.Replace,
			NullValueHandling = NullValueHandling.Ignore,
			Converters = converters,
		};

		internal static readonly JsonSerializerSettings serializerSettingsCompact = new JsonSerializerSettings
		{
			Formatting = Formatting.None,
			DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
			ObjectCreationHandling = ObjectCreationHandling.Replace,
			NullValueHandling = NullValueHandling.Ignore,
			Converters = converters,
		};

		private static readonly IList<JsonConverter> converters = new List<JsonConverter>() {
			new Newtonsoft.Json.Converters.VersionConverter(),
			new ColorJsonConverter(),
		};

		public static readonly string ModConfigPath = Path.Combine(Main.SavePath, "Mod Configs");
		public static readonly string ServerModConfigPath = Path.Combine(Main.SavePath, "Mod Configs", "Server");

		internal static void Add(ModConfig config)
		{
			ConfigManager.Load(config);

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
			if (config.Mode == MultiplayerSyncMode.ServerDictates && ModLoader.PostLoad == ModNet.NetReload)
			{
				//path = Path.Combine(ServerModConfigPath, filename);
				//if (!File.Exists(path))
				//{
				//	throw new Exception("Somehow server config is missing.");
				//}
				string netJson = ModNet.pendingConfigs.Single(x => x.modname == config.mod.Name && x.configname == config.Name).json;
				JsonConvert.PopulateObject(netJson, config, serializerSettingsCompact);
				return;
			}
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

		internal static void Reset(ModConfig config)
		{
			string json = "{}";
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

		internal static ModConfig GetConfig(Mod mod, string config)
		{
			List<ModConfig> configs;
			if (Configs.TryGetValue(mod, out configs))
			{
				return configs.Single(x => x.Name == config);
			}
			throw new MissingResourceException("Missing config named " + config + " in mod " + mod.Name);
		}

		internal static void HandleInGameChangeConfigPacket(BinaryReader reader, int whoAmI)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				bool success = reader.ReadBoolean();
				string message = reader.ReadString();
				if (success)
				{
					string modname = reader.ReadString();
					string configname = reader.ReadString();
					string json = reader.ReadString();
					ModConfig config = GetConfig(ModLoader.GetMod(modname), configname);
					JsonConvert.PopulateObject(json, config, serializerSettingsCompact);
					config.PostSave();

					Main.NewText($"Shared config changed: Message: {message}, Mod: {modname}, Config: {configname}");
					if (Main.InGameUI.CurrentState == Interface.modConfig)
					{
						Main.InGameUI.SetState(Interface.modConfig);
					}
				}
				else
				{
					// rejection only sent back to requester.
					// Update UI with message

					Main.NewText("Changes Rejected: " + message);
					if (Main.InGameUI.CurrentState == Interface.modConfig)
					{
						Interface.modConfig.SetMessage("Server rejected changes: " + message, Color.Red);
						//Main.InGameUI.SetState(Interface.modConfig);
					}

				}
			}
			else
			{
				// no bool in request.
				string modname = reader.ReadString();
				string configname = reader.ReadString();
				string json = reader.ReadString();
				ModConfig config = GetConfig(ModLoader.GetMod(modname), configname);
				ModConfig pending = config.Clone();
				JsonConvert.PopulateObject(json, pending, serializerSettingsCompact);
				bool success = true;
				string message = "Accepted";
				if (pending.NeedsReload(config))
				{
					success = false;
					message = "Can't save because changes would require a reload.";
				}
				if (!pending.AcceptClientChanges(config, whoAmI, ref message))
				{
					success = false;
				}
				if (success)
				{
					// Apply to Servers Config
					JsonConvert.PopulateObject(json, config, ConfigManager.serializerSettingsCompact);
					config.PostSave();
					// Send new config to all clients
					var p = new ModPacket(MessageID.InGameChangeConfig);
					p.Write(true);
					p.Write(message);
					p.Write(modname);
					p.Write(configname);
					p.Write(json);
					p.Send();
				}
				else
				{
					// Send rejections message back to client who requested change
					var p = new ModPacket(MessageID.InGameChangeConfig);
					p.Write(false);
					p.Write(message);
					p.Send(whoAmI);
				}

			}
			return;
		}

		// ReloadPrep?
		// 

		// Save
		
		public static IEnumerable<UI.PropertyFieldWrapper> GetFieldsAndProperties(object item)
		{
			PropertyInfo[] properties = item.GetType().GetProperties(
				//BindingFlags.DeclaredOnly |
				BindingFlags.Public |
				BindingFlags.Instance);

			FieldInfo[] fields = item.GetType().GetFields(
				//BindingFlags.DeclaredOnly |
				BindingFlags.Public |
				BindingFlags.Instance);

			return fields.Select(x => new UI.PropertyFieldWrapper(x)).Concat(properties.Select(x => new UI.PropertyFieldWrapper(x)));
		}
	}

	public class ColorJsonConverter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			Color c = (Color)value;
			writer.WriteValue($"{c.R}, {c.G}, {c.B}, {c.A}");
		}

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(Color);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var colorStr = ((string)reader.Value).Split(',');
			byte r = 255, g = 255, b = 255, a = 255;
			if (colorStr.Length >= 1) r = byte.Parse(colorStr[0]);
			if (colorStr.Length >= 2) g = byte.Parse(colorStr[1]);
			if (colorStr.Length >= 3) b = byte.Parse(colorStr[2]);
			if (colorStr.Length >= 4) a = byte.Parse(colorStr[3]);
			return new Color(r, g, b, a);
		}
	}
}
