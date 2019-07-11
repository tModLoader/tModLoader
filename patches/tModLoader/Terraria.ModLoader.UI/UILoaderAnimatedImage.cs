using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	internal sealed class UILoaderAnimatedImage : UIElement
	{
		public bool withBackground = false;
		public int frameTick = 0;
		public int frame = 0;
		private float scale;
		public const int maxFrames = 16;
		public const int maxDelay = 5;
		private Texture2D backgroundTexture;
		private Texture2D loaderTexture;

		public UILoaderAnimatedImage(float left, float top, float scale = 1f) {
			this.scale = scale;
			Width.Pixels = 200f * scale;
			Height.Pixels = 200f * scale;
			HAlign = left;
			VAlign = top;
		}

		public override void OnInitialize()
		{
			backgroundTexture = UICommon.loaderBGTexture;
			loaderTexture = UICommon.loaderTexture;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			if (++frameTick >= maxDelay) {
				frameTick = 0;
				if (++frame >= maxFrames)
					frame = 0;
			}

			CalculatedStyle dimensions = base.GetDimensions();
			// Draw BG
			if (withBackground) {
				spriteBatch.Draw(
					backgroundTexture,
					new Vector2((int)dimensions.X, (int)dimensions.Y),
					new Rectangle(0, 0, 200, 200),
					Color.White,
					0f,
					new Vector2(0, 0),
					scale,
					SpriteEffects.None,
					0.0f);
			}

			// Draw loader animation
			spriteBatch.Draw(
				loaderTexture,
				new Vector2((int)dimensions.X, (int)dimensions.Y),
				new Rectangle(200 * (frame / 8), 200 * (frame % 8), 200, 200),
				Color.White,
				0f,
				new Vector2(0, 0),
				scale,
				SpriteEffects.None,
				0.0f);
		}
	}
}
