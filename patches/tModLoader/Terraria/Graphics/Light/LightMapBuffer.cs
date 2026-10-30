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
	///		<para />
	///		This may contain padded data; see <see cref="TileArea"/> for more
	///		information.
	/// </summary>
	public required Texture2D Texture { get; init; }

	/// <summary>
	///		The bounds of the buffer.  The X and Y coordinates correspond to
	///		the top-left position of the buffer in tile coordinates, while the
	///		width and height correspond to the region of valid data in the
	///		buffer.
	/// </summary>
	public required Rectangle TileArea { get; init; }
}
