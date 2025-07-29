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

	public static Effect MaskShader => (maskAsset ??= ModLoader.ModLoader.ManifestAssets.Request<Effect>("Terraria.GameContent.Liquid.LiquidMask", AssetRequestMode.ImmediateLoad)).Value;

	private static Asset<Effect>? maskAsset;

	public static readonly BlendState MaskingBlendState = new BlendState() {
		ColorSourceBlend = Blend.Zero,
		AlphaSourceBlend = Blend.Zero,
		ColorDestinationBlend = Blend.InverseSourceAlpha,
		AlphaDestinationBlend = Blend.InverseSourceAlpha
	};

	/// <summary>
	/// Tiles which mask rendered liquid (tiles on the edge of bodies of
	/// liquid).
	/// </summary>
	public static Dictionary<Point, LiquidEdgeCache> Edges { get; } = [];
}
