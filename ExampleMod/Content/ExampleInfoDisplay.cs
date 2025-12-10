using ExampleMod.Common.Players;
using ExampleMod.Content.Items.Accessories;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod.Content
{
	/// <summary>
	/// InfoDisplay that is coupled with <seealso cref="ExampleInfoAccessory"/> and <seealso cref="ExampleInfoDisplayPlayer"/> to show
	/// off how to add a new info accessory (such as a Radar, Lifeform Analyzer, etc.)
	/// </summary>
	public class ExampleInfoDisplay : InfoDisplay
	{
		public static LocalizedText CurrentMinionsText { get; private set; }
		public static LocalizedText NoMinionsText { get; private set; }

		public override void SetStaticDefaults() {
			CurrentMinionsText = this.GetLocalization("CurrentMinions");
			NoMinionsText = this.GetLocalization("NoMinions");
		}

		public static Color RedInfoTextColor => new(255, 19, 19, Main.mouseTextColor);

		// By default, the vanilla circular outline texture will be used.
		// This info display has a square icon instead of a circular one, so we need to use a custom outline texture instead of the vanilla outline texture.
		// You will only need to use a custom hover texture if your info display icon doesn't perfectly match the shape that vanilla info displays use
		public override string HoverTexture => Texture + "_Hover";

		// This dictates whether or not this info display should be active
		public override bool Active() {
			return Main.LocalPlayer.GetModPlayer<ExampleInfoDisplayPlayer>().showMinionCount;
		}

		// Here we can change the value that will be displayed in the game
		public override string DisplayValue(ref Color displayColor, ref Color displayShadowColor) {
			// Counting how many minions we have
			// This is the value that will show up when viewing this display in normal play, right next to the icon
			int minionCount = 0;
			foreach (var proj in Main.ActiveProjectiles) {
				if (proj.minion && proj.owner == Main.myPlayer) {
					minionCount++;
				}
			}

			bool noInfo = minionCount == 0;
			if (noInfo) {
				// If "No minions" will be displayed, grey out the text color, similar to DPS Meter or Radar
				displayColor = InactiveInfoTextColor;
			}
			else if (minionCount < Main.LocalPlayer.maxMinions) {
				// This red color serves as a warning that the player has not summoned all their minions.
				displayColor = RedInfoTextColor;
			}
			/*
			else if (minionCount == Main.LocalPlayer.maxMinions) {
				// The gold text color used for gold critters by the Lifeform Analyzer is easily accessible if needed
				displayColor = GoldInfoTextColor;
				displayShadowColor = GoldInfoTextShadowColor;
			}
			*/

			return !noInfo ? CurrentMinionsText.Format(minionCount) : NoMinionsText.Value;
		}
	}

}
