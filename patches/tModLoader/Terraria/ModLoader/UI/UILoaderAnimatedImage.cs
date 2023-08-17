using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.UI;

namespace Terraria.ModLoader.UI;

internal sealed class UILoaderAnimatedImage : UIElement
{
	public const int MAX_FRAMES = 16;
	public const int MAX_DELAY = 5;

	public bool WithBackground = false;
	public int FrameTick;
	public int Frame;

	private readonly float _scale;
	private Asset<Texture2D> _backgroundTexture;
	private Asset<Texture2D> _loaderTexture;

	public UILoaderAnimatedImage(float left, float top, float scale = 1f)
	{
		_scale = scale;
		Width.Pixels = 200f * scale;
		Height.Pixels = 200f * scale;
		HAlign = left;
		VAlign = top;
	}

	public override void OnInitialize()
	{
		_backgroundTexture = UICommon.LoaderBgTexture;
		_loaderTexture = UICommon.LoaderTexture;
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		if (++FrameTick >= MAX_DELAY) {
			FrameTick = 0;
			if (++Frame >= MAX_FRAMES)
				Frame = 0;
		}

		CalculatedStyle dimensions = GetDimensions();

		// Draw BG
		if (WithBackground) {
			spriteBatch.Draw(
				_backgroundTexture.Value,
				new Vector2((int)dimensions.X, (int)dimensions.Y),
				new Rectangle(0, 0, 200, 200),
				Color.White,
				0f,
				new Vector2(0, 0),
				_scale,
				SpriteEffects.None,
				0.0f);
		}

		// Draw loader animation
		spriteBatch.Draw(
			_loaderTexture.Value,
			new Vector2((int)dimensions.X, (int)dimensions.Y),
			new Rectangle(200 * (Frame / 8), 200 * (Frame % 8), 200, 200),
			Color.White,
			0f,
			new Vector2(0, 0),
			_scale,
			SpriteEffects.None,
			0.0f);
	}
}
