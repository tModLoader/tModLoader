using ExampleMod.Common.Players;
using ExampleMod.Content.Items.Accessories;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace ExampleMod.Content
{
	/// <summary>
	/// InfoDisplay that is coupled with <seealso cref="ExampleInfoAccessory"/> and <seealso cref="ExampleInfoDisplayPlayer"/> to show
	/// off how to add a new info accessory (such as a Radar, Lifeform Analyzer, etc.)
	/// </summary>
	public class ExampleInfoDisplay : InfoDisplay
	{
		// This line says that the outline texture used when hovering over an info display is the same as the one vanilla displays use
		// Since we are using a custom one, this line will be commented out
		// You will only need to use a custom hover texture if your info display icon doesn't perfectly match the shape that vanilla info displays use
		// This info display has a square icon instead of a circular one, so we need to use a custom texture instead of the vanilla texture
		// public override string HoverTexture => VanillaHoverTexture;

		// This dictates whether or not this info display should be active
		public override bool Active() {
			return Main.LocalPlayer.GetModPlayer<ExampleInfoDisplayPlayer>().showMinionCount;
		}

		// Here we can change the value that will be displayed in the game
		public override string DisplayValue(ref Color displayColor, ref Color displayShadowColor) {
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

			return !noInfo ? $"{minionCount} minions" : "No minions";
		}
	}

}
