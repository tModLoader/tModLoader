using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;


namespace Terraria.ModLoader.UI
{
	internal sealed class UILoaderAnimatedImage : UIElement
	{
		public int frameTick = 0;
		public int frame = 0;
		private float scale;
		public const int maxFrames = 15;
		public const int maxDelay = 5;
		private readonly Texture2D loaderTexture;

		public UILoaderAnimatedImage(float left, float top, float scale = 1f)
		{
			loaderTexture = Texture2D.FromStream(Main.instance.GraphicsDevice, Assembly.GetExecutingAssembly().GetManifestResourceStream("Terraria.ModLoader.UI.Loader.png"));
			this.scale = scale;
			base.Width.Set(200f * scale, 0f);
			base.Height.Set(200f * scale, 0f);
			base.Left.Set(0f, left);
			base.Top.Set(0f, top);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			if (frameTick++ >= maxDelay)
			{
				frameTick = 0;
				if (frame++ > maxFrames)
					frame = 0;
			}

			CalculatedStyle dimensions = base.GetDimensions();
			spriteBatch.Draw(
				loaderTexture,
				new Vector2((int)dimensions.X, (int)dimensions.Y),
				new Rectangle(frame/8, 200 * (frame%8), 200, 200),
				Color.White,
				0f,
				new Vector2(100f, 100f),
				scale,
				SpriteEffects.None,
				0.0f);

			//CalculatedStyle dimensions = base.GetDimensions();
			//Point point = new Point(0, 200 * frame);
			//spriteBatch.Draw(
			//	loaderTexture, 
			//	new Rectangle((int)dimensions.X, (int)dimensions.Y, 200, 200), 
			//	new Rectangle?(new Rectangle(point.X, point.Y, 200, 200)),
			//	Color.White);
		}
	}
}
