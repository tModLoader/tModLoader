using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Common.Players
{
	public class ExampleModifyManaRegenEffects : GlobalBuff
	{
		public override List<PlayerRegenEffects.ModifyRegenEffectStruct> ModifyManaRegenEffects() {
			List<PlayerRegenEffects.ModifyRegenEffectStruct> list = new List<PlayerRegenEffects.ModifyRegenEffectStruct>();

			// Example 1: Modifiying player stand still bonus to be always active via overrides
			list.Add(PlayerRegenEffects.ModifyRegenEffectStruct.Create(
				// The internal name of the effect we wish to modify. See Player.TML.cs for Vanilla names.
				targetEffect: "stillPlayerMana",

				// The delegate for the new condition that we want terraria to check for this effect being active.
				isActive: (player) => true,
				
				// The struct defining how we want to modify the existing effect. See CombinationFlags in Player.TML.cs.
				flags: PlayerRegenEffects.ModifyFlags.Create(
						 // Setting to true replaces the current calculation in this effect with ours. As with other override flags, set to false for compatibility!
						 overrideIsActive: true
				)
			));

			// Example 2: Adding additional IsActive conditions to player stand still bonus, restoring it back to normal
			list.Add(PlayerRegenEffects.ModifyRegenEffectStruct.Create(
				// The internal name of the effect we wish to modify. See Player.TML.cs for Vanilla names.
				targetEffect: "stillPlayerMana",

				// The delegate for the new condition that we want terraria to check for this effect being active.
				isActive: PlayerRegenEffects.stillPlayerMana.isActive, // We will re-add the original condition

				// The struct defining how we want to modify the existing effect. See CombinationFlags in Player.TML.cs.
				flags: PlayerRegenEffects.ModifyFlags.Create(
						// Setting to true causes logical AND combination. Setting/defaulting as false uses logical OR combination.
						useANDForIsActiveElseOR: true // We will combine the existing condition with ours based on logical AND. IE: (existing && ours).
				)
			));

			// Example 3: Changing the strength of player stand still bonus and allowing it to cause positive regen in the presence of a debuff
			list.Add(PlayerRegenEffects.ModifyRegenEffectStruct.Create(
				// The internal name of the effect we wish to modify. See Player.TML.cs for Vanilla names.
				targetEffect: "stillPlayerMana",

				// The struct containing information on what we want to modify in CommonRegenStats. See ExampleRadiationDebuff for an example of this struct
				modifyManaCommonWith: PlayerRegenEffects.CommonRegenStats.Create(
					// We want to increase manaRegen granted by half the players current life.
					delta: (player) => (float)player.statLife / 2,
					// We want it to increase regen above zero even if a debuff is present
					forcePositiveRegen: true 
				)
			));

			// Example 4: Changing the strength of player stand still bonus via overrides to restore it to normal
			list.Add(PlayerRegenEffects.ModifyRegenEffectStruct.Create(
				// The internal name of the effect we wish to modify. See Player.TML.cs for Vanilla names.
				targetEffect: "stillPlayerMana",

				// The struct containing information on what we want to modify in CommonRegenStats. See ExampleRadiationDebuff for an example of this struct
				modifyManaCommonWith: PlayerRegenEffects.CommonRegenStats.Create(
					delta: PlayerRegenEffects.stillPlayerMana.manaCommon.deltaRegenPer120Frames,
					forcePositiveRegen: false
				),

				flags: PlayerRegenEffects.ModifyFlags.Create(
					manaRegenFlags: PlayerRegenEffects.CommonRegenStats.CombinationFlags.Create(
						// Setting to true replaces the current calculation in this effect with ours. As with other override flags, set to false for compatibility!
						overrideDelta: true
					)
				)
			));

			// Example 5: Changing the strength of player stand still bonus on reducing ManaDelay
			list.Add(PlayerRegenEffects.ModifyRegenEffectStruct.Create(
				// The internal name of the effect we wish to modify. See Player.TML.cs for Vanilla names.
				targetEffect: "stillPlayerMana",

				// The struct containing information on what we want to modify in ManaRegenDelayStats. See ExampleRadiationDebuff for an example of this struct
				modifyManaDelayWith: PlayerRegenEffects.ManaRegenDelayStats.Create(
					increaseDelaySpeed: (player) => 2f // We want to increase the delay speed by two for this effect
				)
			));

			// Example 6: Changing the strength of player stand still bonus on reducing ManaDelay via overrides to restore it to normal
			list.Add(PlayerRegenEffects.ModifyRegenEffectStruct.Create(
				// The internal name of the effect we wish to modify. See Player.TML.cs for Vanilla names.
				targetEffect: "stillPlayerMana",

				// The struct containing information on what we want to modify in ManaRegenDelayStats. See ExampleRadiationDebuff for an example of this struct
				modifyManaDelayWith: PlayerRegenEffects.ManaRegenDelayStats.Create(
					increaseDelaySpeed: PlayerRegenEffects.stillPlayerMana.manaDelay.increaseDelaySpeed
				),

				flags: PlayerRegenEffects.ModifyFlags.Create(
					manaDelayFlags: PlayerRegenEffects.ManaRegenDelayStats.CombinationFlags.Create(
						// Setting to true replaces the current calculation in this effect with ours. As with other override flags, set to false for compatibility!
						overrideIncreaseDelaySpeed: true
					)
				)
			));

			// Example 7: Fully replacing vanilla mana regen with your own system. WARNING: Obviously this will be incompatible with all mods that rely on vanilla regen behaviour and code paths.
			list.Add(PlayerRegenEffects.ModifyRegenEffectStruct.Create(
				// The internal name of the effect we wish to modify. See Player.TML.cs for Vanilla names.
				targetEffect: "vanillaManaRegenBonus",

				// The struct containing information on what we want to modify in CommonRegenStats. See ExampleRadiationDebuff for an example of this struct
				modifyManaCommonWith: PlayerRegenEffects.CommonRegenStats.Create(
					// Additional Effects allows us to run arbitrary code returning void
					additionalEffects: (player, regen) => {
						// Normally this value can be used as a final multiplier on manaRegen
						player.manaRegenPotencyMultiplier = 0; // but setting to zero forces vanilla manaRegen to not be applied for the cycle.

						player.statMana = player.statManaMax2; // Our system will be straight forward - your mana is always going to be at max.
					}
				)
			));

			// Example 8: Undoing example 7 for practical reasons such as actually being able to see mana decrease.
			list.Add(PlayerRegenEffects.ModifyRegenEffectStruct.Create(
				// The internal name of the effect we wish to modify. See Player.TML.cs for Vanilla names.
				targetEffect: "vanillaManaRegenBonus",

				// AdditionalEffects defaults to null, so we will just override AdditionalEffects on the target effect with our the null.
				// This is fine because we already know we removed any existing additional code in example 7. Normally, you wouldn't do both 7 and 8 for obvious reasons.

				flags: PlayerRegenEffects.ModifyFlags.Create(
					manaRegenFlags: PlayerRegenEffects.CommonRegenStats.CombinationFlags.Create(
						// Setting to true replaces the current calculation in this effect with ours. As with other override flags, set to false for compatibility!
						overrideAdditionalEffects: true
					)
				)
			));

			// Finally, we return our 8 modifications that effectively change nothing, per our design.
			return list;
		}
	}
}
