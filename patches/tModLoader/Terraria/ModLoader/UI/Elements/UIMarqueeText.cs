using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;

namespace Terraria.ModLoader.UI.Elements;

public class UIMarqueeText : UIText
{
    public MarqueeTextOptions MarqueeText = new();

    protected override bool SetTextUsesMinSize => false;

    public UIMarqueeText(object text, float textScale = 1f, bool large = false) : base(text, textScale, large)
    { }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        MarqueeText.Update(GetInnerDimensions(), _textSize, TextOriginX);
    }

    protected override void DrawText(SpriteBatch spriteBatch, Vector2 textOffset)
    {
	    var dims = GetInnerDimensions();
	    if (_textSize.X < dims.Width) {
		    base.DrawText(spriteBatch, textOffset);
		    return;
	    }

	    MarqueeText.BeginDraw(spriteBatch, this, out var rasterizerState, out var scissorRectangle);
	    base.DrawText(spriteBatch, textOffset + MarqueeText.TextOffset);
	    MarqueeText.EndDraw(spriteBatch, this, rasterizerState, scissorRectangle);
    }

    public void ResetScroll()
    {
	    MarqueeText.ResetScroll(GetInnerDimensions(), _textSize, TextOriginX);
    }
}
