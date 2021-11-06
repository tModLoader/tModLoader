using ExampleMod.Content.DamageClasses;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Accessories
{
	public class ExampleStatBonusAccessory : ModItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("20% increased damage\n"
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
			// Since it doesn't return a value, but a reference to it, you can freely modify it with +-*/ operators.
			// StatModifier is a structure that separately holds float additive and multiplicative modifiers.
			// When StatModifier is applied to a value, its additive modifiers are applied before multiplicative ones.

			// In this case, we're multiplying by 1.20f, which will mean a 20% damage increase after every additive modifier (and a number of multiplicative modifiers) are applied.
			// Since we're using DamageClass.Generic, this bonus applies to ALL damage the player deals.
			player.GetDamage(DamageClass.Generic) *= 1.20f;

			// GetCrit, similarly to GetDamage, returns a reference to the specified damage class' crit chance.
			// In this case, we're adding 10% crit chance, but only for the melee DamageClass (as such, only melee weapons will receive this bonus).
			player.GetCritChance(DamageClass.Melee) += 10;

			// GetKnockback is functionally identical to GetDamage, but for the knockback stat instead.
			// In this case, we're adding 100% knockback additively, but only for our custom example DamageClass (as such, only our example class weapons will receive this bonus).
			player.GetKnockback<ExampleDamageClass>() += 1f;

			// GetArmorPen is functionally identical to GetCritChance, but for the armor penetration stat instead.
			// In this case, we'll add 5 armor penetration to magic weapons.
			player.GetArmorPen(DamageClass.Magic) += 5;

			// GetAttackSpeed works similarly to GetCritChance and GetArmorPen, but uses a float instead of an int.
			// In this case, we'll make ranged weapons 15% faster to use.
			player.GetAttackSpeed(DamageClass.Ranged) += 0.15f;
		}
	}
}
