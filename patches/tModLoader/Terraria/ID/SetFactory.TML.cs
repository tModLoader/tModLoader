using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using ReLogic.Reflection;
using ReLogic.Utilities;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;

namespace Terraria.ID;

/// <summary>
/// SetFactory is responsible for creating "custom ID sets" for content. "Custom ID sets" refers to arrays indexed by content ids. The ID set contains data applying to all instances of content of a specific type. This is typically metadata or data controlling how code will interact with each type of content. Each vanilla ID class contains a SetFactory instance called "Factory" which is used to initialize the ID sets contained within the ID class.
/// <para/> For example <see cref="ItemID.Sets.Factory"/> is used to initialize <see cref="ItemID.Sets.IsFood"/> with true values for food items such as <see cref="ItemID.PadThai"/>. Modded content updates ID sets in <see cref="ModType.SetStaticDefaults"/>: <c>ItemID.Sets.IsFood[Type] = true;</c>. Code in tModLoader and individual mods might consult the data in <see cref="ItemID.Sets.IsFood"/> for whatever purpose they want.
/// <para/> Mods can make their own custom ID sets through the methods of this class. The <see cref="CreateNamedSet(string)"/> methods create custom ID sets that facilitate collaborative "named ID sets". Mods using the same "named ID set" will share a reference to the same array merging together all the entries and changes. More information can be found in the <see href="https://github.com/tModLoader/tModLoader/pull/4381">Custom and Named ID Sets pull request</see>.
/// </summary>
public partial class SetFactory
{
	/// <summary>
	/// Used to construct the key for this "named ID set". Must be chained with a <c>RegisterXSet</c> method to create and register the set for sharing.
	/// </summary>
	public class NamedSetKey
	{
		private readonly SetFactory factory;
		internal readonly string fullKey;
		internal string description;

		internal NamedSetKey(SetFactory factory, string fullKey)
		{
			this.factory = factory;

			// Modders are free to collaborate "globally" by using "Terraria" as the mod name if they wish.
			if (!fullKey.Contains('/')) {
				fullKey = $"{ModContent.CurrentlyLoadingMod}/{fullKey}";
			}

			if (fullKey.Contains(' '))
				throw new ArgumentException("Set names may not contain spaces");

			this.fullKey = fullKey;
		}

		internal NamedSetKey(SetFactory factory, string modName, string key) : this(factory, $"{modName}/{key}") { }
		internal NamedSetKey(SetFactory factory, Mod mod, string key) : this(factory, mod.Name, key) { }

		/// <summary>
		/// Adds a description to this named ID set.
		/// <para/> This description serves to communicate to other mod makers interested in interfacing with this set what the entries in the set mean and what your mod does with entries in the set. Multiple mods can register a description and they will all be available to view. Modders can use the "/customsets" chat command to output a complete listing of descriptions for all named ID sets to "CustomSets.txt" in the logs directory.
		/// </summary>
		public NamedSetKey Description(string description)
		{
			this.description = description;
			return this;
		}

		/// <summary> <inheritdoc cref="CreateCustomSet"/> <include file = 'CommonDocs.xml' path='Common/RegisterXSetNotes' /> </summary>
		public T[] RegisterCustomSet<T>(T defaultState, params object[] inputs) => factory.RegisterNamedCustomSet(this, defaultState, factory.CreateCustomSet(defaultState, inputs));

		/// <summary> <inheritdoc cref="CreateFloatSet"/> <include file = 'CommonDocs.xml' path='Common/RegisterXSetNotes' /> </summary>
		public float[] RegisterFloatSet(float defaultState, params float[] inputs) => factory.RegisterNamedCustomSet(this, defaultState, factory.CreateFloatSet(defaultState, inputs));

		/// <summary> <inheritdoc cref="CreateUshortSet(ushort, ushort[])"/> <include file = 'CommonDocs.xml' path='Common/RegisterXSetNotes' /> </summary>
		public ushort[] RegisterUshortSet(ushort defaultState, params ushort[] inputs) => factory.RegisterNamedCustomSet(this, defaultState, factory.CreateUshortSet(defaultState, inputs));

		/// <summary> <inheritdoc cref="CreateIntSet(int, int[])"/> <include file = 'CommonDocs.xml' path='Common/RegisterXSetNotes' /> </summary>
		public int[] RegisterIntSet(int defaultState, params int[] inputs) => factory.RegisterNamedCustomSet(this, defaultState, factory.CreateIntSet(defaultState, inputs));

		/// <summary> <inheritdoc cref="CreateIntSet(int[])"/> <include file = 'CommonDocs.xml' path='Common/RegisterXSetNotes' /> </summary>
		public int[] RegisterIntSet(params int[] types) => RegisterIntSet(-1, types);

		/// <summary> <inheritdoc cref="CreateBoolSet(int[])"/> <include file = 'CommonDocs.xml' path='Common/RegisterXSetNotes' /> </summary>
		public bool[] RegisterBoolSet(params int[] types) => RegisterBoolSet(false, types);

		/// <summary> <inheritdoc cref="CreateBoolSet(bool, int[])"/> <include file = 'CommonDocs.xml' path='Common/RegisterXSetNotes' /> </summary>
		public bool[] RegisterBoolSet(bool defaultState, params int[] types) => factory.RegisterNamedCustomSet(this, defaultState, factory.CreateBoolSet(defaultState, types));
	}

	private abstract class SetMetadata
	{
		public readonly string name;
		public readonly Type type;

		public readonly HashSet<string> registeredNames = [];
		public readonly HashSet<string> involvedMods = [];
		public readonly Dictionary<string, string> descriptions = [];

		protected SetMetadata(string name, Type type)
		{
			this.name = name;
			this.type = type;
		}

		public abstract object DefaultValue { get; }
		public abstract IEnumerable<(int i, object v)> EnumerateNonDefaultValues();
	}

	private class SetMetadata<T> : SetMetadata
	{
		public readonly T defaultValue;
		public readonly T[] array;

		public SetMetadata(string name, Type type, T defaultValue, T[] array) : base(name, type)
		{
			this.defaultValue = defaultValue;
			this.array = array;
		}

		public override object DefaultValue => defaultValue;
		public override IEnumerable<(int, object)> EnumerateNonDefaultValues()
		{
			for (int i = 0; i < array.Length; i++) {
				var v = array[i];
				if (!EqualityComparer<T>.Default.Equals(defaultValue, v))
					yield return (i, v);
			}
		}

		public override int GetHashCode() => defaultValue.GetHashCode() ^ array.GetHashCode();

		public override bool Equals(object obj) => obj is SetMetadata<T> metadata && EqualityComparer<T>.Default.Equals(defaultValue, metadata.defaultValue) && array == metadata.array;
	}

	// Contains all SetFactory instances.
	internal static List<SetFactory> SetFactories = new();

	internal record SetFactoryNameTypePair(string setFactoryName, Type type);

	// This is static since SetFactory instances are reset during ResizeArrays. Default value issues will be detected during RegisterNamedCustomSetWithInfo.
	internal static ConcurrentDictionary<SetFactoryNameTypePair, Dictionary<string, HashSet<string>>> MergedSets = new();

	/// <summary>
	/// Causes sets registered with the provided keys (and matching SetFactory and Type) to be merged as if they are registered with the same key. This is useful for situations where established set keys are determined to have identical meaning but the involved mods are incapable of updating to collaborate on the shared key, either due to dependent mods or inactivity.
	/// <para/> Essentially, the sets will be merged and share the same data. The default value must still be consistent between the sets.
	/// <para/> This must be called before the ResizeArrays stage of mod loading, such as in a Load method.
	/// </summary>
	public void MergeNamedSets<T>(params string[] inputSetNames)
	{
		if (ContentCache.contentLoadingFinished)
			throw new Exception("MergeSets can only be called before sets are initialized, such as in Load.");

		if (inputSetNames == null || inputSetNames.Length <= 1)
			return;

		var mergeRegistry = MergedSets.GetOrAdd(new SetFactoryNameTypePair(ContainingClassName, typeof(T)), _ => new());
		var merged = inputSetNames.ToHashSet();
		var requestedMerge = string.Join(", ", merged);

		foreach (var name in inputSetNames)
			if (mergeRegistry.TryGetValue(name, out var existingMerge))
				merged.UnionWith(existingMerge);

		foreach (var name in merged)
			mergeRegistry[name] = merged;

		var finalMerge = string.Join(", ", merged);
		var log = $"Custom {ContainingClassName} {FullTypeName(typeof(T))}[] sets {{{requestedMerge}}} merged by {ModContent.CurrentlyLoadingMod}.";
		if (finalMerge != requestedMerge)
			log += $" Result: {{{finalMerge}}}";

		Logging.tML.Debug(log);
	}

	public static void ResizeArrays(bool unloading)
	{
		SetFactories.Clear();
		if (unloading)
			MergedSets = new(); // SetFactory.MergedSets.Clear() crashes the game for some reason?
	}

	private readonly string ContainingClassName;
	private readonly Func<int, string> GetName;
	private readonly ConcurrentDictionary<(string name, Type type), SetMetadata> namedSets = new();

	public SetFactory(int size, string idClassName, IdDictionary search) : this(size, idClassName, id => search.TryGetName(id, out var name) ? name : null) { }

	public SetFactory(int size, string idClassName, Func<int, string> getName = null)
	{
		ArgumentNullException.ThrowIfNull(idClassName);

		ContainingClassName = idClassName;
		GetName = getName;

		if (SetFactories.Any(x => x.ContainingClassName == ContainingClassName))
			throw new Exception("SetFactory instances must have unique names");

		SetFactories.Add(this);

		// Note: The original SetFactory method threw an exception if size is 0. This restriction has been removed since it is likely that modded usages might have 0 if no content is loaded.

		_size = size;
	}

	// Each SetFactory will be re-created on mod reload, so this doesn't need to be called by tModLoader code.
	public void Clear()
	{
		namedSets.Clear();
	}

	internal string FullTypeName(Type type) => type.IsGenericType ? $"{type.Name}({string.Join(", ", type.GenericTypeArguments.Select(x => x.Name))})" : type.Name;

	/// <summary>
	/// <include file = 'CommonDocs.xml' path='Common/CreateNamedXSetNotes' />
	/// <para/> The final key for this named ID set using this overload will be <c>"{key}"</c> directly if it contains a "/". Otherwise, the final key will be derived automatically from the currently loading mod: <c>"{loadingMod.Name}/{key}"</c>
	/// </summary>
	public NamedSetKey CreateNamedSet(string fullKey) => new NamedSetKey(this, fullKey);
	/// <summary>
	/// <include file = 'CommonDocs.xml' path='Common/CreateNamedXSetNotes' />
	/// <para/> The final key for this named ID set using this overload will be: <c>"{modName}/{key}"</c>
	/// </summary>
	public NamedSetKey CreateNamedSet(string modName, string key) => new NamedSetKey(this, modName, key);
	/// <summary>
	/// <include file = 'CommonDocs.xml' path='Common/CreateNamedXSetNotes' />
	/// <para/> The final key for this named ID set using this overload will be: <c>"{mod.Name}/{key}"</c>
	/// <see cref="CreateNamedSet(string)"/>
	/// </summary>
	public NamedSetKey CreateNamedSet(Mod mod, string key) => new NamedSetKey(this, mod, key);

	// This is private to prevent potential modder mistake of not using the return value.
	private T[] RegisterNamedCustomSet<T>(NamedSetKey setKey, T defaultValue, T[] input)
	{
		RegisterNamedCustomSet(setKey, defaultValue, ref input);
		return input;
	}

	/// <summary>
	/// Manually registers a named ID set. This is typically done through the <c>Terraria.ID.XID.Sets.Factory.CreateNamedSet().RegisterXSet()</c> methods, but this method can be used for manually initialized arrays.
	/// <para/> The set reference passed in might be changed by this method when merging with existing data.
	/// <para/> Throws an exception if the data length or default value does not match a named ID set with the same key registered before this.
	/// </summary>
	/// <remarks> <include file = 'CommonDocs.xml' path='Common/CreateNamedXSetFinalKeyC' /> </remarks>
	public void RegisterNamedCustomSet<T>(NamedSetKey setKey, T defaultValue, ref T[] input)
	{
		if (ContainingClassName == null)
			throw new ArgumentException("Cannot register named sets on a SetFactory whith no name (using the obsolete constructor)");

		string unmergedKey = setKey.fullKey;
		string key = setKey.fullKey;
		string description = setKey.description;

		if (!ContentCache.contentLoadingFinished) {
			static bool IsReinitArraysCctor(MethodBase method) => method != null && method.MemberType == MemberTypes.Constructor && method.IsStatic && method.DeclaringType?.GetAttribute<ReinitializeDuringResizeArraysAttribute>() != null;
			bool willBeReinitialized = new StackTrace().GetFrames().Any(frame => IsReinitArraysCctor(frame.GetMethod()));
			if (!willBeReinitialized)
				throw new Exception($"Custom sets cannot be initialized during Load phase, except via the static constructor of a class with [ReinitializeDuringResizeArrays]." +
					$"\r\nThis ensures that all content has been registered and that the custom set will have the correct length");

			return; // too early to be doing anything
		}

		if (MergedSets.TryGetValue(new(ContainingClassName, typeof(T)), out var mergeRegistry) && mergeRegistry.TryGetValue(key, out var merged)) {
			key = $"{{{string.Join(", ", merged)}}}";
		}

		var originalInput = input;
		var metadata = (SetMetadata<T>)namedSets.GetOrAdd((key, typeof(T)), _ => new SetMetadata<T>(key, typeof(T), defaultValue, originalInput));

		if (!EqualityComparer<T>.Default.Equals(defaultValue, metadata.defaultValue)) {
			string conflictMergeInfo = "";
			if (metadata.registeredNames.Any() && !metadata.registeredNames.Contains(unmergedKey))
				conflictMergeInfo = $"\nThis set has been merged with [{string.Join(", ", metadata.registeredNames)}]. Check the logs to see if another mod is causing this conflict.";

			throw new Exception(
				$"Failed to register {ContainingClassName} {FullTypeName(typeof(T))}[] set {unmergedKey}. Default value ({(object)defaultValue ?? "null"}) conflicts with existing default value ({metadata.DefaultValue ?? "null"}) registered by the mod(s) [{string.Join(", ", metadata.involvedMods)}]" +
				conflictMergeInfo +
				$"\n\nIf you are the developer of this mod, please visit https://github.com/tModLoader/tModLoader/wiki/Named-ID-Sets to see how existing mods are using named ID sets and adjust accordingly.");
		}

		// If it already exists, merge the data
		var value = metadata.array;
		if (value != input) {
			if (value.Length != input.Length) {
				throw new Exception("Input set and existing set are of different lengths.");
				// This could potentially happen for willBeReinitialized sets if the modder makes the array manually for the current content count instead of using the SetFactory as intended.
			}

			for (int i = 0; i < input.Length; i++) {
				if (!EqualityComparer<T>.Default.Equals(input[i], defaultValue)) {
					value[i] = input[i];
				}
			}

			// Detecting new and replaced entries for logging has been removed since it could be misleading since the entries will be updated in SetStaticDefaults anyway.
			if (ModCompile.activelyModding && !metadata.involvedMods.Contains(ModContent.CurrentlyLoadingMod)) {
				var log = $"Custom {ContainingClassName} {FullTypeName(typeof(T))}[] set {key} registered by {ModContent.CurrentlyLoadingMod}.";
				log += $" Previously registered by [{string.Join(", ", metadata.involvedMods)}]";
				Logging.tML.Debug(log);
			}
		}

		// We need to trach which SetFactory, the set name/Type/default value, metadata strings from each mod for each set, and the list of mods using each set.
		metadata.registeredNames.Add(unmergedKey);
		metadata.involvedMods.Add(ModContent.CurrentlyLoadingMod);
		if (!string.IsNullOrWhiteSpace(description)) {
			metadata.descriptions[ModContent.CurrentlyLoadingMod] = description;
		}

		input = value;
	}

	internal string CustomMetadataInfo(string searchTerm, bool printValues)
	{
		var sb = new StringBuilder();
		IEnumerable<SetMetadata> sets = namedSets.Values.OrderBy(x => x.name);

		if (searchTerm != null) {
			sets = sets.Where(set => set.involvedMods.Any(s => s.Equals(searchTerm, StringComparison.OrdinalIgnoreCase)) || set.name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
		}

		// Return all involved mods, all descriptions, all types and names
		foreach (var set in sets) {
			OutputText(sb, set);
		}

		return sb.ToString();

		void OutputText(StringBuilder sb, SetMetadata set)
		{
			sb.AppendLine($"{ContainingClassName} {FullTypeName(set.type)}[] {set.name}");
			sb.AppendLine($"\tDefault Value: {set.DefaultValue ?? "null"}");
			sb.AppendLine($"\tUsed by: {string.Join(", ", set.involvedMods)}");

			if (set.descriptions.Count == 1) {
				sb.AppendLine($"\tDescription: {set.descriptions.Values.Single()}");
			}
			else if (set.descriptions.Count() > 1) {
				var lines = set.descriptions.Select(x => $"\t\t{x.Key}: {x.Value}");
				sb.AppendLine($"\tDescriptions:\n{string.Join("\n", lines)}");
			}

			if (printValues) {
				sb.AppendLine($"\tContents:");
				if (set.type == typeof(bool)) {
					foreach (var (i, v) in set.EnumerateNonDefaultValues())
						sb.AppendLine($"\t\t{GetName?.Invoke(i) ?? i.ToString()}");
				}
				else {
					foreach (var (i, v) in set.EnumerateNonDefaultValues())
						sb.AppendLine($"\t\t{GetName?.Invoke(i) ?? i.ToString()}, {v ?? "null"}");
				}

				sb.AppendLine();
			}
		}
	}
}
