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

		public override Dictionary<DamageClass, float> BenefitsFrom() {
			// Make this damage class not benefit from any otherclass stat bonuses by default.
			return null;

			// Now, I know you're just dyin' to get into the fun side of things, so let's have ourselves some demonstrations.
			// Feel free to comment out the above null return and uncomment one of the below! Experiment, play with the variables a bit. See what works best for you!

			// PROMPT: You want your damage class to benefit at a standard rate from a vanilla class' stat boosts.
			// The below makes your class benefit at a standard rate (100%) from all melee stat bonuses.
			/*
			Dictionary<DamageClass, float> benefit = new Dictionary<DamageClass, float>();
			benefit.Add(DamageClass.Melee, 1f);
			return benefit;
			*/

			// PROMPT: You want your damage class to benefit at a much higher rate from a vanilla class' stat boosts.
			// The below makes your class benefit at 500% effectiveness from all magic stat bonuses.
			/*
			Dictionary<DamageClass, float> benefit = new Dictionary<DamageClass, float>();
			benefit.Add(DamageClass.Magic, 5f);
			return benefit;
			*/

			// PROMPT: You want your damage class to benefit at a equal rate from two vanilla classes' stat boosts.
			// The below makes your class benefit at a standard rate from all melee and ranged stat bonuses equally.
			// This functionality can be useful for hybrid weapons, such as Calamity's Prismatic Breaker.
			/*
			Dictionary<DamageClass, float> benefit = new Dictionary<DamageClass, float>();
			benefit.Add(DamageClass.Melee, 1f);
			benefit.Add(DamageClass.Ranged, 1f);
			return benefit;
			*/

			// PROMPT: You want your damage class to benefit at a equal rate from a vanilla class' stat boosts and another modded class' stat boosts.
			// The below makes your class benefit at a standard rate from all melee and ranged stat bonuses equally.
			// This functionality can be useful for hybrid weapons, particularly those involving cross-mod content (see the guide on that for more detail!).
			/*
			Dictionary<DamageClass, float> benefit = new Dictionary<DamageClass, float>();
			benefit.Add(DamageClass.Melee, 1f);
			benefit.Add(ModContent.GetInstance<CoolDamageClass>(), 1f);
			return benefit;
			*/
			// Note that the other modded damage class isn't provided here --- that'd ruin the point, now wouldn't it?
		}

		public override List<DamageClass> CountsAs() {
			// Make this damage class not benefit from any otherclass effects (e.g. Spectre bolts, Magma Stone) by default.
			return null;
		}
	}
}
