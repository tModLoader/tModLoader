using Terraria.ModLoader;

namespace ExampleMod.Common.Players
{
	public class ExampleImmunityPlayer : ModPlayer
	{
		public bool HasExampleImmunityAcc;

		// Always reset the accessory field to its default value here.
		public override void ResetEffects() {
			HasExampleImmunityAcc = false;
		}

		// Vanilla applies immunity time before this method and after PreHurt and Hurt
		// Therefore, we should apply our immunity time increment here
		public override void PostHurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit, int cooldownCounter) {
			// Different cooldownCounter values mean different damage types taken and different cooldown slots
			// We should apply our immunity time to the correct cooldown slot
			// Slot -1: Most damages from all other sources not mentioned below
			// Slot 0: Contacting with tiles that deals damage, such as spikes and cactus in don't starve world
			// Slot 1: Special enemies (Vanilla for Moon Lord, Empress of Light and their minions and projectiles)
			// Slot 2: Ogre knockback
			// Slot 3: Trying to catch lava critters with regular bug net
			// Slot 4: Damages from lava

			// Here we increase the player's immunity time by 1 second when Example Immunity Accessory is equipped
			if (!HasExampleImmunityAcc) {
				return;
			}

			// If the cooldownCounter is -1, simply set Player.immuneTime
			if (cooldownCounter == -1) {
				// Do note that we should not give extra immunity time for pvp damages, just as vanilla do
				if (!pvp) {
					Player.immuneTime += 60; // 60 ticks = 1 second
				}
			}
			// Otherwise, set the array Player.hurtCooldowns with given cooldownCounter
			else {
				Player.hurtCooldowns[cooldownCounter] += 60;
			}
		}
	}
}
