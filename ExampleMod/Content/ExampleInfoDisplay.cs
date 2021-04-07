using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Content
{
	// This example show how to create new informational display (like Radar, Watches, etc.)
	// Take a look at the ExampleInfoDisplayPlayer at the end of the file to see how to use it
	class ExampleInfoDisplay : InfoDisplay
	{
		public override void SetDefaults() {
			// This is the name that will show up when hovering over icon of this info display
			InfoName.SetDefault("Minion Count");
		}

		// This dictates whether or not this info display should be active
		public override bool Active() {
			return Main.LocalPlayer.GetModPlayer<ExampleInfoDisplayPlayer>().showMinionCount;
		}

		// Here we can change the value that will be displayed in the game
		public override string DisplayValue() {
			// Counting how many minions we have
			int minionCount = Main.projectile.Count(x =>x.active && x.minion && x.owner == Main.LocalPlayer.whoAmI);
			// This is the value that will show up when viewing this display in normal play, right next to the icon
			return minionCount > 0 ? $"{minionCount} minions." : "No minions";
		}
	}

			player.GetModPlayer<ExampleLifeRegenDebuffPlayer>().lifeRegenDebuff= true;
		}
	}

	public class ExampleLifeRegenDebuffPlayer : ModPlayer
	{
		// Flag checking when life regen debuff should be activated
		public bool lifeRegenDebuff;

		public override void ResetEffects() {
			lifeRegenDebuff = false;
		}

		// Allows you to give the player a negative life regeneration based on its state (for example, the "On Fire!" debuff makes the player take damage-over-time)
		// This is typically done by setting player.lifeRegen to 0 if it is positive, setting player.lifeRegenTime to 0, and subtracting a number from player.lifeRegen
		// The player will take damage at a rate of half the number you subtract per second
		public override void UpdateBadLifeRegen() {
			if (lifeRegenDebuff) {
		}
	}
}
