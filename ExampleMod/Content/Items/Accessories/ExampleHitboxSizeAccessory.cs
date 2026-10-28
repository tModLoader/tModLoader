using ExampleMod.Common.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Accessories
{
	// This accessory makes the player bigger. See ExampleHitboxSizePlayer for how the hitbox and drawing are changed.
	public class ExampleHitboxSizeAccessory : ModItem
	{
		public override string Texture => "Terraria/Images/Item_" + ItemID.TitanGlove;

		public override void SetDefaults() {
			Item.width = 26;
			Item.height = 28;
			Item.value = Item.sellPrice(0, 1);
			Item.accessory = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.GetModPlayer<ExampleHitboxSizePlayer>().WantsToGrow = true;
		}
	}
}
