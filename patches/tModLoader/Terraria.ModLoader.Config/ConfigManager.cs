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
			ContractResolver = new ReferenceDefaultsPreservingResolver()
		};

		internal static readonly JsonSerializerSettings serializerSettingsCompact = new JsonSerializerSettings
		{
			Formatting = Formatting.None,
			DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
			ObjectCreationHandling = ObjectCreationHandling.Replace,
			NullValueHandling = NullValueHandling.Ignore,
			Converters = converters,
			ContractResolver = serializerSettings.ContractResolver
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

			FieldInfo instance = config.GetType().GetField("Instance", BindingFlags.Static | BindingFlags.Public);
			if (instance != null) {
				instance.SetValue(null, config);
			}
			config.OnLoaded();
			config.OnChanged();

			// Maintain a backup of LoadTime Configs.
			List<ModConfig> configList2;
			if (!LoadTimeConfigs.TryGetValue(config.mod, out configList2))
				LoadTimeConfigs.Add(config.mod, configList2 = new List<ModConfig>());
			configList2.Add(GeneratePopulatedClone(config));
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
			serializerSettings.ContractResolver = new ReferenceDefaultsPreservingResolver();
			serializerSettingsCompact.ContractResolver = serializerSettings.ContractResolver;

			Configs.SelectMany(configList => configList.Value).ToList().ForEach(config => {
				FieldInfo instance = config.GetType().GetField("Instance", BindingFlags.Static | BindingFlags.Public);
				if (instance != null) {
					instance.SetValue(null, null);
				}
			});
			Configs.Clear();
			LoadTimeConfigs.Clear();

			Interface.modConfig.Unload();
			Interface.modConfigList.Unload();
		}

		internal static bool AnyModNeedsReload() => ModLoader.Mods.Any(ModNeedsReload);

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
				ModConfig pendingConfig = GeneratePopulatedClone(config);
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

		public static ModConfig GeneratePopulatedClone(ModConfig original) {
			string json = JsonConvert.SerializeObject(original, ConfigManager.serializerSettings);
			ModConfig properClone = original.Clone();
			JsonConvert.PopulateObject(json, properClone, ConfigManager.serializerSettings);
			return properClone;
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
			// TODO: allow for inheriting from parent's parent? (could get attribute from parent ConfigElement)
		}

		public static T GetCustomAttribute<T>(PropertyFieldWrapper memberInfo, Type type) where T : Attribute {
			// Class
			T attribute = (T)Attribute.GetCustomAttribute(memberInfo.Type, typeof(T), true);

			attribute = (T)Attribute.GetCustomAttribute(type, typeof(T), true) ?? attribute;

			// Member
			attribute = (T)Attribute.GetCustomAttribute(memberInfo.MemberInfo, typeof(T)) ?? attribute;
			return attribute;
		}

		public static Tuple<UIElement, UIElement> WrapIt(UIElement parent, ref int top, PropertyFieldWrapper memberInfo, object item, int order, object list = null, Type arrayType = null, int index = -1) 
		{
			// public api for modders.
			return UIModConfig.WrapIt(parent, ref top, memberInfo, item, order, list, arrayType, index);
		}

		public static void SetPendingChanges(bool changes = true) {
			// public api for modders.
			Interface.modConfig.SetPendingChanges(changes);
		}

		// TODO: better home?
		public static bool ObjectEquals(object a, object b) {
			if (ReferenceEquals(a, b)) return true;
			if (a == null || b == null) return false;
			if (a is IEnumerable && b is IEnumerable && !(a is string) && !(b is string))
				return EnumerableEquals((IEnumerable)a, (IEnumerable)b);
			return a.Equals(b);
		}

		public static bool EnumerableEquals(IEnumerable a, IEnumerable b) {
			IEnumerator enumeratorA = a.GetEnumerator();
			IEnumerator enumeratorB = b.GetEnumerator();
			bool hasNextA = enumeratorA.MoveNext();
			bool hasNextB = enumeratorB.MoveNext();
			while (hasNextA && hasNextB) {
				if (!ObjectEquals(enumeratorA.Current, enumeratorB.Current)) return false;
				hasNextA = enumeratorA.MoveNext();
				hasNextB = enumeratorB.MoveNext();
			}
			return !hasNextA && !hasNextB;
		}
	}

	/// <summary>
	/// Custom ContractResolver for facilitating refernce type defaults. 
	/// The ShouldSerialize code enables unchanged-by-user reference type defaults to properly not serialize. 
	/// The ValueProvider code helps during deserialization to not 
	/// </summary>
	class ReferenceDefaultsPreservingResolver : DefaultContractResolver
	{
		// This approach largely based on https://stackoverflow.com/a/52684798. 
		public abstract class ValueProviderDecorator : IValueProvider
		{
			readonly IValueProvider baseProvider;

			public ValueProviderDecorator(IValueProvider baseProvider) {
				if (baseProvider == null)
					throw new ArgumentNullException();
				this.baseProvider = baseProvider;
			}

			public virtual object GetValue(object target) { return baseProvider.GetValue(target); }

			public virtual void SetValue(object target, object value) { baseProvider.SetValue(target, value); }
		}
		class NullToDefaultValueProvider : ValueProviderDecorator
		{
			//readonly object defaultValue;
			readonly Func<object> defaultValueGenerator;

			//public NullToDefaultValueProvider(IValueProvider baseProvider, object defaultValue) : base(baseProvider) {
			//	this.defaultValue = defaultValue;
			//}

			public NullToDefaultValueProvider(IValueProvider baseProvider, Func<object> defaultValueGenerator) : base(baseProvider) {
				this.defaultValueGenerator = defaultValueGenerator;
			}

			public override void SetValue(object target, object value) {
				base.SetValue(target, value ?? defaultValueGenerator.Invoke());
				//base.SetValue(target, value ?? defaultValue);
			}
		}

		protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization) {
			IList<JsonProperty> props = base.CreateProperties(type, memberSerialization);
			if (type.IsClass) {
				ConstructorInfo ctor = type.GetConstructor(Type.EmptyTypes);
				if (ctor != null) {
					object referenceInstance = ctor.Invoke(null);
					foreach (JsonProperty prop in props.Where(p => p.Readable)) {
						if (!prop.PropertyType.IsValueType) {
							var a = type.GetMember(prop.PropertyName); 
							if (prop.Writable) {
								if (prop.PropertyType.GetConstructor(Type.EmptyTypes) != null) {
									// defaultValueCreator will create new instance, then get the value from a field in that object. Prevents deserialized nulls from sharing with other instances.
									Func<object> defaultValueCreator = () => prop.ValueProvider.GetValue(ctor.Invoke(null));
									prop.ValueProvider = new NullToDefaultValueProvider(prop.ValueProvider, defaultValueCreator);
								}
								else if (prop.PropertyType.IsArray) {
									Func<object> defaultValueCreator = () => (prop.ValueProvider.GetValue(referenceInstance) as Array).Clone();
									prop.ValueProvider = new NullToDefaultValueProvider(prop.ValueProvider, defaultValueCreator);
								}
							}
							if (prop.ShouldSerialize == null)
								prop.ShouldSerialize = instance =>
								{
									object val = prop.ValueProvider.GetValue(instance);
									object refVal = prop.ValueProvider.GetValue(referenceInstance);
									return !ConfigManager.ObjectEquals(val, refVal);
								};
						}
					}
				}
			}
			return props;
		}
	}
}
