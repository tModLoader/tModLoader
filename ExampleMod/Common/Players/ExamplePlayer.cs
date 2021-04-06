using Terraria.ModLoader;

namespace ExampleMod.Common.Players
{
	class ExamplePlayer : ModPlayer
	{
		public bool ShowMinionCount;
		// Debuffs
		public bool FireDebuff;

		public override void ResetEffects() {
			ShowMinionCount = false;
			FireDebuff = false;
		}

		public override void UpdateEquips() {
			if (Player.accThirdEye)
				ShowMinionCount = true;
		}

		// Allows you to give the player a negative life regeneration based on its state(for example, the "On Fire!" debuff makes the player take damage-over-time)
		// This is typically done by setting player.lifeRegen to 0 if it is positive, setting player.lifeRegenTime to 0, and subtracting a number from player.lifeRegen
		// The player will take damage at a rate of half the number you subtract per second
		public override void UpdateBadLifeRegen() {
			if (FireDebuff) {
				// These lines zero out any positive lifeRegen. This is expected for all bad life regeneration effects.
				if (Player.lifeRegen > 0)
					Player.lifeRegen = 0;
				
				Player.lifeRegenTime = 0;
				// lifeRegen is measured in 1/2 life per second. Therefore, this effect causes 8 life lost per second.
				Player.lifeRegen -= 16;
			}
		}
	}
}
