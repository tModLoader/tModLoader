using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.EmoteBubbles
{
	// This is a showcase of drawing the emote bubble yourself.
	// It performs totally the same as vanilla.
	// Check Common/GlobalNPC/EmotePickerGlobalNPC.cs for adding this emote for all NPCs.
	public class ExampleBiomeEmote : ModEmoteBubble
	{
		public override void SetStaticDefaults() {
			// Add the emote to "biomes" category
			AddToCategory(EmoteID.Category.NatureAndWeather);
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Rectangle frame, Vector2 origin, SpriteEffects spriteEffects) {
			// Extra_48 is the texture of all vanilla emotes.
			Texture2D bubbleTexture = TextureAssets.Extra[ExtrasID.EmoteBubble].Value;
			// This is the frame rectangle for the bubble in emotes texture.
			Rectangle bubbleFrame = bubbleTexture.Frame(8, 39, EmoteBubble.IsFullyDisplayed ? 1 : 0);

			// Draw the bubble background.
			spriteBatch.Draw(bubbleTexture, position, bubbleFrame, Color.White, 0f, origin, 1f, spriteEffects, 0f);

			// If the emote bubble isn't fully displayed (bubble pop-up animation is being displayed),
			// don't draw the emote content.
			if (!EmoteBubble.IsFullyDisplayed) {
				return false;
			}

			// Draw the emote.
			spriteBatch.Draw(texture, position, frame, Color.White, 0f, origin, 1f, spriteEffects, 0f);

			return false; // Stop vanilla drawing code.
		}

		// This method is for drawing emote in the emotes menu.
		public override bool PreDrawInEmoteMenu(SpriteBatch spriteBatch, EmoteButton uiEmoteButton, Vector2 position, Rectangle frame, Vector2 origin) {
			// This color is used for border that becomes yellow (or blue) when you hover your cursor over it.
			Color borderColor = Color.Black;
			if (uiEmoteButton.Hovered) {
				borderColor = Main.OurFavoriteColor;
			}
			// This is the frame rectangle for the bubble in emotes texture.
			Rectangle bubbleFrame = uiEmoteButton.BubbleTexture.Frame(8, 39, 1, 0);

			// Draw everything
			spriteBatch.Draw(uiEmoteButton.BubbleTexture.Value, position, bubbleFrame, Color.White, 0f, origin, 1f, SpriteEffects.None, 0f);
			spriteBatch.Draw(uiEmoteButton.EmoteTexture.Value, position, frame, Color.White, 0f, origin, 1f, SpriteEffects.None, 0f);
			spriteBatch.Draw(uiEmoteButton.BorderTexture.Value, position - Vector2.One * 2f, null, borderColor, 0f, origin, 1f, SpriteEffects.None, 0f);

			return false;
		}
	}
}
