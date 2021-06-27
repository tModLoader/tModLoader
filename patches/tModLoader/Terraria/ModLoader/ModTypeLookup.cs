using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Localization;

namespace Terraria.ModLoader
{
	internal static class ModTypeLookup
	{
		public static event Action OnClear;

		public static void Clear() => OnClear?.Invoke();
	}

	public static class ModTypeLookup<T> where T : class, IModType
	{
		private static readonly Dictionary<string, T> dict = new Dictionary<string, T>();
		private static readonly Dictionary<string, Dictionary<string, T>> tieredDict = new Dictionary<string, Dictionary<string, T>>();
		private static readonly List<T> instancesById = new(); // Only used if T is IModTypeWithId. Not separate because generics can be a pain.

		static ModTypeLookup() {
			ModTypeLookup.OnClear += () => {
				dict.Clear();
				tieredDict.Clear();
				instancesById.Clear();
			};
		}

		public static void Register(T instance) {
			RegisterWithName(instance, instance.Name, instance.FullName);

			// Add legacy aliases, if the type has any.
			foreach (string legacyName in LegacyNameAttribute.GetLegacyNamesOfType(instance.GetType())) {
				RegisterWithName(instance, legacyName, $"{instance.Mod?.Name ?? "Terraria"}/{legacyName}");
			}

			// Register the integer Id.
			if (instance is IModTypeWithId modTypeWithId)
				RegisterWithId(instance, modTypeWithId.Type);
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

		private static void RegisterWithId(T instance, int id) {
			if (instance == null)
				throw new ArgumentNullException(nameof(instance));

			if (id < -1)
				throw new InvalidOperationException($"{nameof(IModTypeWithId)} implementations aren't allowed to have negative IDs.");

			if (id < instancesById.Count) {
				if (instancesById[id] != null)
					throw new InvalidOperationException($"Id registration has already occured for type '{typeof(T).Name}' (ID - {id}).");

				instancesById[id] = instance;
			}
			else if (id == instancesById.Count) {
				instancesById.Add(instance);
			}
			else {
				instancesById.AddRange(Enumerable.Repeat<T>(null, id - instancesById.Count).Append(instance));
			}
		}

		internal static T Get(int id)
			=> instancesById[id] ?? throw new Exception($"Couldn't find {typeof(T).Name} with id '{id}'.");

		internal static bool TryGet(int id, out T result) {
			if (id < 0 || id >= instancesById.Count) {
				result = default;
				return false;
			}

			result = instancesById[id];

			return result != null;
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
