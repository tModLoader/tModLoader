using System.Collections.Generic;
using Terraria.ModLoader;

namespace ExampleMod.Content.DamageClasses
{
	public class ExampleDamageClass : DamageClass
	{
		public override void SetupContent() {
			// Make weapons with this damage type have a tooltip of 'X example damage'.
			ClassName.SetDefault("example damage");
		}

		protected override float GetBenefitFrom(DamageClass damageClass) {
			// Make this damage class not benefit from any otherclass stat bonuses by default, but still benefit from universal/all-class bonuses.
			if (damageClass == DamageClass.Generic)
				return 1f;

			// Now, I know you're just dyin' to get into the fun side of things, so let's have ourselves some demonstrations.
			// Feel free to uncomment one of the below! Experiment, play with the variables a bit. See what works best for you!

			// PROMPT: You want your damage class to benefit at a standard rate from a vanilla class' stat boosts.
			// The below makes your class benefit at a standard rate (100%) from all melee stat bonuses.
			/*
			if (damageClass == DamageClass.Melee)
				return 1f;
			*/

			// PROMPT: You want your damage class to benefit at a much higher rate from a vanilla class' stat boosts.
			// The below makes your class benefit at 500% effectiveness from all magic stat bonuses.
			/*
			if (damageClass == DamageClass.Magic)
				return 5f;
			*/

			// PROMPT: You want your damage class to benefit at a equal rate from two vanilla classes' stat boosts.
			// The below makes your class benefit at a standard rate from all melee and ranged stat bonuses equally.
			// This functionality can be useful for hybrid weapons, such as Calamity's Prismatic Breaker.
			/*
			if (damageClass == DamageClass.Melee)
				return 1f;
			if (damageClass == DamageClass.Ranged)
				return 1f;
			*/

			// PROMPT: You want your damage class to benefit at a equal rate from a vanilla class' stat boosts and another modded class' stat boosts.
			// The below makes your class benefit at a standard rate from melee stat bonuses and at a 200% rate from another modded class' stat bonuses.
			// This functionality can be useful for hybrid weapons, particularly those involving cross-mod content (see the guide on that for more detail!).
			/*
			if (damageClass == DamageClass.Melee)
				return 1f;
			if (damageClass == ModContent.GetInstance<CoolDamageClass>())
				return 2f;
			*/
			// Note that the other modded damage class isn't provided here --- that'd ruin the point, now wouldn't it?

			return 0;
		}

		public override bool CountsAs(DamageClass damageClass) {
			// Make this damage class not benefit from any otherclass effects (e.g. Spectre bolts, Magma Stone) by default.
			// Note that unlike GetBenefitFrom, you do not need to account for universal bonuses in this method.
			return false;
		}
	}
}
