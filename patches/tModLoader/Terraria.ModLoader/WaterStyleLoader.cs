using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.GameContent.Liquid;

namespace Terraria.ModLoader
{
	//todo: further documentation
	/// <summary>
	/// This serves as the central class from which WaterStyle functions are supported and carried out.
	/// </summary>
	public static class WaterStyleLoader
	{
		/// <summary>
		/// The maximum amount of liquids in vanilla.
		/// </summary>
		public const int vanillaWaterCount = Main.maxLiquidTypes;
		private static int nextWaterStyle = vanillaWaterCount;
		internal static readonly IList<ModWaterStyle> waterStyles = new List<ModWaterStyle>();

		/// <summary>
		/// The number of water styles that exist in the game, both vanilla and modded.
		/// </summary>
		public static int WaterStyleCount => nextWaterStyle;

		internal static int ReserveStyle() {
			int reserve = nextWaterStyle;
			nextWaterStyle++;
			return reserve;
		}

		/// <summary>
		/// Returns the ModWaterStyle with the given ID.
		/// </summary>
		public static ModWaterStyle GetWaterStyle(int style) {
			if (style < vanillaWaterCount || style >= nextWaterStyle) {
				return null;
			}
			return waterStyles[style - vanillaWaterCount];
		}

		internal static void ResizeArrays() {
			Array.Resize(ref LiquidRenderer.Instance._liquidTextures, nextWaterStyle);
			Array.Resize(ref Main.liquidAlpha, nextWaterStyle);
			Array.Resize(ref Main.liquidTexture, nextWaterStyle);
		}

		internal static void Unload() {
			nextWaterStyle = vanillaWaterCount;
			waterStyles.Clear();
		}

		public static void ChooseWaterStyle(ref int style) {
			foreach (ModWaterStyle waterStyle in waterStyles) {
				if (waterStyle.ChooseWaterStyle()) {
					style = waterStyle.Type;
				}
			}
			WorldHooks.ChooseWaterStyle(ref style);
		}

		public static void UpdateLiquidAlphas() {
			if (Main.waterStyle >= vanillaWaterCount) {
				for (int k = 0; k < vanillaWaterCount; k++) {
					if (k == 1 || k == 11) {
						continue;
					}
					Main.liquidAlpha[k] -= 0.2f;
					if (Main.liquidAlpha[k] < 0f) {
						Main.liquidAlpha[k] = 0f;
					}
				}
			}
			foreach (ModWaterStyle waterStyle in waterStyles) {
				int type = waterStyle.Type;
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

		public static void DrawWatersToScreen(bool bg) {
			for (int k = vanillaWaterCount; k < nextWaterStyle; k++) {
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

		public static void DrawWaterfall(WaterfallManager waterfallManager, SpriteBatch spriteBatch) {
			foreach (ModWaterStyle waterStyle in waterStyles) {
				if (Main.liquidAlpha[waterStyle.Type] > 0f) {
					waterfallManager.DrawWaterfall(spriteBatch, waterStyle.ChooseWaterfallStyle(),
						Main.liquidAlpha[waterStyle.Type]);
				}
			}
		}

		public static void LightColorMultiplier(int style, ref float r, ref float g, ref float b) {
			ModWaterStyle waterStyle = GetWaterStyle(style);
			if (waterStyle != null) {
				waterStyle.LightColorMultiplier(ref r, ref g, ref b);
				r *= Lighting.negLight * Lighting.blueWave;
				g *= Lighting.negLight * Lighting.blueWave;
				b *= Lighting.negLight * Lighting.blueWave;
			}
		}
	}

	public static class WaterfallStyleLoader
	{
		public const int vanillaWaterfallCount = WaterfallManager.maxTypes;
		private static int nextWaterfallStyle = vanillaWaterfallCount;
		internal static readonly IList<ModWaterfallStyle> waterfallStyles = new List<ModWaterfallStyle>();

		internal static int ReserveStyle() {
			int reserve = nextWaterfallStyle;
			nextWaterfallStyle++;
			return reserve;
		}

		/// <summary>
		/// Returns the ModWaterfallStyle with the given ID.
		/// </summary>
		public static ModWaterfallStyle GetWaterfallStyle(int style) {
			if (style < vanillaWaterfallCount || style >= nextWaterfallStyle) {
				return null;
			}
			return waterfallStyles[style - vanillaWaterfallCount];
		}

		internal static void ResizeArrays() {
			Array.Resize(ref Main.instance.waterfallManager.waterfallTexture, nextWaterfallStyle);
		}

		internal static void Unload() {
			nextWaterfallStyle = vanillaWaterfallCount;
			waterfallStyles.Clear();
		}
	}
}
