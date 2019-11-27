using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Items.ExampleDamageClass
{
	public class ExampleResourceAccessory : ModItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("\nDrastically increased example resource regen rate");
		}

		public override void SetDefaults() {
			item.Size = new Vector2(22);
			item.rare = 10;
			item.accessory = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
			ExampleDamagePlayer modPlayer = ExampleDamagePlayer.ModPlayer(player);
			//modPlayer.maximumResource2 += 50; // add 50 to the maximumResource2, which is our temporary increases to maximumResource.
			modPlayer.resourceRegenRate -= 0.5f; // subtract 0.5f from the resourceRegenRate, halving the speed it takes for it to regen once.
		}
	}
}
