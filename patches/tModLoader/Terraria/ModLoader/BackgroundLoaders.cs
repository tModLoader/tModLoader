using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.Localization;

namespace Terraria.ModLoader;

/// <summary>
/// This is the class that keeps track of all modded background textures and their slots/IDs.
/// <para/> Remember that unless you manually register backgrounds via <see cref="AddBackgroundTexture(Mod, string)"/>, only files found in a folder or subfolder of a folder named "Backgrounds" will be autoloaded as background textures.
/// </summary>
//TODO: Further documentation.
[Autoload(Side = ModSide.Client)]
public sealed class BackgroundTextureLoader : Loader
{
	private static BackgroundTextureLoader Instance => LoaderManager.Get<BackgroundTextureLoader>();

	internal static IDictionary<string, int> backgrounds = new Dictionary<string, int>();

	public BackgroundTextureLoader()
	{
		Initialize(Main.maxBackgrounds);
	}

	/// <summary> Returns the slot/ID of the background texture with the given full path. The path must be prefixed with a mod name. Throws exceptions on failure. </summary>
	public static int GetBackgroundSlot(string texture) => backgrounds[texture];

	/// <summary> Returns the slot/ID of the background texture with the given mod and path. Throws exceptions on failure. </summary>
	public static int GetBackgroundSlot(Mod mod, string texture) => GetBackgroundSlot($"{mod.Name}/{texture}");

	/// <summary> Safely attempts to output the slot/ID of the background texture with the given full path. The path must be prefixed with a mod name. </summary>
	public static bool TryGetBackgroundSlot(string texture, out int slot) => backgrounds.TryGetValue(texture, out slot);

	/// <summary> Safely attempts to output the slot/ID of the background texture with the given mod and path. </summary>
	public static bool TryGetBackgroundSlot(Mod mod, string texture, out int slot) => TryGetBackgroundSlot($"{mod.Name}/{texture}", out slot);

	/// <summary>
	/// Adds a texture to the list of background textures and assigns it a background texture slot.
	/// <para/> Use this for any background textures not autoloaded by the <see cref="Mod.BackgroundAutoloadingEnabled"/> logic.
	/// </summary>
	/// <param name="mod">The mod that owns this background.</param>
	/// <param name="texture">The texture.</param>
	public static void AddBackgroundTexture(Mod mod, string texture)
	{
		if (mod == null)
			throw new ArgumentNullException(nameof(mod));

		if (texture == null)
			throw new ArgumentNullException(nameof(texture));

		if (!mod.loading)
			throw new Exception(Language.GetTextValue("tModLoader.LoadErrorNotLoading"));

		ModContent.Request<Texture2D>(texture);

		backgrounds[texture] = Instance.Reserve();
	}

	internal override void ResizeArrays()
	{
		Array.Resize(ref TextureAssets.Background, TotalCount);
		Array.Resize(ref Main.backgroundHeight, TotalCount);
		Array.Resize(ref Main.backgroundWidth, TotalCount);

		foreach (string texture in backgrounds.Keys) {
			int slot = backgrounds[texture];
			var tex = ModContent.Request<Texture2D>(texture);

			TextureAssets.Background[slot] = tex;
			Main.backgroundWidth[slot] = tex.Width();
			Main.backgroundHeight[slot] = tex.Height();
		}
	}

	internal override void Unload()
	{
		base.Unload();

		backgrounds.Clear();
	}

	internal static void AutoloadBackgrounds(Mod mod)
	{
		foreach (string fullTexturePath in mod.RootContentSource.EnumerateAssets().Where(t => t.Contains("Backgrounds/"))) {
			string texturePath = Path.ChangeExtension(fullTexturePath, null);
			string textureKey = $"{mod.Name}/{texturePath}";

			AddBackgroundTexture(mod, textureKey);
		}
	}
}

/// <summary>
/// This serves as the central class from which ModUndergroundBackgroundStyle functions are supported and carried out.
/// </summary>
[Autoload(Side = ModSide.Client)]
public class UndergroundBackgroundStylesLoader : SceneEffectLoader<ModUndergroundBackgroundStyle>
{
	public const int VanillaUndergroundBackgroundStylesCount = 22;

	public UndergroundBackgroundStylesLoader()
	{
		Initialize(VanillaUndergroundBackgroundStylesCount);
	}

	public override void ChooseStyle(out int style, out SceneEffectPriority priority)
	{
		priority = SceneEffectPriority.None;
		style = -1;

		if (!GlobalBackgroundStyleLoader.loaded) {
			return;
		}

		int playerUndergroundBackground = Main.LocalPlayer.CurrentSceneEffect.undergroundBackground.value;

		if (playerUndergroundBackground >= VanillaCount) {
			style = playerUndergroundBackground;
			priority = Main.LocalPlayer.CurrentSceneEffect.undergroundBackground.priority;
		}
	}

	public void FillTextureArray(int style, int[] textureSlots)
	{
		if (!GlobalBackgroundStyleLoader.loaded) {
			return;
		}

		Get(style)?.FillTextureArray(textureSlots);

		foreach (var hook in GlobalBackgroundStyleLoader.HookFillUndergroundTextureArray) {
			hook(style, textureSlots);
		}
	}
}

[Autoload(Side = ModSide.Client)]
public class SurfaceBackgroundStylesLoader : SceneEffectLoader<ModSurfaceBackgroundStyle>
{
	internal static bool loaded = false;

	public SurfaceBackgroundStylesLoader()
	{
		Initialize(Main.BG_STYLES_COUNT);
	}

	internal override void ResizeArrays()
	{
		Array.Resize(ref Main.bgAlphaFrontLayer, TotalCount);
		Array.Resize(ref Main.bgAlphaFarBackLayer, TotalCount);
		loaded = true;
	}

	internal override void Unload()
	{
		base.Unload();
		loaded = false;
	}

	public override void ChooseStyle(out int style, out SceneEffectPriority priority)
	{
		priority = SceneEffectPriority.None;
		style = -1;

		if (!loaded || !GlobalBackgroundStyleLoader.loaded) {
			return;
		}

		int playerSurfaceBackground = Main.LocalPlayer.CurrentSceneEffect.surfaceBackground.value;

		if (playerSurfaceBackground >= VanillaCount) {
			style = playerSurfaceBackground;
			priority = Main.LocalPlayer.CurrentSceneEffect.surfaceBackground.priority;
		}
	}

	public void ModifyFarFades(int style, float[] fades, float transitionSpeed)
	{
		if (!GlobalBackgroundStyleLoader.loaded) {
			return;
		}

		Get(style)?.ModifyFarFades(fades, transitionSpeed);

		foreach (var hook in GlobalBackgroundStyleLoader.HookModifyFarSurfaceFades) {
			hook(style, fades, transitionSpeed);
		}
	}

	public void DrawFarTexture()
	{
		if (!GlobalBackgroundStyleLoader.loaded || MenuLoader.loading) {
			return;
		}

		//TODO: This suppresses an error instead of fixing it.
		// Avoids background flicker during load because Main.bgAlphaFarBackLayer is resized after surfaceBackgroundStyles is added to in AutoLoad.
		if (TotalCount != Main.bgAlphaFarBackLayer.Length) {
			return;
		}

		foreach (var style in list) {
			int slot = style.Slot;
			float alpha = Main.bgAlphaFarBackLayer[slot];

			Main.ColorOfSurfaceBackgroundsModified = Main.ColorOfSurfaceBackgroundsBase * alpha;

			if (alpha <= 0f) {
				continue;
			}

			int textureSlot = style.ChooseFarTexture();

			if (textureSlot < 0 || textureSlot >= TextureAssets.Background.Length) {
				continue;
			}

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

	public void DrawMiddleTexture()
	{
		if (!GlobalBackgroundStyleLoader.loaded || MenuLoader.loading) {
			return;
		}

		foreach (var style in list) {
			int slot = style.Slot;
			float alpha = Main.bgAlphaFarBackLayer[slot];

			Main.ColorOfSurfaceBackgroundsModified = Main.ColorOfSurfaceBackgroundsBase * alpha;

			if (alpha <= 0f) {
				continue;
			}

			int textureSlot = style.ChooseMiddleTexture();

			if (textureSlot < 0 || textureSlot >= TextureAssets.Background.Length) {
				continue;
			}

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

	public void DrawCloseBackground(int style)
	{
		if (!GlobalBackgroundStyleLoader.loaded || MenuLoader.loading) {
			return;
		}

		if (Main.bgAlphaFrontLayer[style] <= 0f) {
			return;
		}

		var surfaceBackgroundStyle = Get(style);

		if (surfaceBackgroundStyle == null || !surfaceBackgroundStyle.PreDrawCloseBackground(Main.spriteBatch)) {
			return;
		}

		Main.bgScale = 1.25f;
		Main.instance.bgParallax = 0.37;

		float a = 1800.0f;
		float b = 1750.0f;
		int textureSlot = surfaceBackgroundStyle.ChooseCloseTexture(ref Main.bgScale, ref Main.instance.bgParallax, ref a, ref b);

		if (textureSlot < 0 || textureSlot >= TextureAssets.Background.Length) {
			return;
		}

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

internal static class GlobalBackgroundStyleLoader
{
	internal static readonly IList<GlobalBackgroundStyle> globalBackgroundStyles = new List<GlobalBackgroundStyle>();

	internal static bool loaded = false;

	// Hooks

	internal delegate void DelegateChooseUndergroundBackgroundStyle(ref int style);
	internal delegate void DelegateChooseSurfaceBackgroundStyle(ref int style);

	internal static DelegateChooseUndergroundBackgroundStyle[] HookChooseUndergroundBackgroundStyle;
	internal static DelegateChooseSurfaceBackgroundStyle[] HookChooseSurfaceBackgroundStyle;
	internal static Action<int, int[]>[] HookFillUndergroundTextureArray;
	internal static Action<int, float[], float>[] HookModifyFarSurfaceFades;

	internal static void ResizeAndFillArrays(bool unloading = false)
	{
		// .NET 6 SDK bug: https://github.com/dotnet/roslyn/issues/57517
		// Remove generic arguments once fixed.
		ModLoader.BuildGlobalHook<GlobalBackgroundStyle, DelegateChooseUndergroundBackgroundStyle>(ref HookChooseUndergroundBackgroundStyle, globalBackgroundStyles, g => g.ChooseUndergroundBackgroundStyle);
		ModLoader.BuildGlobalHook<GlobalBackgroundStyle, DelegateChooseSurfaceBackgroundStyle>(ref HookChooseSurfaceBackgroundStyle, globalBackgroundStyles, g => g.ChooseSurfaceBackgroundStyle);
		ModLoader.BuildGlobalHook(ref HookFillUndergroundTextureArray, globalBackgroundStyles, g => g.FillUndergroundTextureArray);
		ModLoader.BuildGlobalHook(ref HookModifyFarSurfaceFades, globalBackgroundStyles, g => g.ModifyFarSurfaceFades);

		if (!unloading) {
			loaded = true;
		}
	}

	internal static void Unload()
	{
		loaded = false;
		globalBackgroundStyles.Clear();
	}
}
