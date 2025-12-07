using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;
using Terraria;

namespace Terraria.DataStructures;

/// <summary>
/// Holds data for modifying the sky drawing in <see cref="ModMenu.ModifyDrawSky(ref SkyDrawParams)"/>.
/// </summary>
public record struct SkyDrawParams()
{
	/// <summary>
	/// Whether or not to draw the stars.
	/// </summary>
	public bool DrawStars = true;

	/// <summary>
	/// Whether or not to draw the sun and moon.
	/// </summary>
	public bool DrawSunAndMoon = true;

	/// <summary>
	/// Whether or not to draw the sky itself, disabling this will not disable the sun, moon, or stars.
	/// </summary>
	public bool DrawSkyGradient = true;

	/// <summary>
	/// A multiplier of the cloud opacity, defaults to 1.
	/// </summary>
	public float CloudAlpha = 1f;

	/// <summary>
	/// A multiplier of the sky opacity, defaults to 1.
	/// </summary>
	public float SkyAlpha = 1f;

	/// <summary>
	/// A multiplier of the star opacity, defaults to 1.
	/// </summary>
	public float StarAlpha = 1f;

	/// <summary>
	/// A multiplier of the moon opacity, defaults to 1.
	/// </summary>
	public float MoonAlpha = 1f;

	/// <summary>
	/// A multiplier of the sun opacity, defaults to 1.
	/// </summary>
	public float SunAlpha = 1f;
	
	public Color MoonColor;
	public Color SunColor;
}