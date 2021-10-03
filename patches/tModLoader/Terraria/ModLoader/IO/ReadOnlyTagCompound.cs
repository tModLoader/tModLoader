using System.Collections.Generic;

namespace Terraria.ModLoader.IO
{
	public readonly ref struct ReadOnlyTagCompound
	{
		public readonly TagCompound Tag;

		public ReadOnlyTagCompound(TagCompound tag) {
			Tag = tag;
		}

		public static implicit operator ReadOnlyTagCompound(TagCompound tag) => new(tag);

		public T Get<T>(string key) => Tag.Get<T>(key);
		public bool ContainsKey(string key) => Tag.ContainsKey(key);

		//NBT spec getters
		public byte GetByte(string key) => Tag.GetByte(key);
		public short GetShort(string key) => Tag.GetShort(key);
		public int GetInt(string key) => Tag.GetInt(key);
		public long GetLong(string key) => Tag.GetLong(key);
		public float GetFloat(string key) => Tag.GetFloat(key);
		public double GetDouble(string key) => Tag.GetDouble(key);
		public byte[] GetByteArray(string key) => Tag.GetByteArray(key);
		public int[] GetIntArray(string key) => Tag.GetIntArray(key);
		public string GetString(string key) => Tag.GetString(key);
		public IList<T> GetList<T>(string key) => Tag.GetList<T>(key);
		public ReadOnlyTagCompound GetCompound(string key) => Tag.GetCompound(key);
		public bool GetBool(string key) => Tag.GetBool(key);

		//type expansion helpers
		public short GetAsShort(string key) => Tag.GetAsShort(key);
		public int GetAsInt(string key) => Tag.GetAsInt(key);
		public long GetAsLong(string key) => Tag.GetAsLong(key);
		public double GetAsDouble(string key) => Tag.GetAsDouble(key);

		public override string ToString() => Tag.ToString();

		public object this[string key] => Tag[key];
		public int Count => Tag.Count;
	}
}
