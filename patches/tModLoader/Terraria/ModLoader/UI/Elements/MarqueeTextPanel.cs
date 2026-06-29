using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Terraria.UI.Chat;

namespace Terraria.ModLoader.UI.Elements;

public class MarqueeTextPanel : UITextPanel<object>
{
	public float ScrollSpeed { get; set; } = 1f;

	public bool IsScrolling { get; set; } = true;

	private float scroll;

	private int scrollTimer;

	private int scrollDirection = 1;

	public MarqueeTextPanel(object text, float textScale = 1, bool large = false) : base(text, textScale, large) { }

	// Copied from original function, but changed MinWidth and Minheight to Width and Height so a maximum size can be enforced
	public override void SetText(object text, float textScale, bool large)
	{
		DynamicSpriteFont dynamicSpriteFont = large ? FontAssets.DeathText.Value : FontAssets.MouseText.Value;
		Vector2 textSize = ChatManager.GetStringSize(dynamicSpriteFont, text.ToString(), new Vector2(textScale));
		textSize.Y = (large ? 32f : 16f) * textScale;

		_text = text;
		_textScale = textScale;
		_textSize = textSize;
		_isLarge = large;
		Width.Set(textSize.X + PaddingLeft + PaddingRight, 0f);
		Height.Set(textSize.Y + PaddingTop + PaddingBottom, 0f);

		var dims = GetInnerDimensions();
		OverflowHidden = textSize.X >= dims.Width;
		drawTextAsChild = textSize.X >= dims.Width;
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		DynamicSpriteFont dynamicSpriteFont = _isLarge ? FontAssets.DeathText.Value : FontAssets.MouseText.Value;
		Vector2 textSize = ChatManager.GetStringSize(dynamicSpriteFont, _text.ToString(), new Vector2(_textScale));
		textSize.Y = (_isLarge ? 32f : 16f) * _textScale;

		var dims = GetInnerDimensions();

		MarqueeText.UpdateScrollValues(textSize, dims, TextHAlign, textSize.X >= dims.Width && IsScrolling, ScrollSpeed, ref scroll, ref scrollTimer, ref scrollDirection);

		textOffset = -scroll * Vector2.UnitX;
	}

	public override Rectangle GetClippingRectangle(SpriteBatch spriteBatch)
	{
		const float ExtraXPadding = 2f; // Extra space to stop the right of the text getting clipped
		const float ExtraYPadding = 1000f; // Stop clipping above and below the panel

		var dims = GetInnerDimensions();
		dims.X -=  ExtraXPadding;
		dims.Y -=  ExtraYPadding;
		dims.Width += ExtraXPadding * 2;
		dims.Height += ExtraYPadding * 2;

		return UIElement.GetClippingRectangleFrom(spriteBatch, dims);
	}
}