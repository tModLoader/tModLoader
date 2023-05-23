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
using Terraria.ModLoader.Core;
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
		//TypeNameHandling = TypeNameHandling.Auto, // We can support polymorphism for collections if requested. Could support an Add button per derived class in UI?
		//Converters = converters,
		ContractResolver = new ReferenceDefaultsPreservingResolver()
	};

	internal static readonly JsonSerializerSettings serializerSettingsCompact = new() {
		Formatting = Formatting.None,
		DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
		ObjectCreationHandling = ObjectCreationHandling.Replace,
		NullValueHandling = NullValueHandling.Ignore,
		//TypeNameHandling = TypeNameHandling.Auto,
		//Converters = converters,
		ContractResolver = serializerSettings.ContractResolver
	};

	/* Wasn't working due to initialization order. Revist later.
	private static readonly IList<JsonConverter> converters = new List<JsonConverter>() {
		new Newtonsoft.Json.Converters.VersionConverter(),
		//new ColorJsonConverter(),
	};
	*/

	private static readonly HashSet<Type> typesWithLocalizationRegistered = new HashSet<Type>();

	public static readonly string ModConfigPath = Path.Combine(Main.SavePath, "ModConfigs");
	public static readonly string ServerModConfigPath = Path.Combine(Main.SavePath, "ModConfigs", "Server");

	static ConfigManager()
	{
		TypeCaching.OnClear += () => typesWithLocalizationRegistered.Clear();
	}

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
				try {
					_ = config.DisplayName;

					RegisterLocalizationKeysForMembers(config.GetType());
				}
				catch (Exception e) {
					e.Data["mod"] = config.Mod.Name;
					throw;
				}
			}
		}
	}

	private static void RegisterLocalizationKeysForMembers(Type type)
	{
		AssemblyManager.GetAssemblyOwner(type.Assembly, out var modName);
		foreach (PropertyFieldWrapper variable in ConfigManager.GetFieldsAndProperties(type)) {
#pragma warning disable CS0618
			if (Attribute.IsDefined(variable.MemberInfo, typeof(JsonIgnoreAttribute)) && !(Attribute.IsDefined(variable.MemberInfo, typeof(LabelAttribute)) || Attribute.IsDefined(variable.MemberInfo, typeof(ShowDespiteJsonIgnoreAttribute))))
				continue;
#pragma warning restore CS0618

			RegisterLocalizationKeysForMemberType(variable.Type, type.Assembly);

			// Handle obsolete attributes. Use them to populate value of key, if present, to ease porting.
#pragma warning disable CS0618 // Type or member is obsolete
			var labelObsolete = (LabelAttribute)Attribute.GetCustomAttribute(variable.MemberInfo, typeof(LabelAttribute));
			var tooltipObsolete = (TooltipAttribute)Attribute.GetCustomAttribute(variable.MemberInfo, typeof(TooltipAttribute));
#pragma warning restore CS0618 // Type or member is obsolete

			// Label and Tooltip will always exist. Header is optional, need to be used to exist.
			var header = GetLocalizedHeader(variable);
			if (header != null) {
				string identifier = header.IsIdentifier ? header.identifier : variable.Name;
				Language.GetOrRegister(header.key, () => $"{Regex.Replace(identifier, "([A-Z])", " $1").Trim()} Header");
			}

			string labelKey = GetConfigKey<LabelKeyAttribute>(variable.MemberInfo, dataName: "Label");
			Language.GetOrRegister(labelKey, () => labelObsolete?.LocalizationEntry ?? Regex.Replace(variable.Name, "([A-Z])", " $1").Trim());

			if (!type.IsEnum) {
				string tooltipKey = GetConfigKey<TooltipKeyAttribute>(variable.MemberInfo, dataName: "Tooltip");
				Language.GetOrRegister(tooltipKey, () => tooltipObsolete?.LocalizationEntry ?? "");
			}
		}
	}

	private static void RegisterLocalizationKeysForMemberType(Type type, Assembly owningAssembly)
	{
		if (type.IsGenericType) {
			// assume it's a collection.
			foreach (var t in type.GetGenericArguments())
				RegisterLocalizationKeysForMemberType(t, owningAssembly);
		}

		// Register localization for classes added in this mod. This code handles the class itself and the fields of the classes
		if ((type.IsClass || type.IsEnum) && type.Assembly == owningAssembly && typesWithLocalizationRegistered.Add(type)) {
			// Only tooltip is registered for the Type itself.
			string typeTooltipKey = GetConfigKey<TooltipKeyAttribute>(type, dataName: "Tooltip");
			Language.GetOrRegister(typeTooltipKey, () => "");

			RegisterLocalizationKeysForMembers(type);
		}
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

	public static IEnumerable<PropertyFieldWrapper> GetFieldsAndProperties(Type type)
	{
		if (type.IsEnum) {
			return type.GetFields(BindingFlags.Public |	BindingFlags.Static).Select(x => new PropertyFieldWrapper(x));
		}
		PropertyInfo[] properties = type.GetProperties(
			BindingFlags.Public |
			BindingFlags.Instance);

		FieldInfo[] fields = type.GetFields(
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

	internal static string FormatTextAttribute(string key, string localization, object[] args)
	{
		if (args == null)
			return localization;
		for (int i = 0; i < args.Length; i++) {
			if (args[i] is string s && s.StartsWith("$"))
				args[i] = Language.GetTextValue(FindKeyInScope(s.Substring(1), key));
		}
		return Language.GetText(key).Format(args);

		string FindKeyInScope(string key, string scope)
		{
			if (LanguageManager.Instance.Exists(key))
				return key;

			string[] splitKey = scope.Split(".");
			for (int j = splitKey.Length - 1; j >= 0; j--) {
				string partialKey = string.Join(".", splitKey.Take(j + 1));
				string combinedKey = partialKey + "." + key;
				if (LanguageManager.Instance.Exists(combinedKey))
					return combinedKey;
			}

			return key;
		}
	}

	private static T GetAndValidate<T>(MemberInfo memberInfo) where T : ConfigKeyAttribute
	{
		var configKeyAttribute = (T)Attribute.GetCustomAttribute(memberInfo, typeof(T));
		if (configKeyAttribute?.malformed == true) {
			string message = $"{typeof(T).Name} only accepts localization keys for the 'key' parameter.";
			if(memberInfo is Type type) {
				message += $"\nThe class '{type.FullName}' caused this exception.";
			}
			else {
				message += $"\nThe member '{memberInfo.Name}' found in the '{memberInfo.DeclaringType}' class caused this exception.";
			}
			throw new ValueNotTranslationKeyException(message);
		}
		return configKeyAttribute;
	}

	// Used to determine which key to register, based only on field/property, not class, not necessarily which key to use in UI.
	private static string GetConfigKey<T>(MemberInfo memberInfo, string dataName) where T : ConfigKeyAttribute
	{
		// Attribute otherwise Autokey: Determine key from the Type the member belongs to.
		return GetAndValidate<T>(memberInfo)?.key ?? GetDefaultLocalizationKey(memberInfo, dataName);
	}

	private static string GetDefaultLocalizationKey(MemberInfo member, string dataName)
	{
		Assembly asm = (member is Type t ? t : member.DeclaringType).Assembly;
		string groupKey = AssemblyManager.GetAssemblyOwner(asm, out var modName) ? $"Mods.{modName}.Configs" : "Config";
		string memberKey = member is Type ? member.Name : $"{member.DeclaringType.Name}.{member.Name}";
		return $"{groupKey}.{memberKey}.{dataName}";
	}

	internal static string GetLocalizedText<T, TArgs>(PropertyFieldWrapper memberInfo, string dataName) where T : ConfigKeyAttribute where TArgs : ConfigArgsAttribute
	{
		bool isTooltip = typeof(T) == typeof(TooltipKeyAttribute);

		// Priority: Provided/AutoKey on member -> Provided/AutoKey on class if member translation is empty string and T is Tooltip -> member name or null
		var args = GetCustomAttribute<TArgs>(memberInfo, memberInfo.Type);
		string configKey = GetConfigKey<T>(memberInfo.MemberInfo, dataName: dataName);
		if (Language.Exists(configKey)) {
			string configLocalization = Language.GetTextValue(configKey);
			if (!(isTooltip && string.IsNullOrEmpty(configLocalization)))
				return FormatTextAttribute(configKey, configLocalization, args?.args);
		}

		if (memberInfo.Type.IsClass || memberInfo.Type.IsEnum) {
			string typeConfigKey = GetConfigKey<T>(memberInfo.Type, dataName: dataName);
			if(Language.Exists(typeConfigKey))
				return FormatTextAttribute(typeConfigKey, Language.GetTextValue(typeConfigKey), args?.args);
		}

		return isTooltip ? null : memberInfo.Name;
	}

	internal static HeaderAttribute GetLocalizedHeader(PropertyFieldWrapper memberInfo)
	{
		// Priority: Provided Key or key derived from identifier on member
		var header = GetCustomAttribute<HeaderAttribute>(memberInfo, null, null);
		if (header == null) {
			return null;
		}
		if (header.malformed) {
			throw new ValueNotTranslationKeyException($"{nameof(HeaderAttribute)} only accepts localization keys or identifiers for the 'identifierOrKey' parameter. Neither can have spaces.\nThe member '{memberInfo.Name}' found in the '{memberInfo.MemberInfo.DeclaringType}' class caused this exception.");
		}

		if (header.IsIdentifier) {
			AssemblyManager.GetAssemblyOwner(memberInfo.MemberInfo.DeclaringType.Assembly, out var modName);
			string className = memberInfo.MemberInfo.DeclaringType.Name;
			if (modName == null) {  // tModLoader keys handle translations for existing classes
				header.key = $"Config.{className}.Headers.{header.identifier}";
			}
			else {
				header.key = $"Mods.{modName}.Configs.{className}.Headers.{header.identifier}";
			}
		}

		return header;
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
