using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.GameContent;
using Terraria.GameContent.Liquid;

namespace Terraria.ModLoader
{
	[Autoload(Side = ModSide.Client)]
	public class WaterFallStylesLoader : SceneEffectLoader<ModWaterfallStyle>
	{
		public WaterFallStylesLoader() => Initialize(WaterfallManager.maxTypes);

		internal override void ResizeArrays() {
			//Textures
			Array.Resize(ref Main.instance.waterfallManager.waterfallTexture, TotalCount);
		}
	}

	[Autoload(Side = ModSide.Client)]
	public class WaterStylesLoader : SceneEffectLoader<ModWaterStyle>
	{
		public WaterStylesLoader() => Initialize(Main.maxLiquidTypes);

		internal override void ResizeArrays() {
			//Textures
			Array.Resize(ref TextureAssets.Liquid, TotalCount);
			Array.Resize(ref LiquidRenderer.Instance._liquidTextures, TotalCount);

			//Etc
			Array.Resize(ref Main.liquidAlpha, TotalCount);
		}

		public override void ChooseStyle(out int style, out SceneEffectPriority priority) {
			int tst = Main.LocalPlayer.CurrentSceneEffect.waterStyle.value;
			style = -1; priority = SceneEffectPriority.None;

			if (tst >= VanillaCount) {
				style = tst;
				priority = Main.LocalPlayer.CurrentSceneEffect.waterStyle.priority;
			}
		}

		public void UpdateLiquidAlphas() {
			if (Main.waterStyle >= VanillaCount) {
				for (int k = 0; k < VanillaCount; k++) {
					if (k == 1 || k == 11) {
						continue;
					}
					Main.liquidAlpha[k] -= 0.2f;
					if (Main.liquidAlpha[k] < 0f) {
						Main.liquidAlpha[k] = 0f;
					}
				}
			}
			foreach (ModWaterStyle waterStyle in list) {
				int type = waterStyle.Slot;
				if (Main.waterStyle == type) {
					Main.liquidAlpha[type] += 0.2f;
					if (Main.liquidAlpha[type] > 1f) {
						Main.liquidAlpha[type] = 1f;
					}
				}
				else {
					Main.liquidAlpha[type] -= 0.2f;
					if (Main.liquidAlpha[type] < 0f) {
						Main.liquidAlpha[type] = 0f;
					}
				}
			}
		}

		public void DrawWatersToScreen(bool bg) {
			for (int k = VanillaCount; k < TotalCount; k++) {
				if (Main.liquidAlpha[k] > 0f) {
					if (bg) {
						if (Main.waterStyle < k) {
							Main.instance.DrawWater(bg, k, Main.liquidAlpha[k]);
						}
						else {
							Main.instance.DrawWater(bg, k, 1f);
						}
					}
					else {
						Main.instance.DrawWater(bg, k, Main.liquidAlpha[k]);
					}
				}
			}
		}

		public void DrawWaterfall(WaterfallManager waterfallManager, SpriteBatch spriteBatch) {
			foreach (ModWaterStyle waterStyle in list) {
				if (Main.liquidAlpha[waterStyle.Slot] > 0f) {
					waterfallManager.DrawWaterfall(spriteBatch, waterStyle.ChooseWaterfallStyle(), Main.liquidAlpha[waterStyle.Slot]);
				}
			}
		}

		public void LightColorMultiplier(int style, ref float r, ref float g, ref float b) {
			ModWaterStyle waterStyle = Get(style);

			if (waterStyle != null) {
				waterStyle.LightColorMultiplier(ref r, ref g, ref b);

				r *= Lighting.LegacyEngine._negLight * Lighting.LegacyEngine._blueWave;
				g *= Lighting.LegacyEngine._negLight * Lighting.LegacyEngine._blueWave;
				b *= Lighting.LegacyEngine._negLight * Lighting.LegacyEngine._blueWave;

				//TODO: Make this work with the new lighting engine.
			}
		}
	}
}