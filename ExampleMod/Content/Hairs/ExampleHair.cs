using System.Collections.Generic;
using System.Linq;
using ExampleMod.Common;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Hairs
{
	// Based on Player_Hair_88 and Player_Hair_98
	// Note that internal hair ids are 1 less than the texture filename
	public class ExampleHair : ModHair
	{
		// This determines what gender the character will be when randomizing during character creation.
		// Keep in mind that hairstyles can be used with any gender; this property just allows for defining behavior during randomization.
		// If not set by us like here, Gender.Unspecified will be used instead, which randomizes the gender independent of the hairstyle.
		// This may be particularly useful if your hairstyle doesn't lean either way.
		public override Gender RandomizedCharacterCreationGender => Gender.Female;
		
		public override void SetStaticDefaults() {
			HairID.Sets.DrawBackHair[Type] = true;
		}

		public override IEnumerable<Condition> GetUnlockConditions() {
			yield return ExampleConditions.InExampleBiome;
		}
	}
}
