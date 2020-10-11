using ExampleMod.Content.DamageClasses;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Accessories
{
	public class ExampleCustomDamageAccessory : ModItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("25% increased example damage");
		}

		public override void SetDefaults() {
			item.width = 40;
			item.height = 40;
			item.accessory = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
			// GetDamage returns a reference to the specified damage class' damage Modifier.
			// Since it doesn't return a value, but a reference to it, you can freely modify it with +-*/ operators.
			// In this case, we're multiplying it by 0.25f, which means a 25% damage increase.
			// If you want to add or subtract a non-multiplicative damage value, you can use + or - operators on it instead.
			player.GetDamage<ExampleDamageClass>() *= 0.25f;
		}
	}
}
