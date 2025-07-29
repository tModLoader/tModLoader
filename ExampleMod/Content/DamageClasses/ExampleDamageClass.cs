using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Content.DamageClasses
{
	public class ExampleDamageClass : DamageClass
	{
		// This is an example damage class designed to demonstrate all the current functionality of the feature and explain how to create one of your own, should you need one.
		// For information about how to apply stat bonuses to specific damage classes, please instead refer to ExampleMod/Content/Items/Accessories/ExampleStatBonusAccessory.
		public override StatInheritanceData GetModifierInheritance(DamageClass damageClass) {
			// This method lets you make your damage class benefit from other classes' stat bonuses by default, as well as universal stat bonuses.
			// To briefly summarize the two nonstandard damage class names used by DamageClass:
			// Default is, you guessed it, the default damage class. It doesn't scale off of any class-specific stat bonuses or universal stat bonuses.
			// There are a number of items and projectiles that use this, such as thrown waters and the Bone Glove's bones.
			// Generic, on the other hand, scales off of all universal stat bonuses and nothing else; it's the base damage class upon which all others that aren't Default are built.
			if (damageClass == DamageClass.Generic)
				return StatInheritanceData.Full;

			return new StatInheritanceData(
				damageInheritance: 0f,
				critChanceInheritance: 0f,
				attackSpeedInheritance: 0f,
				armorPenInheritance: 0f,
				knockbackInheritance: 0f
			);
			// Now, what exactly did we just do, you might ask? Well, let's see here...
			// StatInheritanceData is a struct which you'll need to return one of for any given outcome this method.
			// Normally, the latter of these two would be written as "StatInheritanceData.None", rather than being typed out by hand...
			// ...but for the sake of clarity, we've written it out and labeled each parameter in order; they should be self-explanatory.
			// To explain how these return values work, each one behaves like a percentage, with 0f being 0%, 1f being 100%, and so on.
			// The return value indicates how much your class will scale off of the stat in question for whatever damage class(es) you've returned it for.
			// If you create a StatInheritanceData without any parameters, all of them will be set to 1f.
			// For example, if we propose a hypothetical alternate return for DamageClass.Ranged...
			/*
			if (damageClass == DamageClass.Ranged)
				return new StatInheritanceData(
					damageInheritance: 1f,
					critChanceInheritance: -1f,
					attackSpeedInheritance: 0.4f,
					armorPenInheritance: 2.5f,
					knockbackInheritance: 0f
				);
			*/
			// This would allow our custom class to benefit from the following ranged stat bonuses:
			// - Damage, at 100% effectiveness
			// - Attack speed, at 40% effectiveness
			// - Crit chance, at -100% effectiveness (this means anything that raises ranged crit chance specifically will lower the crit chance of our custom class by the same amount)
			// - Armor penetration, at 250% effectiveness

			// CAUTION: There is no hardcap on what you can set these to. Please be aware and advised that whatever you set them to may have unintended consequences,
			// and that we are NOT responsible for any temporary or permanent damage caused to you, your character, or your world as a result of your morbid curiosity.
			// To refer to a non-vanilla damage class for these sorts of things, use "ModContent.GetInstance<TargetDamageClassHere>()" instead of "DamageClass.XYZ".
		}

		public override bool GetEffectInheritance(DamageClass damageClass) {
			// This method allows you to make your damage class benefit from and be able to activate other classes' effects (e.g. Spectre bolts, Magma Stone) based on what returns true.
			// Note that unlike our stat inheritance methods up above, you do not need to account for universal bonuses in this method.
			// For this example, we'll make our class able to activate melee- and magic-specifically effects.
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
			// You'll see these used all around in reference to vanilla classes and our example class here. Familiarize yourself with them.
		}

		// This property lets you decide whether or not your damage class can use standard critical strike calculations.
		// Note that setting it to false will also prevent the critical strike chance tooltip line from being shown.
		// This prevention will overrule anything set by ShowStatTooltipLine, so be careful!
		public override bool UseStandardCritCalcs => true;

		public override bool ShowStatTooltipLine(Player player, string lineName) {
			// This method lets you prevent certain common statistical tooltip lines from appearing on items associated with this DamageClass.
			// The four line names you can use are "Damage", "CritChance", "Speed", and "Knockback". All four cases default to true, and thus will be shown. For example...
			if (lineName == "Speed")
				return false;

			return true;
			// PLEASE BE AWARE that this hook will NOT be here forever; only until an upcoming revamp to tooltips as a whole comes around.
			// Once this happens, a better, more versatile explanation of how to pull this off will be showcased, and this hook will be removed.
		}
	}
}