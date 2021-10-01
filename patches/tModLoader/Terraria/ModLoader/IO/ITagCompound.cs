using System;
using System.Collections.Generic;

namespace Terraria.ModLoader.IO
{
	public interface ITagCompound : IEnumerable<KeyValuePair<string, object>>, ICloneable
	{
		public T Get<T>(string key);
		public void Set(string key, object value, bool replace = false);

		public bool ContainsKey(string key);
		public bool Remove(string key);

		//NBT spec getters
		public byte GetByte(string key);
		public short GetShort(string key);
		public int GetInt(string key);
		public long GetLong(string key);
		public float GetFloat(string key);
		public double GetDouble(string key);
		public byte[] GetByteArray(string key);
		public int[] GetIntArray(string key);
		public string GetString(string key);
		public IList<T> GetList<T>(string key);
		public ITagCompound GetCompound(string key);
		public bool GetBool(string key);

		//type expansion helpers
		public short GetAsShort(string key);
		public int GetAsInt(string key);
		public long GetAsLong(string key);
		public double GetAsDouble(string key);

		public object this[string key] { get; set; }

		public void Add(string key, object value);
		public void Add(KeyValuePair<string, object> entry);

		public void Clear();
		public int Count { get; }
	}
}
