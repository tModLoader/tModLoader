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
		private static readonly Dictionary<string, T> dict = new Dictionary<string, T>();
		private static readonly Dictionary<string, Dictionary<string, T>> tieredDict = new Dictionary<string, Dictionary<string, T>>();

		static ModTypeLookup() {
			ModTypeLookup.OnClear += () => {
				dict.Clear();
				tieredDict.Clear();
			};
		}

		public static void Register(T instance) {
			if (dict.ContainsKey(instance.FullName))
				throw new Exception(Language.GetTextValue("tModLoader.LoadErrorDuplicateName", typeof(T).Name, instance.FullName));

			dict[instance.FullName] = instance;

			if (!tieredDict.TryGetValue(instance.Mod.Name, out var subDictionary))
				tieredDict[instance.Mod.Name] = subDictionary = new Dictionary<string, T>();

			subDictionary[instance.Name] = instance;
		}

		public static T Get(string fullname) => dict[fullname];

		public static T Get(string modName, string contentName) => tieredDict[modName][contentName];

		public static bool TryGetValue(string fullname, out T value) => dict.TryGetValue(fullname, out value);

		public static bool TryGetValue(string modName, string contentName, out T value) {
			if (!tieredDict.TryGetValue(modName, out var subDictionary)) {
				value = default;

				return false;
			}
				
			return subDictionary.TryGetValue(contentName, out value);
		}
	}
}
