using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics;
using Terraria.ModLoader;

namespace ExampleMod.Common.Players
{
	// This ModPlayer facilitates a set bonus visual effect, also known as an armor set shadow effect. This example shows how either ArmorSetBonusActivated or ArmorSetBonusHeld can be used depending on how you want the player to interact with the set bonus effect.
	// To test out these effects, wear ExampleHood, ExampleBreastplate, and ExampleLeggings.
	// This example demonstrates using several vanilla armor set shadows toggled by double tapping the down key, as well as a completely custom armor set shadow toggled by holding the down key. "Shadow" effects in vanilla are also used for some dodges and dashes.
	public class ExampleArmorSetBonusPlayer : ModPlayer
	{
		public bool ExampleSetHood; // Indicates if the ExampleSet with ExampleHood is the active armor set.
		public int ShadowStyle = 0; // This is the shadow to use. Note that ExampleHood.ArmorSetShadows will only be called if the full armor set is visible.
		public bool CustomShadow; // Indicates that our custom shadow should be used.
		private int CustomShadowTimer; // Used in our custom shadow logic.
		private Color CustomShadowColor = Color.White;

		public override void ResetEffects() {
			ExampleSetHood = false;
			CustomShadow = false;
		}

		public override void ArmorSetBonusActivated() {
			if (!ExampleSetHood) {
				return;
			}

			if (ShadowStyle == 4) {
				ShadowStyle = 0;
			}
			ShadowStyle = (ShadowStyle + 1) % 4;
			ShowMessageForShadowStyle();
		}

		public override void ArmorSetBonusHeld(int holdTime) {
			if (!ExampleSetHood) {
				return;
			}

			if (holdTime == 60) {
				ShadowStyle = ShadowStyle == 4 ? 0 : 4;
				ShowMessageForShadowStyle();
			}
		}

		private void ShowMessageForShadowStyle() {
			string styleName = ShadowStyle switch {
				1 => "armorEffectDrawShadow",
				2 => "armorEffectDrawOutlines",
				3 => "armorEffectDrawOutlinesForbidden",
				4 => "CustomShadow",
				_ => "None",
			};
			Main.NewText("Current shadow style: " + styleName);
		}

		// For implementing a custom armor set "shadow" visual effect, we use the DrawPlayer hook to render the player multiple times.
		public override void DrawPlayer(Camera camera) {
			if (!CustomShadow) {
				return;
			}

			CustomShadowTimer++;
			int clones = 3;
			if (Player.statLife < Player.statLifeMax2 / 2) {
				clones += 1;
			}
			Vector2 playerPosition = Player.position + new Vector2(0f, Player.gfxOffY);
			// Draw the player 3 or 4 times
			for (int i = 0; i < clones; i++) {
				// This logic spins the clones around the player while pulsing them in and out.
				// See Terraria.Graphics.Renderers.LegacyPlayerRenderer.DrawPlayerFull to see how other vanilla effects are implemented.
				float rotation = CustomShadowTimer * 0.03f + i * (MathHelper.TwoPi / clones);
				float distanceFromPlayer = MathF.Sin(CustomShadowTimer * 0.06f);
				Vector2 drawPosition = playerPosition + new Vector2(0f, distanceFromPlayer).RotatedBy(rotation) * 25;
				float shadow = 1 - Utils.Remap(Math.Abs(distanceFromPlayer), 0, 1, 0.1f, 0.4f, clamped: true);

				// CustomShadowColor is used in TransformDrawData to apply a tint to the player clones.
				CustomShadowColor = Color.Lerp(Color.Aqua, Color.Red, (float)i / (clones - 1));

				// Main.PlayerRenderer.DrawPlayer handles drawing the player clone.
				Main.PlayerRenderer.DrawPlayer(camera, Player, drawPosition, Player.fullRotation, Player.fullRotationOrigin, shadow);
			}
			CustomShadowColor = Color.White; // Reset CustomShadowColor so it doesn't affect normal drawing or other clones.

			// Another common player effect is to draw the player at previous positions and slightly faded. Player.shadowPos/shadowRotation/shadowOrigin stores the last 3 positions and is used by shadow effects added to the game in early versions. Player.GetAdvancedShadow is a more recent addition and can be used for more advanced control and stores up to 60 previous positions.
			/*
			int totalShadows = Math.Min(Player.availableAdvancedShadowsCount, 30);
			int skip = 5;
			for (int i = totalShadows - totalShadows % skip; i > 0; i -= skip) {
				EntityShadowInfo advancedShadow = Player.GetAdvancedShadow(i);
				float shadow = Utils.Remap((float)i / totalShadows, 0, 1, 0.5f, 1f, clamped: true);
				Main.PlayerRenderer.DrawPlayer(camera, Player, advancedShadow.Position, advancedShadow.Rotation, advancedShadow.Origin, shadow);
			}
			*/
		}

		// TransformDrawData can be used to affect all PlayerDrawSet.DrawDataCache entries immediately before they are drawn.
		public override void TransformDrawData(ref PlayerDrawSet drawInfo) {
			// Check to only affect specific clones
			if (CustomShadowColor == Color.White) {
				return;
			}

			// Tint the clones with CustomShadowColor by modifying the draw color of every DrawData in drawInfo.DrawDataCache.
			for (int i = 0; i < drawInfo.DrawDataCache.Count; i++) {
				DrawData value = drawInfo.DrawDataCache[i];
				// Multiply the colors to tint it rather than assign it directly since DrawData.color will likely already have some non-white color.
				value.color = value.color.MultiplyRGBA(CustomShadowColor);
				drawInfo.DrawDataCache[i] = value;
			}
		}
	}
}
