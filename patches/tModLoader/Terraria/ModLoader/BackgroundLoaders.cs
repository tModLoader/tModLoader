using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.Graphics.Effects;

namespace Terraria.ModLoader
{
	//todo: further documentation
	/// <summary>
	/// This is the class that keeps track of all modded background textures and their slots/IDs.
	/// </summary>
	public static class BackgroundTextureLoader
	{
		public const int vanillaBackgroundTextureCount = Main.maxBackgrounds;
		private static int nextBackground = vanillaBackgroundTextureCount;
		internal static IDictionary<string, int> backgrounds = new Dictionary<string, int>();

		internal static int ReserveBackgroundSlot() {
			int reserve = nextBackground;
			nextBackground++;
			return reserve;
		}

		/// <summary>
		/// Returns the slot/ID of the background texture with the given name.
		/// </summary>
		public static int GetBackgroundSlot(string texture) => backgrounds.TryGetValue(texture, out int slot) ? slot : -1;

		internal static void ResizeAndFillArrays() {
			Array.Resize(ref TextureAssets.Background, nextBackground);
			Array.Resize(ref Main.backgroundHeight, nextBackground);
			Array.Resize(ref Main.backgroundWidth, nextBackground);

			foreach (string texture in backgrounds.Keys) {
				int slot = backgrounds[texture];
				var tex = ModContent.GetTexture(texture);

				TextureAssets.Background[slot] = tex;
				Main.backgroundWidth[slot] = tex.Width();
				Main.backgroundHeight[slot] = tex.Height();
			}
		}

		internal static void Unload() {
			nextBackground = vanillaBackgroundTextureCount;
			backgrounds.Clear();
		}
	}

	//todo: further documentation
	/// <summary>
	/// This serves as the central class from which ModUndergroundBackgroundStyle functions are supported and carried out.
	/// </summary>
	public class UndergroundBackgroundStylesLoader : SceneEffectLoader<ModUndergroundBackgroundStyle>
	{
		public const int VanillaUndergroundBackgroundStylesCount = 18;
		public UndergroundBackgroundStylesLoader() => Initialize(VanillaUndergroundBackgroundStylesCount);

		public override void ChooseStyle(out int style, out SceneEffectPriority priority) {
			priority = SceneEffectPriority.None; style = -1;
			if (!GlobalBackgroundStyleLoader.loaded) {
				return;
			}

			int tst = Main.LocalPlayer.currentSceneEffect.undergroundBackground.value;
			if (tst >= VanillaCount) {
				style = tst;
				priority = Main.LocalPlayer.currentSceneEffect.undergroundBackground.priority;
			}
		}

		public void FillTextureArray(int style, int[] textureSlots) {
			if (!GlobalBackgroundStyleLoader.loaded) {
				return;
			}
			var undergroundBackgroundStyle = Get(style);
			if (undergroundBackgroundStyle != null) {
				undergroundBackgroundStyle.FillTextureArray(textureSlots);
			}
			foreach (var hook in GlobalBackgroundStyleLoader.HookFillUndergroundTextureArray) {
				hook(style, textureSlots);
			}
		}
	}

	public class SurfaceBackgroundStylesLoader : SceneEffectLoader<ModSurfaceBackgroundStyle>
	{
		public SurfaceBackgroundStylesLoader() => Initialize(Main.BG_STYLES_COUNT);

		internal override void ResizeArrays() {
			Array.Resize(ref Main.bgAlphaFrontLayer, TotalCount);
			Array.Resize(ref Main.bgAlphaFarBackLayer, TotalCount);
		}

		public override void ChooseStyle(out int style, out SceneEffectPriority priority) {
			priority = SceneEffectPriority.None; style = -1;
			if (!GlobalBackgroundStyleLoader.loaded) {
				return;
			}
			int tst = Main.LocalPlayer.currentSceneEffect.surfaceBackground.value;
			if (tst >= VanillaCount) {
				style = tst;
				priority = Main.LocalPlayer.currentSceneEffect.surfaceBackground.priority;
			}
		}

		public void ModifyFarFades(int style, float[] fades, float transitionSpeed) {
			if (!GlobalBackgroundStyleLoader.loaded) {
				return;
			}
			var surfaceBackgroundStyle = Get(style);
			if (surfaceBackgroundStyle != null) {
				surfaceBackgroundStyle.ModifyFarFades(fades, transitionSpeed);
			}
			foreach (var hook in GlobalBackgroundStyleLoader.HookModifyFarSurfaceFades) {
				hook(style, fades, transitionSpeed);
			}
		}

		public void DrawFarTexture() {
			if (!GlobalBackgroundStyleLoader.loaded) {
				return;
			}

			// TODO: Causes background to flicker during load because Main.bgAlpha2 is resized after surfaceBackgroundStyles is added to in AutoLoad.
			foreach (var style in list) {
				int slot = style.Slot;
				float alpha = Main.bgAlphaFarBackLayer[slot];

				Main.ColorOfSurfaceBackgroundsModified = Main.ColorOfSurfaceBackgroundsBase * alpha;

				if (alpha > 0f) {
					int textureSlot = style.ChooseFarTexture();

					if (textureSlot >= 0 && textureSlot < TextureAssets.Background.Length) {
						Main.instance.LoadBackground(textureSlot);

						for (int k = 0; k < Main.instance.bgLoops; k++) {
							Main.spriteBatch.Draw(
								TextureAssets.Background[textureSlot].Value,
								new Vector2(Main.instance.bgStartX + Main.bgWidthScaled * k, Main.instance.bgTopY),
								new Rectangle(0, 0, Main.backgroundWidth[textureSlot], Main.backgroundHeight[textureSlot]),
								Main.ColorOfSurfaceBackgroundsModified,
								0f,
								default,
								Main.bgScale,
								SpriteEffects.None,
								0f
							);
						}
					}
				}
			}
		}

		public void DrawMiddleTexture() {
			if (!GlobalBackgroundStyleLoader.loaded) {
				return;
			}

			foreach (var style in list) {
				int slot = style.Slot;
				float alpha = Main.bgAlphaFarBackLayer[slot];

				Main.ColorOfSurfaceBackgroundsModified = Main.ColorOfSurfaceBackgroundsBase * alpha;

				if (alpha > 0f) {
					int textureSlot = style.ChooseMiddleTexture();

					if (textureSlot >= 0 && textureSlot < TextureAssets.Background.Length) {
						Main.instance.LoadBackground(textureSlot);

						for (int k = 0; k < Main.instance.bgLoops; k++) {
							Main.spriteBatch.Draw(
								TextureAssets.Background[textureSlot].Value,
								new Vector2(Main.instance.bgStartX + Main.bgWidthScaled * k, Main.instance.bgTopY),
								new Rectangle(0, 0, Main.backgroundWidth[textureSlot], Main.backgroundHeight[textureSlot]),
								Main.ColorOfSurfaceBackgroundsModified,
								0f,
								default,
								Main.bgScale,
								SpriteEffects.None,
								0f
							);
						}
					}
				}
			}
		}

		public void DrawCloseBackground(int style) {
			if (!GlobalBackgroundStyleLoader.loaded) {
				return;
			}

			if (Main.bgAlphaFrontLayer[style] <= 0f) {
				return;
			}

			var surfaceBackgroundStyle = Get(style);

			if (surfaceBackgroundStyle != null && surfaceBackgroundStyle.PreDrawCloseBackground(Main.spriteBatch)) {
				Main.bgScale = 1.25f;
				Main.instance.bgParallax = 0.37;

				float a = 1800.0f;
				float b = 1750.0f;
				int textureSlot = surfaceBackgroundStyle.ChooseCloseTexture(ref Main.bgScale, ref Main.instance.bgParallax, ref a, ref b);

				if (textureSlot >= 0 && textureSlot < TextureAssets.Background.Length) {
					//Custom: bgScale, textureslot, patallaz, these 2 numbers...., Top and Start?
					Main.instance.LoadBackground(textureSlot);
					Main.bgScale *= 2f;
					Main.bgWidthScaled = (int)((float)Main.backgroundWidth[textureSlot] * Main.bgScale);

					SkyManager.Instance.DrawToDepth(Main.spriteBatch, 1f / (float)Main.instance.bgParallax);

					Main.instance.bgStartX = (int)(-Math.IEEERemainder(Main.screenPosition.X * Main.instance.bgParallax, Main.bgWidthScaled) - (Main.bgWidthScaled / 2));
					Main.instance.bgTopY = (int)((-Main.screenPosition.Y + Main.instance.screenOff / 2f) / (Main.worldSurface * 16.0) * a + b) + (int)Main.instance.scAdj;

					if (Main.gameMenu) {
						Main.instance.bgTopY = 320;
					}

					Main.instance.bgLoops = Main.screenWidth / Main.bgWidthScaled + 2;

					if (Main.screenPosition.Y < Main.worldSurface * 16.0 + 16.0) {
						for (int k = 0; k < Main.instance.bgLoops; k++) {
							Main.spriteBatch.Draw(
								TextureAssets.Background[textureSlot].Value,
								new Vector2(Main.instance.bgStartX + Main.bgWidthScaled * k, Main.instance.bgTopY),
								new Rectangle(0, 0, Main.backgroundWidth[textureSlot], Main.backgroundHeight[textureSlot]),
								Main.ColorOfSurfaceBackgroundsModified,
								0f,
								default,
								Main.bgScale,
								SpriteEffects.None,
								0f
							);
						}
					}
				}
			}
		}
	}


	internal static class GlobalBackgroundStyleLoader
	{
		internal static readonly IList<GlobalBackgroundStyle> globalBackgroundStyles = new List<GlobalBackgroundStyle>();
		internal static bool loaded = false;
		internal static Action<int, int[]>[] HookFillUndergroundTextureArray;
		internal static Action<int, float[], float>[] HookModifyFarSurfaceFades;

		internal static void ResizeAndFillArrays(bool unloading = false) {
			ModLoader.BuildGlobalHook(ref HookFillUndergroundTextureArray, globalBackgroundStyles, g => g.FillUndergroundTextureArray);
			ModLoader.BuildGlobalHook(ref HookModifyFarSurfaceFades, globalBackgroundStyles, g => g.ModifyFarSurfaceFades);

			if (!unloading) {
				loaded = true;
			}
		}

		internal static void Unload() {
			loaded = false;
			globalBackgroundStyles.Clear();
		}
	}
}
