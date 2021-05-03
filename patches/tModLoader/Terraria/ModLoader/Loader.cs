using System.Collections.Generic;

namespace Terraria.ModLoader
{
	//todo: further documentation
	/// <summary>
	/// This serves as the highest-level class for loaders
	/// </summary>
	public abstract class Loader<T> where T : ModType
	{
		public int VanillaCount { get; internal set; }
		public int TotalCount { get; internal set; }

		internal List<T> list = new List<T>();

		protected Loader(int vanillaCount) {
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

		internal virtual void Unload() {
			TotalCount = VanillaCount;
			list.Clear();
		}

		internal virtual void ResizeArrays() { }
	}

	public static class Loaders {
		public static WaterFallStyles Waterfalls { get; private set; } = new WaterFallStyles();
		public static WaterStyles Waters { get; private set; } = new WaterStyles();
		public static UgBgStyles UgBgs { get; private set; } = new UgBgStyles();
		public static SurfaceBgStyles SurfaceBgs { get; private set; } = new SurfaceBgStyles();
		public static AvfxLoader Avfxs { get; private set; } = new AvfxLoader();
		public static BiomeLoader Biomes { get; private set; } = new BiomeLoader();
	}
}
