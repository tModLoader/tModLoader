using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria.GameContent;
using Terraria.UI;
using Terraria.UI.Chat;

namespace Terraria.ModLoader.UI.Elements;

// TODO: refactor to be based on UIText?
public class UIMarqueeText : UIElement
{
    public MarqueeTextOptions MarqueeText = new();

    private object text;
    public string Text => text?.ToString() ?? string.Empty;

    public float TextAlignX { get; set; } = 0f;
    public float TextAlignY { get; set; } = 0f;
    public float MaxTextScale { get; set; }
    public Color TextColor { get; set; }
    public bool Large { get; set; }

    private float textScale;
    private Vector2 textSize;

    public UIMarqueeText(object text, float scale = 1f, bool large = false) {
        this.text = text;

        MaxTextScale = scale;

        Large = large;

        TextColor = Color.White;

        PaddingLeft = 4f;
        PaddingRight = 4f;
    }

    public override void Recalculate()
    {
        base.Recalculate();

        SetText(text);
    }

    public void SetText(object text)
    {
        this.text = text;

        DynamicSpriteFont font = Large ? FontAssets.DeathText.Value : FontAssets.MouseText.Value;

        var dims = this.GetInnerDimensions();
        textSize = ChatManager.GetStringSize(font, Text, new Vector2(textScale));
        textScale = MathHelper.Min(dims.Height / textSize.Y, MaxTextScale);
        textSize = ChatManager.GetStringSize(font, Text, new Vector2(textScale));
        OverflowHidden = textSize.X >= dims.Width;
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        MarqueeText.Update(GetInnerDimensions(), textSize, TextAlignX);
    }

    public override Rectangle GetClippingRectangle(SpriteBatch spriteBatch)
    {
	    var dims = GetInnerDimensions();
	    MarqueeText.AdjustClippingRectangle(ref dims);
	    return CalculateClippingRectangle(dims, spriteBatch);
    }

    protected override void DrawChildren(SpriteBatch spriteBatch)
    {
	    var dims = this.GetInnerDimensions();
	    var font = FontAssets.MouseText.Value;
	    var position = new Vector2(dims.X + (dims.Width * TextAlignX), dims.Y + (dims.Height * TextAlignY) + 4);
	    var textSize = ChatManager.GetStringSize(font, Text, Vector2.One);
	    var origin = new Vector2(textSize.X * TextAlignX, textSize.Y * TextAlignY);

	    position.X -= MarqueeText.Scroll;

	    // Chat tags don't correctly account for origin nor scale/rotation.
	    position -= origin * textScale;

	    ChatManager.DrawColorCodedStringWithShadow(
		    spriteBatch,
		    font,
		    Text,
		    position,
		    TextColor,
		    0f,
		    Vector2.Zero,
		    new Vector2(textScale)
	    );

	    base.DrawChildren(spriteBatch);
    }
}
