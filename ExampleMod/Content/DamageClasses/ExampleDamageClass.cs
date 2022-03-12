using System.Collections.Generic;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod.Content.DamageClasses
{
	public class ExampleDamageClass : DamageClass
	{
		protected override float CheckClassStatInheritance(DamageClass damageClass) {
			// This method lets you make your damage class benefit from other classes' stat bonuses by default, as well as universal stat bonuses.
			// To briefly summarize the two nonstandard damage class names used by DamageClass:
			// Default is, you guessed it, the default damage class. It doesn't scale off of any class-specific stat bonuses or universal stat bonuses.
			// There are a number of items and projectiles that use this, such as thrown waters and the Bone Glove's bones.
			// Generic, on the other hand, scales off of all universal stat bonuses and nothing else; it's the base damage class upon which all others that aren't Default are built.
			if (damageClass == DamageClass.Generic)
				return 1f;

			return 0f;
			// To explain how these return values work, each one behaves like a percentage, with 0f being 0%, 1f being 100%, and so on.
			// The return value indicates how much your class will scale off of any given damage class.
			// For example, if I were to return 0.5f (50%) for DamageClass.Ranged, this custom class would receive all ranged stat bonuses at 50% effectiveness.
			// There is no hardcap on what you can set this to.You can make it scale very heavily or barely at all...
			// ...and you can even invert the scaling effect if you choose by returning a negative value.

			// BONUS INFO:
			// To refer to a non-vanilla damage class for these sorts of things, use "ModContent.GetInstance<YourDamageClassHere>()" instead of "DamageClass.XYZ".
		}

		public override bool CheckClassEffectInheritance(DamageClass damageClass) {
			// This method allows you to make your damage class benefit from otherclass effects (e.g. Spectre bolts, Magma Stone) based on what returns true.
			// Note that unlike GetBenefitFrom, you do not need to account for universal bonuses in this method.
			// For this example, we'll make our class count as melee and magic for the purpose of class-specific effects.
			if (damageClass == DamageClass.Melee)
				return true;
			if (damageClass == DamageClass.Magic)
				return true;

			return false;
		}

		public override void SetDefaultStats(Player player) {
			// This method lets you set default statistical modifiers for your example damage class.
			// Here, we'll make our example damage class have more critical strike chance and armor penetration than normal.
			player.GetCritChance<ExampleDamageClass>() += 4;
			player.GetArmorPenetration<ExampleDamageClass>() += 10;
			// These sorts of modifiers also exist for damage (GetDamage), knockback (GetKnockback), and attack speed (GetAttackSpeed).
			// You'll see these used all around in referencce to vanilla classes and our example class here.
		}

		// This property lets you decide whether or not your damage class can use standard critical strike calculations.
		// Note that setting it to false will also prevent the critical strike chance tooltip line from being shown.
		// This prevention will overrule anything set by ShowStatTooltipLine, so be careful!
		public override bool AllowStandardCrits => true;

		public override bool ShowStatTooltipLine(Player player, string lineName) {
			// This method lets you prevent certain common statistical tooltip lines from appearing on items associated with this DamageClass.
			// The four line names you can use are "Damage", "CritChance", "Speed", and "Knockback". All four cases default to true.
			// PLEASE BE AWARE that this hook will NOT be here forever; only until an upcoming revamp to tooltips as a whole comes around.
			// Once this happens, a better, more versatile explanation of how to pull this off will be showcased, and this hook will be removed.
			return true;
		}
	}
}