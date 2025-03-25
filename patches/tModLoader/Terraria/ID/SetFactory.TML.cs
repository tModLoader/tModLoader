using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ReLogic.Reflection;
using ReLogic.Utilities;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;

namespace Terraria.ID;

/// <summary>
/// SetFactory is responsible for creating "custom ID sets" for content. "Custom ID sets" refers to arrays indexed by content ids. The ID set contains data applying to all instances of content of a specific type. This is typically metadata or data controlling how code will interact with each type of content. Each vanilla ID class contains a SetFactory instance called "Factory" which is used to initialize the ID sets contained within the ID class.
/// <para/> For example <see cref="ItemID.Sets.Factory"/> is used to initialize <see cref="ItemID.Sets.IsFood"/> with true values for food items such as <see cref="ItemID.PadThai"/>. Modded content updates ID sets in <see cref="ModType.SetStaticDefaults"/>: <c>ItemID.Sets.IsFood[Type] = true;</c>. Code in tModLoader and individual mods might consult the data in <see cref="ItemID.Sets.IsFood"/> for whatever purpose they want.
/// <para/> Mods can make their own custom ID sets through the methods of this class. The methods with "Named" in their method name facilitate collaborative "named ID sets". Mods using the same "named ID set" will share a reference to the same array merging together all the entries and changes. More information can be found in the <see href="https://github.com/tModLoader/tModLoader/pull/4381">Custom and Named ID Sets pull request</see>.
/// </summary>
public partial class SetFactory
{
	/// <summary>
	/// Used to construct the key for this named ID set. 
	/// </summary>
	public class SetKey
	{
		public string fullKey;
		public string additionalInfo;

		/// <summary>
		/// <inheritdoc cref="SetKey"/>
		/// <para/> The final key for this named ID set using this overload will be <c>"{key}"</c> directly if it contains a "/". Otherwise, the final key will be derived automatically from the currently loading mod: <c>"{loadingMod.Name}/{key}"</c>
		/// </summary>
		public SetKey(string fullKey)
		{
			this.fullKey = fullKey;
		}
		/// <summary>
		/// <inheritdoc cref="SetKey"/>
		/// <para/> The final key for this named ID set using this overload will be: <c>"{modName}/{key}"</c>
		/// </summary>
		public SetKey(string modName, string key)
		{
			this.fullKey = $"{modName}/{key}";
		}
		/// <summary>
		/// <inheritdoc cref="SetKey"/>
		/// <para/> The final key for this named ID set using this overload will be: <c>"{mod.Name}/{key}"</c>
		/// </summary>
		public SetKey(Mod mod, string key)
		{
			this.fullKey = $"{mod.Name}/{key}";
		}

		/// <summary>
		/// Adds additional info to this SetKey.
		/// <para/> This info will be used to register additional information about the named ID set. This info serves to communicate to other mod makers interested in interfacing with this set what the entries in the set mean and what your mod does with entries in the set. Multiple mods can register additional info and they will all be available to view. Modders can use the "/customsets" chat command to output a complete listing of additional information for all named ID sets to "CustomSets.txt" in the logs directory.
		/// </summary>
		public SetKey WithInfo(string additionalInfo)
		{
			this.additionalInfo = additionalInfo;
			return this;
		}

		public static implicit operator SetKey(string fullKey) => new SetKey(fullKey);
		//public static implicit operator SetKey((string modName, string key) s) => new SetKey(s.modName, s.key);
	}

	private class SetMetadata
	{
		internal readonly object defaultValue;
		internal readonly object array;
		internal HashSet<string> involvedMods = [];
		internal Dictionary<string, string> setDescriptions = [];

		public SetMetadata(object defaultValue, object array)
		{
			this.defaultValue = defaultValue;
			this.array = array;
		}

		public override int GetHashCode() => defaultValue.GetHashCode() ^ array.GetHashCode();

		public override bool Equals(object obj)
		{
			if (obj is SetMetadata metadata) {
				return defaultValue?.Equals(metadata.defaultValue) == true && array.Equals(metadata.array);
			}
			return false;
		}
	}

	// Additional code to support named custom sets for ad-hoc collaboration

	// Contains all SetFactory instances.
	internal static HashSet<SetFactory> SetFactories = new HashSet<SetFactory>();

	internal record SetFactoryTypeTypePair(string setFactoryName, Type type);
	// This is static since SetFactory instances are reset during ResizeArrays. Default value issues will be detected during RegisterNamedCustomSetWithInfo.
	internal static ConcurrentDictionary<SetFactoryTypeTypePair, List<HashSet<string>>> MergedSets = new ConcurrentDictionary<SetFactoryTypeTypePair, List<HashSet<string>>>();

	/// <summary>
	/// Causes sets registered with the provided keys (and matching SetFactory and Type) to be merged as if they are registered with the same key. This is useful for situations where established set keys are determined to have identical meaning but the involved mods are incapable of updating to collaborate on the shared key, either due to dependent mods or inactivity.
	/// <para/> Essentially, the sets will be merged and share the same data. The default value must still be consistent between the sets.
	/// <para/> This must be called before the ResizeArrays stage of mod loading, such as in a Load method.
	/// </summary>
	public static void MergeSets(SetFactory setFactory, Type type, params string[] inputSetNames)
	{
		if (ContentCache.contentLoadingFinished) {
			throw new Exception("MergeSets can only be called before sets are initialized, such as in Load.");
		}
		if (inputSetNames == null || inputSetNames.Length == 0)
			return;
		var registeredSets = MergedSets.GetOrAdd(new SetFactoryTypeTypePair(setFactory.ContainingClassName, type), new List<HashSet<string>>());
		// Take every existing set matching any input, merge them with inputs and remove excess sets.
		var existing = registeredSets.Where(registeredSet => inputSetNames.Any(a => registeredSet.Contains(a))).ToList();
		if (existing.Any()) {
			var toKeep = existing.First();
			foreach (var toRemove in existing.Skip(1)) {
				toKeep.UnionWith(toRemove);
				registeredSets.Remove(toRemove);
			}
			toKeep.UnionWith(inputSetNames);
		}
		else {
			registeredSets.Add(new HashSet<string>(inputSetNames));
		}
	}

	public static void ResizeArrays(bool unloading)
	{
		SetFactories.Clear();
		if (unloading)
			MergedSets = new ConcurrentDictionary<SetFactoryTypeTypePair, List<HashSet<string>>>(); // SetFactory.MergedSets.Clear() crashes the game for some reason?
	}

	private record SetNameTypePair(string setName, Type type);
	private ConcurrentDictionary<SetNameTypePair, SetMetadata> setMetadataMapping = new ConcurrentDictionary<SetNameTypePair, SetMetadata>();

	private string ContainingClassName;
	private IdDictionary search;

	public SetFactory(int size, string idClassName, IdDictionary search = null)
	{
		ContainingClassName = idClassName ?? "Unknown";
		this.search = search;
		if (SetFactories.Any(x => x.ContainingClassName == ContainingClassName))
			throw new Exception("SetFactory instances must have unique names");
		SetFactories.Add(this);

		if (size == 0)
			throw new ArgumentOutOfRangeException("size cannot be 0, the initializer for Count must run first");

		_size = size;
	}

	// Each SetFactory will be re-created on mod reload, so this doesn't need to be called by tModLoader code.
	public void Clear()
	{
		setMetadataMapping.Clear();
	}

	// Copies of existing methods with an additional key parameter.
	/// <summary> <inheritdoc cref="CreateCustomSet"/> <include file = 'CommonDocs.xml' path='Common/CreateNamedXSetNotes' /> </summary>
	public T[] CreateNamedCustomSet<T>(SetKey setKey, T defaultState, params object[] inputs) => RegisterNamedCustomSet(setKey, defaultState, CreateCustomSet(defaultState, inputs));
	/// <summary> <inheritdoc cref="CreateFloatSet"/> <include file = 'CommonDocs.xml' path='Common/CreateNamedXSetNotes' /> </summary>
	public float[] CreateNamedFloatSet(SetKey setKey, float defaultState, params float[] inputs) => RegisterNamedCustomSet(setKey, defaultState, CreateFloatSet(defaultState, inputs));
	/// <summary> <include file = 'CommonDocs.xml' path='Common/CreateNamedXSetNotes' /> </summary>
	public ushort[] CreateNamedUshortSet(SetKey setKey, ushort defaultState, params ushort[] inputs) => RegisterNamedCustomSet(setKey, defaultState, CreateUshortSet(defaultState, inputs));
	/// <summary> <inheritdoc cref="CreateIntSet(int, int[])"/> <include file = 'CommonDocs.xml' path='Common/CreateNamedXSetNotes' /> </summary>
	public int[] CreateNamedIntSet(SetKey setKey, int defaultState, params int[] inputs) => RegisterNamedCustomSet(setKey, defaultState, CreateIntSet(defaultState, inputs));
	/// <summary> <inheritdoc cref="CreateIntSet(int[])"/> <include file = 'CommonDocs.xml' path='Common/CreateNamedXSetNotes' /> </summary>
	public int[] CreateNamedIntSet(SetKey setKey, params int[] types) => RegisterNamedCustomSet(setKey, -1, CreateIntSet(types));
	/// <summary> <inheritdoc cref="CreateBoolSet(int[])"/> <include file = 'CommonDocs.xml' path='Common/CreateNamedXSetNotes' /> </summary>
	public bool[] CreateNamedBoolSet(SetKey setKey, params int[] types) => RegisterNamedCustomSet(setKey, false, CreateBoolSet(false, types));
	/// <summary> <inheritdoc cref="CreateBoolSet(bool, int[])"/> <include file = 'CommonDocs.xml' path='Common/CreateNamedXSetNotes' /> </summary>
	public bool[] CreateNamedBoolSet(SetKey setKey, bool defaultState, params int[] types) => RegisterNamedCustomSet(setKey, defaultState, CreateBoolSet(defaultState, types));

	// This is private to prevent potential modder mistake of not using the return value.
	private T[] RegisterNamedCustomSet<T>(SetKey setKey, T defaultValue, T[] input)
	{
		RegisterNamedCustomSet(setKey, defaultValue, ref input);
		return input;
	}

	/// <summary>
	/// Manually registers a named ID set. This is typically done through the Terraria.ID.XID.Sets.Factory.CreateNamedXSet methods, but this method can be used for manually initialized arrays.
	/// <para/> The set reference passed in might be changed by this method when merging with existing data.
	/// <para/> Throws an exception if the data length or default value does not match a named ID set with the same key registered before this.
	/// </summary>
	/// <remarks> <include file = 'CommonDocs.xml' path='Common/CreateNamedXSetFinalKeyC' /> </remarks>
	public void RegisterNamedCustomSet<T>(SetKey setKey, T defaultValue, ref T[] input)
	{
		string key = setKey.fullKey;
		string additionalInfo = setKey.additionalInfo;

		// Modders are free to collaborate "globally" by using "Terraria" as the mod name if they wish.
		if (!key.Contains("/")) {
			key = $"{ModContent.CurrentlyLoadingMod}/{key}";
		}

		// If sets with different names are to be merged, find the actual key, which will be the 1st alternate name registered with MergeSets().
		string keyChangedHint = "";
		if (MergedSets.TryGetValue(new SetFactoryTypeTypePair(ContainingClassName, typeof(T)), out List<HashSet<string>> registeredSets)) {
			var matchingSet = registeredSets.FirstOrDefault(x => x.Contains(key));
			if (matchingSet != null) {
				string newKey = matchingSet.OrderBy(x => x).First();
				if (newKey != key) {
					keyChangedHint = $" (originally '{key}')"; // Logs might be confusing without this.
					key = newKey;
				}
			}
		}

		// Could make a ModLoader.loadStage enum or another bool, but this behaves exactly how we want anyway.
		if (!ContentCache.contentLoadingFinished) {
			// If a set is initialized early, throw an error if the class containing the set doesn't have ReinitializeDuringResizeArrays
			bool willBeReinitialized = new StackTrace().GetFrames().Any(frame => frame.GetMethod()?.DeclaringType?.GetAttribute<ReinitializeDuringResizeArraysAttribute>() != null);
			if (!willBeReinitialized)
				throw new Exception($"Custom sets must be initialized from a class with the ReinitializeDuringResizeArrays attribute. This ensures that all content has been registered and that the custom set will have the correct length");
		}

		// Note: Intended to be load order independent as long as all parties agree on default value. Any deviation will throw exception.
		SetMetadata newMetadata = new SetMetadata(defaultValue, input);
		SetNameTypePair dictionaryKey = new SetNameTypePair(key, typeof(T));
		SetMetadata existingMetadata = setMetadataMapping.GetOrAdd(dictionaryKey, newMetadata);

		if (!EqualityComparer<object>.Default.Equals(newMetadata.defaultValue, existingMetadata.defaultValue)) { // Primitive might be boxed, so != doesn't work.
			throw new Exception($"Previously registered named ID set in {ContainingClassName} named '{key}'{keyChangedHint} has a default value of '{existingMetadata.defaultValue ?? "null"}' provided by the mod(s) [{string.Join(", ", existingMetadata.involvedMods)}] but '{newMetadata.defaultValue ?? "null"}' was supplied by '{ModContent.CurrentlyLoadingMod}'. This named ID set can not be registered.\n\nIf you are the developer of this mod, please visit https://github.com/tModLoader/tModLoader/wiki/Named-ID-Sets to see how existing mods are using named ID sets and adjust accordingly.");
		}

		T[] value = (T[])existingMetadata.array;

		// If it already exists, merge the data
		if (value != input) {
			if (value.Length != input.Length) {
				throw new Exception("Input set and existing set are of different lengths.");
				// This could potentially happen for willBeReinitialized sets if the modder makes the array manually for the current content count instead of using the SetFactory as intended.
			}

			bool anyChanges = false;
			// To merge, we find entries in the input that aren't defaultValue and assign them to the result.
			// Existing changes should persist as long as mods agree on the defaultValue passed in and used in CreateXSet
			// For conflicts, mods loading after will have final say.
			for (int i = 0; i < input.Length; i++) {
				if (!EqualityComparer<T>.Default.Equals(input[i], defaultValue)) {
					if (!EqualityComparer<T>.Default.Equals(input[i], value[i])) {
						anyChanges = true;
					}
					value[i] = input[i];
				}
			}

			// TODO: This code will run currently for all sets due to duplicate static initializer issue.
			if (anyChanges && ModCompile.activelyModding)
				Logging.tML.Info($"Custom Set '{key}'{keyChangedHint} (Type: {typeof(T).Name}, SetFactory: {ContainingClassName}) is merging with additional data from '{ModContent.CurrentlyLoadingMod}'. It previously had data from [{string.Join(", ", existingMetadata.involvedMods)}]");
		}

		// We need to trach which SetFactory, the set name/Type/default value, metadata strings from each mod for each set, and the list of mods using each set.
		existingMetadata.involvedMods.Add(ModContent.CurrentlyLoadingMod);
		if (!string.IsNullOrWhiteSpace(additionalInfo)) {
			existingMetadata.setDescriptions[ModContent.CurrentlyLoadingMod] = additionalInfo;
		}

		input = value;
	}

	internal string CustomMetadataInfo(string setKey, bool printValues)
	{
		var sb = new StringBuilder();
		if (setKey != null) {
			if (setKey.Contains("/")) {
				var specificSet = setMetadataMapping.FirstOrDefault(x => x.Key.setName.Equals(setKey, StringComparison.OrdinalIgnoreCase));
				if (specificSet.Key != null) {
					OutputText(sb, specificSet.Key, specificSet.Value);
				}
			}
			else {
				// If no '/', setKey is mod name
				foreach (var (key, value) in setMetadataMapping) {
					if (value.involvedMods.Contains(setKey) || key.setName.StartsWith($"{setKey}/", StringComparison.OrdinalIgnoreCase)) {
						OutputText(sb, key, value);
					}
				}
			}
		}
		else {
			// Return all involved mods, all descriptions, all types and names
			foreach (var (key, value) in setMetadataMapping) {
				OutputText(sb, key, value);
			}
		}
		return sb.ToString();

		void OutputText(StringBuilder sb, SetNameTypePair setNameTypePair, SetMetadata metadata)
		{
			string setName = ContainingClassName ?? this.GetType().FullName;

			sb.AppendLine($"{setName}, \"{setNameTypePair.setName}\", {setNameTypePair.type.Name}, default value {metadata.defaultValue ?? "null"}");
			if (metadata.involvedMods != null)
				sb.AppendLine($"\tUsed by: {string.Join(", ", metadata.involvedMods)}");
			if (metadata.setDescriptions?.Any() == true) {
				var lines = metadata.setDescriptions.Select(x => $"\t\t{x.Key}: {x.Value}");
				sb.AppendLine($"\tAdditional Info:\n{string.Join("\n", lines)}");
			}
			if (MergedSets.TryGetValue(new SetFactoryTypeTypePair(ContainingClassName, setNameTypePair.type), out List<HashSet<string>> registeredSets)) {
				var matchingSet = registeredSets.FirstOrDefault(x => x.Contains(setNameTypePair.setName));
				if (matchingSet != null) {
					sb.AppendLine($"\tMerged Set Names: {string.Join(", ", matchingSet)}");
				}
			}
			if (printValues) {
				// Some SetFactory might not have a corresponding idDictionary
				var array = (metadata.array as Array).Cast<object>().ToArray();
				var nonDefault = array.Select((x, i) => (i, x)).Where(pair => !EqualityComparer<object>.Default.Equals(metadata.defaultValue, pair.x)).Select(pair => $"[{(search?.TryGetName(pair.i, out string name) == true ? name : pair.i)}, {pair.x ?? "null"}]");
				sb.AppendLine($"\tNon-default value: {string.Join(", ", nonDefault)}");
			}
		}
	}
}
