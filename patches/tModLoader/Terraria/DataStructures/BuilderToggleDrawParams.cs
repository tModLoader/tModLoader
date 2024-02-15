using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Terraria.DataStructures;

/// <summary>
/// Holds data required for builder toggle drawing.
/// </summary>
public struct BuilderToggleDrawParams
{
	/// <summary> The icon or icon hover texture </summary>
	public Texture2D Texture = default;
	/// <summary> The position </summary>
	public Vector2 Position = default;
	/// <summary> The frame rectangle (aka source rectangle) </summary>
	public Rectangle Frame = default;
	/// <summary> The color the icon or icon hover is drawn in. Defaults to White for icon, <see cref="Main.OurFavoriteColor"/> (yellow) for icon hover. </summary>
	public Color Color = Color.White;
	/// <summary> The scale of the icon or icon hover </summary>
	public float Scale = 1f;
	/// <summary> The spriteEffects </summary>
	public SpriteEffects SpriteEffects = 0;

	public BuilderToggleDrawParams() { }
}
