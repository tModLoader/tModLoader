using ExampleMod.Common.Players;
using ExampleMod.Content.DamageClasses;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Accessories
{
	public class ExampleStatBonusAccessory : ModItem
	{
		// By declaring these here, changing the values will alter the effect, and the tooltip
		public static readonly int AdditiveDamageBonus = 25;
		public static readonly int MultiplicativeDamageBonus = 12;
		public static readonly int BaseDamageBonus = 4;
		public static readonly int FlatDamageBonus = 5;
		public static readonly int MeleeCritBonus = 10;
		public static readonly int RangedAttackSpeedBonus = 15;
		public static readonly int MagicArmorPenetration = 5;
		public static readonly int ExampleKnockback = 100;
		public static readonly int AdditiveCritDamageBonus = 20;

		// Insert the modifier values into the tooltip localization. More info on this approach can be found on the wiki: https://github.com/tModLoader/tModLoader/wiki/Localization#binding-values-to-localizations
		public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(AdditiveDamageBonus, MultiplicativeDamageBonus, BaseDamageBonus, FlatDamageBonus, MeleeCritBonus, RangedAttackSpeedBonus, MagicArmorPenetration, ExampleKnockback, AdditiveCritDamageBonus);

		public override void SetDefaults() {
			Item.width = 40;
			Item.height = 40;
			Item.accessory = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
			// GetDamage returns a reference to the specified damage class' damage StatModifier.
			// Since it doesn't return a value, but a reference to it, you can freely modify it with mathematics operators (+, -, *, /, etc.).
			// StatModifier is a structure that separately holds float additive and multiplicative modifiers, as well as base damage and flat damage.
			// When StatModifier is applied to a value, its additive modifiers are applied before multiplicative ones.
			// Base damage is added directly to the weapon's base damage and is affected by damage bonuses, while flat damage is applied after all other calculations.
			// In this case, we're doing a number of things:
			// - Adding 25% damage, additively. This is the typical "X% damage increase" that accessories use, use this one.
			// - Adding 12% damage, multiplicatively. This effect is almost never used in Terraria, typically you want to use the additive multiplier above. It is extremely hard to correctly balance the game with multiplicative bonuses.
			// - Adding 4 base damage.
			// - Adding 5 flat damage.
			// Since we're using DamageClass.Generic, these bonuses apply to ALL damage the player deals.
			player.GetDamage(DamageClass.Generic) += AdditiveDamageBonus / 100f;
			player.GetDamage(DamageClass.Generic) *= 1 + MultiplicativeDamageBonus / 100f;
			player.GetDamage(DamageClass.Generic).Base += BaseDamageBonus;
			player.GetDamage(DamageClass.Generic).Flat += FlatDamageBonus;

			// GetCrit, similarly to GetDamage, returns a reference to the specified damage class' crit chance.
			// In this case, we're adding 10% crit chance, but only for the melee DamageClass (as such, only melee weapons will receive this bonus).
			// NOTE: Once all crit calculations are complete, a weapon or class' total crit chance is typically cast to an int. Plan accordingly.
			player.GetCritChance(DamageClass.Melee) += MeleeCritBonus;

			// GetAttackSpeed is functionally identical to GetDamage and GetKnockback; it's for attack speed.
			// In this case, we'll make ranged weapons 15% faster to use overall.
			// NOTE: Zero or a negative value as the result of these calculations will throw an exception. Plan accordingly.
			player.GetAttackSpeed(DamageClass.Ranged) += RangedAttackSpeedBonus / 100f;

			// GetArmorPenetration is functionally identical to GetCritChance, but for the armor penetration stat instead.
			// In this case, we'll add 5 armor penetration to magic weapons.
			// NOTE: Once all armor pen calculations are complete, the final armor pen amount is cast to an int. Plan accordingly.
			player.GetArmorPenetration(DamageClass.Magic) += MagicArmorPenetration;

			// GetKnockback is functionally identical to GetDamage, but for the knockback stat instead.
			// In this case, we're adding 100% knockback additively, but only for our custom example DamageClass (as such, only our example class weapons will receive this bonus).
			player.GetKnockback<ExampleDamageClass>() += ExampleKnockback / 100f;

			player.GetModPlayer<ExampleDamageModificationPlayer>().AdditiveCritDamageBonus += AdditiveCritDamageBonus / 100f;
			// Some effects are applied in ExampleStatBonusAccessoryPlayer below.
			player.GetModPlayer<ExampleStatBonusAccessoryPlayer>().exampleStatBonusAccessory = true;
		}
	}

	// Some movement effects are not suitable to be modified in ModItem.UpdateAccessory due to how the math is done.
	// ModPlayer.PostUpdateRunSpeeds is suitable for these modifications.
	public class ExampleStatBonusAccessoryPlayer : ModPlayer
	{
		public bool exampleStatBonusAccessory = false;

		public override void ResetEffects() {
			exampleStatBonusAccessory = false;
		}

		public override void PostUpdateRunSpeeds() {
			// We only want our additional changes to apply if ExampleStatBonusAccessory is equipped and not on a mount.
			if (Player.mount.Active || !exampleStatBonusAccessory) {
				return;
			}

			// The following modifications are similar to Shadow Armor set bonus
			Player.runAcceleration *= 1.75f; // Modifies player run acceleration
			Player.maxRunSpeed *= 1.15f;
			Player.accRunSpeed *= 1.15f;
			Player.runSlowdown *= 1.75f;
		}
	}
}