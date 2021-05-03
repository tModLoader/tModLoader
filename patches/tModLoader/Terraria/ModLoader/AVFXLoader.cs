using System.Collections.Generic;
using Terraria.Graphics.Capture;

namespace Terraria.ModLoader
{
	//todo: further documentation
	/// <summary>
	/// This serves as the central class from which Avfx functions are supported and carried out.
	/// </summary>
	public abstract class AvfxLoader<T> : Loader<T> where T : ModType
	{
		public virtual void ChooseStyle(out int style, out AvfxPriority priority) { style = -1; priority = AvfxPriority.None; }
	}

	public class AvfxLoader : Loader<ModAvfx>
	{
		public const int VanillaAvfxCount = 0;
		public AvfxLoader() => Initialize(VanillaAvfxCount);

		public class AvfxInstance
		{
			public struct PrioritizedPair
			{
				public int value;
				public AvfxPriority priority;
			}

			public bool anyActive;

			public PrioritizedPair waterStyle;
			public PrioritizedPair ugBG;
			public PrioritizedPair surfaceBG;
			public PrioritizedPair music;

			public CaptureBiome.TileColorStyle tileColorStyle;

			public AvfxInstance() {
				anyActive = false;
				waterStyle = ugBG = surfaceBG = music = new PrioritizedPair();
				tileColorStyle = CaptureBiome.TileColorStyle.Normal;
			}
		}

		private struct AtmosWeight
		{
			public float weight;
			public ModAvfx type;

			public AtmosWeight(float weight, ModAvfx type) {
				this.weight = weight;
				this.type = type;
			}

			public static int InvertedCompare(AtmosWeight a, AtmosWeight b) => -a.weight.CompareTo(b.weight);
		}

		public void UpdateAvfx(Player player) {
			var result = new AvfxInstance();
			List<AtmosWeight> shortList = new List<AtmosWeight>();

			for (int i = 0; i < list.Count; i++) {
				ModAvfx avfx = list[i];

				if (!avfx.IsAvfxActive(player))
					continue;

				shortList.Add(
					new AtmosWeight(avfx.GetCorrWeight(player), avfx)
				);
			}

			if (shortList.Count == 0) {
				player.currentAvfx = result;
				return;
			}

			result.anyActive = true;

			shortList.Sort(AtmosWeight.InvertedCompare);
			int avfxFields = 0;

			for (int i = 0; avfxFields < 5 && i < shortList.Count; i++) {
				ModAvfx avfx = shortList[i].type;

				if (result.waterStyle.priority == 0 && avfx.WaterStyle != null) {
					result.waterStyle.value = avfx.WaterStyle.Slot;
					result.waterStyle.priority = avfx.Priority;
					avfxFields++;
				}

				if (result.ugBG.priority == 0 && avfx.UndergroundBackgroundStyle != null) {
					result.ugBG.value = avfx.UndergroundBackgroundStyle.Slot;
					result.ugBG.priority = avfx.Priority;
					avfxFields++;
				}

				if (result.surfaceBG.priority == 0 && avfx.SurfaceBackgroundStyle != null) {
					result.surfaceBG.value = avfx.SurfaceBackgroundStyle.Slot;
					result.surfaceBG.priority = avfx.Priority;
					avfxFields++;
				}

				if (result.music.priority == 0 && avfx.Music != -1) {
					result.music.value = avfx.Music;
					result.music.priority = avfx.Priority;
					avfxFields++;
				}

				if (result.tileColorStyle == CaptureBiome.TileColorStyle.Normal && avfx.TileColorStyle != CaptureBiome.TileColorStyle.Normal) {
					result.tileColorStyle = avfx.TileColorStyle;
					avfxFields++;
				}
			}

			player.currentAvfx = result;
		}

		// Ref or out? MusicLoader?
		public void UpdateMusic(ref int music, ref AvfxPriority priority) {
			int tst = Main.LocalPlayer.currentAvfx.music.value;
			if (tst > -1) {
				music = tst;
				priority = Main.LocalPlayer.currentAvfx.music.priority;
			}
		}
	}
}
