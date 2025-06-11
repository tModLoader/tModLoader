using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.UI.Chat;

namespace Terraria.ModLoader.UI;

// UITextPanel except we scale and manipulate Text to preserve original dimensions.
public class UIAutoScaleTextTextPanel<T> : UIPanel
{
	public string Text => _text?.ToString() ?? string.Empty;

	public bool IsLarge { get; private set; }
	public bool DrawPanel { get; set; } = true;
	public float TextScaleMax { get; set; } = 1f;
	public float TextScale { get; set; } = 1f;
	public Vector2 TextSize { get; private set; } = Vector2.Zero;
	public Color TextColor { get; set; } = Color.White;
	public bool ScalePanel = false;
	public bool UseInnerDimensions = false;
	public float TextOriginX = 0.5f;
	public float TextOriginY = 0.5f;

	private Rectangle oldInnerDimensions;
	private T _text = default;
	private string oldText;
	private string[] textStrings;
	private Vector2[] drawOffsets;

	public UIAutoScaleTextTextPanel(T text, float textScaleMax = 1f, bool large = false) : base()
	{
		SetText(text, textScaleMax, large);
	}

	public override void Recalculate()
	{
		base.Recalculate();
		SetText(_text, TextScaleMax, IsLarge);
	}

	public void SetText(T text)
	{
		SetText(text, TextScaleMax, IsLarge);
	}

	public virtual void SetText(T text, float textScaleMax, bool large)
	{
		if (ScalePanel) {
			var dynamicSpriteFont = IsLarge ? FontAssets.DeathText.Value : FontAssets.MouseText.Value;
			var textSize = ChatManager.GetStringSize(dynamicSpriteFont, Text, new Vector2(TextScaleMax));

			Width.Set(PaddingLeft + textSize.X + PaddingRight, 0f);
			Height.Set(PaddingTop + (IsLarge ? 32f : 16f) + PaddingBottom, 0f);

			base.Recalculate();
		}

		var innerDimensionsRectangle = UseInnerDimensions ? GetInnerDimensions().ToRectangle() : GetDimensions().ToRectangle();

		if (text.ToString() != oldText || oldInnerDimensions != innerDimensionsRectangle) {
			oldInnerDimensions = innerDimensionsRectangle;

			TextScaleMax = textScaleMax;
			DynamicSpriteFont dynamicSpriteFont = large ? FontAssets.DeathText.Value : FontAssets.MouseText.Value;
			Vector2 textSize = ChatManager.GetStringSize(dynamicSpriteFont, text.ToString(), new Vector2(TextScaleMax));

			if (UseInnerDimensions)
				innerDimensionsRectangle.Inflate(0, 6);
			else
				innerDimensionsRectangle.Inflate(-4, 0);// Why not put -8 in inflate parameter here?

			var availableSpace = new Vector2(innerDimensionsRectangle.Width, innerDimensionsRectangle.Height);

			if (textSize.X > availableSpace.X || textSize.Y > availableSpace.Y) {
				float scale = (textSize.X / availableSpace.X > textSize.Y / availableSpace.Y) ? availableSpace.X / textSize.X : availableSpace.Y / textSize.Y;
				TextScale = scale;
				textSize = ChatManager.GetStringSize(dynamicSpriteFont, text.ToString(), new Vector2(TextScale));
			}
			else {
				TextScale = TextScaleMax;
			}

			if (!UseInnerDimensions) {
				innerDimensionsRectangle.Y += 8;
				innerDimensionsRectangle.Height -= 8;
			}
			_text = text;
			oldText = _text?.ToString();
			TextSize = textSize;
			IsLarge = large;
			textStrings = text.ToString().Split('\n');

			// Offset of left corner for centering
			drawOffsets = new Vector2[textStrings.Length];
			for (int i = 0; i < textStrings.Length; i++) {
				Vector2 size = ChatManager.GetStringSize(dynamicSpriteFont, textStrings[i], new Vector2(TextScale));
				if (UseInnerDimensions)
					size.Y = IsLarge ? 32f : 16f;

				float x = (innerDimensionsRectangle.Width - size.X) * TextOriginX;
				float y = (-textStrings.Length * size.Y * TextOriginY) + i * size.Y + innerDimensionsRectangle.Height * TextOriginY;

				if (UseInnerDimensions)
					y -= 2;

				drawOffsets[i] = new Vector2(x, y);
			}
			//this.MinWidth.Set(textSize.X + this.PaddingLeft + this.PaddingRight, 0f);
			//this.MinHeight.Set(textSize.Y + this.PaddingTop + this.PaddingBottom, 0f);
		}
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		if (float.IsNaN(TextScale))
			Recalculate();

		if (DrawPanel)
			base.DrawSelf(spriteBatch);

		var innerDimensions = UseInnerDimensions ? GetInnerDimensions().ToRectangle() : GetDimensions().ToRectangle();
		if (UseInnerDimensions)
			innerDimensions.Inflate(0, 6);
		else
			innerDimensions.Inflate(-4, -8);

		for (int i = 0; i < textStrings.Length; i++) {
			//Vector2 pos = innerDimensions.Center.ToVector2() + drawOffsets[i];
			Vector2 pos = innerDimensions.TopLeft() + drawOffsets[i];
			if (IsLarge)
				Utils.DrawBorderStringBig(spriteBatch, textStrings[i], pos, TextColor, TextScale, 0f, 0f, -1);
			else
				Utils.DrawBorderString(spriteBatch, textStrings[i], pos, TextColor, TextScale, 0f, 0f, -1);
		}

		//foreach (var singleLine in textStrings)
		//{
		//	Vector2 pos = innerDimensions.Position();
		//	if (this.IsLarge)
		//	{
		//		pos.Y -= 10f * this.TextScale * this.TextScale;
		//	}
		//	else
		//	{
		//		pos.Y -= 2f * this.TextScale;
		//	}
		//	pos.X += (innerDimensions.Width - this.TextSize.X) * 0.5f;
		//	if (this.IsLarge)
		//	{
		//		Utils.DrawBorderStringBig(spriteBatch, this.Text, pos, this.TextColor, this.TextScale, 0f, 0f, -1);
		//		return;
		//	}
		//	Utils.DrawBorderString(spriteBatch, this.Text, pos, this.TextColor, this.TextScale, 0f, 0f, -1);
		//}
	}
}