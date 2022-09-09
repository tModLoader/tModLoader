using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent.UI.ResourceSets;

#nullable enable

namespace Terraria.ModLoader
{
	public struct ResourceOverlayDrawContext
	{
		/// <summary>
		/// A snapshot of the player's health and mana stats
		/// </summary>
		public readonly PlayerStatsSnapshot snapshot;
		/// <summary>
		/// Which heart/star/bar/panel is being drawn<br/>
		/// <b>NOTE:</b> This value starts at 1, not 0
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
		/// The source for the resource drawing.
		/// </summary>
		public IResourceDrawSource DrawSource { get; init; }

		public SpriteBatch SpriteBatch { get; init; }

		public ResourceOverlayDrawContext(PlayerStatsSnapshot snapshot, int resourceNumber, Asset<Texture2D> texture, IResourceDrawSource drawSource) {
			this.snapshot = snapshot;
			this.resourceNumber = resourceNumber;
			this.texture = texture;
			position = Vector2.Zero;
			source = null;
			color = Color.White;
			rotation = 0;
			origin = Vector2.Zero;
			scale = Vector2.One;
			effects = SpriteEffects.None;
			DrawSource = drawSource;
			SpriteBatch = Main.spriteBatch;
		}

		public void Draw() {
			SpriteBatch.Draw(texture.Value, position, source, color, rotation, origin, scale, effects, 0);
		}
	}
}
