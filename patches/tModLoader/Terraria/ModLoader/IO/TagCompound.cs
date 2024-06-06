using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Terraria.ModLoader.IO;

/// <summary>
/// Tag compounds contained named values, serializable as per the NBT spec: <see href="https://minecraft.wiki/w/NBT_format">NBT spec wiki page</see> <br/>
/// All primitive data types are supported as well as byte[], int[] and Lists of other supported data types <br/>
/// Lists of Lists are internally stored as IList&lt;IList&gt; <br/>
/// Modification of lists stored in a TagCompound will only work if there were no type conversions involved and is not advised <br/>
/// bool is supported using TagConverter, serialized as a byte. IList&lt;bool&gt; will serialize as IList&lt;byte&gt; (quite inefficient) <br/>
/// Additional conversions can be added using TagConverter
/// <para/> The <see href="https://github.com/tModLoader/tModLoader/wiki/Saving-and-loading-using-TagCompound">Saving and loading using TagCompound</see> teaches how to properly use the TagCompound class.
/// </summary>
public class TagCompound : IEnumerable<KeyValuePair<string, object>>, ICloneable
{
	private Dictionary<string, object> dict = new Dictionary<string, object>();

	/// <summary>
	/// Retrieves the value corresponding to the <paramref name="key"/> of the Type <typeparamref name="T"/>.
	/// <para/> If no entry is found, a default value will be returned. For primitives this will be the typical default value for that primitive (0, false, ""). For classes and structs the returned value will be the result of calling the appropriate deserialize method with an empty TagCompound. This will usually be a default instance of that class or struct. For <see cref="List{T}"/>, an empty list is returned. For arrays, an empty array would be returned.
	/// <para/> If the found entry is not of the Type <typeparamref name="T"/> an exception will be thrown.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="key"></param>
	/// <returns></returns>
	/// <exception cref="IOException"></exception>
	public T Get<T>(string key)
	{
		if(!TryGet(key, out T value) && value == null) {
			try {
				return TagIO.Deserialize<T>(null);
			}
			catch (Exception e) {
				throw new IOException(
					$"NBT Deserialization (type={typeof(T)}," +
					$"entry={TagPrinter.Print(new KeyValuePair<string, object>(key, null))})", e);
			}
		}
		return value;
	}

	/// <summary>
	/// Attempts to retrieve the value corresponding to the provided <paramref name="key"/> and sets it to <paramref name="value"/>. If found, true is returned by this method, otherwise false is returned.
	/// <para/> Unlike <see cref="Get{T}(string)"/>, TryGet will not attempt to set <paramref name="value"/> to a valid fallback default created by the deserialize method of classes and structs in the case where the key is not found.
	/// <para/> Use this instead of <see cref="Get{T}(string)"/> in situations where falling back to the default value for missing entries would be undesirable.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="key"></param>
	/// <param name="value"></param>
	/// <returns></returns>
	/// <exception cref="IOException"></exception>
	public bool TryGet<T>(string key, out T value)
	{
		if (!dict.TryGetValue(key, out object tag)) {
			value = default;
			return false;
		}
		try {
			value = TagIO.Deserialize<T>(tag);
			return true;
		}
		catch (Exception e) {
			throw new IOException(
				$"NBT Deserialization (type={typeof(T)}," +
				$"entry={TagPrinter.Print(new KeyValuePair<string, object>(key, tag))})", e);
		}
	}

	//if value is null, calls RemoveTag, also performs type checking
	/// <summary> Use this to set values to the TagCompound indexed by the specified <paramref name="key"/>. </summary>
	public void Set(string key, object value, bool replace = false)
	{
		if (value == null) {
			Remove(key);
			return;
		}

		object serialized;
		try {
			serialized = TagIO.Serialize(value);
		}
		catch (IOException e) {
			var valueInfo = "value=" + value;
			if (value.GetType().ToString() != value.ToString())
				valueInfo = "type=" + value.GetType() + "," + valueInfo;
			throw new IOException($"NBT Serialization (key={key},{valueInfo})", e);
		}
		if (replace)
			dict[key] = serialized;
		else
			dict.Add(key, serialized);
	}

	/// <summary> Returns true if an entry with specified key exists. This is useful to check if a key is present prior to retrieving the value in the case that key potentially won't exist and the default behavior of <see cref="Get{T}(string)"/> would be undesirable. The <see href="https://github.com/tModLoader/tModLoader/wiki/Saving-and-loading-using-TagCompound#mod-version-updates">Mod Version Updates section of the wiki guide</see> shows an example of one such situation. </summary>
	public bool ContainsKey(string key) => dict.ContainsKey(key);
	/// <summary> Removed the entry corresponding to the <paramref name="key"/>. Returns true if the element is successfully found and removed; otherwise, false. </summary>
	public bool Remove(string key) => dict.Remove(key);

	//NBT spec getters
	/// <inheritdoc cref="Get{T}(string)" />
	public byte GetByte(string key) => Get<byte>(key);
	/// <inheritdoc cref="Get{T}(string)" />
	public short GetShort(string key) => Get<short>(key);
	/// <inheritdoc cref="Get{T}(string)" />
	public int GetInt(string key) => Get<int>(key);
	/// <inheritdoc cref="Get{T}(string)" />
	public long GetLong(string key) => Get<long>(key);
	/// <inheritdoc cref="Get{T}(string)" />
	public float GetFloat(string key) => Get<float>(key);
	/// <inheritdoc cref="Get{T}(string)" />
	public double GetDouble(string key) => Get<double>(key);
	/// <inheritdoc cref="Get{T}(string)" />
	public byte[] GetByteArray(string key) => Get<byte[]>(key);
	/// <inheritdoc cref="Get{T}(string)" />
	public int[] GetIntArray(string key) => Get<int[]>(key);
	/// <inheritdoc cref="Get{T}(string)" />
	public string GetString(string key) => Get<string>(key);
	/// <inheritdoc cref="Get{T}(string)" />
	public IList<T> GetList<T>(string key) => Get<List<T>>(key);
	/// <summary>
	/// GetCompound can be used to retrieve nested TagCompounds. This can be useful for saving complex data. An empty TagCompound is returned if not present.
	/// <para/> <inheritdoc cref="Get{T}(string)" />
	/// </summary>
	public TagCompound GetCompound(string key) => Get<TagCompound>(key);
	/// <inheritdoc cref="Get{T}(string)" />
	public bool GetBool(string key) => Get<bool>(key);

	//type expansion helpers
	public short GetAsShort(string key)
	{
		var o = Get<object>(key);
		return o as short? ?? o as byte? ?? 0;
	}

	public int GetAsInt(string key)
	{
		var o = Get<object>(key);
		return o as int? ?? o as short? ?? o as byte? ?? 0;
	}

	public long GetAsLong(string key)
	{
		var o = Get<object>(key);
		return o as long? ?? o as int? ?? o as short? ?? o as byte? ?? 0;
	}

	public double GetAsDouble(string key)
	{
		var o = Get<object>(key);
		return o as double? ?? o as float? ?? 0;
	}

	public object Clone()
	{
		var copy = new TagCompound();
		foreach (var entry in this)
			copy.Set(entry.Key, TagIO.Clone(entry.Value));

		return copy;
	}

	public override string ToString()
	{
		return TagPrinter.Print(this);
	}

	/// <summary>
	/// Use this to add values to the TagCompound, similar to working with a <see cref="Dictionary{TKey, TValue}"/>. An alternate to this is calling <see cref="Add(string, object)"/> directly.
	/// <para/> If is also possible to use this to retrieve entries from the TagCompound directly, but since the return value is <see cref="Object"/> this is rarely the best approach. Usually one of the <see cref="Get{T}(string)"/> methods should be used for this. One situation where this is necessary is described in the <see href="https://github.com/tModLoader/tModLoader/wiki/Saving-and-loading-using-TagCompound#updates-to-data-type">Updates to data Type section of the wiki guide</see>.
	/// </summary>
	public object this[string key] {
		get { return Get<object>(key); }
		set { Set(key, value, true); }
	}

	//collection initializer
	/// <summary> Use this to add values to the TagCompound indexed by the specified <paramref name="key"/>. </summary>
	public void Add(string key, object value) => Set(key, value);
	/// <summary> Use this to add a KeyValuePair to the TagCompound. </summary>
	public void Add(KeyValuePair<string, object> entry) => Set(entry.Key, entry.Value);

	//delegate some collection implementations
	public void Clear() { dict.Clear(); }
	public int Count => dict.Count;
	public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => dict.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
