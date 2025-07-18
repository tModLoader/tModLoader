#nullable enable

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;

namespace Terraria.GameContent.Liquid;

/// <summary>
/// Responsible for special rendering of liquid edges/slopes for the rewritten
/// liquid slope handling.
/// </summary>
/// <remarks>
/// See the related pull request:
/// https://github.com/tModLoader/tModLoader/pull/4714
/// </remarks>
public static class LiquidEdgeRenderer
{
	/// <summary>
	/// Whether the special edge rendering logic is enabled.
	/// <br />
	/// Even if it's enabled, it will only apply if <see cref="Active"/>
	/// is <see langword="true"/>.
	/// </summary>
	public static bool Enabled = true;

	/// <summary>
	/// Whether the new rendering is actually active for this frame.
	/// </summary>
	public static bool Active => Enabled && Lighting.Mode is Graphics.Light.LightMode.Color or Graphics.Light.LightMode.White;

	internal static bool NeedsToInitializeTargets => TargetNeedsInitializing(BackMaskTarget) || TargetNeedsInitializing(FrontMaskTarget);

	public static RenderTarget2D? BackMaskTarget;
	public static RenderTarget2D? FrontMaskTarget;

	public static Effect MaskShader => (maskAsset ??= ModLoader.ModLoader.ManifestAssets.Request<Effect>("Terraria.GameContent.Liquid.LiquidMask", AssetRequestMode.ImmediateLoad)).Value;

	private static Asset<Effect>? maskAsset;

	/// <summary>
	/// Tiles which mask rendered liquid (tiles on the edge of bodies of
	/// liquid).
	/// </summary>
	public static Dictionary<Point, LiquidEdgeCache> Edges { get; } = [];

	internal static void InitTargets(GraphicsDevice gd, int width, int height, SurfaceFormat surfaceFormat)
	{
		BackMaskTarget = new RenderTarget2D(gd, width, height, mipMap: false, surfaceFormat, DepthFormat.None);
		FrontMaskTarget = new RenderTarget2D(gd, width, height, mipMap: false, surfaceFormat, DepthFormat.None);
	}

	internal static void DisposeOfTargets()
	{
		BackMaskTarget?.Dispose();
		FrontMaskTarget?.Dispose();
	}

	private static bool TargetNeedsInitializing(RenderTarget2D? target) => target is null || target.IsContentLost;
}
