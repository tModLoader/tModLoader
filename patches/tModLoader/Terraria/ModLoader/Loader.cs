using System;
using System.Collections.Generic;

namespace Terraria.ModLoader
{
	//todo: further documentation
	/// <summary>
	/// This serves as the highest-level class for loaders
	/// </summary>
	public abstract class Loader<T> : ILoader where T : ModType
	{
		public int VanillaCount { get; internal set; }
		public int TotalCount { get; internal set; }

		internal List<T> list = new List<T>();

		/// <summary>
		/// Initilizes the loader based on the vanilla count of the ModType.
		/// </summary>
		internal void Initialize(int vanillaCount) {
			this.VanillaCount = vanillaCount;
			TotalCount = vanillaCount;
		}

		public int Reserve() {
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
		private static Dictionary<Type, object> list = new Dictionary<Type, object>();

		internal static void AutoLoad() {
			List<Type> allSubTypes = new List<Type>();
			foreach (var assem in AppDomain.CurrentDomain.GetAssemblies()) {
				if (!assem.FullName.Contains("tModLoader"))
					continue;

				foreach (var subtype in assem.GetTypes()) {
					if (typeof(ILoader).IsAssignableFrom(subtype) && !subtype.IsAbstract && subtype.IsClass) {
						allSubTypes.Add(subtype);
					}
				}
			}

			foreach (var type in allSubTypes) {
				object instance = Activator.CreateInstance(type);
				list.Add(type, instance);
			}
		}

		public static T Get<T>() {
			if (!list.TryGetValue(typeof(T), out object result))
				return Activator.CreateInstance<T>();

			return (T)result;
		}

		public static void Unload() {
			foreach (var item in list.Values) {
				ILoader loader = (ILoader)item;
				loader.Unload();
			}
		}

		public static void ResizeArrays() {
			foreach (var item in list.Values) {
				ILoader loader = (ILoader)item;
				loader.ResizeArrays();
			}
		}
	}
}
