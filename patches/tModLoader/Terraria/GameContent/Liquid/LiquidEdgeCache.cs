using Microsoft.Xna.Framework;

namespace Terraria.GameContent.Liquid;

/// <summary>
/// Cached data for rendering liquids on solid tiles at the edge of liquid
/// pools.
/// </summary>
public struct LiquidEdgeCache
{
	/// <summary>
	/// The X coordinate of the tile.
	/// </summary>
	public int TileX;

	/// <summary>
	/// The Y coordinate of the tile.
	/// </summary>
	public int TileY;

	/// <summary>
	/// The actual liquid type to be rendered.
	/// </summary>
	public int LiquidType;

	/// <summary>
	/// The additional offset to apply based which edge is being drawn.
	/// </summary>
	public Vector2 LiquidPosition;

	/// <summary>
	/// The framing of the liquid slope.
	/// </summary>
	public Rectangle LiquidFrame;
}
