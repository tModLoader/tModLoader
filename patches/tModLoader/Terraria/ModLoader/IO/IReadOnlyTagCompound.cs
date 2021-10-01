using System;
using System.Collections.Generic;

namespace Terraria.ModLoader.IO
{
	public interface IReadOnlyTagCompound : IEnumerable<KeyValuePair<string, object>>, ICloneable
	{
		public T Get<T>(string key);
		public bool ContainsKey(string key);

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
		public IReadOnlyTagCompound GetCompound(string key);
		public bool GetBool(string key);

		//type expansion helpers
		public short GetAsShort(string key);
		public int GetAsInt(string key);
		public long GetAsLong(string key);
		public double GetAsDouble(string key);

		public object this[string key] { get; }
		public int Count { get; }
	}
}
