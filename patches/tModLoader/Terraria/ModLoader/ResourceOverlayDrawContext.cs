using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent.UI.ResourceSets;

namespace Terraria.ModLoader;

public struct ResourceOverlayDrawContext
{
	/// <summary>
	/// A snapshot of the player's health and mana stats
	/// </summary>
	public readonly PlayerStatsSnapshot snapshot;
	/// <summary>
	/// Which heart/star/bar/panel is being drawn<br/>
	/// <b>NOTE:</b> This value usually starts at 0, but it can start at other values
	/// </summary>
	public readonly int resourceNumber;
	public Asset<Texture2D> texture;
	public Vector2 position;
	/// <summary>
	/// The slice of the texture to draw<br/>
	/// <see langword="null"/> represents the entire texture
	/// </summary>
	public Rectangle? source;
	public Color color;
	public float rotation;
	/// <summary>
	/// The center for rotation and scaling within the source rectangle
	/// </summary>
	public Vector2 origin;
	public Vector2 scale;
	public SpriteEffects effects;

	/// <summary>
	/// The resource display set that this context is drawing from
	/// </summary>
	public IPlayerResourcesDisplaySet DisplaySet { get; init; }

	public SpriteBatch SpriteBatch { get; init; }

	/// <summary>
	/// Creates a context for drawing resources from a display set
	/// </summary>
	/// <param name="snapshot">A snapshot of a player's life and mana stats</param>
	/// <param name="displaySet">The display set that this context is for</param>
	/// <param name="resourceNumber">The resource number within the resource set</param>
	/// <param name="texture">The texture being drawn</param>
	public ResourceOverlayDrawContext(PlayerStatsSnapshot snapshot, IPlayerResourcesDisplaySet displaySet, int resourceNumber, Asset<Texture2D> texture)
	{
		this.snapshot = snapshot;
		DisplaySet = displaySet;
		this.resourceNumber = resourceNumber;
		this.texture = texture;
		position = Vector2.Zero;
		source = null;
		color = Color.White;
		rotation = 0;
		origin = Vector2.Zero;
		scale = Vector2.One;
		effects = SpriteEffects.None;
		SpriteBatch = Main.spriteBatch;
	}

	public void Draw()
	{
		SpriteBatch.Draw(texture.Value, position, source, color, rotation, origin, scale, effects, 0);
	}
}
