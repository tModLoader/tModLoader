using Microsoft.Xna.Framework;

namespace Terraria.Map;

public partial struct MapOverlayDrawContext
{
	/// <summary>
	/// The tile coordinate that the top left corner of the map is showing. Note that this will always be <see cref="Vector2.Zero"/> when drawing the overlay and fullscreen maps.
	/// </summary>
	public readonly Vector2 MapPosition => _mapPosition;

	/// <summary>
	/// The position of the top left corner of the map in screen coordinates.
	/// </summary>
	public readonly Vector2 MapOffset => _mapOffset;

	/// <summary>
	/// The bounding box of the map in screen coordinates. This is used in vanilla to make icons disappear on the minimap when they go out of view. Note this only applies to the minimap and will be <see langword="null"/> when drawing the overlay and fullscreen maps.
	/// </summary>
	public readonly Rectangle? ClippingRectangle => _clippingRect;

	/// <summary>
	/// A scale multiplier for the size of the tiles on the map. See <see cref="Main.mapMinimapScale"/>, <see cref="Main.mapFullscreenScale"/>, and <see cref="Main.mapOverlayScale"/>.
	/// </summary>
	public readonly float MapScale => _mapScale;

	/// <summary>
	/// A scale multiplier for the size of the icons on the map. This is typically passed as the 'scale' parameter of draw calls and accounts for things like <see cref="Main.UIScale"/>.
	/// </summary>
	public readonly float DrawScale => _drawScale;
}
