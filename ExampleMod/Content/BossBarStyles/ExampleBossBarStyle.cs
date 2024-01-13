using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.BigProgressBar;
using Terraria.ModLoader;

namespace ExampleMod.Content.BossBars
{
	// Showcases very basic code for a custom boss bar style that is selectable in the menu in "Interface"
	// If you want custom NPC selection code for which boss bars to display, return true for PreventUpdate, and implement your own code in the Update hook
	public class ExampleBossBarStyle : ModBossBarStyle
	{
		public override bool PreventDraw => true; // Prevents the default drawing code

		public override void Draw(SpriteBatch spriteBatch, IBigProgressBar currentBar, BigProgressBarInfo info) {
			if (currentBar == null) {
				return;
				// Only draw if vanilla decided to draw one (we let it update because we didn't override PreventUpdate to return true)
			}

			if (currentBar is CommonBossBigProgressBar) {
				// If this is a regular bar without any special features, we draw our own thing. Sadly, "life to display" is not a variable we can access,
				// but since we are dealing with the very basic implementation that only tracks a single NPC, we can use "info"

				NPC npc = Main.npc[info.npcIndexToAimAt];
				float lifePercent = Utils.Clamp(npc.life / (float)npc.lifeMax, 0f, 1f);

				// Unused method by vanilla, which simply draws a few boxes that represent a boss bar (fixed position, colors, no icon)
				BigProgressBarHelper.DrawBareBonesBar(spriteBatch, lifePercent);

				if (info.showText && BigProgressBarSystem.ShowText) {
					// If the bar can currently draw text and the setting for it is enabled, draw the "life/lifeMax" text in the center of the bar (position code taken from DrawBareBonesBar)
					Rectangle barDimensions = Utils.CenteredRectangle(Main.ScreenSize.ToVector2() * new Vector2(0.5f, 1f) + new Vector2(0f, -50f), new Vector2(400f, 20f));
					BigProgressBarHelper.DrawHealthText(spriteBatch, barDimensions, 2 * Vector2.UnitY, npc.life, npc.lifeMax);
				}
			}
			else {
				// If a bar with special behavior is currently selected, draw it instead because we don't have access to its special features

				currentBar.Draw(ref info, spriteBatch);
			}
		}
	}
}
