using System;
using System.Collections.Concurrent;

namespace Terraria.ModLoader;

/// <summary>
/// Provides access to shared data arrays, ensuring that their Type, default value, and length are consistent. 
/// </summary>
public static class SetHandler
{
	private record SetMetadata(Type type, object defaultValue, int length);

	// store lengths.
	private static ConcurrentDictionary<string, SetMetadata> setMetadataMapping = new ConcurrentDictionary<string, SetMetadata>();

	internal static void Clear()
	{
		setMetadataMapping.Clear();
	}

	/// <summary>
	/// Registers a custom "set", meaning an array of values of length equal to the count of the content the set corresponds to. This is typically done through the Terraria.ID.XID.Sets.Factory.CreateXSet method.
	/// <para/> The set reference passed in may change as a result of this method. This method will merge sets together regardless of mod load order, allowing for ad-hoc collaboration. Note that this merge behavior is dependent on mods agreeing on key and default value. It is important that set names are unique, so it is good practice to include the entity name in the set name to avoid mods accidentally using the same name for different things. For example, a set named "Acidic" might be used by 1 mod to describe projectiles and another mod to describe items. Sets representing mod-specific ideas should prepend the key with the mod name to ensure a unique key that will not be used by any other mod: "ExampleMod/Jiggly"
	/// <para/> Throws an exception if the Type, data length, or default value does not match the data registered using the same key by any mod loaded before this mod.
	/// </summary>
	public static void RegisterCustomSet<T>(string key, T defaultValue, ref T[] input)
	{
		// TODO: Return bool to represent already exists or merged?
		// TODO: if(loadStage < ResizeArrays) throw new Exception? It's probably always wrong to do it earlier.
		// TODO: We could store defaultValue and throw on mismatch.
		// TODO: Another Generic for Content Type? Item or ItemID for example?

		// Note: Intended to be load order independent as long as all parties agree on default value, Type, and length. Any deviation will throw exception.

		SetMetadata newMetadata = new SetMetadata(typeof(T), defaultValue, input.Length);
		SetMetadata existingMetadata = setMetadataMapping.GetOrAdd(key, newMetadata);
		if(newMetadata.type != existingMetadata.type) {
			throw new Exception($"Previously registered set for {key} is of type {existingMetadata.type} but {newMetadata.type} was supplied. Custom data set will not be registered");
		}
		if (!newMetadata.defaultValue.Equals(existingMetadata.defaultValue)) { // Primitive might be boxed, so != doesn't work.
			throw new Exception($"Previously registered set for {key} has a default value of {existingMetadata.defaultValue} but {newMetadata.defaultValue} was supplied. Custom data set will not be registered");
		} 
		if (newMetadata.length != existingMetadata.length) {
			throw new Exception($"Previously registered set for {key} is has length {existingMetadata.length} but supplied set has length {newMetadata.length}. Custom data set will not be registered");
		}

		object entry = DataInstance.GetOrAdd(key, input);

		if (entry is not T[]) {
			throw new Exception($"Existing set is not the expected Type {typeof(T)}, but is {entry.GetType()}");
			// This could potentially happen if a modder bypasses SetHandler and registers a set using DataInstance instead. setMetadataMapping checks won't catch that.
		}

		var value = entry as T[];

		// If it already exists, merge the data
		if (value != input) {
			if (value.Length != input.Length) {
				throw new Exception("Input set and existing set are of different lengths.");
			}

			// To merge, we find entries in the input that aren't defaultValue and assign them to the result.
			// Existing changes should persist as long as mods agree on the defaultValue passed in and used in CreateXSet
			for (int i = 0; i < input.Length; i++) {
				if (!input[i].Equals(defaultValue)) {
					value[i] = input[i];
				}
			}
		}

		input = value;
	}


	/// <summary>
	/// <inheritdoc cref="RegisterCustomSet{T}(string, T, ref T[])"/>
	/// <para/> This particular overload will result in a final key constructed from the provided <paramref name="modName"/> and <paramref name="key"/>: "{modName}/{key}".
	/// </summary>
	public static void RegisterCustomSet<T>(string modName, string key, T defaultValue, ref T[] input) => RegisterCustomSet($"{modName}/{key}", defaultValue, ref input);

	/// <summary>
	/// <inheritdoc cref="RegisterCustomSet{T}(string, T, ref T[])"/>
	/// <para/> This particular overload will result in a final key constructed from the provided <paramref name="mod"/> and <paramref name="key"/>: "{mod.Name}/{key}".
	/// </summary>
	public static void RegisterCustomSet<T>(Mod mod, string key, T defaultValue, ref T[] input) => RegisterCustomSet($"{mod.Name}/{key}", defaultValue, ref input);

	/* Error prone method overload? No need.
	public static T[] RegisterCustomSet<T>(Mod mod, string key, T defaultValue, T[] input) => RegisterCustomSet($"{mod.Name}/{key}", defaultValue, input);
	*/
}
