using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace Terraria.ID;

public partial class SetFactory
{
	// Additional code to support named custom sets for ad-hoc collaboration
	private record SetMetadata(Type type, object defaultValue); // Note: Stores the array Type, not Type[]

	private ConcurrentDictionary<(string, Type), SetMetadata> setMetadataMapping = new ConcurrentDictionary<(string, Type), SetMetadata>();

	public void Clear() // call where?
	{
		setMetadataMapping.Clear();
	}

	// Copies of existing methods with an additional key parameter.
	// TODO: CreateNamedCustomSet?
	public T[] CreateNamedCustomSet<T>(string key, T defaultState, params object[] inputs)
	{
		var set = CreateCustomSet(defaultState, inputs);
		RegisterNamedCustomSet(key, defaultState, ref set);
		return set;
	}

	public float[] CreateNamedFloatSet(string key, float defaultState, params float[] inputs)
	{
		var set = CreateFloatSet(defaultState, inputs);
		RegisterNamedCustomSet(key, defaultState, ref set);
		return set;
	}
	public ushort[] CreateNamedUshortSet(string key, ushort defaultState, params ushort[] inputs)
	{
		var set = CreateUshortSet(defaultState, inputs);
		RegisterNamedCustomSet(key, defaultState, ref set);
		return set;
	}
	public int[] CreateNamedIntSet(string key, int defaultState, params int[] inputs)
	{
		var set = CreateIntSet(defaultState, inputs);
		RegisterNamedCustomSet(key, defaultState, ref set);
		return set;
	}
	public int[] CreateNamedIntSet(string key, params int[] types)
	{
		var set = CreateIntSet(types);
		RegisterNamedCustomSet(key, -1, ref set);
		return set;
	}
	public bool[] CreateNamedBoolSet(string key, params int[] types)
	{
		var set = CreateBoolSet(false, types);
		RegisterNamedCustomSet(key, false, ref set);
		return set;
	}
	public bool[] CreateNamedBoolSet(string key, bool defaultState, params int[] types)
	{
		var set = CreateBoolSet(defaultState, types);
		RegisterNamedCustomSet(key, defaultState, ref set);
		return set;
	}

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

	/// <summary>
	/// Registers a custom "set", meaning an array of values of length equal to the count of the content the set corresponds to. This is typically done through the Terraria.ID.XID.Sets.Factory.CreateXSet method.
	/// <para/> The set reference passed in may change as a result of this method. This method will merge sets together regardless of mod load order, allowing for ad-hoc collaboration. Note that this merge behavior is dependent on mods agreeing on key and default value. It is important that set names are unique, so it is good practice to include the entity name in the set name to avoid mods accidentally using the same name for different things. For example, a set named "Acidic" might be used by 1 mod to describe projectiles and another mod to describe items. Sets representing mod-specific ideas should prepend the key with the mod name to ensure a unique key that will not be used by any other mod: "ExampleMod/Jiggly"
	/// <para/> Throws an exception if the Type, data length, or default value does not match the data registered using the same key by any mod loaded before this mod.
	/// </summary>
	public void RegisterNamedCustomSet<T>(string key, T defaultValue, ref T[] input)
	{
		// Could make a ModLoader.loadStage enum or another bool, but this behaves exactly how we want anyway.
		if (false && !ContentCache.contentLoadingFinished)
		{
			// TODO: This throws during initial modded Set class initialization. Ignore sometimes somehow? Or maybe, track a list of keys registered during load and make sure they all reset during ResizeArrays?
			throw new Exception($"Custom sets can only be registered during or after ModSystem.ResizeArrays. This ensures that all content has been registered and that the custom set will have the correct length");
		}

		// TODO: Return bool to represent already exists or merged?
		// TODO: if(loadStage < ResizeArrays) throw new Exception? It's probably always wrong to do it earlier.
		// TODO: We could store defaultValue and throw on mismatch.
		// TODO: Another Generic for Content Type? Item or ItemID for example?

		// Note: Intended to be load order independent as long as all parties agree on default value, Type, and length. Any deviation will throw exception.

		SetMetadata newMetadata = new SetMetadata(typeof(T), defaultValue);
		SetMetadata existingMetadata = setMetadataMapping.GetOrAdd((key, typeof(T)), newMetadata);
		/*if (newMetadata.type != existingMetadata.type)
		{
			throw new Exception($"Previously registered set for {key} is of type {existingMetadata.type} but {newMetadata.type} was supplied. Custom data set will not be registered");
		}*/
		if (!newMetadata.defaultValue.Equals(existingMetadata.defaultValue))
		{ // Primitive might be boxed, so != doesn't work.
			throw new Exception($"Previously registered set for {key} has a default value of {existingMetadata.defaultValue} but {newMetadata.defaultValue} was supplied. Custom data set will not be registered");
			// TODO: just allow this and output a warning? Keep both somehow?
		}
		/*if (newmetadata.length != existingmetadata.length)
		{
			throw new exception($"previously registered set for {key} is has length {existingmetadata.length} but supplied set has length {newmetadata.length}. custom data set will not be registered");
		}*/

		// Custom sets are registered as (SetFactory, key) in DataInstance.
		object entry = DataInstance<T[]>.GetOrAdd((this, key), input);

		if (entry is not T[])
		{
			throw new Exception($"Existing set is not the expected Type {typeof(T)}, but is {entry.GetType()}");
			// This could potentially happen if a modder bypasses SetHandler and registers a set using DataInstance instead. setMetadataMapping checks won't catch that.
		}

		var value = entry as T[];

		// If it already exists, merge the data
		if (value != input)
		{
			if (value.Length != input.Length)
			{
				throw new Exception("Input set and existing set are of different lengths.");
			}

			// To merge, we find entries in the input that aren't defaultValue and assign them to the result.
			// Existing changes should persist as long as mods agree on the defaultValue passed in and used in CreateXSet
			for (int i = 0; i < input.Length; i++)
			{
				if (!input[i].Equals(defaultValue))
				{
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

	/* Error prone method overload? No need.
	public T[] RegisterCustomSet<T>(Mod mod, string key, T defaultValue, T[] input) => RegisterCustomSet($"{mod.Name}/{key}", defaultValue, input);
	*/
}
