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
			item.rare = ItemRarityID.Red;
			item.accessory = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
			var modPlayer = ExampleDamagePlayer.ModPlayer(player);
			modPlayer.exampleResourceMax2 += 50; // add 50 to the exampleResourceMax2, which is our max for example resource.
			modPlayer.exampleResourceRegenRate -= 0.5f; // subtract 0.5f from the resourceRegenRate, halving the speed it takes for it to regen once.
		}
	}
}
