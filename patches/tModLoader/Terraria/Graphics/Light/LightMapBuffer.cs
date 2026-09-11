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
	///		The bounds of the texture with light data.  The X and Y coordinates
	///		indicate the top-left position of the buffer in tile coordinates,
	///		and the Width and Height indicate the full area being procesesed.
	///		<para />
	///		The light map buffer is subject to two possible kinds of padding:
	///		1) padding in the allocated color/mask buffers, and 2) padding with
	///		valid data, but is exempt from further processing (i.e. blurring in
	///		<see cref="LightingEngine"/>).
	///		<br />
	///		Width and Height will exclude the former but, include the latter.
	/// </summary>
	public required Rectangle TileArea { get; init; }
}
