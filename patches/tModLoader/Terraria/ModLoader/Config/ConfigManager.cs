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
#nullable enable
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

	/* Wasn't working due to initialization order. Revisit later.
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

		if (!Configs.TryGetValue(config.Mod, out var configList))
			Configs[config.Mod] = configList = new List<ModConfig>();

		configList.Add(config);

		var instanceField = config.GetType().GetField("Instance", BindingFlags.Static | BindingFlags.Public);
		instanceField?.SetValue(null, config);

		config.OnLoaded();
		config.OnChanged();

		// Maintain a backup of LoadTime Configs.
		if (!loadTimeConfigs.TryGetValue(config.Mod, out var loadTimeConfigList))
			loadTimeConfigs[config.Mod] = loadTimeConfigList = new List<ModConfig>();

		loadTimeConfigList.Add(GeneratePopulatedClone(config));
	}

	internal static void FinishSetup()
	{
		// Register localization for all fields and properties that should show
		foreach (var activeConfigs in Configs) {
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
		foreach (PropertyFieldWrapper variable in GetFieldsAndProperties(type)) {
			// Handle obsolete attributes. Use them to populate value of key, if present, to ease porting.
			var labelObsolete = GetLegacyLabelAttribute(variable.MemberInfo);
			var tooltipObsolete = GetLegacyTooltipAttribute(variable.MemberInfo);

			if (Attribute.IsDefined(variable.MemberInfo, typeof(JsonIgnoreAttribute)) && labelObsolete == null && !Attribute.IsDefined(variable.MemberInfo, typeof(ShowDespiteJsonIgnoreAttribute)))
				continue;

			RegisterLocalizationKeysForMemberType(variable.Type, type.Assembly);

			// Label and Tooltip will always exist. Header is optional, need to be used to exist.
			var header = GetLocalizedHeader(variable.MemberInfo);
			if (header != null) {
				string identifier = header.IsIdentifier ? header.identifier : variable.Name;
				Language.GetOrRegister(header.key, () => $"{Regex.Replace(identifier, "([A-Z])", " $1").Trim()} Header");
			}

			string labelKey = GetConfigKey<LabelKeyAttribute>(variable.MemberInfo, "Label");
			Language.GetOrRegister(labelKey, () => labelObsolete?.LocalizationEntry ?? Regex.Replace(variable.Name, "([A-Z])", " $1").Trim());

			string tooltipKey = GetConfigKey<TooltipKeyAttribute>(variable.MemberInfo, "Tooltip");
			Language.GetOrRegister(tooltipKey, () => tooltipObsolete?.LocalizationEntry ?? "");
		}
	}

	private static void RegisterLocalizationKeysForEnumMembers(Type type)
	{
		var enumFields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
		foreach (var field in enumFields) {
			// Handle obsolete attributes. Use them to populate value of key, if present, to ease porting.
			var labelObsolete = GetLegacyLabelAttribute(field);

			string labelKey = GetConfigKey<LabelKeyAttribute>(field, "Label");
			Language.GetOrRegister(labelKey, () => labelObsolete?.LocalizationEntry ?? Regex.Replace(field.Name, "([A-Z])", " $1").Trim());

			string tooltipKey = GetConfigKey<TooltipKeyAttribute>(field, "Tooltip");
			Language.GetOrRegister(tooltipKey, () => "");
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

			if (type.IsClass)
				RegisterLocalizationKeysForMembers(type);
			else
				RegisterLocalizationKeysForEnumMembers(type);
		}
	}

#pragma warning disable CS0618 // Type or member is obsolete
	internal static LabelAttribute? GetLegacyLabelAttribute(MemberInfo memberInfo) => (LabelAttribute?)Attribute.GetCustomAttribute(memberInfo, typeof(LabelAttribute));
	internal static TooltipAttribute? GetLegacyTooltipAttribute(MemberInfo memberInfo) => (TooltipAttribute?)Attribute.GetCustomAttribute(memberInfo, typeof(TooltipAttribute));
#pragma warning restore CS0618 // Type or member is obsolete

	// This method for refreshing configs (ServerSide mostly) after events that could change configs: Multiplayer play.
	internal static void LoadAll()
	{
		foreach (var activeConfigs in Configs) {
			foreach (var activeConfig in activeConfigs.Value) {
				Load(activeConfig);
			}
		}
	}

	internal static void OnChangedAll()
	{
		foreach (var activeConfigs in Configs) {
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
			FieldInfo? instance = config.GetType().GetField("Instance", BindingFlags.Static | BindingFlags.Public);
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

	internal static bool AnyModNeedsReloadCheckOnly(out List<Mod> modsWithChangedConfigs)
	{
		modsWithChangedConfigs = new List<Mod>();
		bool result = false;
		foreach (var mod in ModLoader.Mods) {
			if (!Configs.ContainsKey(mod)) {
				continue;
			}
			var configs = Configs[mod];
			var loadTimeConfigs = ConfigManager.loadTimeConfigs[mod];
			for (int i = 0; i < configs.Count; i++) {
				var configClone = GeneratePopulatedClone(configs[i]);
				Load(configClone); // Should load non-server config values from disk since ModNet.NetReloadActive should be false
				if (loadTimeConfigs[i].NeedsReload(configClone)) {
					result = true;
					modsWithChangedConfigs.Add(mod);
					break;
				}
			}
		}
		return result;
	}

	// GetConfig...returns the config instance
	internal static ModConfig GetConfig(ModNet.NetConfig netConfig) => ConfigManager.GetConfig(ModLoader.GetMod(netConfig.modname), netConfig.configname);
	internal static ModConfig GetConfig(Mod mod, string config)
	{
		if (Configs.TryGetValue(mod, out List<ModConfig>? configs)) {
			return configs.Single(x => x.Name == config);
		}
		throw new MissingResourceException("Missing config named " + config + " in mod " + mod.Name);
	}

	internal static ModConfig GetLoadTimeConfig(Mod mod, string config)
	{
		if (loadTimeConfigs.TryGetValue(mod, out List<ModConfig>? configs)) {
			return configs.Single(x => x.Name == config);
		}
		throw new MissingResourceException("Missing config named " + config + " in mod " + mod.Name);
	}

	internal static void HandleInGameChangeConfigPacket(BinaryReader reader, int whoAmI)
	{
		if (Main.netMode == NetmodeID.MultiplayerClient) {
			bool success = reader.ReadBoolean();
			NetworkText message = NetworkText.Deserialize(reader);
			string modname = reader.ReadString();
			string configname = reader.ReadString();
			bool broadcast = reader.ReadBoolean();
			ModConfig activeConfig = GetConfig(ModLoader.GetMod(modname), configname);
			int requestor = reader.ReadByte();
			if (success) {
				string json = reader.ReadString();
				JsonConvert.PopulateObject(json, activeConfig, serializerSettingsCompact);
				activeConfig.OnChanged();
				activeConfig.HandleAcceptClientChangesReply(success, requestor, message);

				if(broadcast)
					Main.NewText(Language.GetTextValue("tModLoader.ModConfigSharedConfigChanged", message, modname, configname));
				if (Main.InGameUI.CurrentState == Interface.modConfig) {
					Main.InGameUI.SetState(null);
					Main.InGameUI.SetState(Interface.modConfig); // Refresh with changes from server, config might have been tweaked.
					Interface.modConfig.SetMessage(Language.GetTextValue("tModLoader.ModConfigServerResponse", message), Color.Green);
				}
			}
			else {
				// rejection only sent back to requester.
				// Update UI with message, but don't clear user's pending changes
				activeConfig.HandleAcceptClientChangesReply(success, requestor, message);
				if (broadcast)
					Main.NewText(Language.GetTextValue("tModLoader.ModConfigServerRejectedChanges", message));
				if (Main.InGameUI.CurrentState == Interface.modConfig) {
					Interface.modConfig.SetMessage(Language.GetTextValue("tModLoader.ModConfigServerRejectedChanges", message), Color.Red);
				}
			}
		}
		else {
			// no bool in request.
			string modname = reader.ReadString();
			string configname = reader.ReadString();
			bool broadcast = reader.ReadBoolean();
			string json = reader.ReadString();

			var mod = ModLoader.GetMod(modname);

			ModConfig config = GetConfig(mod, configname);
			ModConfig loadTimeConfig = GetLoadTimeConfig(mod, configname);
			ModConfig pendingConfig = GeneratePopulatedClone(config);
			JsonConvert.PopulateObject(json, pendingConfig, serializerSettingsCompact);
			bool success = true;
			NetworkText message = NetworkText.FromKey("tModLoader.ModConfigAccepted");
			if (loadTimeConfig.NeedsReload(pendingConfig)) {
				success = false;
				message = NetworkText.FromKey("tModLoader.ModConfigCantSaveBecauseChangesWouldRequireAReload");
			}

			string stringMessage = ""; // For compatibility with mods that haven't updated yet
			success &= config.AcceptClientChanges(pendingConfig, whoAmI, ref message);
#pragma warning disable CS0618 // Type or member is obsolete
			success &= config.AcceptClientChanges(pendingConfig, whoAmI, ref stringMessage);
#pragma warning restore CS0618 // Type or member is obsolete

			if (!string.IsNullOrEmpty(stringMessage))
				message = NetworkText.FromLiteral(stringMessage);

			if (success) {
				// Apply to Servers Config
				ConfigManager.Save(pendingConfig);
				json = JsonConvert.SerializeObject(pendingConfig, serializerSettingsCompact); // AcceptClientChanges can modify pendingConfig for advanced permissions filtering.
				JsonConvert.PopulateObject(json, config, ConfigManager.serializerSettingsCompact);
				config.OnChanged();
				// Send new config to all clients
				var p = new ModPacket(MessageID.InGameChangeConfig);
				p.Write(true);
				message.Serialize(p);
				p.Write(modname);
				p.Write(configname);
				p.Write(broadcast);
				p.Write((byte)whoAmI);
				p.Write(json);
				p.Send();
			}
			else {
				// Send rejections message back to client who requested change
				var p = new ModPacket(MessageID.InGameChangeConfig);
				p.Write(false);
				message.Serialize(p);
				p.Write(modname);
				p.Write(configname);
				p.Write(broadcast);
				p.Write((byte)whoAmI);
				p.Send(whoAmI);
			}

		}
		return;
	}

	public static IEnumerable<PropertyFieldWrapper> GetFieldsAndProperties(object item) => GetFieldsAndProperties(item.GetType());

	public static IEnumerable<PropertyFieldWrapper> GetFieldsAndProperties(Type type)
	{
		PropertyInfo[] properties = type.GetProperties(
			BindingFlags.Public |
			BindingFlags.Instance);

		FieldInfo[] fields = type.GetFields(
			BindingFlags.Public |
			BindingFlags.Instance);

		return fields.Select(x => new PropertyFieldWrapper(x)).Concat(properties.Select(x => new PropertyFieldWrapper(x)));
	}

	/// <summary>
	/// Creates a clone of the provided ModConfig. This clone can be modified independently of the active config.
	/// <br/><br/> Mods can use this method to create a clone of the active config, then use it to populate a UI. The player can use the UI to make several changes then save them all at once.
	/// </summary>
	public static ModConfig GeneratePopulatedClone(ModConfig original)
	{
		string json = JsonConvert.SerializeObject(original, ConfigManager.serializerSettings);
		ModConfig properClone = original.Clone();
		JsonConvert.PopulateObject(json, properClone, ConfigManager.serializerSettings);
		return properClone;
	}

	public static object? AlternateCreateInstance(Type type)
	{
		if (type == typeof(string))
			return "";
		return Activator.CreateInstance(type, true);
	}

	// Gets an Attribute from a property or field. Attribute defined on Member has highest priority, followed by attribute defined on the Class of that member.
	public static T? GetCustomAttributeFromMemberThenMemberType<T>(PropertyFieldWrapper memberInfo, object? item, object? array) where T : Attribute
	{
		return
			(T?)Attribute.GetCustomAttribute(memberInfo.MemberInfo, typeof(T)) ?? // on the member itself
			(T?)Attribute.GetCustomAttribute(memberInfo.Type, typeof(T), true); // on the class
		// TODO: The intention was to prioritize the Type of the element in the array at this index. That was never hooked up it seems, and it might not make sense to apply this behavior to all config attributes at this time. Needs more thought, specifically about collections and which attributes to "inherit". For example, currently ListOfPair won't use Pair BackgroundColor
	}

	// Used to get an attribute for a member that is a generic collection. The member attribute has highest priority, then attribute on the generic Type.
	public static T? GetCustomAttributeFromCollectionMemberThenElementType<T>(MemberInfo memberInfo, Type elementType) where T : Attribute
	{
		return
			(T?)Attribute.GetCustomAttribute(memberInfo, typeof(T)) ?? // on the member itself
			(T?)Attribute.GetCustomAttribute(elementType, typeof(T), true); // on a provided fallback type
	}

	public static Tuple<UIElement, UIElement> WrapIt(UIElement parent, ref int top, PropertyFieldWrapper memberInfo, object item, int order, object? list = null, Type? arrayType = null, int index = -1)
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
	public static bool ObjectEquals(object? a, object? b)
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

	internal static string FormatTextAttribute(LocalizedText localizedText, object[]? args)
	{
		if (args == null)
			return localizedText.Value;
		for (int i = 0; i < args.Length; i++) {
			if (args[i] is string s && s.StartsWith("$"))
				args[i] = Language.GetTextValue(FindKeyInScope(s.Substring(1), localizedText.Key));
		}
		return localizedText.Format(args);

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

	private static T? GetAndValidate<T>(MemberInfo memberInfo) where T : ConfigKeyAttribute
	{
		var configKeyAttribute = (T?)Attribute.GetCustomAttribute(memberInfo, typeof(T));
		if (configKeyAttribute?.malformed is true) {
			string message = $"{typeof(T).Name} only accepts localization keys for the 'key' parameter.";
			if (memberInfo is Type type) {
				message += $"\nThe class '{type.FullName}' caused this exception.";
			}
			else {
				message += $"\nThe member '{memberInfo.Name}' found in the '{memberInfo.DeclaringType}' class caused this exception.";
			}
			message += $"\nClick Open Web Help for more information.";
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
		Assembly asm = (member is Type t ? t : member.DeclaringType!).Assembly;
		string groupKey = AssemblyManager.GetAssemblyOwner(asm, out var modName) ? $"Mods.{modName}.Configs" : "Config";
		string memberKey = member is Type ? member.Name : $"{member.DeclaringType!.Name}.{member.Name}";
		return $"{groupKey}.{memberKey}.{dataName}";
	}

	internal static string GetLocalizedLabel(PropertyFieldWrapper member) => GetLocalizedText<LabelKeyAttribute, LabelArgsAttribute>(member, "Label") ?? member.Name;
	internal static string GetLocalizedTooltip(PropertyFieldWrapper member) => GetLocalizedText<TooltipKeyAttribute, TooltipArgsAttribute>(member, "Tooltip") ?? "";

	private static string? GetLocalizedText<T, TArgs>(PropertyFieldWrapper memberInfo, string dataName) where T : ConfigKeyAttribute where TArgs : ConfigArgsAttribute
	{
		// Priority: Provided/AutoKey on member -> Provided/AutoKey on class if member translation is empty string and T is Tooltip -> member name or null
		string memberKey = GetConfigKey<T>(memberInfo.MemberInfo, dataName);
		if (!Language.Exists(memberKey)) // If we didn't register a localization for the member, then it must be a member of a vanilla type and there's nothing more to do
			return null;

		var memberLocalization = Language.GetText(memberKey);
		if (memberLocalization.Value != "")
			return FormatTextAttribute(memberLocalization, memberInfo.MemberInfo.GetCustomAttribute<TArgs>()?.args);

		// bool, int, etc.
		if (memberInfo.Type.IsPrimitive || memberInfo.IsFieldOfAnEnum)
			return null;

		// try falling back to the type
		string typeKey = GetConfigKey<T>(memberInfo.Type, dataName);
		if (!Language.Exists(typeKey)) // if we didn't register a localization for the type, there's a good reason (T is a LabelKeyAttribute, or type is a vanilla type)
			return null;

		var typeLocalization = Language.GetText(typeKey);
		return FormatTextAttribute(typeLocalization, memberInfo.Type.GetCustomAttribute<TArgs>()?.args);
	}

	internal static HeaderAttribute? GetLocalizedHeader(MemberInfo memberInfo)
	{
		// Priority: Provided Key or key derived from identifier on member
		var header = memberInfo.GetCustomAttribute<HeaderAttribute>();
		if (header == null)
			return null;

		if (header.malformed)
			throw new ValueNotTranslationKeyException($"{nameof(HeaderAttribute)} only accepts localization keys or identifiers for the 'identifierOrKey' parameter. Neither can have spaces.\nThe member '{memberInfo.Name}' found in the '{memberInfo.DeclaringType}' class caused this exception.\nClick Open Web Help for more information.");

		if (header.IsIdentifier)
			header.key = GetDefaultLocalizationKey(memberInfo.DeclaringType!, $"Headers.{header.identifier}");

		return header;
	}
}

/// <summary>
/// Custom ContractResolver for facilitating reference type defaults.
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

		public virtual object? GetValue(object target)
			=> baseProvider.GetValue(target);

		public virtual void SetValue(object target, object? value)
			=> baseProvider.SetValue(target, value);
	}

	private class NullToDefaultValueProvider : ValueProviderDecorator
	{
		//readonly object defaultValue;
		readonly Func<object?> defaultValueGenerator;

		//public NullToDefaultValueProvider(IValueProvider baseProvider, object defaultValue) : base(baseProvider) {
		//	this.defaultValue = defaultValue;
		//}

		public NullToDefaultValueProvider(IValueProvider baseProvider, Func<object?> defaultValueGenerator) : base(baseProvider)
		{
			this.defaultValueGenerator = defaultValueGenerator;
		}

		public override void SetValue(object target, object? value)
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

		ConstructorInfo? ctor = type.GetConstructor(Type.EmptyTypes);

		if (ctor == null) {
			return props;
		}

		object referenceInstance = ctor.Invoke(null);

		foreach (JsonProperty prop in props.Where(p => p.Readable)) {
			if (prop.PropertyType == null)
				continue;

			if (prop.PropertyType.IsValueType) {
				continue;
			}

			// var a = type.GetMember(prop.PropertyName);

			if (prop.Writable) {
				if (prop.PropertyType.GetConstructor(Type.EmptyTypes) != null) {
					// defaultValueCreator will create new instance, then get the value from a field in that object. Prevents deserialized nulls from sharing with other instances.
					Func<object?> defaultValueCreator = () => prop.ValueProvider!.GetValue(ctor.Invoke(null));
					prop.ValueProvider = new NullToDefaultValueProvider(prop.ValueProvider!, defaultValueCreator);
				}
				else if (prop.PropertyType.IsArray) {
					Func<object?> defaultValueCreator = () => prop.ValueProvider!.GetValue(ctor.Invoke(null));
					// Func<object?> defaultValueCreator = () => (prop.ValueProvider!.GetValue(referenceInstance) as Array)!.Clone(); // Cloning an array copies element references, not what we need.
					prop.ValueProvider = new NullToDefaultValueProvider(prop.ValueProvider!, defaultValueCreator);
				}
			}

			prop.ShouldSerialize ??= instance => {
				object? val = prop.ValueProvider!.GetValue(instance);
				object? refVal = prop.ValueProvider.GetValue(referenceInstance);

				return !ConfigManager.ObjectEquals(val, refVal);
			};
		}

		return props;
	}
}
