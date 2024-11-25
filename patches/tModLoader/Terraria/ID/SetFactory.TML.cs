using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ReLogic.Utilities;
using Terraria.ModLoader;

namespace Terraria.ID;

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
	public T[] CreateNamedCustomSet<T>(string key, T defaultState, params object[] inputs) => RegisterNamedCustomSet(key, defaultState, CreateCustomSet(defaultState, inputs));
	public float[] CreateNamedFloatSet(string key, float defaultState, params float[] inputs) => RegisterNamedCustomSet(key, defaultState, CreateFloatSet(defaultState, inputs));
	public ushort[] CreateNamedUshortSet(string key, ushort defaultState, params ushort[] inputs) => RegisterNamedCustomSet(key, defaultState, CreateUshortSet(defaultState, inputs));
	public int[] CreateNamedIntSet(string key, int defaultState, params int[] inputs) => RegisterNamedCustomSet(key, defaultState, CreateIntSet(defaultState, inputs));
	public int[] CreateNamedIntSet(string key, params int[] types) => RegisterNamedCustomSet(key, -1, CreateIntSet(types));
	public bool[] CreateNamedBoolSet(string key, params int[] types) => RegisterNamedCustomSet(key, false, CreateBoolSet(false, types));
	public bool[] CreateNamedBoolSet(string key, bool defaultState, params int[] types) => RegisterNamedCustomSet(key, defaultState, CreateBoolSet(defaultState, types));

	// modName + key overloads
	public T[] CreateNamedCustomSet<T>(string modName, string key, T defaultState, params object[] inputs) => CreateNamedCustomSet<T>($"{modName}/{key}", defaultState, inputs);
	public float[] CreateNamedFloatSet(string modName, string key, float defaultState, params float[] inputs) => CreateNamedFloatSet($"{modName}/{key}", defaultState, inputs);
	public ushort[] CreateNamedUshortSet(string modName, string key, ushort defaultState, params ushort[] inputs) => CreateNamedUshortSet($"{modName}/{key}", defaultState, inputs);
	public int[] CreateNamedIntSet(string modName, string key, int defaultState, params int[] inputs) => CreateNamedIntSet($"{modName}/{key}", defaultState, inputs);
	public int[] CreateNamedIntSet(string modName, string key, params int[] types) => CreateNamedIntSet($"{modName}/{key}", -1, types);
	public bool[] CreateNamedBoolSet(string modName, string key, params int[] types) => CreateNamedBoolSet($"{modName}/{key}", false, types);
	public bool[] CreateNamedBoolSet(string modName, string key, bool defaultState, params int[] types) => CreateNamedBoolSet($"{modName}/{key}", defaultState, types);

	// Mod + key overloads
	public T[] CreateNamedCustomSet<T>(Mod mod, string key, T defaultState, params object[] inputs) => CreateNamedCustomSet<T>($"{mod.Name}/{key}", defaultState, inputs);
	public float[] CreateNamedFloatSet(Mod mod, string key, float defaultState, params float[] inputs) => CreateNamedFloatSet($"{mod.Name}/{key}", defaultState, inputs);
	public ushort[] CreateNamedUshortSet(Mod mod, string key, ushort defaultState, params ushort[] inputs) => CreateNamedUshortSet($"{mod.Name}/{key}", defaultState, inputs);
	public int[] CreateNamedIntSet(Mod mod, string key, int defaultState, params int[] inputs) => CreateNamedIntSet($"{mod.Name}/{key}", defaultState, inputs);
	public int[] CreateNamedIntSet(Mod mod, string key, params int[] types) => CreateNamedIntSet($"{mod.Name}/{key}", -1, types);
	public bool[] CreateNamedBoolSet(Mod mod, string key, params int[] types) => CreateNamedBoolSet($"{mod.Name}/{key}", false, types);
	public bool[] CreateNamedBoolSet(Mod mod, string key, bool defaultState, params int[] types) => CreateNamedBoolSet($"{mod.Name}/{key}", defaultState, types);

	// This is private to prevent potential modder mistake.
	private T[] RegisterNamedCustomSet<T>(string key, T defaultValue, T[] input)
	{
		RegisterNamedCustomSet(key, defaultValue, ref input);
		return input;
	}

	/// <summary>
	/// Registers a custom "set", meaning an array of values of length equal to the count of the content the set corresponds to. This is typically done through the Terraria.ID.XID.Sets.Factory.CreateXSet method.
	/// <para/> The set reference passed in may change as a result of this method. This method will merge sets together regardless of mod load order, allowing for ad-hoc collaboration. Note that this merge behavior is dependent on mods agreeing on key and default value. It is important that set names are unique, so it is good practice to include the entity name in the set name to avoid mods accidentally using the same name for different things. For example, a set named "Acidic" might be used by one mod to describe projectiles and another mod to describe items. Sets representing mod-specific ideas should prepend the key with the mod name to ensure a unique key that will not be used by any other mod: "ExampleMod/Jiggly"
	/// <para/> Throws an exception if the Type, data length, or default value does not match the data registered using the same key by any mod loaded before this mod.
	/// </summary>
	public void RegisterNamedCustomSet<T>(string key, T defaultValue, ref T[] input)
	{
		// TODO: Return bool to represent already exists or merged?
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
			throw new Exception($"Previously registered set for {key} has a default value of {existingMetadata.defaultValue} but {newMetadata.defaultValue} was supplied. Custom data set will not be registered");
			// TODO: We could just allow this and output a warning that the data will not be shared. Keep both somehow?
		}

		// TODO: Once DataInstance feature is merged, Custom sets can be registered as (SetFactory, key) in DataInstance.
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
	/// <inheritdoc cref="RegisterNamedCustomSet{T}(string, T, ref T[])"/>
	/// <para/> This particular overload will result in a final key constructed from the provided <paramref name="modName"/> and <paramref name="key"/>: "{modName}/{key}".
	/// </summary>
	public void RegisterNamedCustomSet<T>(string modName, string key, T defaultValue, ref T[] input) => RegisterNamedCustomSet($"{modName}/{key}", defaultValue, ref input);

	/// <summary>
	/// <inheritdoc cref="RegisterNamedCustomSet{T}(string, T, ref T[])"/>
	/// <para/> This particular overload will result in a final key constructed from the provided <paramref name="mod"/> and <paramref name="key"/>: "{mod.Name}/{key}".
	/// </summary>
	public void RegisterNamedCustomSet<T>(Mod mod, string key, T defaultValue, ref T[] input) => RegisterNamedCustomSet($"{mod.Name}/{key}", defaultValue, ref input);
}
