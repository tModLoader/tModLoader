using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Terraria.ID;
using Terraria.ModLoader.Config.UI;
using Terraria.ModLoader.Exceptions;
using Terraria.UI;

namespace Terraria.ModLoader.Config
{
	public static class ConfigManager
	{
		// These are THE active configs.
		internal static readonly IDictionary<Mod, List<ModConfig>> Configs = new Dictionary<Mod, List<ModConfig>>();
		// Configs should never violate reload required.
		// Menu save should force reload

		// This copy of Configs stores instances present during load. Its only use in detecting if a reload is needed.
		private static readonly IDictionary<Mod, List<ModConfig>> LoadTimeConfigs = new Dictionary<Mod, List<ModConfig>>();

		public static readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings
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
			//new ColorJsonConverter(),
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

			config.OnLoaded();
			config.OnChanged();

			// Maintain a backup of LoadTime Configs.
			List<ModConfig> configList2;
			if (!LoadTimeConfigs.TryGetValue(config.mod, out configList2))
				LoadTimeConfigs.Add(config.mod, configList2 = new List<ModConfig>());
			configList2.Add(config.Clone());
		}

		// This method for refreshing configs (ServerSide mostly) after events that could change configs: Multiplayer play.
		internal static void LoadAll()
		{
			foreach (var activeConfigs in ConfigManager.Configs)
			{
				foreach (var activeConfig in activeConfigs.Value)
				{
					Load(activeConfig);
				}
			}
		}

		internal static void OnChangedAll() {
			foreach (var activeConfigs in ConfigManager.Configs) {
				foreach (var activeConfig in activeConfigs.Value) {
					activeConfig.OnChanged();
				}
			}
		}

		internal static void Load(ModConfig config)
		{
			string filename = config.mod.Name + "_" + config.Name + ".json";
			string path = Path.Combine(ModConfigPath, filename);
			if (config.Mode == ConfigScope.ServerSide && Main.netMode == 1)
			{
				string netJson = ModNet.pendingConfigs.Single(x => x.modname == config.mod.Name && x.configname == config.Name).json;
				JsonConvert.PopulateObject(netJson, config, serializerSettingsCompact);
				return;
			}
			string json = File.Exists(path) ? File.ReadAllText(path) : "{}";
			JsonConvert.PopulateObject(json, config, serializerSettings);
		}

		internal static void Reset(ModConfig pendingConfig)
		{
			string json = "{}";
			JsonConvert.PopulateObject(json, pendingConfig, serializerSettings);
		}

		internal static void Save(ModConfig config)
		{
			Directory.CreateDirectory(ModConfigPath);
			string filename = config.mod.Name + "_" + config.Name + ".json";
			string path = Path.Combine(ModConfigPath, filename);
			string json = JsonConvert.SerializeObject(config, serializerSettings);
			File.WriteAllText(path, json);
		}

		internal static void Unload()
		{
			Configs.Clear();
			LoadTimeConfigs.Clear();

			Interface.modConfig.Unload();
			Interface.modConfigList.Unload();
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

		internal static bool AnyModNeedsReload()
		{
			foreach (var mod in ModLoader.Mods)
			{
				if (ModNeedsReload(mod))
				{
					return true;
				}
			}
			return false;
		}

		internal static bool ModNeedsReload(Mod mod)
		{
			if (Configs.ContainsKey(mod))
			{
				var configs = Configs[mod];
				var loadTimeConfigs = LoadTimeConfigs[mod];
				for (int i = 0; i < configs.Count; i++)
				{
					if (loadTimeConfigs[i].NeedsReload(configs[i]))
					{
						return true;
					}
				}
			}
			return false;
		}

		// GetConfig...returns the config instance

		internal static ModConfig GetConfig(ModNet.NetConfig netConfig) => ConfigManager.GetConfig(ModLoader.GetMod(netConfig.modname), netConfig.configname);
		internal static ModConfig GetConfig(Mod mod, string config)
		{
			List<ModConfig> configs;
			if (Configs.TryGetValue(mod, out configs))
			{
				return configs.Single(x => x.Name == config);
			}
			throw new MissingResourceException("Missing config named " + config + " in mod " + mod.Name);
		}

		internal static ModConfig GetLoadTimeConfig(Mod mod, string config) {
			List<ModConfig> configs;
			if (LoadTimeConfigs.TryGetValue(mod, out configs)) {
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
					ModConfig activeConfig = GetConfig(ModLoader.GetMod(modname), configname);
					JsonConvert.PopulateObject(json, activeConfig, serializerSettingsCompact);
					activeConfig.OnChanged();

					Main.NewText($"Shared config changed: Message: {message}, Mod: {modname}, Config: {configname}");
					if (Main.InGameUI.CurrentState == Interface.modConfig)
					{
						Main.InGameUI.SetState(Interface.modConfig);
						Interface.modConfig.SetMessage("Server response: " + message, Color.Green);
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
				ModConfig loadTimeConfig = GetLoadTimeConfig(ModLoader.GetMod(modname), configname);
				ModConfig pendingConfig = config.Clone();
				JsonConvert.PopulateObject(json, pendingConfig, serializerSettingsCompact);
				bool success = true;
				string message = "Accepted";
				if (loadTimeConfig.NeedsReload(pendingConfig))
				{
					success = false;
					message = "Can't save because changes would require a reload.";
				}
				if (!config.AcceptClientChanges(pendingConfig, whoAmI, ref message))
				{
					success = false;
				}
				if (success)
				{
					// Apply to Servers Config
					JsonConvert.PopulateObject(json, config, ConfigManager.serializerSettingsCompact);
					config.OnChanged();
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

		public static IEnumerable<PropertyFieldWrapper> GetFieldsAndProperties(object item)
		{
			PropertyInfo[] properties = item.GetType().GetProperties(
				//BindingFlags.DeclaredOnly |
				BindingFlags.Public |
				BindingFlags.Instance);

			FieldInfo[] fields = item.GetType().GetFields(
				//BindingFlags.DeclaredOnly |
				BindingFlags.Public |
				BindingFlags.Instance);

			return fields.Select(x => new PropertyFieldWrapper(x)).Concat(properties.Select(x => new PropertyFieldWrapper(x)));
		}

		public static object AlternateCreateInstance(Type type)
		{
			if (type == typeof(string))
				return "";
			return Activator.CreateInstance(type);
		}

		// Gets an Attribute from a property or field. Attribute defined on Member has highest priority, 
		// followed by the containing data structure, followed by attribute defined on the Class. 
		public static T GetCustomAttribute<T>(PropertyFieldWrapper memberInfo, object item, object array) where T : Attribute {
			// Class
			T attribute = (T)Attribute.GetCustomAttribute(memberInfo.Type, typeof(T), true);
			if (array != null)
			{
				// item null?
			//	attribute = (T)Attribute.GetCustomAttribute(item.GetType(), typeof(T), true) ?? attribute; // TODO: is this wrong?
			}
			// Member
			attribute = (T)Attribute.GetCustomAttribute(memberInfo.MemberInfo, typeof(T)) ?? attribute;
			return attribute;
		}

		public static Tuple<UIElement, UIElement> WrapIt(UIElement parent, ref int top, PropertyFieldWrapper memberInfo, object item, int order, object array = null, Type arrayType = null, int index = -1) 
		{
			// public api for modders.
			return UIModConfig.WrapIt(parent, ref top, memberInfo, item, order, array, arrayType, index);
		}

		public static void SetPendingChanges(bool changes = true) {
			// public api for modders.
			Interface.modConfig.SetPendingChanges(changes);
		}
	}

	//public class ColorJsonConverter : JsonConverter
	//{
	//	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	//	{
	//		Color c = (Color)value;
	//		writer.WriteValue($"{c.R}, {c.G}, {c.B}, {c.A}");
	//	}

	//	public override bool CanConvert(Type objectType)
	//	{
	//		return objectType == typeof(Color);
	//	}

	//	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	//	{
	//		var colorStr = ((string)reader.Value).Split(',');
	//		byte r = 255, g = 255, b = 255, a = 255;
	//		if (colorStr.Length >= 1) r = byte.Parse(colorStr[0]);
	//		if (colorStr.Length >= 2) g = byte.Parse(colorStr[1]);
	//		if (colorStr.Length >= 3) b = byte.Parse(colorStr[2]);
	//		if (colorStr.Length >= 4) a = byte.Parse(colorStr[3]);
	//		return new Color(r, g, b, a);
	//	}
	//}
}
