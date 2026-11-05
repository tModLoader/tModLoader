using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace ExampleMod.Content.Rarities
{
	public class ExampleHigherTierModRarity : ModRarity
	{
		public override Color RarityColor => new Color(Main.DiscoR / 2, (byte)(Main.DiscoG / 1.25f), (byte)(Main.DiscoB / 1.5f));

		public override int GetPrefixedRarity(int offset, float valueMult) {
			if (offset < 0) { // If the offset is -1 or -2 (a negative modifier).
				return ModContent.RarityType<ExampleModRarity>(); // Make the rarity of items that have this rarity with a negative modifier the lower tier one.
			}

			return Type; // no 'higher' tier to go to, so return the type of this rarity.
		}

		// TODO: Needs more comprehensive comments, maybe.
		// Renders an underline effect using Pre/PostDrawText and mutates the
		// scale over time with PreDrawText to demonstrate custom rendering and
		// mutation of RarityTextContext.
		public override bool PreDrawText(SpriteBatch sb, ref RarityTextContext textContext) {
			// Change scale based on a global counter.
			textContext.Scale *= 0.98f + float.Sin(Main.GlobalTimeWrappedHourly * 10f) * 0.04f;

			// Because we change the scale, we may also want to ensure the
			// origin of the text is centered. This demonstrates how to change
			// the origin to a desired value.
			var size = ChatManager.GetStringSize(textContext.Font, textContext.Text, Vector2.One, textContext.MaxTextWidth);
			var center = size * 0.5f;
			textContext.Position += center - textContext.Origin;
			textContext.Origin = center;

			// Draw the outlines of the bar in PreDrawText so it draws below
			// the rarity text, letting its outlines merge with the actual
			// text.
			for (int i = 0; i < 4; i++) {
				// Calculate the spread. We draw the outline for times -- one
				// in each cardinal direction -- and use
				// ChatManager.ShadowDirections to get those values for us. We
				// can use whatever spread value we'd like, but let's use the
				// supplied spread for this rarity rendering context.
				Vector2 offset = ChatManager.ShadowDirections[i] * textContext.TextSpread;

				// In some cases, the text context may have a secondary outline
				// color. This is used for popup text rendering. For vanilla
				// accuracy, you should check for its presence and draw it
				// before
				if (textContext.SecondaryOutlineColor.HasValue)
					DrawUnderlineBar(sb, in textContext, offset, textContext.SecondaryOutlineColor.Value);
				DrawUnderlineBar(sb, in textContext, offset, textContext.OutlineColor);
			}

			// Return true to preserve Vanilla text rendering, which we want here.
			return true;
		}

		public override void PostDrawText(SpriteBatch sb, in RarityTextContext textContext) {
			// Draw the main (non-outlined) part of the bar in PostDrawText so
			// it draws above the regular text and outlines.
			DrawUnderlineBar(sb, in textContext, Vector2.Zero, textContext.PrimaryColor);
		}

		private static void DrawUnderlineBar(SpriteBatch sb, in RarityTextContext textContext, Vector2 offset, Color color) {
			// Measure the text size to get the unscaled size of the bar.
			var textSize = ChatManager.GetStringSize(textContext.Font, textContext.Text, Vector2.One, textContext.MaxTextWidth);

			// Calculate the actual position of the bar with the origin in
			// mind.
			var position = textContext.Position
				+ (new Vector2(0f, textSize.Y - 12f) - textContext.Origin) * textContext.Scale
				+ offset;

			var barSize = new Vector2(textSize.X, 2f) * textContext.Scale;

			sb.Draw(
				TextureAssets.MagicPixel.Value,
				position,
				new Rectangle(0, 0, 1, 1),
				color,
				0f,
				Vector2.Zero,
				barSize,
				SpriteEffects.None,
				0f
			);
		}
	}
}
