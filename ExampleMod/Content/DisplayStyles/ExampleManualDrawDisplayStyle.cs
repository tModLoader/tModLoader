using Microsoft.Xna.Framework;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.DisplayStyles
{
	// Example of a ModResourceDisplayStyle with entirely manual drawing
	// See ExampleDrawSettingsDisplayStyle for an example of a ModResourcesDisplayStyle that uses
	// the ResourceDrawSettings struct instead.
	public class ExampleManualDrawDisplayStyle : ModResourcesDisplayStyle
	{
		public override void SetupContent() {
			base.SetupContent();
			
			// Change the display name shown in the settings menu
			// This is entirely irrelevant to the internal name of a style, which is "ModName/Name"
			DisplayName.SetDefault("Text Example");
		}

		// Here we have an example of manually drawing the value of a player's HP and mana through text
		public override void Draw() {
			const int spacingOffset = 8;
			int drawX = Main.screenWidth - 75; // Define an anchor for out text to draw
			float textHeight = FontAssets.MouseText.Value.MeasureString("Y").Y; // Get the height of MouseText for positioning extra text under existing text

			// It's safe to use Main.LocalPlayer for our player instance since this is entirely client-side UI drawing
			Player player = Main.LocalPlayer;

			// Draw the player's HP and tint it the same rainbow color as stuff like the Expert Mode tooltip
			// Use statLifeMax2 for displaying maximum HP since it has extra HP bonuses applied
			Main.spriteBatch.DrawString(FontAssets.MouseText.Value, $"{player.statLife}/{player.statLifeMax2}", new Vector2(drawX, spacingOffset), Main.DiscoColor);
			
			// We use the height of a character in FontAssets.MouseText for the initial offset, and then multiply and add spacingOffset for proper spacing
			Vector2 manaDrawPos = new Vector2(drawX, textHeight + (spacingOffset * 2));
			
			// Draw the player's mana with the same color as Terraria's blue rarity
			// Use statManaMax2 for the maximum mana since it has extra mana bonuses applied
			Main.spriteBatch.DrawString(FontAssets.MouseText.Value, $"{player.statMana}/{player.statManaMax2}", manaDrawPos, Colors.RarityBlue);
		}

		// See ExampleSettingsDisplaySet for TryToHover examples
		// (Do note that you will need to manually check for when
		// the user is hovering over an element since this is all manually drawn
		// If you're using ResourceDrawSettings, it has a method that detects when you're hovering over it. 
		// public override void TryToHover() => base.TryToHover();
	}
}
