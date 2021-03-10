using ExampleMod.Content.Items;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.Players
{
	public class ExampleModifyManaRegenEffects : ModPlayer
	{
		public override List<Player.RegenEffect.ModifyRegenEffectStruct> ModifyManaRegenEffects() {
			List<Player.RegenEffect.ModifyRegenEffectStruct> list = new List<Player.RegenEffect.ModifyRegenEffectStruct>();

			list.Add(new Player.RegenEffect.ModifyRegenEffectStruct {
				// The internal name of the effect we wish to modify. See Player.TML.cs for Vanilla names.
				targetEffect = ("manaRegenBonus2"),

				// The struct containing information on what we want to modify to. See ExampleRadiationDebuff for an example of this struct
				modifyWith = Player.RegenEffect.ByStatStruct.Create(
						_ => true,
						(obj) => obj.statLife / 2,
						true,
						(obj, delta) => obj.manaRegenDelay--
					),

				// The struct defining how we want to modify the existing effect. See CombinationFlags in Player.TML.cs.
				flags = Player.RegenEffect.CombinationFlags.Create(
						true, // Setting to true means we want to replace the current condition in targetEffect with ours. Set to false for compatibility!
						false, // When overrideCondition is false, this would set that we want to combine the conditions using Logical OR.
						false, // Setting to false means that we want our change to deltaRegenPer120Frames to be additive with existing. True would replace it.
						false, // Setting to false means that we want to use OR to forcePositiveRegen - ie we force it ourselves!
						false // setting to false means that we won't replace the current code in additionalEffects, and instead just pile on more.
					)
			});

			return list;
		}
	}
}
