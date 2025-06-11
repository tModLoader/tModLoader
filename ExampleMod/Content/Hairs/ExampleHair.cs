using ExampleMod.Common;
using System.Collections.Generic;
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

		// This determines whether the hairstyle will appear in the character creation UI.
		// You may run into cases where you want special cases for showing hairstyles here,
		// or to outright disable showing it like in our case where we want it exclusively in-game.
		public override bool AvailableDuringCharacterCreation => false;

		public override void SetStaticDefaults() {
			HairID.Sets.DrawBackHair[Type] = true;
		}

		// These conditions determine whether our hair is available in-game in the Stylist UI.
		// These conditions are not used to determine if the hairstyle is available during character creation.
		// See AvailableDuringCharacterCreation for that.
		public override IEnumerable<Condition> GetUnlockConditions() {
			yield return ExampleConditions.InExampleBiome;
		}
	}
}
