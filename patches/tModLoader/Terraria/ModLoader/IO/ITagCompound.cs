using System;
using System.Collections.Generic;

namespace Terraria.ModLoader.IO
{
	public interface ITagCompound : IReadOnlyTagCompound
	{
		public void Set(string key, object value, bool replace = false);
		
		public bool Remove(string key);

		public new ITagCompound GetCompound(string key);

		public new object this[string key] { get; set; }

		public void Add(string key, object value);
		public void Add(KeyValuePair<string, object> entry);

		public void Clear();
	}
}
