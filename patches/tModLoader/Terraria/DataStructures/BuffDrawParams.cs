using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Terraria.DataStructures;

/// <summary>
/// Holds data required for buff drawing.
/// </summary>
public struct BuffDrawParams
{
	/// <summary>
	/// The texture used for drawing the buff.
	/// </summary>
	public Texture2D Texture;

	/// <summary>
	/// Top-left position of the buff on the screen.
	/// </summary>
	public Vector2 Position;

	/// <summary>
	/// Top left position of the text below the buff (remaining time).
	/// </summary>
	public Vector2 TextPosition;

	/// <summary>
	/// The frame displayed from the texture. Defaults to the entire texture size.
	/// </summary>
	public Rectangle SourceRectangle;

	/// <summary>
	/// Defaults to the size of the autoloaded buffs' sprite, it handles mouseovering and clicking on the buff icon.
	/// If you offset the position, or have a non-standard size, change it accordingly.
	/// </summary>
	public Rectangle MouseRectangle;

	/// <summary>
	/// Color used to draw the buff. Use Main.buffAlpha[buffIndex] accordingly if you change it.
	/// </summary>
	public Color DrawColor;

	public BuffDrawParams(Texture2D texture, Vector2 position, Vector2 textPosition, Rectangle sourceRectangle, Rectangle mouseRectangle, Color drawColor)
	{
		Texture = texture;
		Position = position;
		TextPosition = textPosition;
		SourceRectangle = sourceRectangle;
		MouseRectangle = mouseRectangle;
		DrawColor = drawColor;
	}

	public void Deconstruct(out Texture2D texture, out Vector2 position, out Vector2 textPosition, out Rectangle sourceRectangle, out Rectangle mouseRectangle, out Color drawColor)
	{
		texture = Texture;
		position = Position;
		textPosition = TextPosition;
		sourceRectangle = SourceRectangle;
		mouseRectangle = MouseRectangle;
		drawColor = DrawColor;
	}

	// If new fields get added post-release, add new con/deconstructors instead of expanding the current ones to maintain backwards compatibility
}
