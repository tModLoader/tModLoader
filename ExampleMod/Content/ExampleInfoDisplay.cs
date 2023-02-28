using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace ExampleMod.Content
{
	// This example show how to create new informational display (like Radar, Watches, etc.)
	// Take a look at the ExampleInfoDisplayPlayer at the end of the file to see how to use it
	class ExampleInfoDisplay : InfoDisplay
	{
		// This dictates whether or not this info display should be active
		public override bool Active() {
			return Main.LocalPlayer.GetModPlayer<ExampleInfoDisplayPlayer>().showMinionCount;
		}

		// Here we can change the value that will be displayed in the game
		public override string DisplayValue(ref Color displayColor) {
			// Counting how many minions we have
			// This is the value that will show up when viewing this display in normal play, right next to the icon
			int minionCount = 0;
			for (int i = 0; i < Main.maxProjectiles; i++) {
				Projectile proj = Main.projectile[i];
				if (proj.active && proj.minion && proj.owner == Main.myPlayer) {
					minionCount++;
				}
			}

			bool noInfo = minionCount == 0;
			if (noInfo) {
				// If "No minions" will be displayed, grey out the text color, similar to DPS Meter or Radar
				displayColor = InactiveInfoTextColor;
			}

			return !noInfo ? $"{minionCount} minions." : "No minions";
		}
	}

	public class ExampleInfoDisplayPlayer : ModPlayer
	{
		// Flag checking when information display should be activated
		public bool showMinionCount;

		public override void ResetEffects() {
			showMinionCount = false;
		}

		public override void UpdateEquips() {
			// The information display is only activated when a Radar is present
			if (Player.accThirdEye)
				showMinionCount = true;
		}
	}
}
