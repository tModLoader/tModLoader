using ExampleMod.Content.DamageClasses;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Accessories
{
	public class ExampleStatBonusAccessory : ModItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("25% increased damage, this is an additive multiplier with other damage bonuses\n"
							 + "12% increased multiplicative damage multiplier; this is multiplicative with other damage bonuses\n"
							 + "Increases base damage for all weapons by 4\n"
							 + "Increases total damage for all weapons by 5\n"
							 + "10% increased melee crit chance\n"
							 + "100% increased example knockback\n"
							 + "Magic attacks ignore an additional 5 defense points\n"
							 + "Increases ranged firing speed by 15%");

			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

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
			// - Adding 12% damage, multiplicatively. This effect is almost never useds in Terraria, typically you want to use the additive multiplier above. It is extremely hard to correctly balance the game with multiplicative bonuses.
			// - Adding 4 base damage.
			// - Adding 5 flat damage.
			// Since we're using DamageClass.Generic, these bonuses apply to ALL damage the player deals.
			player.GetDamage(DamageClass.Generic) += 0.25f;
			player.GetDamage(DamageClass.Generic) *= 1.12f;
			player.GetDamage(DamageClass.Generic).Base += 4f;
			player.GetDamage(DamageClass.Generic).Flat += 5f;

			// GetCrit, similarly to GetDamage, returns a reference to the specified damage class' crit chance.
			// In this case, we're adding 10% crit chance, but only for the melee DamageClass (as such, only melee weapons will receive this bonus).
			// NOTE: Once all crit calculations are complete, a weapon or class' total crit chance is typically cast to an int. Plan accordingly.
			player.GetCritChance(DamageClass.Melee) += 10f;

			// GetAttackSpeed is functionally identical to GetDamage and GetKnockback; it's for attack speed.
			// In this case, we'll make ranged weapons 15% faster to use overall.
			// NOTE: Zero or a negative value as the result of these calculations will throw an exception. Plan accordingly.
			player.GetAttackSpeed(DamageClass.Ranged) += 0.15f;

			// GetArmorPenetration is functionally identical to GetCritChance, but for the armor penetration stat instead.
			// In this case, we'll add 5 armor penetration to magic weapons.
			// NOTE: Once all armor pen calculations are complete, the final armor pen amount is cast to an int. Plan accordingly.
			player.GetArmorPenetration(DamageClass.Magic) += 5f;

			// GetKnockback is functionally identical to GetDamage, but for the knockback stat instead.
			// In this case, we're adding 100% knockback additively, but only for our custom example DamageClass (as such, only our example class weapons will receive this bonus).
			player.GetKnockback<ExampleDamageClass>() += 1f;
		}
	}
}