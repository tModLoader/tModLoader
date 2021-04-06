using System.Collections.Generic;
using Terraria.Graphics.Capture;

namespace Terraria.ModLoader
{
	//todo: further documentation
	/// <summary>
	/// This serves as the central class from which Atmospheric functions are supported and carried out.
	/// </summary>
	public abstract class AtmosphericLoader<T>
	{
		public int vanillaCount { get; internal set; }
		public int totalCount { get; internal set; }

		internal List<T> list = new List<T>();

		protected AtmosphericLoader(int vanillaCount) {
			this.vanillaCount = vanillaCount;
			totalCount = vanillaCount;
		}

		public int Reserve() {
			int reserve = totalCount;
			totalCount++;
			return reserve;
		}

		public T Get(int id) {
			if (id < vanillaCount || id >= totalCount) {
				return default(T);
			}
			return list[id - vanillaCount];
		}

		internal void Unload() {
			totalCount = vanillaCount;
			list.Clear();
		}

		internal virtual void ResizeArrays() { }

		public virtual void ChooseStyle(ref int style) { }
	}

	public static class AtmosphericHelper {
		public static WaterFallStyles Waterfalls = new WaterFallStyles();
		public static WaterStyles Waters = new WaterStyles();
		public static UgBgStyles UgBg = new UgBgStyles();
		public static SurfaceBgStyles SurfaceBg = new SurfaceBgStyles();
	}

	public static class AtmosphericLoader
	{
		internal static List<ModAtmosphericType> atmosphericTypes = new List<ModAtmosphericType>();

		public class ModAtmosphere
		{
			public bool anyActive;
			public AtmosphericPriority priority;
			public int waterStyle;
			public int ugBG;
			public int surfaceBG;
			public CaptureBiome.TileColorStyle tileColorStyle;
			public int music;

			public ModAtmosphere() {
				anyActive = false;
				priority = AtmosphericPriority.None;
				waterStyle = ugBG = surfaceBG = 0;
				music = -1;
				tileColorStyle = CaptureBiome.TileColorStyle.Normal;
			}
		}

		internal struct AtmosWeight{
			internal int weight;
			internal ModAtmosphericType type;

			internal AtmosWeight(int weight, ModAtmosphericType type) {
				this.weight = weight;
				this.type = type;
			}
		}

		public static void UpdateModAtmosphere(Player player) {
			var result = new ModAtmosphere();
			List<AtmosWeight> shortList = new List<AtmosWeight>();

			foreach (var atmos in atmosphericTypes) {
				int corrWeight = atmos.GetCorrWeight(player);
				if (corrWeight == 0)
					continue;

				shortList.Add(
					new AtmosWeight(corrWeight, atmos)
				);
			}

			if (!(shortList.Count == 0)) {
				result.anyActive = true;
				int weight = 0;

				shortList.Sort((s1, s2) => s1.weight.CompareTo(s2.weight));

				foreach (var atmos in shortList) {
					if (atmos.weight > weight) {
						weight = atmos.weight;

						result.priority = atmos.type.Priority;

						//TODO: de:Terrible Hardcode
						result.waterStyle = atmos.type.WaterStyle != null ? atmos.type.WaterStyle.Slot : result.waterStyle;
						result.ugBG = atmos.type.UndergroundBackgroundStyle != null ? atmos.type.UndergroundBackgroundStyle.Slot : result.ugBG;
						result.surfaceBG = atmos.type.SurfaceBackgroundStyle != null ? atmos.type.SurfaceBackgroundStyle.Slot : result.surfaceBG;
						result.tileColorStyle = atmos.type.tileColorStyle != CaptureBiome.TileColorStyle.Normal ? atmos.type.tileColorStyle : result.tileColorStyle;
						result.music = atmos.type.Music != -1 ? atmos.type.Music : result.music;
					}
				}

			}

			player.currentModAtmosphere = result;
		}

		public static void UpdateMusic(ref int music, ref AtmosphericPriority priority) {
			int tst = Main.LocalPlayer.currentModAtmosphere.music;
			if (tst > -1) {
				music = tst;
				priority = Main.LocalPlayer.currentModAtmosphere.priority;
			}
		}

		internal static void Unload() {
			atmosphericTypes.Clear();
		} 
	}
}
