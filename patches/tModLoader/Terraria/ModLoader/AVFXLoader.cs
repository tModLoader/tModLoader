using System.Collections.Generic;
using Terraria.Graphics.Capture;

namespace Terraria.ModLoader
{
	//todo: further documentation
	/// <summary>
	/// This serves as the central class from which AVFX functions are supported and carried out.
	/// </summary>
	public abstract class AVFXLoader<T>
	{
		public int vanillaCount { get; internal set; }
		public int totalCount { get; internal set; }

		internal List<T> list = new List<T>();

		protected AVFXLoader(int vanillaCount) {
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
				return default;
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

	public static class AVFXHelper {
		public static WaterFallStyles Waterfalls = new WaterFallStyles();
		public static WaterStyles Waters = new WaterStyles();
		public static UgBgStyles UgBg = new UgBgStyles();
		public static SurfaceBgStyles SurfaceBg = new SurfaceBgStyles();
	}

	public static class AVFXLoader
	{
		public class AVFXInstance
		{
			public bool anyActive;
			public AVFXPriority priority;
			public int waterStyle;
			public int ugBG;
			public int surfaceBG;
			public CaptureBiome.TileColorStyle tileColorStyle;
			public int music;

			public AVFXInstance() {
				anyActive = false;
				priority = AVFXPriority.None;
				waterStyle = ugBG = surfaceBG = 0;
				music = -1;
				tileColorStyle = CaptureBiome.TileColorStyle.Normal;
			}
		}

		private struct AtmosWeight
		{
			public int weight;
			public ModAVFX type;

			public AtmosWeight(int weight, ModAVFX type) {
				this.weight = weight;
				this.type = type;
			}

			public static int InvertedCompare(AtmosWeight a, AtmosWeight b) => -a.weight.CompareTo(b.weight);
		}

		public static WaterFallStyles Waterfalls = new WaterFallStyles();
		public static WaterStyles Waters = new WaterStyles();
		public static UgBgStyles UgBg = new UgBgStyles();
		public static SurfaceBgStyles SurfaceBg = new SurfaceBgStyles();

		internal static List<ModAVFX> AVFXs = new List<ModAVFX>();

		public static void UpdateAVFX(Player player) {
			var result = new AVFXInstance();
			List<AtmosWeight> shortList = new List<AtmosWeight>();

			for (int i = 0; i < AVFXs.Count; i++) {
				ModAVFX avfx = AVFXs[i];

				if (!avfx.IsActive(player))
					continue;

				shortList.Add(
					new AtmosWeight(avfx.GetCorrWeight(player), avfx)
				);
			}

			if (shortList.Count == 0)
				return;

			result.anyActive = true;

			shortList.Sort(AtmosWeight.InvertedCompare);
			int avfxFields = 0;

			for (int i = 0; avfxFields < 7 && i < shortList.Count; i++) {
				ModAVFX avfx = shortList[i].type;

				if (result.waterStyle == 0 && avfx.WaterStyle != null) {
					result.waterStyle = avfx.WaterStyle.Slot;
					avfxFields++;
				}

				if (result.ugBG == 0 && avfx.UndergroundBackgroundStyle != null) {
					result.ugBG = avfx.UndergroundBackgroundStyle.Slot;
					avfxFields++;
				}

				if (result.surfaceBG == 0 && avfx.SurfaceBackgroundStyle != null) {
					result.surfaceBG = avfx.SurfaceBackgroundStyle.Slot;
					avfxFields++;
				}

				if (result.tileColorStyle == CaptureBiome.TileColorStyle.Normal && avfx.tileColorStyle != CaptureBiome.TileColorStyle.Normal) {
					result.tileColorStyle = avfx.tileColorStyle;
					avfxFields++;
				}

				if (result.music == -1 && avfx.Music != -1) {
					result.priority = avfx.Priority;
					result.music = avfx.Music;
					avfxFields++;
				}
			}

			player.currentAVFX = result;
		}

		public static void UpdateMusic(ref int music, ref AVFXPriority priority) {
			int tst = Main.LocalPlayer.currentAVFX.music;
			if (tst > -1) {
				music = tst;
				priority = Main.LocalPlayer.currentAVFX.priority;
			}
		}

		internal static void Unload() {
			AVFXs.Clear();
		}
	}
}
