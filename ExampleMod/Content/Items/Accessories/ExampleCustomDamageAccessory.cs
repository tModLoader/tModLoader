using ExampleMod.Content.DamageClasses;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Accessories
{
	public class ExampleCustomDamageAccessory : ModItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("25% increased example damage\r\n10% increased example crit chance\r\n100% increased example knockback");
		}

		public override void SetDefaults() {
			Item.width = 40;
			Item.height = 40;
			Item.accessory = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
			// GetDamage returns a reference to the specified damage class' damage StatModifier.
			// Since it doesn't return a value, but a reference to it, you can freely modify it with +-*/ operators.
			// Modifier is a structure that separately holds float additive and multiplicative modifiers.
			// When Modifier is applied to a value, its additive modifiers are applied before multiplicative ones.

			// In this case, we're multiplying by 1.25f, which will mean a 25% damage increase after every additive modifier (and a number of multiplicative modifiers) are applied.
			player.GetDamage<ExampleDamageClass>() *= 1.25f;

			// GetCrit, similarly to GetDamage, returns a reference to the specified damage class' crit chance.
			// In this case, we're adding 10% crit chance.
			player.GetCrit<ExampleDamageClass>() += 10;

			// GetKnockback is functionally identical to GetDamage, but for the knockback stat instead.
			// In this case, we're adding 100% knockback additively.
			player.GetKnockback<ExampleDamageClass>() += 1f;
		}
	}
}
