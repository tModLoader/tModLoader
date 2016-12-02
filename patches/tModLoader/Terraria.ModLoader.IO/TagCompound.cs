using System;
using System.Collections;
using System.Collections.Generic;

namespace Terraria.ModLoader.IO
{
	//Tag compounds contained named values, serialisable as per the NBT spec http://minecraft.gamepedia.com/NBT_format
	//All primitive data types are supported as well as byte[], int[] and Lists of other supported data types
	//Lists must be strongly typed. Lists of Lists will be read as IList<IList> not List<IList<int>>
	//bool is supported with an extension to the nbt spec, serialised as a byte. IList<bool> is not recommended
	public class TagCompound : IEnumerable<KeyValuePair<string, object>>, ICloneable
	{
		private Dictionary<string, object> dict = new Dictionary<string, object>();
		public T GetTag<T>(string key) {
			object tag = null;
			dict.TryGetValue(key, out tag);
			return (T) tag;
		}

		//if value is null, calls RemoveTag, also performs type checking
		public void SetTag(string key, object value) {
			if (value == null)
				RemoveTag(key);
			else {
				TagIO.TypeCheck(value.GetType());
				dict.Add(key, value);
			}
		}

		public bool HasTag(string key) => dict.ContainsKey(key);
		public bool RemoveTag(string key) => dict.Remove(key);
		
		//NBT spec getters
		public byte GetByte(string key) => GetTag<byte?>(key) ?? 0;
		public short GetShort(string key) => GetTag<short?>(key) ?? 0;
		public int GetInt(string key) => GetTag<int?>(key) ?? 0;
		public long GetLong(string key) => GetTag<long?>(key) ?? 0;
		public float GetFloat(string key) => GetTag<float?>(key) ?? 0;
		public double GetDouble(string key) => GetTag<double?>(key) ?? 0;
		public byte[] GetByteArray(string key) => GetTag<byte[]>(key) ?? new byte[0];
		public int[] GetIntArray(string key) => GetTag<int[]>(key) ?? new int[0];
		public string GetString(string key) => GetTag<string>(key) ?? "";
		public IList<T> GetList<T>(string key) => GetTag<IList<T>>(key) ?? new List<T>();
		public TagCompound GetCompound(string key) => GetTag<TagCompound>(key) ?? new TagCompound();
		
		//extension to the NBT spec, boolean as byte
		public bool GetBool(string key)
		{
			var o = GetTag<object>(key);
			if (o is byte)
				return (byte)o != 0;

			return o as bool? ?? false;
		}

		public short GetAsShort(string key) {
			var o = GetTag<object>(key);
			return o as short? ?? o as byte? ?? 0;
		}

		public int GetAsInt(string key) {
			var o = GetTag<object>(key);
			return o as int? ?? o as short? ?? o as byte? ?? 0;
		}

		public long GetAsLong(string key) {
			var o = GetTag<object>(key);
			return o as long? ?? o as int? ?? o as short? ?? o as byte? ?? 0;
		}

		public double GetAsDouble(string key) {
			var o = GetTag<object>(key);
			return o as double? ?? o as float? ?? 0;
		}

		public object Clone() {
			var copy = new TagCompound();
			foreach (var entry in this)
				if (entry.Value != null)
					copy.SetTag(entry.Key, TagIO.Clone(entry.Value));

			return copy;
		}
		
		public object this[string key] {
			get { return GetTag<object>(key); }
			set { SetTag(key, value); }
		}

		//collection initialiser
		public void Add(string key, object value) => SetTag(key, value);
		public void Add(KeyValuePair<string, object> entry) => SetTag(entry.Key, entry.Value);

		//delegate some collection implementations
		public void Clear() { dict.Clear(); }
		public int Count => dict.Count;
		public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => dict.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
