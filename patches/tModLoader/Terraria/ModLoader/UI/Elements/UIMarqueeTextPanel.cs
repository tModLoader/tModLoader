using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;

namespace Terraria.ModLoader.UI.Elements;

public class UIMarqueeTextPanel : UITextPanel
{
	public MarqueeTextOptions MarqueeText = new();

	protected override bool SetTextUsesMinSize => false;

	public UIMarqueeTextPanel(object text, float textScale = 1, bool large = false) : base(text, textScale, large)
	{ }

	public override void SetText(object text, float textScale, bool large)
	{
		base.SetText(text, textScale, large);

		var dims = GetInnerDimensions();
		bool textIsClipping = _textSize.X >= dims.Width;
		OverflowHidden = textIsClipping;
		drawTextAsChild = textIsClipping;
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

	public void ResetScroll()
	{
		MarqueeText.ResetScroll(GetInnerDimensions(), _textSize, TextHAlign);
		textOffset = -MarqueeText.Scroll * Vector2.UnitX; // ResetScroll may set Scroll to be non-zero if AlignToStartWhenNotScrolling == true
	}
}
