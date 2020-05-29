using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Items.ExampleDamageClass
{
	public class ExampleDamageAccessory : ModItem
	{
		public override string Texture => "Terraria/Item_" + ItemID.AnglerEarring;

		public override void SetStaticDefaults() {
			Tooltip.SetDefault("20% increased additive example damage" +
							   "\n20% more multiplicative example damage" +
							   "\n15% increased example critical strike chance" +
							   "\n5 increased increased example knockback");
		}

		public override void SetDefaults() {
			item.Size = new Vector2(34);
			item.rare = ItemRarityID.Red;
			item.accessory = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
			ExampleDamagePlayer modPlayer = ExampleDamagePlayer.ModPlayer(player);
			modPlayer.exampleDamageAdd += 0.2f; // add 20% to the additive bonus
			modPlayer.exampleDamageMult *= 1.2f; // add 20% to the multiplicative bonus
			modPlayer.exampleCrit += 15; // add 15% crit
			modPlayer.exampleKnockback += 5; // add 5 knockback
		}
	}
}
