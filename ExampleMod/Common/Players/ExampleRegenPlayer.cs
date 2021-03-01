using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Common.Players
{
	public class ExampleRegenPlayer : ModPlayer
	{
		public override ModPlayer.Regen ModifyLifeRegen() {
			var modifiers = ModPlayer.Regen.defaultRegen; // Grabs a copy of the struct with all values configured to do no modifications.

			// Modifies the time until player reaches maximum natural life regeneration by 1 / N per player update. Bounded -2 < N < -0.5 and 0.05 < N < 2 by vanilla. Positives for buffs, negatives for debuffs.
			modifiers.deltaRate = 0.5f; // Decreases the time to reach maximum regen by 2 counts per player update cycle

			// Modifies the speed at which life is recovered by N / 120 player updates. Positives for buffs, negatives for debuffs.
			modifiers.deltaRegen = 6f; // Increases the speed by 6 / 120 player updates.

			// Changes the potency of regeneration. Farther from zero for enhancing Regen, and closer to zero for weakening Regen. Warning: negatives invert regen behaviour and stack across mods (ie if the number of inversions is odd then poison heals, you take poison-esque damage faster when standing still etc).
			modifiers.multiplyRegen = 0.2f; // Decreases effective regen to 20% potency 

			// Set to true to declare that you want regen, after handling all debuffs to be <= 0. Typically true for damaging debuffs, false for buffs.
			modifiers.disablePositiveRegen = false;

			return modifiers; // Return your modifiers 
		}

		public override ModPlayer.Regen ModifyManaRegen() {
			var modifiers = ModPlayer.Regen.defaultRegen; // Grabs a copy of the struct with all values configured to do no modifications.

			// Modifies the delay before a player regenerates mana by 1 / N per player update. Bounded -2 < N < ~ -0.25 and 0.05 < N < 2 by vanilla. Total increased delay given by [(1 + 1/N) ^ (20)  - 1] for negative values. Positives for buffs, negatives for debuffs. Closer to zero is higher strength.
			modifiers.deltaRate = -10f; // Increases delay by 1 / 10 per player update. Total delay is ~ 6 update cycles, or roughly 25% more than the default 20 cycles.

			// Modifies the speed and direction at which mana is recovered by N / 120 * (vanilla manaregen scaling) per player update. Positives for buffs, negatives for debuffs.
			modifiers.deltaRegen = -10f; // Decreases the speed by 10 units, and attempts to drain player mana (this particular value coincides draining with player is walking).

			// Changes the potency of regeneration. Farther from zero for enhancing Regen, and closer to zero for weakening Regen.
			modifiers.multiplyRegen = 1.5f; // Increases effective regen to 150% potency. 

			// Set to true to declare that you want regen, after handling all debuffs to be <= 0. Typically true for damaging debuffs, false for buffs.
			modifiers.disablePositiveRegen = true;

			return modifiers; // Return your modifiers 
		}
	}
}
