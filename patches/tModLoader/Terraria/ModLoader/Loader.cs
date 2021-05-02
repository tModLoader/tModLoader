using System.Collections.Generic;

namespace Terraria.ModLoader
{
	//todo: further documentation
	/// <summary>
	/// This serves as the highest-level class for loaders
	/// </summary>
	public abstract class Loader<T> where T : ModType
	{
		public int vanillaCount { get; internal set; }
		public int totalCount { get; internal set; }

		internal List<T> list = new List<T>();

		protected Loader(int vanillaCount) {
			this.vanillaCount = vanillaCount;
			totalCount = vanillaCount;
		}

		public int Reserve() {
			int reserve = totalCount;
			totalCount++;
			return reserve;
		}

		// TODO: Possibly convert all ModTypes to have 'int Type' as their indexing field.
		public int Register(T obj) {
			int type = Reserve();
			ModTypeLookup<T>.Register(obj);
			list.Add(obj);
			return type;
		}

		public T Get(int id) {
			if (id < vanillaCount || id >= totalCount) {
				return default;
			}
			return list[id - vanillaCount];
		}

		internal virtual void Unload() {
			totalCount = vanillaCount;
			list.Clear();
		}

		internal virtual void ResizeArrays() { }
	}

	public static class Loaders {
		public static WaterFallStyles Waterfalls = new WaterFallStyles();
		public static WaterStyles Waters = new WaterStyles();
		public static UgBgStyles UgBgs = new UgBgStyles();
		public static SurfaceBgStyles SurfaceBgs = new SurfaceBgStyles();
		public static AVFXLoader AVFXs = new AVFXLoader();
		public static BiomeLoader Biomes = new BiomeLoader();
	}
}
