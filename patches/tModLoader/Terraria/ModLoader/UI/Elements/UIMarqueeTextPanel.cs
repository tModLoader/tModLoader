using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.UI.Chat;

namespace Terraria.ModLoader.UI.Elements;

public class UIMarqueeTextPanel : UITextPanel<object>
{
	public MarqueeTextOptions MarqueeText;

	public UIMarqueeTextPanel(object text, float textScale = 1, bool large = false) : base(text, textScale, large)
	{
		MarqueeText.ClippingYPadding = 1000f;
	}

	// Copied from original function, but changed MinWidth and Minheight to Width and Height so a maximum size can be enforced
	// TODO: figure out how to avoid this copying
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

		MarqueeText.Update(GetInnerDimensions(), _textSize, TextHAlign);
		textOffset = -MarqueeText.Scroll * Vector2.UnitX;
	}

	public override Rectangle GetClippingRectangle(SpriteBatch spriteBatch)
	{
		var dims = GetInnerDimensions();
		MarqueeText.AdjustClippingRectangle(ref dims);
		return CalculateClippingRectangle(dims, spriteBatch);
	}
}
