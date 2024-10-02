using System.Collections.Generic;
using Terraria.Graphics.Capture;

namespace Terraria.ModLoader;

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
			public static readonly PrioritizedPair Default = new() {
				value = -1
			};

			public int value;
			public SceneEffectPriority priority;
		}

		public bool anyActive;
		public PrioritizedPair waterStyle;
		public PrioritizedPair undergroundBackground;
		public PrioritizedPair surfaceBackground;
		public PrioritizedPair music;
		public string mapBackground;
		public ModSceneEffect mapBackgroundSceneEffect;
		public CaptureBiome.TileColorStyle tileColorStyle;
		public PrioritizedPair biomeTorchItemType;
		public PrioritizedPair biomeCampfireItemType;

		public SceneEffectInstance()
		{
			waterStyle = undergroundBackground = surfaceBackground = music = biomeTorchItemType = biomeCampfireItemType = PrioritizedPair.Default;
			tileColorStyle = CaptureBiome.TileColorStyle.Normal;
			mapBackground = null;
		}
	}

	private struct AtmosWeight
	{
		public float weight;
		public ModSceneEffect type;

		public AtmosWeight(float weight, ModSceneEffect type)
		{
			this.weight = weight;
			this.type = type;
		}

		public static int InvertedCompare(AtmosWeight a, AtmosWeight b) => -a.weight.CompareTo(b.weight);
	}

	public void UpdateSceneEffect(Player player)
	{
		var result = new SceneEffectInstance();
		List<AtmosWeight> shortList = new List<AtmosWeight>();

		for (int i = 0; i < list.Count; i++) {
			ModSceneEffect sceneEffect = list[i];

			bool isActive = sceneEffect.IsSceneEffectActive(player);

			sceneEffect.SpecialVisuals(player, isActive);
			if (!isActive)
				continue;

			shortList.Add(
				new AtmosWeight(sceneEffect.GetCorrWeight(player), sceneEffect)
			);
		}

		if (shortList.Count == 0) {
			player.CurrentSceneEffect = result;
			return;
		}

		result.anyActive = true;

		shortList.Sort(AtmosWeight.InvertedCompare);
		int sceneEffectFields = 0;

		for (int i = 0; sceneEffectFields < 8 && i < shortList.Count; i++) {
			ModSceneEffect sceneEffect = shortList[i].type;

			if (result.waterStyle.priority == 0 && sceneEffect.WaterStyle != null) {
				result.waterStyle.value = sceneEffect.WaterStyle.Slot;
				result.waterStyle.priority = sceneEffect.Priority;
				sceneEffectFields++;
			}

			if (result.undergroundBackground.priority == 0 && sceneEffect.UndergroundBackgroundStyle != null) {
				result.undergroundBackground.value = sceneEffect.UndergroundBackgroundStyle.Slot;
				result.undergroundBackground.priority = sceneEffect.Priority;
				sceneEffectFields++;
			}

			if (result.surfaceBackground.priority == 0 && sceneEffect.SurfaceBackgroundStyle != null) {
				result.surfaceBackground.value = sceneEffect.SurfaceBackgroundStyle.Slot;
				result.surfaceBackground.priority = sceneEffect.Priority;
				sceneEffectFields++;
			}

			if (result.music.priority == 0 && sceneEffect.Music != -1) {
				result.music.value = sceneEffect.Music;
				result.music.priority = sceneEffect.Priority;
				sceneEffectFields++;
			}

			if (sceneEffect is ModBiome modBiome) {
				if (result.biomeTorchItemType.priority == 0 && modBiome.BiomeTorchItemType != -1) {
					result.biomeTorchItemType.value = modBiome.BiomeTorchItemType;
					result.biomeTorchItemType.priority = modBiome.Priority;
					sceneEffectFields++;
				}

				if (result.biomeCampfireItemType.priority == 0 && modBiome.BiomeCampfireItemType != -1) {
					result.biomeCampfireItemType.value = modBiome.BiomeCampfireItemType;
					result.biomeCampfireItemType.priority = modBiome.Priority;
					sceneEffectFields++;
				}
			}

			if (result.tileColorStyle == CaptureBiome.TileColorStyle.Normal && sceneEffect.TileColorStyle != CaptureBiome.TileColorStyle.Normal) {
				result.tileColorStyle = sceneEffect.TileColorStyle;
				sceneEffectFields++;
			}

			if (result.mapBackground == null && sceneEffect.MapBackground != null) {
				result.mapBackground = sceneEffect.MapBackground;
				result.mapBackgroundSceneEffect = sceneEffect;
				sceneEffectFields++;
			}
		}

		player.CurrentSceneEffect = result;
	}

	// Ref or out? MusicLoader?
	public void UpdateMusic(ref int music, ref SceneEffectPriority priority)
	{
		var currentMusic = Main.LocalPlayer.CurrentSceneEffect.music;

		if (currentMusic.value > -1 && currentMusic.priority > priority) {
			music = currentMusic.value;
			priority = currentMusic.priority;
		}
	}
}
