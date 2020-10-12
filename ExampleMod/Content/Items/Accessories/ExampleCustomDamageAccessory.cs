using ExampleMod.Content.DamageClasses;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Accessories
{
	public class ExampleCustomDamageAccessory : ModItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("25% increased example damage\r\n+10% example damage crit chance");
		}

		public override void SetDefaults() {
			item.width = 40;
			item.height = 40;
			item.accessory = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
			// GetDamage returns a reference to the specified damage class' damage Modifier.
			// Since it doesn't return a value, but a reference to it, you can freely modify it with +-*/ operators.
			// Modifier is a structure that separately holds float additive and multiplicative modifiers.
			// When Modifier is applied to a value, its additive modifiers are applied before multiplicative ones.

			// In this case, we're multiplying by 1.25f, which will mean a 25% damage increase after every other additive modifier is applied.
			player.GetDamage<ExampleDamageClass>() *= 1.25f;

			// GetCrit, similarly to GetDamage, returns a reference to the specified damage class' crit Modifier.
			// In this case, we're adding 10% additive crit chance without multiplying others' bonuses.
			player.GetCrit<ExampleDamageClass>() += 10;
		}
	}
}
