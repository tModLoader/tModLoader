using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.GameContent;
using Terraria.GameContent.Liquid;

namespace Terraria.ModLoader;

[Autoload(Side = ModSide.Client)]
public class WaterFallStylesLoader : SceneEffectLoader<ModWaterfallStyle>
{
	public WaterFallStylesLoader() => Initialize(WaterfallManager.maxTypes);

	internal override void ResizeArrays()
	{
		//Textures
		Array.Resize(ref Main.instance.waterfallManager.waterfallTexture, TotalCount);
	}
}

[Autoload(Side = ModSide.Client)]
public class WaterStylesLoader : SceneEffectLoader<ModWaterStyle>
{
	public WaterStylesLoader() => Initialize(Main.maxLiquidTypes);

	internal override void ResizeArrays()
	{
		//Textures
		Array.Resize(ref TextureAssets.Liquid, TotalCount);
		Array.Resize(ref TextureAssets.LiquidSlope, TotalCount);
		Array.Resize(ref LiquidRenderer.Instance._liquidTextures, TotalCount);

		//Etc
		Array.Resize(ref Main.liquidAlpha, TotalCount);
	}

	public override void ChooseStyle(out int style, out SceneEffectPriority priority)
	{
		int tst = Main.LocalPlayer.CurrentSceneEffect.waterStyle.value;
		style = -1;
		priority = SceneEffectPriority.None;

		if (tst >= VanillaCount) {
			style = tst;
			priority = Main.LocalPlayer.CurrentSceneEffect.waterStyle.priority;
		}
	}

	public void UpdateLiquidAlphas()
	{
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

	public void DrawWaterfall(WaterfallManager waterfallManager)
	{
		foreach (ModWaterStyle waterStyle in list) {
			if (Main.liquidAlpha[waterStyle.Slot] > 0f) {
				waterfallManager.DrawWaterfall(waterStyle.ChooseWaterfallStyle(), Main.liquidAlpha[waterStyle.Slot]);
			}
		}
	}

	public void LightColorMultiplier(int style, float factor, ref float r, ref float g, ref float b)
	{
		ModWaterStyle waterStyle = Get(style);

		if (waterStyle != null) {
			waterStyle.LightColorMultiplier(ref r, ref g, ref b);

			r *= factor;
			g *= factor;
			b *= factor;
		}
	}
}