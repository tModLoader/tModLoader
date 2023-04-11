using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Config.UI;
using Terraria.ModLoader.Exceptions;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace Terraria.ModLoader.Config;

public static class ConfigManager
{
	// These are THE active configs.
	internal static readonly IDictionary<Mod, List<ModConfig>> Configs = new Dictionary<Mod, List<ModConfig>>();
	// Configs should never violate reload required.
	// Menu save should force reload

	// This copy of Configs stores instances present during load. Its only use in detecting if a reload is needed.
	private static readonly IDictionary<Mod, List<ModConfig>> loadTimeConfigs = new Dictionary<Mod, List<ModConfig>>();

	public static readonly JsonSerializerSettings serializerSettings = new() {
		Formatting = Formatting.Indented,
		DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
		ObjectCreationHandling = ObjectCreationHandling.Replace,
		NullValueHandling = NullValueHandling.Ignore,
		Converters = converters,
		ContractResolver = new ReferenceDefaultsPreservingResolver()
	};

	internal static readonly JsonSerializerSettings serializerSettingsCompact = new() {
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

	public static readonly string ModConfigPath = Path.Combine(Main.SavePath, "ModConfigs");
	public static readonly string ServerModConfigPath = Path.Combine(Main.SavePath, "ModConfigs", "Server");

	internal static void Add(ModConfig config)
	{
		Load(config);

		if (!Configs.TryGetValue(config.Mod, out List<ModConfig> configList))
			Configs.Add(config.Mod, configList = new List<ModConfig>());
		configList.Add(config);

		FieldInfo instance = config.GetType().GetField("Instance", BindingFlags.Static | BindingFlags.Public);
		if (instance != null) {
			instance.SetValue(null, config);
		}
		config.OnLoaded();
		config.OnChanged();

		// Maintain a backup of LoadTime Configs.
		if (!loadTimeConfigs.TryGetValue(config.Mod, out List<ModConfig> configList2))
			loadTimeConfigs.Add(config.Mod, configList2 = new List<ModConfig>());
		configList2.Add(GeneratePopulatedClone(config));
	}

	internal static void FinishSetup()
	{
		// Register localization for all fields and properties that should show
		foreach (var activeConfigs in ConfigManager.Configs) {
			foreach (var config in activeConfigs.Value) {
				string modConfigLabelKey = GetModConfigLabelKey(config, throwErrors: true);
				Language.GetOrRegister(modConfigLabelKey, () => Regex.Replace(config.Name, "([A-Z])", " $1").Trim());

				foreach (PropertyFieldWrapper variable in ConfigManager.GetFieldsAndProperties(config)) {
					if (variable.IsProperty && variable.Name == "Mode")
						continue;

					if (Attribute.IsDefined(variable.MemberInfo, typeof(JsonIgnoreAttribute)) && !Attribute.IsDefined(variable.MemberInfo, typeof(LabelAttribute))) // TODO, appropriately named attribute
						continue;

					try {
						// TODO: throw error after updating hjson somehow, so auto keys are populated?
						// TODO: Recursive? Find Label on members of Classes used?

						// Label and Tooltip will always exist. Header is optional, need to be used to exist.
						var header = GetLocalizedHeader(variable, config, throwErrors: true);
						if (header != null) {
							string identifier = header.IsIdentifier ? header.identifier : variable.Name;
							Language.GetOrRegister(header.key, () => $"{Regex.Replace(identifier, "([A-Z])", " $1").Trim()} Header");
						}

						string labelKey = GetLabelKey(variable, config, fallbackToClass: false, throwErrors: true);
						Language.GetOrRegister(labelKey, () => Regex.Replace(variable.Name, "([A-Z])", " $1").Trim());

						string tooltipKey = GetTooltipKey(variable, config, fallbackToClass: false, throwErrors: true);
						Language.GetOrRegister(tooltipKey, () => "");
					}
					catch (ValueNotTranslationKeyException e) {
						// TODO: Not necessarily the member, could be an attribute on the Type, but checking is messy
						e.additional = $"The member '{variable.Name}' found in the ModConfig '{config.Name}' caused this exception.";
						throw;
					}
				}
			}
		}
		// Current code gets all fields/properties in ModConfig class.
		// What about fields/properties of classes used in ModConfig?
	}

	// This method for refreshing configs (ServerSide mostly) after events that could change configs: Multiplayer play.
	internal static void LoadAll()
	{
		foreach (var activeConfigs in ConfigManager.Configs) {
			foreach (var activeConfig in activeConfigs.Value) {
				Load(activeConfig);
			}
		}
	}

	internal static void OnChangedAll()
	{
		foreach (var activeConfigs in ConfigManager.Configs) {
			foreach (var activeConfig in activeConfigs.Value) {
				activeConfig.OnChanged();
			}
		}
	}

	internal static void Load(ModConfig config)
	{
		string filename = config.Mod.Name + "_" + config.Name + ".json";
		string path = Path.Combine(ModConfigPath, filename);

		if (config.Mode == ConfigScope.ServerSide && ModNet.NetReloadActive) { // #999: Main.netMode isn't 1 at this point due to #770 fix.
			string netJson = ModNet.pendingConfigs.Single(x => x.modname == config.Mod.Name && x.configname == config.Name).json;
			JsonConvert.PopulateObject(netJson, config, serializerSettingsCompact);
			return;
		}

		bool jsonFileExists = File.Exists(path);
		string json = jsonFileExists ? File.ReadAllText(path) : "{}";

		try {
			JsonConvert.PopulateObject(json, config, serializerSettings);
		}
		catch (Exception e) when (jsonFileExists && (e is JsonReaderException || e is JsonSerializationException)) {
			Logging.tML.Warn($"Then config file {config.Name} from the mod {config.Mod.Name} located at {path} failed to load. The file was likely corrupted somehow, so the defaults will be loaded and the file deleted.");
			File.Delete(path);
			JsonConvert.PopulateObject("{}", config, serializerSettings);
		}
	}

	internal static void Reset(ModConfig pendingConfig)
	{
		string json = "{}";
		JsonConvert.PopulateObject(json, pendingConfig, serializerSettings);
	}

	internal static void Save(ModConfig config)
	{
		Directory.CreateDirectory(ModConfigPath);
		string filename = config.Mod.Name + "_" + config.Name + ".json";
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
		loadTimeConfigs.Clear();

		Interface.modConfig.Unload();
		Interface.modConfigList.Unload();
	}

	internal static bool AnyModNeedsReload() => ModLoader.Mods.Any(ModNeedsReload);

	internal static bool ModNeedsReload(Mod mod)
	{
		if (Configs.ContainsKey(mod)) {
			var configs = Configs[mod];
			var loadTimeConfigs = ConfigManager.loadTimeConfigs[mod];
			for (int i = 0; i < configs.Count; i++) {
				if (loadTimeConfigs[i].NeedsReload(configs[i])) {
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
		if (Configs.TryGetValue(mod, out List<ModConfig> configs)) {
			return configs.Single(x => x.Name == config);
		}
		throw new MissingResourceException("Missing config named " + config + " in mod " + mod.Name);
	}

	internal static ModConfig GetLoadTimeConfig(Mod mod, string config)
	{
		if (loadTimeConfigs.TryGetValue(mod, out List<ModConfig> configs)) {
			return configs.Single(x => x.Name == config);
		}
		throw new MissingResourceException("Missing config named " + config + " in mod " + mod.Name);
	}

	internal static void HandleInGameChangeConfigPacket(BinaryReader reader, int whoAmI)
	{
		if (Main.netMode == NetmodeID.MultiplayerClient) {
			bool success = reader.ReadBoolean();
			string message = reader.ReadString();
			if (success) {
				string modname = reader.ReadString();
				string configname = reader.ReadString();
				string json = reader.ReadString();
				ModConfig activeConfig = GetConfig(ModLoader.GetMod(modname), configname);
				JsonConvert.PopulateObject(json, activeConfig, serializerSettingsCompact);
				activeConfig.OnChanged();

				Main.NewText($"Shared config changed: Message: {message}, Mod: {modname}, Config: {configname}");
				if (Main.InGameUI.CurrentState == Interface.modConfig) {
					Main.InGameUI.SetState(Interface.modConfig);
					Interface.modConfig.SetMessage("Server response: " + message, Color.Green);
				}
			}
			else {
				// rejection only sent back to requester.
				// Update UI with message

				Main.NewText("Changes Rejected: " + message);
				if (Main.InGameUI.CurrentState == Interface.modConfig) {
					Interface.modConfig.SetMessage("Server rejected changes: " + message, Color.Red);
					//Main.InGameUI.SetState(Interface.modConfig);
				}

			}
		}
		else {
			// no bool in request.
			string modname = reader.ReadString();
			string configname = reader.ReadString();
			string json = reader.ReadString();

			var mod = ModLoader.GetMod(modname);

			ModConfig config = GetConfig(mod, configname);
			ModConfig loadTimeConfig = GetLoadTimeConfig(mod, configname);
			ModConfig pendingConfig = GeneratePopulatedClone(config);
			JsonConvert.PopulateObject(json, pendingConfig, serializerSettingsCompact);
			bool success = true;
			string message = "Accepted";
			if (loadTimeConfig.NeedsReload(pendingConfig)) {
				success = false;
				message = "Can't save because changes would require a reload.";
			}
			if (!config.AcceptClientChanges(pendingConfig, whoAmI, ref message)) {
				success = false;
			}
			if (success) {
				// Apply to Servers Config
				ConfigManager.Save(pendingConfig);
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
			else {
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

	public static ModConfig GeneratePopulatedClone(ModConfig original)
	{
		string json = JsonConvert.SerializeObject(original, ConfigManager.serializerSettings);
		ModConfig properClone = original.Clone();
		JsonConvert.PopulateObject(json, properClone, ConfigManager.serializerSettings);
		return properClone;
	}

	public static object AlternateCreateInstance(Type type)
	{
		if (type == typeof(string))
			return "";
		return Activator.CreateInstance(type, true);
	}

	// Gets an Attribute from a property or field. Attribute defined on Member has highest priority,
	// followed by the containing data structure, followed by attribute defined on the Class.
	public static T GetCustomAttribute<T>(PropertyFieldWrapper memberInfo, object item, object array) where T : Attribute
	{
		// Class
		T attribute = (T)Attribute.GetCustomAttribute(memberInfo.Type, typeof(T), true);
		if (array != null) {
			// item null?
			//	attribute = (T)Attribute.GetCustomAttribute(item.GetType(), typeof(T), true) ?? attribute; // TODO: is this wrong?
		}
		// Member
		attribute = (T)Attribute.GetCustomAttribute(memberInfo.MemberInfo, typeof(T)) ?? attribute;
		return attribute;
		// TODO: allow for inheriting from parent's parent? (could get attribute from parent ConfigElement)
	}

	public static T GetCustomAttribute<T>(PropertyFieldWrapper memberInfo, Type type) where T : Attribute
	{
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

	public static void SetPendingChanges(bool changes = true)
	{
		// public api for modders.
		Interface.modConfig.SetPendingChanges(changes);
	}

	// TODO: better home?
	public static bool ObjectEquals(object a, object b)
	{
		if (ReferenceEquals(a, b))
			return true;
		if (a == null || b == null)
			return false;
		if (a is IEnumerable && b is IEnumerable && !(a is string) && !(b is string))
			return EnumerableEquals((IEnumerable)a, (IEnumerable)b);
		return a.Equals(b);
	}

	public static bool EnumerableEquals(IEnumerable a, IEnumerable b)
	{
		IEnumerator enumeratorA = a.GetEnumerator();
		IEnumerator enumeratorB = b.GetEnumerator();
		bool hasNextA = enumeratorA.MoveNext();
		bool hasNextB = enumeratorB.MoveNext();
		while (hasNextA && hasNextB) {
			if (!ObjectEquals(enumeratorA.Current, enumeratorB.Current))
				return false;
			hasNextA = enumeratorA.MoveNext();
			hasNextB = enumeratorB.MoveNext();
		}
		return !hasNextA && !hasNextB;
	}

	internal static string GetLabelKey(PropertyFieldWrapper memberInfo, ModConfig config, bool fallbackToClass, bool throwErrors)
	{
		var label = (LabelAttribute)Attribute.GetCustomAttribute(memberInfo.MemberInfo, typeof(LabelAttribute));
		if (label != null) {
			if (label.malformed && throwErrors)
				throw new ValueNotTranslationKeyException($"{nameof(LabelAttribute)} only accepts localization keys for the 'key' parameter.");
			return label.key;
		}
		label = (LabelAttribute)Attribute.GetCustomAttribute(memberInfo.Type, typeof(LabelAttribute));
		if (label != null) {
			if (label.malformed && throwErrors)
				throw new ValueNotTranslationKeyException($"{nameof(LabelAttribute)} only accepts localization keys for the 'key' parameter.");
			if (fallbackToClass) // FinishSetup will catch errors on Label annotations on classes
				return label.key;
		}
		return config.Mod.GetLocalizationKey($"Configs.{config.Name}.{memberInfo.Name}.Label");
	}

	internal static string GetLocalizedLabel(LabelAttribute labelAttribute, PropertyFieldWrapper memberInfo) {
		// Priority: Provided/Auto Key on member -> Key on class if member translation is empty string -> member name
		var config = Interface.modConfig.pendingConfig;
		string labelKey = GetLabelKey(memberInfo, config, fallbackToClass: false, throwErrors: false);
		if (Language.Exists(labelKey)) {
			string labelLocalization = Language.GetTextValue(labelKey);
			if (!string.IsNullOrEmpty(labelLocalization))
				return labelLocalization;
		}

		var typeLabel = (LabelAttribute)Attribute.GetCustomAttribute(memberInfo.Type, typeof(LabelAttribute));
		if (typeLabel != null && Language.Exists(typeLabel.key)) {
			string labelLocalization = Language.GetTextValue(typeLabel.key);
			if (!string.IsNullOrEmpty(labelLocalization))
				return labelLocalization;
		}

		return memberInfo.Name;
	}

	internal static string GetTooltipKey(PropertyFieldWrapper memberInfo, ModConfig config, bool fallbackToClass, bool throwErrors)
	{
		var tooltip = (TooltipAttribute)Attribute.GetCustomAttribute(memberInfo.MemberInfo, typeof(TooltipAttribute));
		if (tooltip != null) {
			if (tooltip.malformed && throwErrors)
				throw new ValueNotTranslationKeyException($"{nameof(TooltipAttribute)} only accepts localization keys for the 'key' parameter.");
			return tooltip.key;
		}
		tooltip = (TooltipAttribute)Attribute.GetCustomAttribute(memberInfo.Type, typeof(TooltipAttribute));
		if (tooltip != null) {
			if (tooltip.malformed && throwErrors)
				throw new ValueNotTranslationKeyException($"{nameof(TooltipAttribute)} only accepts localization keys for the 'key' parameter.");
			if (fallbackToClass) // FinishSetup will catch errors on Tooltip annotations on classes
				return tooltip.key;
		}
		return config.Mod.GetLocalizationKey($"Configs.{config.Name}.{memberInfo.Name}.Tooltip");
	}

	internal static string GetLocalizedTooltip(TooltipAttribute tooltipAttribute, PropertyFieldWrapper memberInfo)
	{
		// Priority: Provided/Auto Key on member -> Key on class if member translation is empty string -> null
		var config = Interface.modConfig.pendingConfig;
		string tooltipKey = GetTooltipKey(memberInfo, config, fallbackToClass: false, throwErrors: false);
		if (Language.Exists(tooltipKey)) {
			string tooltipLocalization = Language.GetTextValue(tooltipKey);
			if (!string.IsNullOrEmpty(tooltipLocalization))
				return tooltipLocalization;
		}

		var typeTooltip = (TooltipAttribute)Attribute.GetCustomAttribute(memberInfo.Type, typeof(TooltipAttribute));
		if (typeTooltip != null && Language.Exists(typeTooltip.key)) {
			string tooltipLocalization = Language.GetTextValue(typeTooltip.key);
			if (!string.IsNullOrEmpty(tooltipLocalization))
				return tooltipLocalization;
		}

		return null;
	}

	internal static HeaderAttribute GetLocalizedHeader(PropertyFieldWrapper memberInfo, ModConfig config, bool throwErrors)
	{
		// Priority: Auto/Provided Key on member -> Key on class -> null
		var header = GetCustomAttribute<HeaderAttribute>(memberInfo, null, null);
		if (header != null) {
			if (header.malformed && throwErrors)
				throw new ValueNotTranslationKeyException($"{nameof(HeaderAttribute)} only accepts localization keys or identifiers for the 'identifierOrKey' parameter. Neither can have spaces.");

			if (header.malformed)
				header.identifier = memberInfo.Name;

			if (header.IsIdentifier)
				header.key = config.Mod.GetLocalizationKey($"Configs.{config.Name}.Headers.{header.identifier}");

			return header;
		}
		return null;
	}

	internal static string GetModConfigLabelKey(ModConfig config, bool throwErrors)
	{
		string labelKey = config.Mod.GetLocalizationKey($"Configs.{config.Name}.DisplayName");
		var label = (LabelAttribute)Attribute.GetCustomAttribute(config.GetType(), typeof(LabelAttribute));
		if (label != null) {
			if (label.malformed && throwErrors)
				throw new ValueNotTranslationKeyException($"{nameof(LabelAttribute)} only accepts localization keys for the 'key' parameter.");

			labelKey = label.key;
		}
		return labelKey;
	}

	internal static string GetModConfigDisplayName(ModConfig config)
	{
		// Priority: Provided Localization Key -> Auto Localization Key -> InternalName (Type.Name)
		string labelKey = GetModConfigLabelKey(config, throwErrors: false);
		if (Language.Exists(labelKey))
			return Language.GetTextValue(labelKey);
		
		return config.Name;
	}
}

/// <summary>
/// Custom ContractResolver for facilitating refernce type defaults.
/// The ShouldSerialize code enables unchanged-by-user reference type defaults to properly not serialize.
/// The ValueProvider code helps during deserialization to not
/// </summary>
internal class ReferenceDefaultsPreservingResolver : DefaultContractResolver
{
	// This approach largely based on https://stackoverflow.com/a/52684798.
	public abstract class ValueProviderDecorator : IValueProvider
	{
		readonly IValueProvider baseProvider;

		public ValueProviderDecorator(IValueProvider baseProvider)
		{
			this.baseProvider = baseProvider ?? throw new ArgumentNullException();
		}

		public virtual object GetValue(object target)
			=> baseProvider.GetValue(target);

		public virtual void SetValue(object target, object value)
			=> baseProvider.SetValue(target, value);
	}

	private class NullToDefaultValueProvider : ValueProviderDecorator
	{
		//readonly object defaultValue;
		readonly Func<object> defaultValueGenerator;

		//public NullToDefaultValueProvider(IValueProvider baseProvider, object defaultValue) : base(baseProvider) {
		//	this.defaultValue = defaultValue;
		//}

		public NullToDefaultValueProvider(IValueProvider baseProvider, Func<object> defaultValueGenerator) : base(baseProvider)
		{
			this.defaultValueGenerator = defaultValueGenerator;
		}

		public override void SetValue(object target, object value)
		{
			base.SetValue(target, value ?? defaultValueGenerator.Invoke());
			//base.SetValue(target, value ?? defaultValue);
		}
	}

	protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
	{
		IList<JsonProperty> props = base.CreateProperties(type, memberSerialization);

		if (!type.IsClass) {
			return props;
		}

		ConstructorInfo ctor = type.GetConstructor(Type.EmptyTypes);

		if (ctor == null) {
			return props;
		}

		object referenceInstance = ctor.Invoke(null);

		foreach (JsonProperty prop in props.Where(p => p.Readable)) {
			if (prop.PropertyType.IsValueType) {
				continue;
			}

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

			prop.ShouldSerialize ??= instance => {
				object val = prop.ValueProvider.GetValue(instance);
				object refVal = prop.ValueProvider.GetValue(referenceInstance);

				return !ConfigManager.ObjectEquals(val, refVal);
			};
		}

		return props;
	}
}
