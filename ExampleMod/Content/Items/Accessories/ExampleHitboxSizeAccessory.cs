using ExampleMod.Common.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Accessories
{
	// This accessory makes the player's hitbox bigger. See ExampleHitboxSizePlayer for how the size is changed.
	// Only the hitbox changes, the player is still drawn at their normal size.
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
