using Terraria;
using Terraria.ModLoader;
using System.Linq;

namespace ExampleMod.Content.Buffs
{
	public class ExampleRadiationDebuff : ModBuff
	{
		public override PlayerRegenEffects.RegenEffect ModManaRegenEffects() {
			return new PlayerRegenEffects.RegenEffect(
				// The name you want this effect to be internally known and searchable by in RegenEffect.effects. Recommend using full name
				name: FullName,

				// Uses the available player instance, and returns if the debuff is active
				isActive: (player) => player.buffType.Contains(Type),

				// The (optional) struct of the mana regen stat calculations to apply associated with this debuff, used every frame update.
				manaCommon: PlayerRegenEffects.CommonRegenStats.Create(

					// (optional parameter) Returns a float corresponding to the mana drain this debuff does 
					delta: _ => -(0.5f + Main.rand.Next(32)), // Does between 0.5 and 32.5 drain every 120 seconds 
					
					// (optional parameter) Can apply additional effects based on this void delegate, which receives parameters player and the regen calculated up to this point.
					additionalEffects: (player, regen) => {
						player.manaRegenPotencyMultiplier *= (85 + Main.rand.Next(30)) / 100; // randomly modifies the calculated regen multiplicatively in range of +/- 15%.
					},

					// (optional parameter) Set to true to allow your entry to have net positive regen despite a damaging debuff being present.
					forcePositiveRegen: false
				),

				// The (optional) struct of the mana delay calculations to apply associated with this debuff, used every frame update
				PlayerRegenEffects.ManaRegenDelayStats.Create(

					// (optional parameter) Sets the maximum Maximum Delay that the player has to wait, in frames assuming a speed of one, for mana to begin regeneration/draining after mana usage. 
					maxDelayCap: (player) => 120,

					// (optional parameter) Attempts to increase the Maximum Delay by this in frames, assuming a speed of one. Won't increase above the lowest maxDelayCap of all active effects.
					increaseMaxDelay: (player) => 35,

					// (optional parameter) Increases the speed at which the Delay is consumed by this number for each frame.
					increaseDelaySpeed: (player) => 5, // setting to 5 means that assuming the current speed is one frame per frame, we will now do 6 frames of elasping delay per frame.

					// (optional parameter) This bool will forcefully reset mana delay to zero, bypassing all remaining calls to speed up mana delay. If you want to continuously drain mana in a debuff, set this to true.
					resetDelayToZero: (player) => true // We have a debuff setup to drain mana, so we set it to true so it doesn't stop draining when player uses mana.
				)
			);
		}

		public override void SetDefaults() {
			DisplayName.SetDefault("Radiated debuff");
			Description.SetDefault("The radiation is burning your lungs");
			Main.buffNoTimeDisplay[Type] = false;
			Main.debuff[Type] = true; //Nurse removes the buff when healing
		}
	}
}