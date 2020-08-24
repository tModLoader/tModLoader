using System;
using System.Collections.Generic;
using Terraria.Localization;

namespace Terraria.ModLoader
{
	internal class ModTypeLookup
	{
		public static event Action OnClear;

		public static void Clear() => OnClear?.Invoke();
	}

	internal class ModTypeLookup<T> where T : IModType
	{
		private static Dictionary<string, T> dict = new Dictionary<string, T>();

		static ModTypeLookup() {
			ModTypeLookup.OnClear += () => dict.Clear();
		}

		public static void Register(T instance) {
			if (!instance.Mod.loading)
				throw new Exception("AddBackgroundTexture can only be called from Mod.Load or IModContent.Load");

			if (dict.ContainsKey(instance.FullName))
				throw new Exception(Language.GetTextValue("tModLoader.LoadErrorDuplicateName", typeof(T).Name, instance.FullName));

			dict[instance.FullName] = instance;
		}

		public static T Get(string fullname) => dict[fullname];

		public static bool TryGetValue(string fullname, out T value) => dict.TryGetValue(fullname, out value);
	}
}
