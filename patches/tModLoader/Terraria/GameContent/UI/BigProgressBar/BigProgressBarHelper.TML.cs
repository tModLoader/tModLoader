using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;

namespace Terraria.GameContent.UI.BigProgressBar;

public partial class BigProgressBarHelper
{
	// Copy of the private BigProgressBarHelper.DrawHealthText with an offset parameter
	/// <summary>
	/// Draws "<paramref name="current"/>/<paramref name="max"/>" as text centered on <paramref name="area"/>, offset by <paramref name="textOffset"/>.
	/// </summary>
	/// <param name="spriteBatch">The spriteBatch that is drawn on</param>
	/// <param name="area">The Rectangle that the text is centered on</param>
	/// <param name="textOffset">Offset for the text position</param>
	/// <param name="current">Number shown left of the "/"</param>
	/// <param name="max">Number shown right of the "/"</param>
	public static void DrawHealthText(SpriteBatch spriteBatch, Rectangle area, Vector2 textOffset, float current, float max)
	{
		DynamicSpriteFont font = FontAssets.ItemStack.Value;
		Vector2 center = area.Center.ToVector2() + textOffset;
		center.Y += 1f;
		string text = "/";
		Vector2 textSize = font.MeasureString(text);
		Utils.DrawBorderStringFourWay(spriteBatch, font, text, center.X, center.Y, Color.White, Color.Black, textSize * 0.5f);
		text = ((int)current).ToString();
		textSize = font.MeasureString(text);
		Utils.DrawBorderStringFourWay(spriteBatch, font, text, center.X - 5f, center.Y, Color.White, Color.Black, textSize * new Vector2(1f, 0.5f));
		text = ((int)max).ToString();
		textSize = font.MeasureString(text);
		Utils.DrawBorderStringFourWay(spriteBatch, font, text, center.X + 5f, center.Y, Color.White, Color.Black, textSize * new Vector2(0f, 0.5f));
	}
}
