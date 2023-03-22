using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader.Core;

namespace Terraria.ModLoader;

public static class ContentInstance
{
	private class ContentEntry
	{
		private object instance;
		private List<object> instances;
		private Action<object, IEnumerable> staticUpdate;

		public void Register(object obj)
		{
			lock (this) {
				if (instances != null) {
					instances.Add(obj);
				}
				else if (instance != null) {
					instances = new List<object> { instance, obj };
					instance = null;
				}
				else {
					instance = obj;
				}

				staticUpdate?.Invoke(instance, instances);
			}
		}

		public void Link(Action<object, IEnumerable> update)
		{
			lock (this) {
				staticUpdate = update;
				update(instance, instances);
			}
		}

		public void Clear()
		{
			lock (this) {
				instance = null;
				instances = null;
				staticUpdate?.Invoke(instance, instances);
			}
		}
	}

	static ContentInstance()
	{
		TypeCaching.OnClear += Clear;
	}

	private static ConcurrentDictionary<Type, ContentEntry> contentByType = new ConcurrentDictionary<Type, ContentEntry>();

	private static ContentEntry Factory(Type t) => new ContentEntry();

	internal static void Link(Type t, Action<object, IEnumerable> update) => contentByType.GetOrAdd(t, Factory).Link(update);

	public static void Register(object obj) => contentByType.GetOrAdd(obj.GetType(), Factory).Register(obj);

	private static void Clear()
	{
		foreach (var entry in contentByType) {
			entry.Value.Clear();
			if (entry.Key.Assembly != typeof(ContentEntry).Assembly)
				contentByType.TryRemove(entry.Key, out _);
		}
	}
}

public static class ContentInstance<T> where T : class
{
	public static T Instance { get; private set; }
	public static IReadOnlyList<T> Instances { get; private set; }

	static ContentInstance()
	{
		ContentInstance.Link(typeof(T), Update);
	}

	private static void Update(object instance, IEnumerable instances)
	{
		Instance = (T)instance;
		Instances = instances?.Cast<T>()?.ToArray();
	}
}
