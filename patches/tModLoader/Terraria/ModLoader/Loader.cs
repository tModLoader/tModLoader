using System;
using System.Collections.Generic;
using System.Reflection;

namespace Terraria.ModLoader
{
	//todo: further documentation
	/// <summary>
	/// This serves as the highest-level class for loaders
	/// </summary>
	public abstract class Loader<T> : ILoader where T : ModType
	{
		public int VanillaCount { get; set; }
		internal int TotalCount { get; set; }

		internal List<T> list = new List<T>();

		/// <summary>
		/// Initilizes the loader based on the vanilla count of the ModType.
		/// </summary>
		internal void Initialize(int vanillaCount) {
			VanillaCount = vanillaCount;
			TotalCount = vanillaCount;
		}

		private int Reserve() {
			int reserve = TotalCount;
			TotalCount++;
			return reserve;
		}

		//TODO: Possibly convert all ModTypes to have 'int Type' as their indexing field.
		public int Register(T obj) {
			int type = Reserve();
			ModTypeLookup<T>.Register(obj);
			list.Add(obj);
			return type;
		}

		public T Get(int id) {
			if (id < VanillaCount || id >= TotalCount) {
				return default;
			}
			return list[id - VanillaCount];
		}

		void ILoader.ResizeArrays() => ResizeArrays();
		internal virtual void ResizeArrays() { }

		void ILoader.Unload() => Unload();
		internal virtual void Unload() {
			TotalCount = VanillaCount;
			list.Clear();
		}
	}

	public interface ILoader
	{
		internal void ResizeArrays() { }

		internal void Unload() { }
	}

	public static class LoaderManager {
		private static Dictionary<Type, ILoader> dict = new Dictionary<Type, ILoader>();

		internal static void AutoLoad() {
			List<Type> allSubTypes = new List<Type>();
			foreach (var subtype in Assembly.GetExecutingAssembly().GetTypes()) {
				if (typeof(ILoader).IsAssignableFrom(subtype) && !subtype.IsAbstract && subtype.IsClass) {
					dict.Add(subtype, (ILoader)Activator.CreateInstance(subtype));
				}
			}
		}

		public static T Get<T>() {
			if (!dict.TryGetValue(typeof(T), out var result))
				return Activator.CreateInstance<T>(); // Return empty instance in case of static Player constructor or similar

			return (T)result;
		}

		internal static void Unload() {
			foreach (var loader in dict.Values) {
				loader.Unload();
			}
		}

		internal static void ResizeArrays() {
			foreach (var loader in dict.Values) {
				loader.ResizeArrays();
			}
		}
	}
}
