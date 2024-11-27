using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ReLogic.Utilities;
using Terraria.ModLoader;

namespace Terraria.ID;

/// <summary>
/// SetFactory is responsible for creating "custom ID sets" for content. "Custom ID sets" refers to arrays indexed by content ids. The ID set contains data applying to all instances of content of a specific type. This is typically metadata or data controlling how code will interact with each type of content. Each vanilla ID class contains a SetFactory instance called "Factory" which is used to initialize the ID sets contained within the ID class.
/// <para/> For example <see cref="ItemID.Sets.Factory"/> is used to initialize <see cref="ItemID.Sets.IsFood"/> with true values for food items such as <see cref="ItemID.PadThai"/>. Modded content updates ID sets in <see cref="ModType.SetStaticDefaults"/>: <c>ItemID.Sets.IsFood[Type] = true;</c>. Code in tModLoader and individual mods might consult the data in <see cref="ItemID.Sets.IsFood"/> for whatever purpose they want.
/// <para/> Mods can make their own custom ID sets through the methods of this class. The methods with "Named" in their method name facilitate collaborative "named ID sets". Mods using the same "named ID set" will share a reference to the same array merging together all the entries and changes. More information can be found in the <see href="https://github.com/tModLoader/tModLoader/pull/4381">Custom and Named ID Sets pull request</see>.
/// </summary>
public partial class SetFactory
{
	// Additional code to support named custom sets for ad-hoc collaboration
	private record SetMetadata(object defaultValue, object array);

	private ConcurrentDictionary<(string, Type), SetMetadata> setMetadataMapping = new ConcurrentDictionary<(string, Type), SetMetadata>();

	// Each SetFactory will be re-created on mod reload, so this doesn't need to be called by tModLoader code.
	public void Clear()
	{
		setMetadataMapping.Clear();
	}

	// Copies of existing methods with an additional key parameter.
	/// <summary> <inheritdoc cref="CreateCustomSet"/> <include file = 'CommonDocs.xml' path='Common/CreateNamedXSetNotes' /> </summary>
	public T[] CreateNamedCustomSet<T>(string key, T defaultState, params object[] inputs) => RegisterNamedCustomSet(key, defaultState, CreateCustomSet(defaultState, inputs));
	/// <summary> <inheritdoc cref="CreateFloatSet"/> <include file = 'CommonDocs.xml' path='Common/CreateNamedXSetNotes' /> </summary>
	public float[] CreateNamedFloatSet(string key, float defaultState, params float[] inputs) => RegisterNamedCustomSet(key, defaultState, CreateFloatSet(defaultState, inputs));
	public ushort[] CreateNamedUshortSet(string key, ushort defaultState, params ushort[] inputs) => RegisterNamedCustomSet(key, defaultState, CreateUshortSet(defaultState, inputs));
	/// <summary> <inheritdoc cref="CreateIntSet(int, int[])"/> <include file = 'CommonDocs.xml' path='Common/CreateNamedXSetNotes' /> </summary>
	public int[] CreateNamedIntSet(string key, int defaultState, params int[] inputs) => RegisterNamedCustomSet(key, defaultState, CreateIntSet(defaultState, inputs));
	/// <summary> <inheritdoc cref="CreateIntSet(int[])"/> <include file = 'CommonDocs.xml' path='Common/CreateNamedXSetNotes' /> </summary>
	public int[] CreateNamedIntSet(string key, params int[] types) => RegisterNamedCustomSet(key, -1, CreateIntSet(types));
	/// <summary> <inheritdoc cref="CreateBoolSet(int[])"/> <include file = 'CommonDocs.xml' path='Common/CreateNamedXSetNotes' /> </summary>
	public bool[] CreateNamedBoolSet(string key, params int[] types) => RegisterNamedCustomSet(key, false, CreateBoolSet(false, types));
	/// <summary> <inheritdoc cref="CreateBoolSet(bool, int[])"/> <include file = 'CommonDocs.xml' path='Common/CreateNamedXSetNotes' /> </summary>
	public bool[] CreateNamedBoolSet(string key, bool defaultState, params int[] types) => RegisterNamedCustomSet(key, defaultState, CreateBoolSet(defaultState, types));

	// modName + key overloads
	/// <summary> <inheritdoc cref="CreateNamedCustomSet{T}(string, T, object[])"/> <include file = 'CommonDocs.xml' path='Common/CreateNamedXSetFinalKeyA' /> </summary>
	public T[] CreateNamedCustomSet<T>(string modName, string key, T defaultState, params object[] inputs) => CreateNamedCustomSet<T>($"{modName}/{key}", defaultState, inputs);
	/// <summary> <inheritdoc cref="CreateNamedFloatSet(string, float, float[])"/> <include file = 'CommonDocs.xml' path='Common/CreateNamedXSetFinalKeyA' /> </summary>
	public float[] CreateNamedFloatSet(string modName, string key, float defaultState, params float[] inputs) => CreateNamedFloatSet($"{modName}/{key}", defaultState, inputs);
	public ushort[] CreateNamedUshortSet(string modName, string key, ushort defaultState, params ushort[] inputs) => CreateNamedUshortSet($"{modName}/{key}", defaultState, inputs);
	/// <summary> <inheritdoc cref="CreateNamedIntSet(string, int, int[])"/> <include file = 'CommonDocs.xml' path='Common/CreateNamedXSetFinalKeyA' /> </summary>
	public int[] CreateNamedIntSet(string modName, string key, int defaultState, params int[] inputs) => CreateNamedIntSet($"{modName}/{key}", defaultState, inputs);
	/// <summary> <inheritdoc cref="CreateNamedIntSet(string, int[])"/> <include file = 'CommonDocs.xml' path='Common/CreateNamedXSetFinalKeyA' /> </summary>
	public int[] CreateNamedIntSet(string modName, string key, params int[] types) => CreateNamedIntSet($"{modName}/{key}", types);
	/// <summary> <inheritdoc cref="CreateNamedBoolSet(string, int[])"/> <include file = 'CommonDocs.xml' path='Common/CreateNamedXSetFinalKeyA' /> </summary>
	public bool[] CreateNamedBoolSet(string modName, string key, params int[] types) => CreateNamedBoolSet($"{modName}/{key}", false, types);
	/// <summary> <inheritdoc cref="CreateNamedBoolSet(string, bool, int[])"/> <include file = 'CommonDocs.xml' path='Common/CreateNamedXSetFinalKeyA' /> </summary>
	public bool[] CreateNamedBoolSet(string modName, string key, bool defaultState, params int[] types) => CreateNamedBoolSet($"{modName}/{key}", defaultState, types);

	// Mod + key overloads
	/// <summary> <inheritdoc cref="CreateNamedCustomSet{T}(string, T, object[])"/> <include file = 'CommonDocs.xml' path='Common/CreateNamedXSetFinalKeyB' /> </summary>
	public T[] CreateNamedCustomSet<T>(Mod mod, string key, T defaultState, params object[] inputs) => CreateNamedCustomSet<T>($"{mod.Name}/{key}", defaultState, inputs);
	/// <summary> <inheritdoc cref="CreateNamedFloatSet(string, float, float[])"/> <include file = 'CommonDocs.xml' path='Common/CreateNamedXSetFinalKeyB' /> </summary>
	public float[] CreateNamedFloatSet(Mod mod, string key, float defaultState, params float[] inputs) => CreateNamedFloatSet($"{mod.Name}/{key}", defaultState, inputs);
	public ushort[] CreateNamedUshortSet(Mod mod, string key, ushort defaultState, params ushort[] inputs) => CreateNamedUshortSet($"{mod.Name}/{key}", defaultState, inputs);
	/// <summary> <inheritdoc cref="CreateNamedIntSet(string, int, int[])"/> <include file = 'CommonDocs.xml' path='Common/CreateNamedXSetFinalKeyB' /> </summary>
	public int[] CreateNamedIntSet(Mod mod, string key, int defaultState, params int[] inputs) => CreateNamedIntSet($"{mod.Name}/{key}", defaultState, inputs);
	/// <summary> <inheritdoc cref="CreateNamedIntSet(string, int[])"/> <include file = 'CommonDocs.xml' path='Common/CreateNamedXSetFinalKeyB' /> </summary>
	public int[] CreateNamedIntSet(Mod mod, string key, params int[] types) => CreateNamedIntSet($"{mod.Name}/{key}", -1, types);
	/// <summary> <inheritdoc cref="CreateNamedBoolSet(string, int[])"/> <include file = 'CommonDocs.xml' path='Common/CreateNamedXSetFinalKeyB' /> </summary>
	public bool[] CreateNamedBoolSet(Mod mod, string key, params int[] types) => CreateNamedBoolSet($"{mod.Name}/{key}", false, types);
	/// <summary> <inheritdoc cref="CreateNamedBoolSet(string, bool, int[])"/> <include file = 'CommonDocs.xml' path='Common/CreateNamedXSetFinalKeyB' /> </summary>
	public bool[] CreateNamedBoolSet(Mod mod, string key, bool defaultState, params int[] types) => CreateNamedBoolSet($"{mod.Name}/{key}", defaultState, types);

	// This is private to prevent potential modder mistake.
	private T[] RegisterNamedCustomSet<T>(string key, T defaultValue, T[] input)
	{
		RegisterNamedCustomSet(key, defaultValue, ref input);
		return input;
	}

	/// <summary>
	/// Manually registers a named ID set. This is typically done through the Terraria.ID.XID.Sets.Factory.CreateNamedXSet methods, but this method can be used for manually initialized arrays.
	/// <para/> The set reference passed in might be changed by this method when merging with existing data.
	/// <para/> Throws an exception if the data length or default value does not match a named ID set with the same key registered before this.
	/// </summary>
	public void RegisterNamedCustomSet<T>(string key, T defaultValue, ref T[] input)
	{
		// Could make a ModLoader.loadStage enum or another bool, but this behaves exactly how we want anyway.
		if (!ContentCache.contentLoadingFinished) {
			// If a set is initialized early, throw an error if the class containing the set doesn't have ReinitializeDuringResizeArrays
			bool willBeReinitialized = new StackTrace().GetFrames().Any(frame => frame.GetMethod()?.DeclaringType?.GetAttribute<ReinitializeDuringResizeArraysAttribute>() != null);
			if (!willBeReinitialized)
				throw new Exception($"Custom sets must be initialized from a class with the ReinitializeDuringResizeArrays attribute. This ensures that all content has been registered and that the custom set will have the correct length");
		}

		// Note: Intended to be load order independent as long as all parties agree on default value. Any deviation will throw exception.
		SetMetadata newMetadata = new SetMetadata(defaultValue, input);
		(string key, Type) dictionaryKey = (key, typeof(T));
		SetMetadata existingMetadata = setMetadataMapping.GetOrAdd(dictionaryKey, newMetadata);

		if (!EqualityComparer<object>.Default.Equals(newMetadata.defaultValue, existingMetadata.defaultValue)) { // Primitive might be boxed, so != doesn't work.
			throw new Exception($"Previously registered named ID set for {key} has a default value of {existingMetadata.defaultValue} but {newMetadata.defaultValue} was supplied. This named ID set can not be registered. Please visit https://github.com/tModLoader/tModLoader/wiki/Named-ID-Sets to see how existing mods are using named ID sets and adjust accordingly.");
		}

		T[] value = (T[])existingMetadata.array;

		// If it already exists, merge the data
		if (value != input) {
			if (value.Length != input.Length) {
				throw new Exception("Input set and existing set are of different lengths.");
				// This could potentially happen for willBeReinitialized sets if the modder makes the array manually for the current content count instead of using the SetFactory as intended.
			}

			// To merge, we find entries in the input that aren't defaultValue and assign them to the result.
			// Existing changes should persist as long as mods agree on the defaultValue passed in and used in CreateXSet
			// For conflicts, mods loading after will have final say.
			for (int i = 0; i < input.Length; i++) {
				if (!EqualityComparer<T>.Default.Equals(input[i], defaultValue)) {
					value[i] = input[i];
				}
			}
		}

		input = value;
	}

	/// <summary>
	/// <inheritdoc cref="RegisterNamedCustomSet{T}(string, T, ref T[])"/> <include file = 'CommonDocs.xml' path='Common/CreateNamedXSetFinalKeyA' />
	/// </summary>
	public void RegisterNamedCustomSet<T>(string modName, string key, T defaultValue, ref T[] input) => RegisterNamedCustomSet($"{modName}/{key}", defaultValue, ref input);

	/// <summary>
	/// <inheritdoc cref="RegisterNamedCustomSet{T}(string, T, ref T[])"/> <include file = 'CommonDocs.xml' path='Common/CreateNamedXSetFinalKeyB' />
	/// </summary>
	public void RegisterNamedCustomSet<T>(Mod mod, string key, T defaultValue, ref T[] input) => RegisterNamedCustomSet($"{mod.Name}/{key}", defaultValue, ref input);
}
