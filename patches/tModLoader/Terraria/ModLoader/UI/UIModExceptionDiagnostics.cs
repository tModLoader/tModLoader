using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent;

namespace Terraria.ModLoader.UI;

// Modelled after NetDiagnosticsUI for consistent visuals.
public class UIModExceptionDiagnostics
{
	private const float TextScale = 0.7f;
	private const string Suffix = ": ";
	private const string ModString = "Mod";
	private const string ErrorsString = "Errors";

	private Asset<DynamicSpriteFont> fontAsset;
	private Asset<DynamicSpriteFont> FontAsset => fontAsset ??= FontAssets.MouseText;

	private readonly Mod[] Mods;
	private int HighestFoundErrorCount = 1;
	private float FirstColumnWidth;

	public UIModExceptionDiagnostics(IEnumerable<Mod> mods)
	{
		Mods = mods.ToArray();
		Reset();
	}

	public void Reset()
	{
		var font = FontAsset.Value;
		FirstColumnWidth = font.MeasureString(ModString).X; // Default in case no mods are loaded

		for (int i = 0; i < Mods.Length; i++) {
			float length = font.MeasureString(Mods[i].Name).X;
			if (FirstColumnWidth < length)
				FirstColumnWidth = length;
		}

		FirstColumnWidth += font.MeasureString(Suffix).X + 2;
		FirstColumnWidth *= TextScale;
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		int count = ModLoader.Mods.Length;
		const int maxLinesPerCol = 50;
		int numCols = (count - 1) / maxLinesPerCol;
		int x = 190;
		int xBuf = x + 10;
		int y = 110;
		int yBuf = y + 10;

		int width = 232;
		// Adjust based on left column width and right column width
		width += (int)(FirstColumnWidth + fontAsset.Value.MeasureString(HighestFoundErrorCount.ToString()).X * TextScale);
		int widthBuf = width + 10;
		int lineHeight = 13;

		for (int i = 0; i <= numCols; i++) {
			int lineCountInCol = i == numCols ? 1 + (count - 1) % maxLinesPerCol : maxLinesPerCol;
			int height = lineHeight * (lineCountInCol + 2);
			int heightBuf = height + 10;
			if (i == 0) {
				Utils.DrawInvBG(spriteBatch, x + widthBuf * i, y - 20, width, heightBuf + 20);
				spriteBatch.DrawString(FontAssets.MouseText.Value, Localization.Language.GetTextValue("tModLoader.PressXToClose", Microsoft.Xna.Framework.Input.Keys.F8), new Vector2(200, 96), Color.White, 0f, default(Vector2), 1f, SpriteEffects.None, 0f);
			}
			else {
				Utils.DrawInvBG(spriteBatch, x + widthBuf * i, y, width, heightBuf);
			}

			Vector2 modPos = new Vector2(xBuf + widthBuf * i, yBuf);
			Vector2 headerPos = modPos + new Vector2(FirstColumnWidth, 0);
			DrawText(spriteBatch, ErrorsString, headerPos, Color.White);
			DrawText(spriteBatch, ModString, modPos, Color.White);
		}

		Vector2 position = default;
		for (int j = 0; j < count; j++) {
			Logging.nonIgnoredExceptionCountByMod.TryGetValue(ModLoader.Mods[j].Name, out int errorCount);

			int colNum = j / maxLinesPerCol;
			int lineNum = j - colNum * maxLinesPerCol;
			position.X = xBuf + colNum * widthBuf;
			position.Y = yBuf + lineHeight + lineNum * lineHeight;

			DrawCounter(spriteBatch, errorCount, ModLoader.Mods[j].Name, position);
		}
	}

	private void DrawCounter(SpriteBatch spriteBatch, int errorCount, string title, Vector2 position)
	{
		if (HighestFoundErrorCount < errorCount)
			HighestFoundErrorCount = errorCount;

		Vector2 pos = position;
		string lineName = title + Suffix;
		float num = Utils.Remap(errorCount, 0f, HighestFoundErrorCount, 0f, 1f);
		Color color = Main.hslToRgb(0.3f * (1f - num), 1f, 0.5f);

		string drawText = lineName;
		DrawText(spriteBatch, drawText, pos, color);
		pos.X += FirstColumnWidth;
		drawText = string.Format("{0,0}", errorCount);
		DrawText(spriteBatch, drawText, pos, color);
	}

	private void DrawText(SpriteBatch spriteBatch, string text, Vector2 pos, Color color) =>
		spriteBatch.DrawString(FontAsset.Value, text, pos, color, 0f, Vector2.Zero, TextScale, SpriteEffects.None, 0f);
}
