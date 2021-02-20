using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Terraria.ModLoader
{
	public abstract class ModInvasion : ModEvent
	{
		internal class ModInvasionProgressDisplay
		{
			public int DisplayLeft;
			public float Alpha;

			public ModInvasionProgressDisplay(int displayLeft, float alpha) {
				DisplayLeft = displayLeft;
				Alpha = alpha;
			}
		}

		public float Progress { get; set; }

		public abstract string Title { get; }

		public abstract string ProgressText { get; }

		public virtual bool PreDrawBottomPanel(SpriteBatch spriteBatch, ref Rectangle destination, ref Color color) => true;

		public virtual void PostDrawBottomPanel(SpriteBatch spriteBatch, Rectangle destination, Color color) { }

		public virtual bool PreDrawProgressText(SpriteBatch spriteBatch, ref Vector2 anchorPosition, ref Color color, ref float scale, ref float anchorX, ref float anchorY) => true;

		public virtual void PostDrawProgressText(SpriteBatch spriteBatch, Vector2 anchorPosition, Color color, float scale, float anchorX, float anchorY) { }

		public virtual bool PreDrawProgressBarOutline(SpriteBatch spriteBatch, ref Vector2 center, ref Color color, ref float rotation, ref Vector2 origin, ref float scale) => true;
		
		public virtual void PostDrawProgressBarOutline(SpriteBatch spriteBatch, Vector2 center, Color color, float rotation, Vector2 origin, float scale) { }

		public virtual bool PreDrawProgressBar(SpriteBatch spriteBatch, ref Vector2 position, ref float width, ref float height, ref Color color) => true;

		public virtual void PostDrawProgressBar(SpriteBatch spriteBatch, Vector2 position, float width, float height, Color color) { }

		public virtual bool PreDrawTopPanel(SpriteBatch spriteBatch, ref Rectangle destination, ref Color color) => true;

		public virtual void PostDrawTopPanel(SpriteBatch spriteBatch, Rectangle destination, Color color) { }

		public virtual bool PreDrawIcon(SpriteBatch spriteBatch, ref Vector2 position, ref Color color, ref float rotation, ref Vector2 origin, ref float scale, ref SpriteEffects effects) => true;

		public virtual void PostDrawIcon(SpriteBatch spriteBatch, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects) { }

		public virtual bool PreDrawTitle(SpriteBatch spriteBatch, ref Vector2 position, ref Color color, ref float scale, ref float anchorX, ref float anchorY) => true;

		public virtual void PostDrawTitle(SpriteBatch spriteBatch, Vector2 position, Color color, float scale, float anchorX, float anchorY) { }
	}
}