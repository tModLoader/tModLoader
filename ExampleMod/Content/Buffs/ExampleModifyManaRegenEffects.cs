using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Common.Players
{
	public class ExampleModifyManaRegenEffects : GlobalBuff
	{
		public override List<Player.RegenEffect.ModifyRegenEffectStruct> ModifyManaRegenEffects() {
			List<Player.RegenEffect.ModifyRegenEffectStruct> list = new List<Player.RegenEffect.ModifyRegenEffectStruct>();

			list.Add(new Player.RegenEffect.ModifyRegenEffectStruct {
				// The internal name of the effect we wish to modify. See Player.TML.cs for Vanilla names.
				targetEffect = ("stillPlayerMana"),

				// The new condition we want to apply; set to null to not change it
				isActive = null, 

				// The struct containing information on what we want to modify in CommonRegenStats. See ExampleRadiationDebuff for an example of this struct
				modifyManaCommonWith = Player.RegenEffect.CommonRegenStats.Create(
						delta: (player) => player.statLife / 2, // We want to increase manaRegen by half the players current life.
						forcePositiveRegen: true // We want it to increase regen above zero even if a debuff is present
					),

				// The struct containing information on what we want to modify in ManaRegenDelayStats
				modifyManaDelayWith = Player.RegenEffect.ManaRegenDelayStats.Create(
						increaseDelaySpeed: _ => 2 // We want to increase the delay speed by two for this effect
					),

				// The struct defining how we want to modify the existing effect. See CombinationFlags in Player.TML.cs.
				flags = Player.RegenEffect.ModifyFlags.Create(
						Player.RegenEffect.CommonRegenStats.CombinationFlags.Create(
							false, // Setting to false means that we want our change to deltaRegenPer120Frames to be additive with existing. True would replace it.
							false // setting to false means that we won't replace the current code in additionalEffects, and instead just add ours (if it existed) to the run.
						),

						Player.RegenEffect.ManaRegenDelayStats.CombinationFlags.Create(
							// Bool 2 corresponds to overrideIncreaseDelaySpeed in ManaRegenDelayStats.CombinationFlags. We can directly work with flags for only what we modified.
							b2: true // We set it to true, meaning that we will replace the existing speed bonus with ours, and discard the existing bonus. Not very cross-mod compatible, but desirable in some circumstances.
						),

						false, // Setting to true would mean we want to replace the current condition in targetEffect with ours. As with other override flags, set to false for compatibility!
						false // When overrideCondition is false, this would set that we want to combine the conditions using Logical OR.
					)
			});

			return list;
		}
	}
}
