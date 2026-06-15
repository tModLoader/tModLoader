using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Terraria.Graphics.Light;

/// <summary>
///		Represents the texture buffer for a <see cref="LightMap"/>.
/// </summary>
public readonly struct LightMapBuffer
{
	/// <summary>
	///		The texture containing the light map data.  One pixel corresponds
	///		to one tile.
	/// </summary>
	public required Texture2D Texture { get; init; }

	/// <summary>
	///		The area of the texture that contains on-screen data.
	/// </summary>
	public required Rectangle ScreenTileArea { get; init; }
}
