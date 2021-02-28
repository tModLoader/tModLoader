using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Common.Players
{
	public class ExampleRegenPlayer : ModPlayer
	{
		public override ModPlayer.Regen ModifyLifeRegen() {
			var modifiers = ModPlayer.Regen.Create(); // Creates the struct with all values configured to do no modifications.
			modifiers.deltaRate = 3f; // Increases the rate at which player reaches maximum natural life regeneration for every multiple of 300 up to 3600. Positives for buffs, negatives for debuffs.
			modifiers.deltaRegen = 6f; // Increases the life per second recovered by typically 6 / 2 per second. Positives for buffs, negatives for debuffs.
			modifiers.multiplyRegen = -0.2f; // Decreases effective regen to 20% potency and negates it. Typically positive and >1 for strong buffs, and negative and closer to zero for weak debuffs.
			return modifiers; // Return your modifiers 
		}

		public override ModPlayer.Regen ModifyManaRegen() {
			var modifiers = ModPlayer.Regen.Create(); // Creates the struct with all values configured to do no modifications.
			modifiers.deltaRate = -3f; // Increases the delay before a player regenerates mana. Positives for buffs, negatives for debuffs.
			modifiers.deltaRegen = -2f; // Decreases the mana per second recovered by 2 / 2 per second. Positives for buffs, negatives for debuffs.
			modifiers.multiplyRegen = 0.8f; // Typically use positive values. Decreases effective regen to 80% potency.
			modifiers.maxZeroRegen = true; // Declares that you want effective regen, after handling all debuffs and prior to multiplication, to be less than or equal to zero. Typically true for debuffs, false for buffs.
			return modifiers; // Return your modifiers 
		}
	}
}
