using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent.UI.ResourceSets;

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
		/// What "slice" of the resource is being drawn.<br/>
		/// For example, if a player had 65 life, this value would be expected to take the values 20, 40, 60, 65 and then -1.<br/>
		/// -1 indicates that this resource would normally be not displayed
		/// </summary>
		public double ResourceStat { get; init; }

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
		/// <param name="stat">What slice of the player's stat is being drawn</param>
		/// <param name="resourceNumber">The resource number within the resource set</param>
		/// <param name="texture">The texture being drawn</param>
		public ResourceOverlayDrawContext(PlayerStatsSnapshot snapshot, IPlayerResourcesDisplaySet displaySet, double stat, int resourceNumber, Asset<Texture2D> texture) {
			this.snapshot = snapshot;
			DisplaySet = displaySet;
			ResourceStat = stat;
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

		public void Draw() {
			SpriteBatch.Draw(texture.Value, position, source, color, rotation, origin, scale, effects, 0);
		}
	}
}
