using System;
using System.Collections.Generic;
using Terraria.Localization;

namespace Terraria.ModLoader
{
	internal static class ModTypeLookup
	{
		public static event Action OnClear;

		public static void Clear() => OnClear?.Invoke();
	}

	public static class ModTypeLookup<T> where T : IModType
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
			RegisterWithName(instance, instance.Name, instance.FullName);

			//Add legacy aliases, if the type has any.
			foreach (string legacyName in LegacyNameAttribute.GetLegacyNamesOfType(instance.GetType())) {
				RegisterWithName(instance, legacyName, $"{instance.Mod?.Name ?? "Terraria"}/{legacyName}");
			}
		}

		private static void RegisterWithName(T instance, string name, string fullName) {
			if (dict.ContainsKey(fullName))
				throw new Exception(Language.GetTextValue("tModLoader.LoadErrorDuplicateName", typeof(T).Name, fullName));

			dict[fullName] = instance;

			string modName = instance.Mod?.Name ?? "Terraria";

			if (!tieredDict.TryGetValue(modName, out var subDictionary))
				tieredDict[modName] = subDictionary = new Dictionary<string, T>();

			subDictionary[name] = instance;
		}

		internal static T Get(string fullName) => dict[fullName];

		internal static T Get(string modName, string contentName) => tieredDict[modName][contentName];

		internal static bool TryGetValue(string fullName, out T value) => dict.TryGetValue(fullName, out value);

		internal static bool TryGetValue(string modName, string contentName, out T value) {
			if (!tieredDict.TryGetValue(modName, out var subDictionary)) {
				value = default;

				return false;
			}
				
			return subDictionary.TryGetValue(contentName, out value);
		}
	}
}
