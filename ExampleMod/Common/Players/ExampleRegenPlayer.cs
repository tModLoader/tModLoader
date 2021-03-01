using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Common.Players
{
	public class ExampleRegenPlayer : ModPlayer
	{
		public override ModPlayer.Regen ModifyLifeRegen() {
			var modifiers = ModPlayer.Regen.Create(); // Creates the struct with all values configured to do no modifications.
			modifiers.deltaRate = 3f; // Increases the rate at which player reaches maximum natural life regeneration. Positives for buffs, negatives for debuffs. Min: -3600. Max: 30000.
			modifiers.deltaRegen = 6f; // Increases the life recovered per second up to a max of 6. Positives for buffs, negatives for debuffs.
			//modifiers.multiplyRegen = -0.2f; // Decreases effective regen to 20% potency and negates it flipping all preconceptions. Typically positive and >1 for enhancing Regen, and negative and closer to zero for weakening and inverting Regen. 
			return modifiers; // Return your modifiers 
		}

		public override ModPlayer.Regen ModifyManaRegen() {
			var modifiers = ModPlayer.Regen.Create(); // Creates the struct with all values configured to do no modifications.
			modifiers.deltaRate = -3f; // Increases the delay before a player regenerates mana. Positives for buffs, negatives for debuffs.
			modifiers.deltaRegen = -4f; // Decreases the mana recovered per second. Positives for buffs, negatives for debuffs.
			modifiers.multiplyRegen = 0.8f; // Decreases effective regen to 80% potency. Only positive values work here.
			//modifiers.maxZeroRegen = true; // Declares that you want effective regen, after handling all debuffs to be <= 0. Typically true for debuffs, false for buffs.
			return modifiers; // Return your modifiers 
		}
	}
}
