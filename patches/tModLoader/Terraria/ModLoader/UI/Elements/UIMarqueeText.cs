using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;

namespace Terraria.ModLoader.UI.Elements;

public class UIMarqueeText : UIText
{
    public MarqueeTextOptions MarqueeText = new();

    protected override bool setTextUsesMinSize => false;

    public UIMarqueeText(object text, float textScale = 1f, bool large = false) : base(text, textScale, large)
    {
	    // TODO: was this because of clipping?
        // PaddingLeft = 4f;
        // PaddingRight = 4f;
    }

    protected override void InternalSetText(object text, float textScale, bool large)
    {
	    base.InternalSetText(text, textScale, large);

	    var dims = GetInnerDimensions();
	    bool textIsClipping = _textSize.X >= dims.Width;
	    OverflowHidden = textIsClipping;
	    drawTextAsChild = textIsClipping;
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        MarqueeText.Update(GetInnerDimensions(), _textSize, TextOriginX);
        textOffset = -MarqueeText.Scroll * Vector2.UnitX;
    }

    public override Rectangle GetClippingRectangle(SpriteBatch spriteBatch)
    {
	    var dims = GetInnerDimensions();
	    MarqueeText.AdjustClippingRectangle(ref dims);
	    return CalculateClippingRectangle(dims, spriteBatch);
    }
}
