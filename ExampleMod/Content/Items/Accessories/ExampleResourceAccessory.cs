using ExampleMod.Common.Players;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Accessories
{
	public class ExampleResourceAccessory : ModItem
	{
		public static readonly int ResourceBoost = 100;

		public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(ResourceBoost);

		public override void SetDefaults() {
			Item.width = 26;
			Item.height = 32;
			Item.maxStack = 1;
			Item.value = Item.sellPrice(gold: 5);
			Item.accessory = true;
			Item.rare = ItemRarityID.Red;
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
			var modPlayer = player.GetModPlayer<ExampleResourcePlayer>();
			modPlayer.exampleResourceMax2 += ResourceBoost; // add 100 to the exampleResourceMax2, which is our max for example resource.
			modPlayer.exampleResourceRegenRate *= 6f; // multiply our resource regeneration speed by 6.
			modPlayer.exampleResourceMagnet = true; // Boosts pickup range for ExampleResourcePickup
		}
	}
}
