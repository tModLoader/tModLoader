using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;

namespace Terraria.DataStructures;

/// <summary>
/// Holds data for modifying the sky drawing in <see cref="ModMenu.PreDrawSky(SpriteBatch, ref SkyDrawParams)"/>.
/// </summary>
public struct SkyDrawParams
{
	/// <summary>
	/// Whether or not to draw the stars.
	/// </summary>
	public bool DrawStars;

	/// <summary>
	/// Whether or not to draw the sun and moon.
	/// </summary>
	public bool DrawSunAndMoon;

	public SkyDrawParams(bool drawStars, bool drawSunAndMoon)
	{
		DrawStars = drawStars;
		DrawSunAndMoon = drawSunAndMoon;
	}
}