using System;
using System.Collections.Generic;
using System.Reflection;

namespace Terraria.ModLoader;

//TODO: Further documentation.

/// <summary> Serves as a highest-level base for loaders. </summary>
public abstract class Loader : ILoader
{
	public int VanillaCount { get; set; }
	internal int TotalCount { get; set; }

	/// <summary>
	/// Initializes the loader based on the vanilla count of the ModType.
	/// </summary>
	internal void Initialize(int vanillaCount)
	{
		VanillaCount = vanillaCount;
		TotalCount = vanillaCount;
	}

	protected int Reserve() => TotalCount++;

	internal virtual void ResizeArrays() { }

	internal virtual void Unload()
	{
		TotalCount = VanillaCount;
	}

	void ILoader.ResizeArrays() => ResizeArrays();

	void ILoader.Unload() => Unload();
}

/// <summary> Serves as a highest-level base for loaders of mod types. </summary>
public abstract class Loader<T> : Loader
	where T : ModType
{
	internal List<T> list = new List<T>();

	//TODO: Possibly convert all ModTypes to have 'int Type' as their indexing field.
	public int Register(T obj)
	{
		int type = Reserve();

		ModTypeLookup<T>.Register(obj);
		list.Add(obj);

		return type;
	}

	public T Get(int id)
	{
		if (id < VanillaCount || id >= TotalCount) {
			return default;
		}

		return list[id - VanillaCount];
	}

	internal override void Unload()
	{
		base.Unload();

		list.Clear();
	}
}

public interface ILoader
{
	internal void ResizeArrays() { }

	internal void Unload() { }
}

public static class LoaderManager
{
	private static readonly Dictionary<Type, ILoader> loadersByType = new Dictionary<Type, ILoader>();

	internal static void AutoLoad()
	{
		foreach (var type in Assembly.GetExecutingAssembly().GetTypes()) {
			if (!typeof(ILoader).IsAssignableFrom(type) || type.IsAbstract || !type.IsClass) {
				continue;
			}

			var autoload = AutoloadAttribute.GetValue(type);

			if (autoload.NeedsAutoloading) {
				loadersByType.Add(type, (ILoader)Activator.CreateInstance(type, true)!);
			}
		}
	}

	// TODO: Constrain to ILoader?
	public static T Get<T>()
	{
		if (!loadersByType.TryGetValue(typeof(T), out var result))
			return (T)Activator.CreateInstance(typeof(T), true)!; // Return empty instance in case of static Player constructor or similar

		return (T)result;
	}

	internal static void Unload()
	{
		foreach (var loader in loadersByType.Values) {
			loader.Unload();
		}
	}

	internal static void ResizeArrays()
	{
		foreach (var loader in loadersByType.Values) {
			loader.ResizeArrays();
		}
	}
}
