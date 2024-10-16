using System.Collections.Concurrent;

namespace Terraria.ModLoader;

/// <summary>
/// Allows arbitrary cross-mod data collaboration.
/// <para/> Primarily used by <see cref="SetHandler"/> to store custom ID sets, but can be used directly for any reference type a modder might want to share with other mods.
/// <para/> Data is registered using a string key. String keys may or may not have a "ModName/" prepended, depending on their intended use-case. Data "owned" by a specific mod should use the "ModName/SubKey" approach while data intended to serve ad-hoc collaboration would use a normal key.
/// <para/> For example, mods A, B, and C might all want to collaborate on a <c>List&lt;string&gt;</c> named "BannedWords", but they have no dependencies on each other. Using <see cref="GetOrAdd(string, ref object)"/> each mod could attempt to register the object, resulting in all mods sharing the same instance. If used correctly this will work as expected whether all the mods or a subset of the mods interested in the data are enabled.
/// <para/> For data specific to a mod, the <see cref="Expose(Mod, string, object)"/> and <see cref="Retrieve(string, string)"/> methods facilitate a similar approach to collaboration except the exposing mod is intended to initialize the data and other mods access the data at a later stage.
/// </summary>
public static class DataInstance
{
	/// <summary> Stores the data. </summary>
	private static ConcurrentDictionary<string, object> data = new ConcurrentDictionary<string, object>();

	/*
	// Rather than a string->object mapping, we could do string+Type->object mapping. This might allow for less Type checking and allow mods to migrate to new approaches while keeping the same key if needs change.
	private static ConcurrentDictionary<KeyTypePair, object> dataByType = new ConcurrentDictionary<KeyTypePair, object>();
	public static T GetOrAdd<T>(string key, object obj) => (T)dataByType.GetOrAdd(new KeyTypePair(key, typeof(T)), obj);
	*/
	// TODO: Do we want to track Type information? Make methods generic? Might limit usability or conflicts.

	/// <summary>
	/// Registers a data object using the supplied key. The passed in <paramref name="obj"/> reference may change as a result of this method. The object should not be initialized with data prior to registration since it may or may not be the final shared object instance.
	/// </summary>
	public static void GetOrAdd(string key, ref object obj)
	{
		object entry = data.GetOrAdd(key, obj);
		obj = entry;
	}

	internal static object GetOrAdd(string key, object obj) => data.GetOrAdd(key, obj);

	/// <summary>
	/// Allows a mod to expose an object to other mods. The resulting key will be "ModName/Key". This is load order dependent, so collaboration about the Type, usage, and timing for access is required to properly use exposed objects in other mods. A typical approach would be to call this method in <c>ModSystem.Load</c> and have other mods call <see cref="Retrieve(string)"/> in <c>ModSystem.SetStaticDefaults</c>. The owner mod would then be able to access the final data during <c>ModSystem.PostSetupContent</c>
	/// <para/> This object can be initialized with data.
	/// <para/> Use <see cref="GetOrAdd(string, ref object)"/> instead for ah-hoc collaboration.
	/// </summary>
	public static void Expose(Mod mod, string key, object entry)
	{
		// TODO: Does the mentioned timing work for collaborating
		string realKey = $"{mod.Name}/{key}";
		if (data.ContainsKey(realKey)) {
			mod.Logger.Warn($"A DataInstance with the provided key, {realKey}, had already been registered and is being overwritten");
		}
		data[realKey] = entry; // Overwrite, intentional.
	}

	/// <summary>
	/// Gets registered data with the provided key. Returns null if the key is not found. Use this for one-way collaborations, where the data doesn't need to be populated if it doesn't exist. See <see cref="Expose(Mod, string, object)"/> for more information.
	/// </summary>
	public static object Retrieve(string key)
	{
		if (data.TryGetValue(key, out var value))
			return value;
		return null;
	}

	/// <summary>
	/// <inheritdoc cref="Retrieve(string)"/>
	/// <para/> This particular overload will result in a final key constructed from the provided <paramref name="modName"/> and <paramref name="key"/>: "{modName}/{key}".
	/// </summary>
	public static object Retrieve(string modName, string key) => Retrieve($"{modName}/{key}");

	/// <summary>
	/// <inheritdoc cref="Retrieve(string)"/>
	/// <para/> This particular overload will result in a final key constructed from the provided <paramref name="mod"/> and <paramref name="key"/>: "{mod.Name}/{key}".
	/// </summary>
	public static object Retrieve(Mod mod, string key) => Retrieve($"{mod.Name}/{key}");

	internal static void Clear()
	{
		data.Clear();
	}
}

/* Unused, would be used for potential string+Type->object mapping approach mentioned above.
internal record struct KeyTypePair(string Item1, Type Item2)
{
	public static implicit operator (string, Type)(KeyTypePair value)
	{
		return (value.Item1, value.Item2);
	}

	public static implicit operator KeyTypePair((string, Type) value)
	{
		return new KeyTypePair(value.Item1, value.Item2);
	}
}
*/