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

		// By default, Visibility is set to Visibility.All, which allows this hairstyle appear in all UIs.
		// We want our hairstyle to specifically appear in just the Stylist's UI, not both the Stylist UI and the Character Creation UI.
		// This is because all modded hairstyles which may appear in the Character Creation UI entirely ignore GetUnlockConditions.
		public override HairVisibility Visibility => HairVisibility.Stylist;

		public override void SetStaticDefaults() {
			HairID.Sets.DrawBackHair[Type] = true;
		}

		// See the comment about Visibility for additional information.
		public override IEnumerable<Condition> GetUnlockConditions() {
			yield return ExampleConditions.InExampleBiome;
		}
	}
}
