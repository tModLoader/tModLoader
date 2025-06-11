using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.UI;

namespace Terraria.ModLoader.UI;

internal class UIModStateText : UIElement
{
	private bool _enabled;

	private string DisplayText
		=> _enabled
			? Language.GetTextValue("GameUI.Enabled")
			: Language.GetTextValue("GameUI.Disabled");

	private Color DisplayColor
		=> _enabled ? Color.Green : Color.Red;

	public UIModStateText(bool enabled = true)
	{
		_enabled = enabled;
		PaddingLeft = PaddingRight = 5f;
		PaddingBottom = PaddingTop = 10f;
	}

	public void SetEnabled()
	{
		_enabled = true;
		Recalculate();
	}

	public void SetDisabled()
	{
		_enabled = false;
		Recalculate();
	}

	public override void Recalculate()
	{
		var textSize = new Vector2(FontAssets.MouseText.Value.MeasureString(DisplayText).X, 16f);
		Width.Set(textSize.X + PaddingLeft + PaddingRight, 0f);
		Height.Set(textSize.Y + PaddingTop + PaddingBottom, 0f);
		base.Recalculate();
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		base.DrawSelf(spriteBatch);
		DrawPanel(spriteBatch);
		DrawEnabledText(spriteBatch);
	}

	private void DrawPanel(SpriteBatch spriteBatch)
	{
		var position = GetDimensions().Position();
		var width = Width.Pixels;
		spriteBatch.Draw(UICommon.InnerPanelTexture.Value, position, new Rectangle(0, 0, 8, UICommon.InnerPanelTexture.Height()), Color.White);
		spriteBatch.Draw(UICommon.InnerPanelTexture.Value, new Vector2(position.X + 8f, position.Y), new Rectangle(8, 0, 8, UICommon.InnerPanelTexture.Height()), Color.White, 0f, Vector2.Zero, new Vector2((width - 16f) / 8f, 1f), SpriteEffects.None, 0f);
		spriteBatch.Draw(UICommon.InnerPanelTexture.Value, new Vector2(position.X + width - 8f, position.Y), new Rectangle(16, 0, 8, UICommon.InnerPanelTexture.Height()), Color.White);
	}

	private void DrawEnabledText(SpriteBatch spriteBatch)
	{
		var pos = GetDimensions().Position() + new Vector2(PaddingLeft, PaddingTop * 0.5f);
		Utils.DrawBorderString(spriteBatch, DisplayText, pos, DisplayColor);
	}
}