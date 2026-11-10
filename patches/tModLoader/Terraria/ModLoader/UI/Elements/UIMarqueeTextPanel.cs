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

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		MarqueeText.Update(GetInnerDimensions(), _textSize, TextHAlign);
	}

	protected override void DrawText(SpriteBatch spriteBatch, string text, Vector2 position, Color color)
	{
		var dims = GetInnerDimensions();
		if (_textSize.X < dims.Width) {
			base.DrawText(spriteBatch, text, position, color);
			return;
		}

		MarqueeText.BeginDraw(spriteBatch, this, out var rasterizerState, out var scissorRectangle);
		base.DrawText(spriteBatch, text, position + MarqueeText.TextOffset, color);
		MarqueeText.EndDraw(spriteBatch, this, rasterizerState, scissorRectangle);
	}

	public void ResetScroll()
	{
		MarqueeText.ResetScroll(GetInnerDimensions(), _textSize, TextHAlign);
	}
}
