using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;

namespace Terraria.UI.Chat;
public static partial class ChatManager
{
	// Overload with shadowColor param
	// Fix all instances of drawing text to use TextSnippets instead of strings (#FixNPCChat)
	public static void DrawColorCodedStringWithShadow(SpriteBatch spriteBatch, DynamicSpriteFont font, TextSnippet[] snippets, Vector2 position, Color color, Color shadowColor, float rotation, Vector2 origin, Vector2 baseScale, out int hoveredSnippet, float maxWidth = -1f, float spread = 2f)
	{
		DrawColorCodedStringShadow(spriteBatch, font, snippets, position, shadowColor, rotation, origin, baseScale, maxWidth, spread);
		DrawColorCodedString(spriteBatch, font, snippets, position, color, rotation, origin, baseScale, out hoveredSnippet, maxWidth);
	}

	// Overload with shadowColor param
	// Fix all instances of drawing text to use TextSnippets instead of strings (#FixNPCChat)
	public static void DrawColorCodedStringWithShadow(SpriteBatch spriteBatch, DynamicSpriteFont font, string text, Vector2 position, Color baseColor, Color shadowColor, float rotation, Vector2 origin, Vector2 scale, float maxWidth = -1f, float spread = 2f)
	{
		if (maxWidth < 0f && !MayNeedParsing(text)) {
			Vector2[] shadowDirections = ShadowDirections;
			foreach (Vector2 vector in shadowDirections) {
				spriteBatch.DrawString(font, text, position + vector * spread, shadowColor, rotation, origin, scale, SpriteEffects.None, 0f);
			}

			spriteBatch.DrawString(font, text, position, baseColor, rotation, origin, scale, SpriteEffects.None, 0f);
		}
		else {
			List<TextSnippet> snippets = ParseMessage(text, baseColor);
			ConvertNormalSnippets(snippets);
			List<PositionedSnippet> snippets2 = LayoutSnippets(font, snippets, scale, maxWidth).ToList();
			DrawColorCodedStringShadow(spriteBatch, font, snippets2, position, shadowColor, rotation, origin, scale, spread);
			DrawColorCodedString(spriteBatch, font, snippets2, position, rotation, origin, scale, out var _);
		}
	}
}
