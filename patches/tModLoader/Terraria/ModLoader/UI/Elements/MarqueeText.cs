using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria.GameContent;
using Terraria.UI;
using Terraria.UI.Chat;

namespace Terraria.ModLoader.UI.Elements;

public class MarqueeText : UIElement
{
    private object text;
    public string Text => text?.ToString() ?? string.Empty;

    public float TextAlignX { get; set; } = 0f;
    public float TextAlignY { get; set; } = 0f;
    public float MaxTextScale { get; set; }
    public Color TextColor { get; set; }
    public bool Large { get; set; }
    public float ScrollSpeed { get; set; } = 1f;
    public bool IsScrolling { get; set; } = true;
    public float ClippingXPadding { get; set; } = 2f;
    public float ClippingYPadding { get; set; } = 0f;
    public bool AlignToStartWhenNotScrolling = false;

    private float textScale;
    private Vector2 textSize;
    private float scroll;
    private int scrollTimer;
    private int scrollDirection = 1;

    public MarqueeText(object text, float scale = 1f, bool large = false) {
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

        var dims = GetInnerDimensions();
        UpdateScrollValues(
	        textSize,
	        dims,
	        TextAlignX,
	        textSize.X >= dims.Width && IsScrolling,
	        ScrollSpeed,
	        ref scroll,
	        ref scrollTimer,
	        ref scrollDirection,
	        ResetScroll
        );
    }

    public static void UpdateScrollValues(Vector2 textSize, CalculatedStyle innerDimensions, float textAlignX, bool shouldScroll, float scrollSpeed, ref float scroll, ref int scrollTimer, ref int scrollDirection, Action resetScroll)
    {
	    const float scroll_increment = 1.5f;
	    const int scroll_delay = 30;

	    if (!shouldScroll) {
		    resetScroll();
		    return;
	    }

	    scrollTimer--;
	    if (scrollTimer > 0) {
		    return;
	    }

	    // Each half of the text seperated by the alignment.
	    var left =
		    (textSize.X * textAlignX) -
		    (innerDimensions.Width * textAlignX);

	    var right =
		    (textSize.X * (1f - textAlignX)) -
		    (innerDimensions.Width * (1f - textAlignX));

	    scroll += scroll_increment * scrollSpeed * scrollDirection;

	    if (scroll >= right) {
		    scroll = right;
		    scrollTimer = scroll_delay;
		    scrollDirection = -1;
	    }
	    else if (scroll <= -left) {
		    scroll = -left;
		    scrollTimer = scroll_delay;
		    scrollDirection = 1;
	    }
    }

    // TODO: consider languages that read from right to left
    public static float GetStartAlignment(Vector2 textSize, float innerWidth, float textAlignX)
    {
	    float align = 1f - textAlignX;
	    return -((textSize.X * align) - (innerWidth * align));
    }

    public void ResetScroll()
    {
	    scroll = 0;
	    scrollTimer = 0;
	    scrollDirection = 1;

	    if (AlignToStartWhenNotScrolling && textSize.X >= GetInnerDimensions().Width) {
		    scroll = GetStartAlignment(textSize, GetInnerDimensions().Width, TextAlignX);
	    }
    }

    public void SetScrollDelay(int scrollDelay)
    {
	    scrollTimer = scrollDelay;
    }

    public override Rectangle GetClippingRectangle(SpriteBatch spriteBatch)
    {
	    var dims = GetInnerDimensions();
	    dims.X -= ClippingXPadding;
	    dims.Y -= ClippingYPadding;
	    dims.Width += ClippingXPadding * 2;
	    dims.Height += ClippingYPadding * 2;

	    return UIElement.GetClippingRectangleFrom(spriteBatch, dims);
    }

    protected override void DrawChildren(SpriteBatch spriteBatch)
    {
	    var dims = this.GetInnerDimensions();
	    var font = FontAssets.MouseText.Value;
	    var position = new Vector2(dims.X + (dims.Width * TextAlignX), dims.Y + (dims.Height * TextAlignY) + 4);
	    var textSize = ChatManager.GetStringSize(font, Text, Vector2.One);
	    var origin = new Vector2(textSize.X * TextAlignX, textSize.Y * TextAlignY);

	    position.X -= scroll;

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