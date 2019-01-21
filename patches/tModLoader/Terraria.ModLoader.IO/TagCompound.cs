using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Terraria.ModLoader.IO
{
	//Tag compounds contained named values, serialisable as per the NBT spec http://minecraft.gamepedia.com/NBT_format
	//All primitive data types are supported as well as byte[], int[] and Lists of other supported data types
	//Lists of Lists are internally stored as IList<IList>
	//Modification of lists stored in a TagCompound will only work if there were no type conversions involved and is not advised
	//bool is supported using TagConverter, serialised as a byte. IList<bool> will serialise as IList<byte> (quite inefficient)
	//Additional conversions can be added using TagConverter
	public class TagCompound : IEnumerable<KeyValuePair<string, object>>, ICloneable
	{
		private Dictionary<string, object> dict = new Dictionary<string, object>();
		public T Get<T>(string key) {
			object tag = null;
			dict.TryGetValue(key, out tag);
			try {
				return TagIO.Deserialize<T>(tag);
			}
			catch (Exception e) {
				throw new IOException(
					$"NBT Deserialization (type={typeof(T)}," +
					$"entry={TagPrinter.Print(new KeyValuePair<string, object>(key, tag))})", e);
			}
		}

		// adding default param to Set overload is a breaking changefor now.
		public void Set(string key, object value) => Set(key, value, false);
		
		//if value is null, calls RemoveTag, also performs type checking
		public void Set(string key, object value, bool replace = false) {
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

		public bool ContainsKey(string key) => dict.ContainsKey(key);
		public bool Remove(string key) => dict.Remove(key);

		[Obsolete] public T GetTag<T>(string key) => Get<T>(key);
		[Obsolete] public void SetTag(string key, object value) => Set(key, value);
		[Obsolete] public bool HasTag(string key) => ContainsKey(key);
		[Obsolete] public bool RemoveTag(string key) => Remove(key);

		//NBT spec getters
		public byte GetByte(string key) => Get<byte>(key);
		public short GetShort(string key) => Get<short>(key);
		public int GetInt(string key) => Get<int>(key);
		public long GetLong(string key) => Get<long>(key);
		public float GetFloat(string key) => Get<float>(key);
		public double GetDouble(string key) => Get<double>(key);
		public byte[] GetByteArray(string key) => Get<byte[]>(key);
		public int[] GetIntArray(string key) => Get<int[]>(key);
		public string GetString(string key) => Get<string>(key);
		public IList<T> GetList<T>(string key) => Get<List<T>>(key);
		public TagCompound GetCompound(string key) => Get<TagCompound>(key);
		public bool GetBool(string key) => Get<bool>(key);

		//type expansion helpers
		public short GetAsShort(string key) {
			var o = Get<object>(key);
			return o as short? ?? o as byte? ?? 0;
		}

		public int GetAsInt(string key) {
			var o = Get<object>(key);
			return o as int? ?? o as short? ?? o as byte? ?? 0;
		}

		public long GetAsLong(string key) {
			var o = Get<object>(key);
			return o as long? ?? o as int? ?? o as short? ?? o as byte? ?? 0;
		}

		public double GetAsDouble(string key) {
			var o = Get<object>(key);
			return o as double? ?? o as float? ?? 0;
		}

		public object Clone() {
			var copy = new TagCompound();
			foreach (var entry in this)
				copy.Set(entry.Key, TagIO.Clone(entry.Value));

			return copy;
		}

		public override string ToString() {
			return TagPrinter.Print(this);
		}

		public object this[string key] {
			get { return Get<object>(key); }
			set { Set(key, value, true); }
		}

		//collection initialiser
		public void Add(string key, object value) => Set(key, value);
		public void Add(KeyValuePair<string, object> entry) => Set(entry.Key, entry.Value);

		//delegate some collection implementations
		public void Clear() { dict.Clear(); }
		public int Count => dict.Count;
		public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => dict.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
