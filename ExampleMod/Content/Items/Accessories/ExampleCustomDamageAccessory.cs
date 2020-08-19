using ExampleMod.Content.DamageClasses;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Accessories
{
	public class ExampleCustomDamageAccessory : ModItem
	{
		public override string Texture => "ExampleMod/Content/Items/Accessories/ExampleShield"; // TODO: remove when sprite is made for this

		public override void SetStaticDefaults() {
			Tooltip.SetDefault("25% increased example damage");
		}

		public override void SetDefaults() {
			item.width = 40;
			item.height = 40;
			item.accessory = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.SetDamage<ExampleDamageClass>(0.25f); // This method will add the given value onto the given damage class' damage stat - in this case, 0.25f or a 25% increase.
		}
	}
}
