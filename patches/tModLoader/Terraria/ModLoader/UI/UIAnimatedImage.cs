using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace Terraria.ModLoader.UI;

public class UIAnimatedImage : UIElement
{
	private readonly Asset<Texture2D> _texture;
	private readonly int _padding;
	private readonly int _textureOffsetX;
	private readonly int _textureOffsetY;
	private readonly int _countX;
	private readonly int _countY;

	public int FrameStart { get; set; } = 0;
	public int FrameCount { get; set; } = 1;
	public int TicksPerFrame { get; set; } = 5;

	private int _tickCounter = 0;
	private int _frameCounter = 0;

	protected int DrawHeight => (int)Height.Pixels;
	protected int DrawWidth => (int)Width.Pixels;

	public UIAnimatedImage(Asset<Texture2D> texture, int width, int height, int textureOffsetX, int textureOffsetY, int countX, int countY, int padding = 2)
	{
		_texture = texture;
		_textureOffsetX = textureOffsetX;
		_textureOffsetY = textureOffsetY;
		_countX = countX;
		_countY = countY;
		Width.Pixels = width;
		Height.Pixels = height;
		_padding = padding;
	}

	private Rectangle FrameToRect(int frame)
	{
		if ((frame < 0) || (frame >= _countX * _countY))
			return new Rectangle(0, 0, 0, 0);
		var x = frame % _countX;
		var y = frame / _countX;
		return new Rectangle(
			_textureOffsetX + (_padding + DrawHeight) * x,
			_textureOffsetY + (_padding + DrawHeight) * y,
			DrawWidth,
			DrawHeight
		);
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		if (++_tickCounter >= TicksPerFrame) {
			_tickCounter = 0;
			if (++_frameCounter >= FrameCount) {
				_frameCounter = 0;
			}
		}
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		CalculatedStyle dimensions = GetDimensions();
		Color color = IsMouseHovering ? Color.White : Color.Silver;
		var frame = FrameStart + _frameCounter % FrameCount;
		spriteBatch.Draw(_texture.Value, dimensions.ToRectangle(), FrameToRect(frame), color);
	}
}
