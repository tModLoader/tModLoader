using System.Collections.Generic;
using Terraria.Graphics.Capture;

namespace Terraria.ModLoader
{
	//todo: further documentation
	/// <summary>
	/// This serves as the central class from which AVFX functions are supported and carried out.
	/// </summary>
	public abstract class AVFXLoader<T> : Loader<T> where T : ModType
	{
		protected AVFXLoader(int vanillaCount) : base(vanillaCount) { }

		public virtual void ChooseStyle(out int style, out AVFXPriority priority) { style = -1; priority = AVFXPriority.None; }
	}

	public class AVFXLoader : Loader<ModAVFX>
	{
		public AVFXLoader(int vanillaCount = 0) : base(vanillaCount) { }

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

		public void UpdateAVFX(Player player) {
			var result = new AVFXInstance();
			List<AtmosWeight> shortList = new List<AtmosWeight>();

			for (int i = 0; i < list.Count; i++) {
				ModAVFX avfx = list[i];

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

			for (int i = 0; avfxFields < 5 && i < shortList.Count; i++) {
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

				if (result.tileColorStyle == CaptureBiome.TileColorStyle.Normal && avfx.TileColorStyle != CaptureBiome.TileColorStyle.Normal) {
					result.tileColorStyle = avfx.TileColorStyle;
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

		// Ref or out? MusicLoader?
		public void UpdateMusic(ref int music, ref AVFXPriority priority) {
			int tst = Main.LocalPlayer.currentAVFX.music;
			if (tst > -1) {
				music = tst;
				priority = Main.LocalPlayer.currentAVFX.priority;
			}
		}
	}
}
