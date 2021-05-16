using System.Collections.Generic;
using Terraria.Graphics.Capture;

namespace Terraria.ModLoader
{
	//todo: further documentation
	/// <summary>
	/// This serves as the central class from which SceneEffect functions are supported and carried out.
	/// </summary>
	public abstract class SceneEffectLoader<T> : Loader<T> where T : ModType
	{
		public virtual void ChooseStyle(out int style, out SceneEffectPriority priority) { style = -1; priority = SceneEffectPriority.None; }
	}

	public class SceneEffectLoader : Loader<ModSceneEffect>
	{
		public const int VanillaSceneEffectCount = 0;
		public SceneEffectLoader() => Initialize(VanillaSceneEffectCount);

		public class SceneEffectInstance
		{
			public struct PrioritizedPair
			{
				public int value;
				public SceneEffectPriority priority;
			}

			public bool anyActive;
			public PrioritizedPair waterStyle;
			public PrioritizedPair undergroundBackground;
			public PrioritizedPair surfaceBackground;
			public PrioritizedPair music;
			public CaptureBiome.TileColorStyle tileColorStyle;

			public SceneEffectInstance() {
				anyActive = false;
				waterStyle = undergroundBackground = surfaceBackground = music = new PrioritizedPair();
				tileColorStyle = CaptureBiome.TileColorStyle.Normal;
			}
		}

		private struct AtmosWeight
		{
			public float weight;
			public ModSceneEffect type;

			public AtmosWeight(float weight, ModSceneEffect type) {
				this.weight = weight;
				this.type = type;
			}

			public static int InvertedCompare(AtmosWeight a, AtmosWeight b) => -a.weight.CompareTo(b.weight);
		}

		public void UpdateSceneEffect(Player player) {
			var result = new SceneEffectInstance();
			List<AtmosWeight> shortList = new List<AtmosWeight>();

			for (int i = 0; i < list.Count; i++) {
				ModSceneEffect avfx = list[i];

				if (!avfx.IsSceneEffectActive(player))
					continue;

				shortList.Add(
					new AtmosWeight(avfx.GetCorrWeight(player), avfx)
				);
			}

			if (shortList.Count == 0) {
				player.CurrentSceneEffect = result;
				return;
			}

			result.anyActive = true;

			shortList.Sort(AtmosWeight.InvertedCompare);
			int avfxFields = 0;

			for (int i = 0; avfxFields < 5 && i < shortList.Count; i++) {
				ModSceneEffect avfx = shortList[i].type;

				if (result.waterStyle.priority == 0 && avfx.WaterStyle != null) {
					result.waterStyle.value = avfx.WaterStyle.Slot;
					result.waterStyle.priority = avfx.Priority;
					avfxFields++;
				}

				if (result.undergroundBackground.priority == 0 && avfx.UndergroundBackgroundStyle != null) {
					result.undergroundBackground.value = avfx.UndergroundBackgroundStyle.Slot;
					result.undergroundBackground.priority = avfx.Priority;
					avfxFields++;
				}

				if (result.surfaceBackground.priority == 0 && avfx.SurfaceBackgroundStyle != null) {
					result.surfaceBackground.value = avfx.SurfaceBackgroundStyle.Slot;
					result.surfaceBackground.priority = avfx.Priority;
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

			player.CurrentSceneEffect = result;
		}

		// Ref or out? MusicLoader?
		public void UpdateMusic(ref int music, ref SceneEffectPriority priority) {
			int tst = Main.LocalPlayer.CurrentSceneEffect.music.value;
			if (tst > -1) {
				music = tst;
				priority = Main.LocalPlayer.CurrentSceneEffect.music.priority;
			}
		}
	}
}
